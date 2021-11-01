using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using ScriptableObjectArchitecture;
using Cysharp.Threading.Tasks;

namespace RetinaNetworking.Server
{
    [RequireComponent(typeof(Strapi))]
    public class UserAuth : MonoBehaviour
    {
        [Header("DATA")]
        public ConnectionParams connectionParams;

        [Header("Network Properties")]
        public string defaultURL = "https://kouo-strapi-staging.herokuapp.com";
        public string defaultEndpoint = "/auth/local/register";

        [Header("Events")]
        public StringGameEvent DebuggerEvent = default(StringGameEvent);
        public GameEvent closeFormEvent = default(GameEvent);
        public GameEvent resetFormEvent = default(GameEvent);
        public GameEvent showInstructions = default(GameEvent);
        public GameEvent dataError = default(GameEvent);
        
        //public GameEvent activateSimEvent = default(GameEvent);

        private Strapi auth;

        private void Awake()
        {
            auth = GetComponent<Strapi>();
        }

        async public void RequestAuth(string username, string email, string gender, int age, string language, string password)
        {
             Wenzil.Console.Console.Log(" -- attempting to request authentication -- ");

            Dictionary<string, dynamic> data = new Dictionary<string, dynamic>();

            // data
            data.Add("username", username);
            data.Add("email", email);
            data.Add("gender", gender);
            data.Add("age", age);
            data.Add("language", language);
            data.Add("password", password);

            try
            {
                await POST(defaultURL, defaultEndpoint, data);
            }
            catch (System.Exception)
            {
                Wenzil.Console.Console.LogError("Authentication Error");
                dataError.Raise();
            }
        }

        async UniTask POST(string _URL, string _endPoint, Dictionary<string, dynamic> _data)
        {
            WWWForm form = new WWWForm();

            // Wenzil.Console.Console.Log(" -- header -- ");
            //foreach(KeyValuePair<string,string> item in form.headers)
            //{
            //     Wenzil.Console.Console.Log(item.Key + ": " + item.Value);
            //}

            // Wenzil.Console.Console.Log(" -- adding fields -- ");
            foreach(KeyValuePair<string, dynamic> item in _data)
            {
                // Wenzil.Console.Console.Log($"{item.Key} : {item.Value}");
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
                    Wenzil.Console.Console.LogError($"Exception: {e}");
                }

                if (www.result == UnityWebRequest.Result.Success)
                {
                    // Wenzil.Console.Console.Log("Form upload complete!");
                     Debug.Log(www.result);

                    // error
                    GoodResponse response = new GoodResponse();
                    JsonUtility.FromJsonOverwrite(www.downloadHandler.text, response);

                    Wenzil.Console.Console.Log("-- USER AUTHENTICATED --");
                    Wenzil.Console.Console.Log($"JWT: {response.jwt}");
                    Wenzil.Console.Console.Log($"User ID: {response.user.id}");
                    Wenzil.Console.Console.Log($"Username: {response.user.username}");

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

                    Wenzil.Console.Console.Log(response.ToString());

                    Wenzil.Console.Console.Log("-- POST REQUEST ERROR --");
                    Wenzil.Console.Console.Log(" Error Code: " + response.statusCode);
                    Wenzil.Console.Console.Log(" Error: " + response.error);

                    dataError.Raise();

                    foreach(BadResponse.embeddedMessage message in response.message)
                    {
                        foreach(BadResponse.mess mess in message.messages)
                        {
                            // Wenzil.Console.Console.Log($"message ID: {mess.id}");
                            Wenzil.Console.Console.Log($" message: {mess.message}");

                            //  Wenzil.Console.Console text
                            DebuggerEvent.Raise($"User authentication failed! \n message: {mess.message}");
                        }
                    }
                }
            }
        }

        async public void RequestSessionToken(string JWT)
        {
            Wenzil.Console.Console.Log($"REQUESTING SESSION TOKEN with JWT: {JWT}");
            await FetchSessionToken(JWT);
        }

        async public UniTask FetchSessionToken(string JWT)
        {
             Wenzil.Console.Console.Log($"Fetching Session Token");
            var sessionToken = await auth.RequestSessionToken(JWT);
            sessionToken = sessionToken.ToString();

             Wenzil.Console.Console.Log($"Session token fetched: {sessionToken}");
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