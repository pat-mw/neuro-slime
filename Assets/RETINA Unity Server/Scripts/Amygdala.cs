using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ScriptableObjectArchitecture;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace RetinaNetworking.Server
{
    /// <summary>
    /// For POST requests relating to mood
    /// </summary>
    public class Amygdala : MonoBehaviour
    {
        [Header("Network Properties")]
        public string AmyURL = "https://kouo-amygdala-staging.herokuapp.com";
        public string AmyEndpointStart = "/v2/inference/timeseries/start/";
        public string AmyEndpointChunk = "/v2/inference/timeseries/chunk/";

        [Header("EVENTS")]
        public GameEvent OnFetchMood;
        public GameEvent OnSendCalibration;
        public StringGameEvent OnFetchAuthToken;

        [Header("DATA")]
        [InlineEditor(InlineEditorModes.FullEditor)] [OdinSerialize] public ConnectionParams connectionParams;
        [InlineEditor(InlineEditorModes.FullEditor)] [OdinSerialize] public BrainData brainData;

        
        private void Start()
        {
            connectionParams.Reset();
            OnFetchMood.AddListener(FetchMood);
            OnSendCalibration.AddListener(SendCalibrationData);
        }

        /// <summary>
        /// Begins the inference session by:
        /// 1. asserting presence of a session token
        /// 2. building a form
        /// 3. POST request to the start endpoint on amygdala
        /// </summary>

        async void SendCalibrationData()
        {
            Debug.Log("SENDING CALIBRATION");
            Dictionary<string, dynamic> data = new Dictionary<string, dynamic>();
            data.Add("userId", connectionParams.UserID());
            var timeseries = new Dictionary<string, float[]>();
            timeseries.Add("ch1", brainData.calibrationPeriod.left);
            timeseries.Add("ch2", brainData.calibrationPeriod.right);


            Debug.Log($"calibration timeseries: {timeseries}");
            data.Add("timeseries", timeseries);

            try
            {
                string responseText = await POST(AmyURL + AmyEndpointStart, data);
                if (responseText == null)
                {
                    Debug.LogError("START SESSION RESPONSE: Response was empty, cannot decode");
                }
                else
                {
                    Debug.Log($"START SESSION RESPONSE: {responseText}");
                    SessionStartResponse response = new SessionStartResponse();
                    JsonUtility.FromJsonOverwrite(responseText, response);
                    Debug.Log($"SESSION ID: {response.datasetId}");
                    connectionParams.SetSessionID(response.datasetId);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"AMYGDALA ERROR: {ex}");
                throw;
            }
        }



        

        

        /// <summary>
        /// Called automatically when the last epoch of EEG data is completed.
        /// This will POST the eeg data to the "CHUNK" endpoint and receive the inferenced mood.
        /// </summary>
        async void FetchMood()
        {
            Debug.Log($"FETCHING MOOD!");

            // data
            Dictionary<string, dynamic> data = new Dictionary<string, dynamic>();
            data.Add("datasetId", connectionParams.SessionID());

            string[] events = new string[0];
            data.Add("events", events);


            // TODO: rename "headset" -> "timeseries"

            // two fields:
            // inferenceID from start
            // time series
            Dictionary<string, float[]> headset = new Dictionary<string, float[]>();
            headset.Add("channel0", brainData.previousEpoch.left);
            headset.Add("channel1", brainData.previousEpoch.right);
            data.Add("headset", headset);

            try
            {
                string responseText = await POST(AmyURL + AmyEndpointChunk, data);
                if (responseText == null)
                {
                    Debug.LogError("Response was empty, cannot decode");
                }
                else
                {
                    MoodResponse response = new MoodResponse();
                    JsonUtility.FromJsonOverwrite(responseText, response);
                    Debug.Log($"RESPONSE MESSAGE: {response.message}");

                    //connectionParams.SetMood(response.message);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Error occured when starting amygdala session: {ex.Message}");
                throw;
            }
        }

        [System.Obsolete]
        async UniTask<string> POST(string _URL, Dictionary<string, dynamic> data)
        {
            Debug.Log($"-- attempting POST request: {_URL} --");

            string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(data);

            Debug.Log($"JSON Data: {jsonData}");

            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);

            using (UnityWebRequest www = new UnityWebRequest(_URL, "POST"))
            {
                www.chunkedTransfer = false;
                www.useHttpContinue = false;

                www.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

                // authentication
                www.SetRequestHeader("Authorization", "Bearer " + connectionParams.SessionToken());

                // content type json
                www.SetRequestHeader("Content-Type", "application/json");

                await www.SendWebRequest();

                string[] pages = _URL.Split('/');
                int page = pages.Length - 1;

                Debug.Log($"Checking POST result: {www.result}");
                switch (www.result)
                {
                    case UnityWebRequest.Result.Success:
                        Debug.Log(pages[page] + ": Received: " + www.downloadHandler.text);
                        return www.downloadHandler.text;

                    case UnityWebRequest.Result.ConnectionError:
                        Debug.LogWarning(pages[page] + ": Connection error: " + www.error);
                        break;

                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogWarning(pages[page] + ": Error: " + www.error);
                        break;

                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogWarning(pages[page] + ": HTTP Error: " + www.error);
                        break;

                    default:
                        Debug.LogWarning($"Response not recognised: {www.error}");
                        break;
                }
                return null;
            }
        }


        [System.Serializable]
        public class MoodResponse
        {
            public string message;
        }

        [System.Serializable]
        public class SessionStartResponse
        {
            public int datasetId;
        }


        [GUIColor(1, 0, 1)]
        [Button(ButtonSizes.Large)]
        async void InferenceEndpointTest()
        {
            Debug.Log("FORMING DATA PACKET");
            Dictionary<string, dynamic> data = new Dictionary<string, dynamic>();
            var timeseries = new Dictionary<string, float[]>();

            data.Add("userId", 2);
            float[] testData = new float[4];
            testData[0] = 1;
            testData[1] = 2;
            testData[2] = 3;
            testData[3] = 4;
            timeseries.Add("ch1", testData);
            timeseries.Add("ch2", testData);
            data.Add("timeseries", timeseries);


            Debug.Log("START INFERENCE POST");
            try
            {
                string responseText = await POST(AmyURL + AmyEndpointStart, data);
                if (responseText == null)
                {
                    Debug.LogError("START SESSION RESPONSE: Response was empty, cannot decode");
                }
                else
                {
                    Debug.Log($"START SESSION RESPONSE: {responseText}");
                    SessionStartResponse response = new SessionStartResponse();
                    JsonUtility.FromJsonOverwrite(responseText, response);
                    Debug.Log($"SESSION ID: {response.datasetId}");
                    connectionParams.SetSessionID(response.datasetId);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"AMYGDALA ERROR: {ex}");
                throw;
            }
        }


    }
}

