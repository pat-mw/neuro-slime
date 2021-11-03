using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


[CreateAssetMenu(menuName = "MAPPINGS/BandMapping")]
public class bandMapping : SerializedScriptableObject, IMapping
{
    public BrainData brainData;
    public SlimeSettings slimeSettings;

    public MapType mapType;

    public MappingIndex.Influencers influencer;
    public MappingIndex.Receivers receiver;

    private float influencerValue;

    public void Map()
    {
        // fetch influencer value from brain data
        switch (influencer)
        {
            // BOTH BANDS
            case MappingIndex.Influencers.totalBandPower:
                // range: (0, 100) roughly
                influencerValue = totalBandPower(brainData.leftBands, brainData.rightBands);
                break;
            // LEFT BAND
            case MappingIndex.Influencers.totalBandsLeft:
                // range: (0, 50) roughly
                influencerValue = totalBandPowerSingleChannel(brainData.leftBands);
                break;
            // RIGHT BAND
            case MappingIndex.Influencers.totalBandsRight:
                // range: (0, 50) roughly
                influencerValue = totalBandPowerSingleChannel(brainData.rightBands);
                break;
            // ERROR HANDLING
            default:
                Debug.LogError("Sorry, the Influencer you chose is not valid");
                break;
        }

        // fetch receiver value from brain data
        switch (receiver)
        {
            case MappingIndex.Receivers.stepsPerFrame:
                // stepsPerFrame range: (1, 5) (int)
                // (0f, 100f) --> (0, 5)
                influencerValue = LinearRemap(influencerValue, (0, 100f), (1f, 3f));
                influencerValue = Mathf.RoundToInt(influencerValue);
                Mappings.ApplyMap(mapType, (int)influencerValue, ref slimeSettings.stepsPerFrame);
                break;
            case MappingIndex.Receivers.numAgents:
                // stepsPerFrame range: (100000, 500000) (int)
                influencerValue = LinearRemap(influencerValue, (0, 100f), (100000, 500000));
                influencerValue = Mathf.RoundToInt(influencerValue);
                Mappings.ApplyMap(mapType, (int)influencerValue, ref slimeSettings.numAgents);
                break;

            // ERROR HANDLING
            default:
                Debug.LogError("Sorry, the Receiver you chose is not valid");
                break;
        }
    }


    float totalBandPower(BrainData.BandPower leftBands, BrainData.BandPower rightBands)
    {
        float totalLeft = leftBands.alpha + leftBands.delta + leftBands.theta;
        float totalRight = rightBands.alpha + rightBands.delta + rightBands.theta;
        float total = totalLeft + totalRight;
        return total;
    }

    float totalBandPowerSingleChannel(BrainData.BandPower bands)
    {
        float total = bands.alpha + bands.delta + bands.theta;
        return total;
    }

    float LogarithmicMap(float influencerValue, (float, float) influencerRange, (float, float) receiverRange)
    {
        // TODO: figure out mapping from log scale to linear scale.
        return influencerValue;
    }

    private float LinearRemap(float influencerValue, (float, float) influencerRange, (float, float) receiverRange)
    {
        var x = influencerValue;
        var I1 = influencerRange.Item1;
        var I2 = influencerRange.Item2;

        var R1 = receiverRange.Item1;
        var R2 = receiverRange.Item2;

        // make sure the influencerValue actually sits in the influencerRange, if not - then set it to the boundary
        if (x < I1) { x = I1; }
        if (x > I2) { x = I2; }

        // y = mx + c
        var y = ((R2 - R1) / (I2 - I1)) * (x - I1) + R1;

        //Debug.Log($"LINEAR MAP - input: {x} - output: {y}");
        return y;
    }
}

