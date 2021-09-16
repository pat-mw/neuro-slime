using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public static int EPOCH_LENGTH = 10;
    public static int EPOCH_SAMPLE_COUNT = SAMPLE_RATE * EPOCH_LENGTH;
}
