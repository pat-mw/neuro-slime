using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ScriptableObjectArchitecture;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

namespace RetinaNetworking.Server
{
    /// <summary>
    /// The Amygdyla class is for all behaviour relating to the backend connection
    /// GET method to retrieve amygdyla session token, and POST for depositing data to the backend
    /// </summary>
    public class GetAuthToken : MonoBehaviour
    {
        [Header("Network Properties")]
        public string strapiURL = "https://kouo-strapi-staging.herokuapp.com";
        public string strapiEndpoint = "/datasets/getAuthToken/";
        public string strapiParams = "?expires_after=";
        public int sessionTimeLimit = 20;
        private StrapiTokenResponse lastResponse = new StrapiTokenResponse();

        public async UniTask<string> RequestSessionToken(string _JWT)
        {
            //Debug.Log(" -- attempting to request session token -- ");

            await GetSessionToken(strapiURL + strapiEndpoint + strapiParams + sessionTimeLimit.ToString(), _JWT);

            return lastResponse.token;
        }
        
        async UniTask GetSessionToken(string _URL, string _JWT)
        {
            lastResponse = new StrapiTokenResponse();
            //Debug.Log($"-- attempting GET request: {_URL} --");
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
                        Debug.LogWarning(pages[page] + ": Connection error: " + webRequest.error);

                        lastResponse = null;
                        break;

                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogWarning(pages[page] + ": Error: " + webRequest.error);

                        lastResponse = null;
                        break;

                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogWarning(pages[page] + ": HTTP Error: " + webRequest.error);

                        lastResponse = null;
                        break;

                    case UnityWebRequest.Result.Success:
                        //Debug.Log(pages[page] + ":\n Received: " + webRequest.downloadHandler.text);
                        StrapiTokenResponse response = new StrapiTokenResponse();
                        JsonUtility.FromJsonOverwrite(webRequest.downloadHandler.text, response);

                        Debug.Log(">- USER AUTHENTICATED -<");
                        Debug.Log($"token: {response.token}");
                        Debug.Log($"expires at: {response.expires_at}");

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

