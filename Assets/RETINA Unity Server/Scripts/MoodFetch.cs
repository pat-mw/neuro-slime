//  api / inference / start

//      - pass a chunk(calibration)
//      -returns inference ID
//      -pass inference ID to subsequent chunk calls(for same baseline)

//  api / inference / chunk

//  api / inference / end//stop
//      - pass reported mood matrix



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
    public class MoodFetch : MonoBehaviour
    {
        [Header("Network Properties")]
        public string AmyURL = "https://kouo-amygdala-staging.herokuapp.com";
        public string AmyEndpointStart = "/v2/recording/start/";
        public string AmyEndpointChunk = "/v2/recording/chunk/";
        public string AmyEndpointEnd = "/v2/recording/end/";
        public string AmyEndpointInference = "/v2/infer/";

        [Header("EVENTS")]
        [InlineEditor(InlineEditorModes.FullEditor)] [OdinSerialize] public GameEvent OnStartInferenceSession = default(GameEvent);
        [InlineEditor(InlineEditorModes.FullEditor)] [OdinSerialize] public GameEvent OnFetchMood = default(GameEvent);
        [InlineEditor(InlineEditorModes.FullEditor)] [OdinSerialize] public StringGameEvent OnFetchAuthToken = default(StringGameEvent);
        [InlineEditor(InlineEditorModes.FullEditor)] [OdinSerialize] public GameEvent OnEndInferenceSession = default(GameEvent);

        [Header("DATA")]
        [InlineEditor(InlineEditorModes.FullEditor)] [OdinSerialize] public ConnectionParams connectionParams;
        [InlineEditor(InlineEditorModes.FullEditor)] [OdinSerialize] public BrainData brainData;

        
        private void Start()
        {
            OnStartInferenceSession.AddListener(StartSession);
            OnFetchMood.AddListener(FetchMood);
            OnEndInferenceSession.AddListener(EndSession);
        }


        /// <summary>
        /// Begins the inference session by:
        /// 1. asserting presence of a session token
        /// 2. building a form
        /// 3. POST request to the start endpoint on amygdala
        /// </summary>
        async void StartSession()
        {
            Debug.Log($"STARTING INFERENCE SESSION FOR USER: {connectionParams.UserID()}");
            if (connectionParams.SessionToken() == "")
            {
                try
                {
                    Debug.Log($"FETCHING SESSION TOKEN!");
                    OnFetchAuthToken.Raise(connectionParams.JWT());
                    await UniTask.WaitUntil(() => connectionParams.SessionToken() != "");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error occured when fetching auth token: {ex}");
                }
            }

            // data
            Dictionary<string, dynamic> data = new Dictionary<string, dynamic>();
            // delete deviceID + mediaID
            // two field: userID + timeseries {ch1: [], ch2: []}
            data.Add("deviceId", "2");
            data.Add("userId", connectionParams.UserID());
            data.Add("mediaId", 1);

            // return inference ID

            try
            {
                string responseText = await POST(AmyURL + AmyEndpointStart, data);
                if (responseText == null)
                {
                    Debug.LogError("Response was empty, cannot decode");
                }
                else
                {
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


        /// <summary>
        /// Ends the inference session:
        /// Passes in the sessionID, and reported mood data
        /// </summary>
        async void EndSession()
        {
            if (connectionParams.SessionToken() == "")
                return;

            // data
            Dictionary<string, dynamic> data = new Dictionary<string, dynamic>();
            data.Add("datasetId", connectionParams.SessionID());
            data.Add("perceivedEmotion", connectionParams.MoodReport().perceivedMood);
            data.Add("happy", connectionParams.MoodReport().happy);
            data.Add("control", connectionParams.MoodReport().control);
            data.Add("excited", connectionParams.MoodReport().excited);

            try
            {
                string response = await POST(AmyURL + AmyEndpointEnd, data);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"AMYGDALA ERROR: {ex}");
                throw;
            }

        }


        async UniTask<string> POST(string _URL, Dictionary<string, dynamic> data)
        {
            Debug.Log($"-- attempting POST request: {_URL} --");

            string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(data);

            Debug.Log($"JSON Data: {jsonData}");

            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);

            using (UnityWebRequest www = new UnityWebRequest(_URL, "POST"))
            {
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
                        Debug.LogError(pages[page] + ": Connection error: " + www.error);
                        break;

                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError(pages[page] + ": Error: " + www.error);
                        break;

                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError(pages[page] + ": HTTP Error: " + www.error);
                        break;

                    default:
                        Debug.LogError($"Response not recognised: {www.error}");
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
    }
}

