using UnityEngine;

public interface IMapping
{
    void Map();
}

public enum MapType
{
    Follow,
    //Multiply,
    //Add,
    //Subtract
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
    // one for floats
    public static void ApplyMap(MapType type, float influencer, ref float receiver)
    {
        // Debug.Log($"Applying mapping || type: {type} | influencer: {influencer} | receiver: {receiver}");
        switch (type)
        {
            case MapType.Follow:
                receiver = influencer;
                break;

            // dynamic mappings to be added later
            //case MapType.Multiply:
            //    receiver *= influencer;
            //    break;
            //case MapType.Add:
            //    receiver += influencer;
            //    break;
            //case MapType.Subtract:
            //    receiver -= influencer;
            //    break;
            default:
                receiver = 0;
                break;
        }
    }

    // one for ints
    public static void ApplyMap(MapType type, int influencer, ref int receiver)
    {
        switch (type)
        {
            case MapType.Follow:
                receiver = influencer;
                break;
            default:
                receiver = 0;
                break;
        }
    }
}