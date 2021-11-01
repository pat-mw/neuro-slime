public static class GlobalConfig
{
    public enum CHANNEL
    {
        LEFT,
        RIGHT
    }

    public static int LEFT_CHANNEL = 1;
    public static int RIGHT_CHANNEL = 8;

    public static int SAMPLE_RATE = 250;

    public static int CALIBRATION_DURATION = 20;
    public static int CALIBRATION_SAMPLE_COUNT = SAMPLE_RATE * CALIBRATION_DURATION;

    public static int EPOCH_DURATION = 10;
    public static int EPOCH_SAMPLE_COUNT = SAMPLE_RATE * EPOCH_DURATION;

    public static int DATA_BUFFER_DURATION = 2;
    public static int DATA_BUFFER_COUNT = SAMPLE_RATE * DATA_BUFFER_DURATION;

    public static float TRANSITION_DURATION = 10f;
}
