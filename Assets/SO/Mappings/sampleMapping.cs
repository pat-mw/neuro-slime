using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// THOUGHTS
/// 
/// Mapping currently takes place in an update loop
/// specifically SlowUpdate() in simulation
/// This causes a bottleneck for one big reasons:
/// - for minor changes in the influencer, the receiver is still updated in the simulation loop, which is unnecessary
/// 
/// Instead, there should be an events based system 
/// One script (MappingViewer) to monitor the influencer values
/// triggers ApplyMapping() events on whichever maps use that influencer
///
/// Happens only when the influencer value changes beyond a certain & since the last frame
/// essentially having a distanceThreshold beyond which the mapping is activated
/// 
/// Should save simulation time.
/// </summary>

[CreateAssetMenu(menuName = "MAPPINGS/SampleMapping")]
public class sampleMapping : SerializedScriptableObject, IMapping
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
            // SAMPLE MAPPING
            case MappingIndex.Influencers.sampleAmplitudeLeft:
                influencerValue = brainData.currentEpoch.FetchLatestSampleLeft();
                break;
            case MappingIndex.Influencers.sampleAmplitudeRight:
                influencerValue = brainData.currentEpoch.FetchLatestSampleRight();
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
}