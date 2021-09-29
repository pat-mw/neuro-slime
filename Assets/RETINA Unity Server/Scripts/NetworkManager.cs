using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wenzil.Console;


namespace RetinaNetworking.Server
{
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Instance;

        public int MaxPlayers = 5;
        public int Port = 26950; 

        // public GameObject playerPrefab;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Console.Log("Instance of Network Manager already exists - destroying object");
                Destroy(this);
            }
        }

        public void StartServer()
        {
            #if UNITY_EDITOR
            Console.Log("Please Build the application to start the server");
            #else
            Server.Start(MaxPlayers, Port);
            #endif
        }

    }
}

