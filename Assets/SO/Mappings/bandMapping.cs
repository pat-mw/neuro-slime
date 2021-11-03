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
            // LEFT BANDS
            case MappingIndex.Influencers.bandLeftDelta:
                // range (0.1 - 100) (log10 scale)
                influencerValue = brainData.leftBands.delta;
                break;
            case MappingIndex.Influencers.bandLeftTheta:
                influencerValue = brainData.leftBands.theta;
                break;
            case MappingIndex.Influencers.bandLeftAlpha:
                influencerValue = brainData.leftBands.alpha;
                break;

            // RIGHT BANDS
            case MappingIndex.Influencers.bandRightDelta:
                influencerValue = brainData.rightBands.delta;
                break;
            case MappingIndex.Influencers.bandRightTheta:
                influencerValue = brainData.rightBands.theta;
                break;
            case MappingIndex.Influencers.bandRightAlpha:
                influencerValue = brainData.rightBands.alpha;
                break;

            // ERROR HANDLING
            default:
                Debug.LogError("Sorry, the Influencer you chose is not valid");
                break;
        }

        // fetch receiver value from brain data
        switch (receiver)
        {
            case MappingIndex.Receivers.trailWeight:
                Mappings.ApplyMap(mapType, influencerValue, ref slimeSettings.trailWeight);
                break;
            case MappingIndex.Receivers.decayRate:
                Mappings.ApplyMap(mapType, influencerValue, ref slimeSettings.decayRate);
                break;
            case MappingIndex.Receivers.diffuseRate:
                Mappings.ApplyMap(mapType, influencerValue, ref slimeSettings.diffuseRate);
                break;

            // ERROR HANDLING
            default:
                Debug.LogError("Sorry, the Receiver you chose is not valid");
                break;
        }
    }

    float LogarithmicMap(float influencerValue, (float, float) influencerRange, (float, float) receiverRange)
    {
        // TODO: figure out mapping from log scale to linear scale.
        return influencerValue;
    }
}

