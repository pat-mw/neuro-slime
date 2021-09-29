using UnityEngine;
using Sirenix.OdinInspector;

public interface IMapping
{
    void Map();
}

public enum MapType
{
    Multiply,
    Add,
    Follow,
    Subtract
}

public static class MappingIndex
{
    public enum Influencers
    {
        bandLeftDelta,
        bandLeftTheta,
        bandLeftAlpha,
        bandLeftBeta,
        bandLeftGamma,
        bandRightDelta,
        bandRightTheta,
        bandRightAlpha,
        bandRightBeta,
        bandRightGamma,
        accelerometerX,
        accelerometerY,
        accelerometerZ,
        sampleAmplitudeLeft,
        sampleAmplitudeRight
    }

    public enum Receivers
    {
        stepsPerFrame,
        numAgents,
        trailWeight,
        decayRate,
        diffuseRate
    }
}

public static class Mappings
{
    public static void ApplyMap(MapType type, float influencer, ref float receiver)
    {

        // Debug.Log($"Applying mapping || type: {type} | influencer: {influencer} | receiver: {receiver}");
        switch (type)
        {
            case MapType.Follow:
                receiver = influencer;
                break;
            case MapType.Multiply:
                receiver *= influencer;
                break;
            case MapType.Add:
                receiver += influencer;
                break;
            case MapType.Subtract:
                receiver -= influencer;
                break;
            default:
                receiver = 0;
                break;
        }
    }
}


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
                influencerValue = brainData.accelerometer.X;
                break;
            case MappingIndex.Influencers.accelerometerY:
                influencerValue = brainData.accelerometer.Y;
                break;
            case MappingIndex.Influencers.accelerometerZ:
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
                influencerValue = brainData.leftBands.delta;
                break;
            case MappingIndex.Influencers.bandLeftTheta:
                influencerValue = brainData.leftBands.theta;
                break;
            case MappingIndex.Influencers.bandLeftAlpha:
                influencerValue = brainData.leftBands.alpha;
                break;
            case MappingIndex.Influencers.bandLeftBeta:
                influencerValue = brainData.leftBands.beta;
                break;
            case MappingIndex.Influencers.bandLeftGamma:
                influencerValue = brainData.leftBands.gamma;
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
            case MappingIndex.Influencers.bandRightBeta:
                influencerValue = brainData.rightBands.beta;
                break;
            case MappingIndex.Influencers.bandRightGamma:
                influencerValue = brainData.rightBands.gamma;
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


