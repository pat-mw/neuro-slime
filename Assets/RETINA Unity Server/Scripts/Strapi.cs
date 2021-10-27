using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ScriptableObjectArchitecture;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using Wenzil.Console;
namespace RetinaNetworking.Server
{
    /// <summary>
    /// The Strapi class is for all behaviour relating to the backend connection
    /// GET method to retrieve strapi session token, and POST for depositing data to the backend
    /// </summary>
    public class Strapi : MonoBehaviour
    {
        [Header("Network Properties")]
        public string strapiURL = "https://kouo-strapi-staging.herokuapp.com";
        public string strapiEndpoint = "/datasets/getAuthToken/";
        public string strapiParams = "?expires_after=";
        public int sessionTimeLimit = 20;
        public string testJWT = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6MTA1LCJpYXQiOjE2MjM2ODY4ODUsImV4cCI6MTYyNjI3ODg4NX0.j1Q2r-7pOOoAzDcDjQu0oQnRiMoWBqmW5Xh7FmMlXY4";

        private StrapiTokenResponse lastResponse = new StrapiTokenResponse();

        /// <summary>
        /// for testing
        /// </summary>
        private void Awake()
        {
            Console.Log($"STARTING STRAPI");
        }

        public async UniTask<string> RequestSessionToken(string _JWT)
        {
            Console.Log(" -- attempting to request session token -- ");

            //StartCoroutine(GetRequest(strapiURL + strapiEndpoint + strapiParams + sessionTimeLimit.ToString(), _JWT));

            await GetSessionToken(strapiURL + strapiEndpoint + strapiParams + sessionTimeLimit.ToString(), _JWT);

            return lastResponse.token;
        }
        
        async UniTask GetSessionToken(string _URL, string _JWT)
        {
            lastResponse = new StrapiTokenResponse();
            Console.Log($"-- attempting GET request: {_URL} --");
            using (UnityWebRequest webRequest = UnityWebRequest.Get(_URL))
            {
                // authentication
                webRequest.SetRequestHeader("Authorization", "Bearer " + _JWT);

                // Request and wait for the desired page.
                await webRequest.SendWebRequest();

                string[] pages = _URL.Split('/');
                int page = pages.Length - 1;

                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                        Console.Log(pages[page] + ": Connection error: " + webRequest.error);

                        lastResponse = null;
                        break;

                    case UnityWebRequest.Result.DataProcessingError:
                        Console.Log(pages[page] + ": Error: " + webRequest.error);

                        lastResponse = null;
                        break;

                    case UnityWebRequest.Result.ProtocolError:
                        Console.Log(pages[page] + ": HTTP Error: " + webRequest.error);

                        lastResponse = null;
                        break;

                    case UnityWebRequest.Result.Success:
                        Console.Log(pages[page] + ":\n Received: " + webRequest.downloadHandler.text);
                        StrapiTokenResponse response = new StrapiTokenResponse();
                        JsonUtility.FromJsonOverwrite(webRequest.downloadHandler.text, response);

                        Console.Log("-- USER AUTHENTICATED --");
                        Console.Log($"token: {response.token}");
                        Console.Log($"expires at: {response.expires_at}");

                        lastResponse = response;
                        break;
                }
                return;
            }
        }


        [System.Serializable]
        public class StrapiTokenResponse
        {
            public string token;
            public double expires_at;
        }
    }
}

