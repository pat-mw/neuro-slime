using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using ScriptableObjectArchitecture;

[CreateAssetMenu(menuName ="Brain Data Store")]
[System.Serializable]
public class BrainData : SerializedScriptableObject
{
    [Header("EVENTS")]
    public GameEvent onStartInference = default(GameEvent);
    public GameEvent OnFetchMood = default(GameEvent);

    [Header("BAND POWER SPECTRA")]
    public BandPower leftBands = new BandPower(GlobalConfig.LEFT_CHANNEL);
    public BandPower rightBands = new BandPower(GlobalConfig.RIGHT_CHANNEL);

    [Header("ACCELEROMETER")]
    public Accelerometer accelerometer = new Accelerometer();

    [Header("EEG SAMPLES")]
    public EEGBuffer dataBuffer = new EEGBuffer(GlobalConfig.DATA_BUFFER_COUNT);
    public EEGSample currentEpoch = new EEGSample(GlobalConfig.EPOCH_SAMPLE_COUNT);
    public EEGSample previousEpoch = new EEGSample(GlobalConfig.EPOCH_SAMPLE_COUNT);
    public EEGSample calibrationPeriod = new EEGSample(GlobalConfig.CALIBRATION_SAMPLE_COUNT);


    [GUIColor(0, 0, 1)]
    [Button(ButtonSizes.Large)]
    [ButtonGroup("Reset Brain Data")]
    public void Reset()
    {
        dataBuffer = new EEGBuffer(GlobalConfig.DATA_BUFFER_COUNT);
        currentEpoch = new EEGSample(GlobalConfig.EPOCH_SAMPLE_COUNT);
        previousEpoch = new EEGSample(GlobalConfig.EPOCH_SAMPLE_COUNT);
        calibrationPeriod = new EEGSample(GlobalConfig.CALIBRATION_SAMPLE_COUNT);
        leftBands = new BandPower(GlobalConfig.LEFT_CHANNEL);
        rightBands = new BandPower(GlobalConfig.RIGHT_CHANNEL);
        accelerometer = new Accelerometer();
    }

    public void BackupEpoch()
    {
        if (currentEpoch.epochComplete)
        {
            previousEpoch = currentEpoch;

            currentEpoch = new EEGSample(GlobalConfig.EPOCH_SAMPLE_COUNT);

            OnFetchMood.Raise();
        }
        else
        {
            Debug.LogError($"Epoch not complete, cannot back up " +
                $"\n count L: {currentEpoch.leftIndex} - " +
                $"count R: {currentEpoch.rightIndex} - " +
                $"epoch count: {GlobalConfig.EPOCH_SAMPLE_COUNT} ");
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

        public float FetchLatestSampleLeft()
        {
            float value = left[leftIndex];
            return value;
        }

        public float FetchLatestSampleRight()
        {
            float value = right[rightIndex];
            return value;
        }

        public void AddSample(float value, GlobalConfig.CHANNEL channel)
        {
            try
            {
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
                        // HERE WE WANT TO FETCH THE MOOD
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
    public class EEGBuffer
    {
        public int size;
        public float[] left;
        public float[] right;

        public EEGBuffer(int _size)
        {
            size = _size;
            left = new float[size];
            right = new float[size];
        }

        public float FetchLatestSampleLeft()
        {
            return left[0];
        }

        public float FetchLatestSampleRight()
        {
            return right[0];
        }

        public void AddSample(float value, GlobalConfig.CHANNEL channel)
        {
            try
            {
                switch (channel)
                {
                    case GlobalConfig.CHANNEL.LEFT:
                        // add new sample to front of left array and shift everything else - destroying overflow
                        left = InsertFront(value, left);
                        break;
                    case GlobalConfig.CHANNEL.RIGHT:
                        // add new sample to front of the right array and shift everything else - destroying overflow
                        right = InsertFront(value, right);
                        break;
                    default:
                        Debug.LogError($"Channel type not recognised as a valid channel: {channel}");
                        break;
                }

            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error adding value: {value} to channel: {channel} \n Full Exception: {e}");
            }
        }

        float[] InsertFront(float value, float[] array)
        {
            float[] output = new float[array.Length];

            output[0] = value;
            
            for (int i = 1; i < output.Length; i++)
            {
                output[i] = array[i - 1];
            }

            return output;
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

}



