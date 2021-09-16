using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName ="Brain Data Store")]
[System.Serializable]
public class BrainDataStore : SerializedScriptableObject 
{
    [Header("BAND POWER SPECTRA")]
    public BandPower leftBands = new BandPower(GlobalConfig.LEFT_CHANNEL);
    public BandPower rightBands = new BandPower(GlobalConfig.RIGHT_CHANNEL);

    [Header("ACCELEROMETER")]
    public Accelerometer accelerometer = new Accelerometer();

    [Header("EEG SAMPLES")]
    public EEGSample currentEpoch = new EEGSample(GlobalConfig.EPOCH_SAMPLE_COUNT);
    public EEGSample previousEpoch = new EEGSample(GlobalConfig.EPOCH_SAMPLE_COUNT);

    public void BackupEpoch()
    {
        if (currentEpoch.epochComplete)
        {
            previousEpoch = currentEpoch;
            currentEpoch = new EEGSample(GlobalConfig.EPOCH_SAMPLE_COUNT);
        }
        else
        {
            Debug.LogError($"Epoch not complete, cannot back up " +
                $"\n count L: {currentEpoch.leftIndex} - " +
                $"count R: {currentEpoch.rightIndex} - " +
                $"epoch count: {GlobalConfig.EPOCH_SAMPLE_COUNT} ");
        }
    }
}


[System.Serializable]
public class EEGSample
{
    public float[] left;
    public float[] right;

    public bool leftFull = false;
    public bool rightFull = false;
    public bool epochComplete = false;

    public int leftIndex = 0;
    public int rightIndex = 0;

    public EEGSample(int sampleCount)
    {
        left = new float[sampleCount];
        right = new float[sampleCount];
    }

    public void AddSample(float value, GlobalConfig.CHANNEL channel)
    {
        try
        {
            Debug.Log("ADDING SAMPLE");
            if (!epochComplete)
            {
                switch (channel)
                {
                    case GlobalConfig.CHANNEL.LEFT:
                        if (leftIndex < left.Length)
                        {
                            left[leftIndex] = value;
                            leftIndex += 1;
                        }
                        else
                        {
                            leftFull = true;
                        }
                        break;
                    case GlobalConfig.CHANNEL.RIGHT:
                        if (rightIndex < right.Length)
                        {
                            right[rightIndex] = value;
                            rightIndex += 1;
                        }
                        else
                        {
                            rightFull = true;
                        }
                        break;
                    default:
                        Debug.LogError($"Channel type not recognised as a valid channel: {channel}");
                        break;
                }

                if (leftFull == true && rightFull == true)
                {
                    epochComplete = true;
                }
            }
            else
            {
                Debug.LogError("Attempted to add sample to an already full epoch!!");
            }
            
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error adding value: {value} to channel: {channel} \n Full Exception: {e}");
        }
    }
}


[System.Serializable]
public class BandPower
{
    public int channelNo;
    public float delta;
    public float theta;
    public float alpha;
    public float beta;
    public float gamma;

    public BandPower(int _channelNo)
    {
        channelNo = _channelNo;
    }
}

[System.Serializable]
public class Accelerometer
{
    public float X;
    public float Y;
    public float Z;
}
