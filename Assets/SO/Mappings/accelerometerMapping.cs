using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "MAPPINGS/AccelerometerMapping")]
public class accelerometerMapping : SerializedScriptableObject, IMapping
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
            // ACCELEROMETER MAPPINGS
            case MappingIndex.Influencers.accelerometerX:
                // accelerometerX range: (-1f, 1f)
                influencerValue = brainData.accelerometer.X;
                break;
            case MappingIndex.Influencers.accelerometerY:
                // accelerometerY range: (-1f, 1f)
                influencerValue = brainData.accelerometer.Y;
                break;
            case MappingIndex.Influencers.accelerometerZ:
                // accelerometerZ range: (-1f, 1f)
                influencerValue = brainData.accelerometer.Z;
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
                // stepsPerFrame range: (0, 5) (int)
                // (-1f, 1f) --> (0, 5)
                influencerValue = LinearRemap(influencerValue, (-1f, 1f), (0f, 5f));
                influencerValue = Mathf.RoundToInt(influencerValue);
                Mappings.ApplyMap(mapType, (int)influencerValue, ref slimeSettings.stepsPerFrame);
                break;
            case MappingIndex.Receivers.numAgents:
                // stepsPerFrame range: (100000, 500000) (int)
                influencerValue = LinearRemap(influencerValue, (-1f, 1f), (100000, 500000));
                influencerValue = Mathf.RoundToInt(influencerValue);
                Mappings.ApplyMap(mapType, (int)influencerValue, ref slimeSettings.numAgents);
                break;
            case MappingIndex.Receivers.trailWeight:
                // trailWeight range: (0f, 100f)
                influencerValue = LinearRemap(influencerValue, (-1f, 1f), (0f, 100f));
                Mappings.ApplyMap(mapType, influencerValue, ref slimeSettings.trailWeight);
                break;
            case MappingIndex.Receivers.decayRate:
                // decayRate range: (0.1f, 10f)
                influencerValue = LinearRemap(influencerValue, (-1f, 1f), (0.1f, 10f));
                Mappings.ApplyMap(mapType, influencerValue, ref slimeSettings.decayRate);
                break;
            case MappingIndex.Receivers.diffuseRate:
                // diffuseRate range: (0f, 100f)
                influencerValue = LinearRemap(influencerValue, (-1f, 1f), (0f, 100f));
                Mappings.ApplyMap(mapType, influencerValue, ref slimeSettings.diffuseRate);
                break;

            // ERROR HANDLING
            default:
                Debug.LogError("Sorry, the Receiver you chose is not valid");
                break;
        }
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

        Debug.Log($"LINEAR MAP - input: {x} - output: {y}");
        return y;
    }
}
