/* 
 
Using OSC Protocol to receive brain data from OpenBCI Cyton

References:
- OSC: https://thomasfredericks.github.io/UnityOSC/
- OpenBCI OSC Ref: https://docs.google.com/document/d/e/2PACX-1vR_4DXPTh1nuiOwWKwIZN3NkGP3kRwpP4Hu6fQmy3jRAOaydOuEI1jket6V4V6PG4yIG15H1N7oFfdV/pub
- OpenBCI Networking: https://docs.openbci.com/Software/OpenBCISoftware/GUIWidgets/#osc
 
 */

using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections;
using Cysharp.Threading.Tasks;
using ScriptableObjectArchitecture;

public class ReceiveBrainData : SerializedMonoBehaviour
{
    [Header("NETWORK")]
    public OSC osc;

    [Header("EVENTS")]
    public GameEvent onStartReceivingBrainData;
    public GameEvent onStopReceivingBrainData;
    public GameEvent onStartCalib;
    public GameEvent onEndCalib;
    public GameEvent onStartStim;
    public GameEvent onEndStim;

    [Header("KEYS")]
    [SerializeField] private string bandpowerKey = "/band";
    [SerializeField] private string signalKey = "/signal";
    [SerializeField] private string accelerometerKey = "/accelerometer";

    [Header("MODE")]
    [SerializeField] private HandlerMode handler = HandlerMode.Custom;

    [Header("DATA STORE")]
    [OdinSerialize] private BrainData brainData;

    private bool isCalibPeriod = false;
    private bool isStimPeriod = false;
    private bool isReceiving = false;

    void Start()
    {
        Wenzil.Console.Console.Log($"STARTING BRAIN DATA RECEIVER");

        brainData.Reset();

        onStartReceivingBrainData.AddListener(StartReceiving);
        onStopReceivingBrainData.AddListener(StopReceiving);

        onStartCalib.AddListener(StartCalib);
        onEndCalib.AddListener(EndCalib);

        onStartStim.AddListener(StartStimuli);
        onEndStim.AddListener(EndStimuli);

        switch (handler)
        {
            case HandlerMode.Custom:
                osc.SetAllMessageHandler(OnReceiveAnything);
                break;
            case HandlerMode.Default:
                osc.SetAddressHandler(bandpowerKey, OnReceiveBandpower);
                osc.SetAddressHandler(signalKey, OnReceiveSignal);
                osc.SetAddressHandler(accelerometerKey, OnReceiveAccelerometer);
                break;
            default:
                Wenzil.Console.Console.LogError($"Handler mode not recognised: {handler}");
                break;
        }
    }

    void StartStimuli()
    {
        isStimPeriod = true;
    }

    void EndStimuli()
    {
        isStimPeriod = false;
    }


    void StartCalib()
    {
        isCalibPeriod = true;
    }


    void EndCalib()
    {
        isCalibPeriod = false;
    }

    void StartReceiving()
    {
        isReceiving = true;
    }

    void StopReceiving()
    {
        isReceiving = false;
    }

    /// <summary>
    /// Default Callback handler
    /// Is called when any data is received
    /// </summary>
    void OnReceiveAnything(OscMessage message)
    {
        // Wenzil.Console.Console.Log($"Receiving message: {message}");
        
        if (isReceiving)
        {
            string address = message.address;

            address = CleanAddress(address);

            switch (address)
            {
                case "/band":
                    OnReceiveBandpower(message);
                    break;
                case "/focus":
                    OnReceiveSignal(message);
                    break;
                case "/accelerometer":
                    OnReceiveAccelerometer(message);
                    break;
                default:
                    OnReceiveUnknown(message);
                    break;
            }
        }
    }


    /// <summary>
    /// Removes the character (CTRL+S) (Unicode 19) from a given string
    /// Response to the strange behaviour from OpenBCI
    /// </summary>
    string CleanAddress(string address)
    {
        string cleanAddress = "";

        foreach(char ch in address)
        {
            int code = ch;
            if (code != 19)
            {
                cleanAddress += ch;
            }
        }
        return cleanAddress;
    }


    /// <summary>
    /// Channel number followed by floats for each power band: 
    /// Delta (0.5-4Hz), Theta(4-8Hz), Alpha(8-13Hz), Beta(13-32Hz), Gamma(32-100Hz) 
    /// sent sequentially
    /// </summary>
    void OnReceiveBandpower(OscMessage message)
    {
        // Wenzil.Console.Console.Log($"Receiving Bandpower: {message}");
        if (isReceiving)
        {
            try
            {
                int channelNumber = message.GetInt(0);

                int leftChannel = brainData.leftBands.channelNo;
                int rightChannel = brainData.rightBands.channelNo;

                if (channelNumber == leftChannel)
                {
                    float delta = message.GetFloat(1);
                    float theta = message.GetFloat(2);
                    float alpha = message.GetFloat(3);
                    float beta = message.GetFloat(4);
                    float gamma = message.GetFloat(5);

                    // Wenzil.Console.Console.Log($"Received bandpower data: \n" +
                    //    $"channel: {channelNumber} \n" +
                    //    $"delta: {delta} \n" +
                    //    $"theta: {theta} \n" +
                    //    $"alpha: {alpha} \n" +
                    //    $"beta: {beta} \n" +
                    //    $"gamma: {gamma} \n");

                    brainData.leftBands.delta = delta;
                    brainData.leftBands.theta = theta;
                    brainData.leftBands.alpha = alpha;
                    brainData.leftBands.beta = beta;
                    brainData.leftBands.gamma = gamma;
                }

                if (channelNumber == rightChannel)
                {
                    float delta = message.GetFloat(1);
                    float theta = message.GetFloat(2);
                    float alpha = message.GetFloat(3);
                    float beta = message.GetFloat(4);
                    float gamma = message.GetFloat(5);

                    // Wenzil.Console.Console.Log($"Received bandpower data: \n" +
                    //    $"channel: {channelNumber} \n" +
                    //    $"delta: {delta} \n" +
                    //    $"theta: {theta} \n" +
                    //    $"alpha: {alpha} \n" +
                    //    $"beta: {beta} \n" +
                    //    $"gamma: {gamma} \n");

                    brainData.rightBands.delta = delta;
                    brainData.rightBands.theta = theta;
                    brainData.rightBands.alpha = alpha;
                    brainData.rightBands.beta = beta;
                    brainData.rightBands.gamma = gamma;
                }
            }
            catch (System.Exception e)
            {
                Wenzil.Console.Console.LogError($"Error Reading Bandpower: {e}");
            }
        }
       
    }


    /// <summary>
    /// Data as floats for each channel, sent all at once
    /// 0. 1. 2. 3. ...
    /// </summary>
    void OnReceiveSignal(OscMessage message)
    {
        // Wenzil.Console.Console.Log($"Receiving signal: {message}");

        if (isReceiving)
        {

            float left = message.GetFloat(0);
            float right = message.GetFloat(7);


            brainData.dataBuffer.AddSample(left, GlobalConfig.CHANNEL.LEFT);
            brainData.dataBuffer.AddSample(right, GlobalConfig.CHANNEL.RIGHT);

            if (isCalibPeriod)
            {
                try
                {

                    // Wenzil.Console.Console.Log($"Received Signal - left: {left} - right: {right}");

                    brainData.calibrationPeriod.AddSample(left, GlobalConfig.CHANNEL.LEFT);
                    brainData.calibrationPeriod.AddSample(right, GlobalConfig.CHANNEL.RIGHT);

                    if (brainData.calibrationPeriod.epochComplete)
                    {
                        Wenzil.Console.Console.Log("Calibration Complete! Backing up now");
                        isCalibPeriod = false;
                        
                    }
                }
                catch (System.Exception e)
                {
                    Wenzil.Console.Console.LogError($"Error receiving signal: {e}");
                }
            }
            else if (isStimPeriod)// not calibration period
            {
                try
                {
                    // Wenzil.Console.Console.Log($"Received Signal - left: {left} - right: {right}");

                    brainData.currentEpoch.AddSample(left, GlobalConfig.CHANNEL.LEFT);
                    brainData.currentEpoch.AddSample(right, GlobalConfig.CHANNEL.RIGHT);

                    if (brainData.currentEpoch.epochComplete)
                    {
                        Wenzil.Console.Console.Log("Epoch completed!! Backing up now");

                        brainData.BackupEpoch();
                    }
                }
                catch (System.Exception e)
                {
                    Wenzil.Console.Console.LogError($"Error receiving signal: {e}");
                }
            }
        }   
    }


    /// <summary>
    /// Three floats, one for each axis:
    /// X, Y and Z
    /// Can be a positive or negative value
    /// </summary>
    void OnReceiveAccelerometer(OscMessage message)
    {
        // Wenzil.Console.Console.Log($"Receiving Accelerometer: {message}");


        if (isReceiving)
        {
            try
            {
                int index = message.GetInt(0);
                float value = message.GetFloat(1);

                switch (index)
                {
                    case 1:
                        brainData.accelerometer.X = value;
                        break;
                    case 2:
                        brainData.accelerometer.Y = value;
                        break;
                    case 3:
                        brainData.accelerometer.Z = value;
                        break;
                    default:
                         Wenzil.Console.Console.LogError($"Did not recognise index: {index}");
                        break;
                }
            }
            catch (System.Exception)
            {
                 Wenzil.Console.Console.LogError("Error fetching accelerometer data");
            }
        }
        
    }


    /// <summary>
    /// default switch case
    /// Called when the message address is unknown
    /// </summary>
    void OnReceiveUnknown(OscMessage message)
    {
        if (isReceiving)
        {
             Wenzil.Console.Console.LogError($"Message received from unrecognised address: {message.address} \n message: {message}");
        }
    }

}
