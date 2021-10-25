using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using ScriptableObjectArchitecture;
using Cysharp.Threading.Tasks;

namespace RetinaNetworking.Server
{
    [RequireComponent(typeof(GetAuthToken))]
    public class UserAuth : MonoBehaviour
    {
        [Header("DATA")]
        public ConnectionParams connectionParams;

        [Header("Network Properties")]
        public string defaultURL = "https://kouo-strapi-staging.herokuapp.com";
        public string defaultEndpoint = "/auth/local/register";

        [Header("Events")]
        public StringGameEvent formDebuggerEvent = default(StringGameEvent);
        public GameEvent closeFormEvent = default(GameEvent);
        public GameEvent resetFormEvent = default(GameEvent);
        public GameEvent showInstructions = default(GameEvent);
        
        //public GameEvent activateSimEvent = default(GameEvent);

        private GetAuthToken auth;

        private void Awake()
        {
            auth = GetComponent<GetAuthToken>();
        }

        async public void RequestAuth(string username, string email, string gender, int age, string language, string password)
        {
            Debug.Log(" -- attempting to request authentication -- ");

            Dictionary<string, dynamic> data = new Dictionary<string, dynamic>();

            // data
            data.Add("username", username);
            data.Add("email", email);
            data.Add("gender", gender);
            data.Add("age", age);
            data.Add("language", language);
            data.Add("password", password);

            await POST(defaultURL, defaultEndpoint, data);
        }

        async UniTask POST(string _URL, string _endPoint, Dictionary<string, dynamic> _data)
        {
            WWWForm form = new WWWForm();

            Debug.Log(" -- header -- ");
            foreach(KeyValuePair<string,string> item in form.headers)
            {
                Debug.Log(item.Key + ": " + item.Value);
            }

            Debug.Log(" -- adding fields -- ");
            foreach(KeyValuePair<string, dynamic> item in _data)
            {
                Debug.Log($"{item.Key} : {item.Value}");
                form.AddField(item.Key, item.Value);
            }

            using (UnityWebRequest www = UnityWebRequest.Post(_URL + _endPoint, form))
            {
                try
                {
                    await www.SendWebRequest();
                }
                catch (UnityWebRequestException e)
                {
                    Debug.LogWarning($"Exception: {e}");
                }

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("Form upload complete!");
                    Debug.Log(www.result);

                    // error
                    GoodResponse response = new GoodResponse();
                    JsonUtility.FromJsonOverwrite(www.downloadHandler.text, response);

                    Debug.Log("-- USER AUTHENTICATED --");
                    Debug.Log($"JWT: {response.jwt}");
                    Debug.Log($"User ID: {response.user.id}");
                    Debug.Log($"Username: {response.user.username}");

                    connectionParams.SetParams(response.user.id, response.user.username, response.jwt);

                    // get amy session token
                    await FetchSessionToken(response.jwt);

                    // reset form
                    resetFormEvent.Raise();

                    // close panel
                    closeFormEvent.Raise();

                    // show instructions panel
                    showInstructions.Raise();
                }
                else
                {
                    // error
                    BadResponse response = new BadResponse();
                    JsonUtility.FromJsonOverwrite(www.downloadHandler.text, response);

                    Debug.Log(response);

                    Debug.Log("-- POST REQUEST ERROR --");
                    Debug.Log(" Error Code: " + response.statusCode);
                    Debug.Log(" Error: " + response.error);

                    foreach(BadResponse.embeddedMessage message in response.message)
                    {
                        foreach(BadResponse.mess mess in message.messages)
                        {
                            //Debug.Log($"message ID: {mess.id}");
                            Debug.Log($" message: {mess.message}");

                            // debug text
                            formDebuggerEvent.Raise($"User authentication failed! \n message: {mess.message}");
                        }
                    }
                }
            }
        }

        async public void RequestSessionToken(string JWT)
        {
            Debug.Log($"REQUESTING SESSION TOKEN with JWT: {JWT}");
            await FetchSessionToken(JWT);
        }

        async public UniTask FetchSessionToken(string JWT)
        {
            Debug.Log($"Fetching Session Token");
            var sessionToken = await auth.RequestSessionToken(JWT);
            sessionToken = sessionToken.ToString();

            Debug.Log($"Session token fetched: {sessionToken}");
            connectionParams.SetSessionToken(sessionToken);
        }

        private void OnDisable()
        {
            connectionParams.Reset();
        }

    }



    [System.Serializable]
    public class BadResponse
    {
        public int statusCode;
        public string error;
        public List<embeddedMessage> message;

        [System.Serializable]
        public class embeddedMessage
        {
            public List<mess> messages;
        }
        [System.Serializable]
        public class mess
        {
            public string id;
            public string message;
        }
    }

    [System.Serializable]
    public class GoodResponse
    {
        public string jwt;
        public User user;

        [System.Serializable]
        public class User
        {
            public int id;
            public string username;
        }
    }

}