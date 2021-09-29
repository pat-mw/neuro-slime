using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wenzil.Console;

namespace RetinaNetworking.Server
{
    public class Player: MonoBehaviour
    {
        public int ID { get; private set; }
        public string username { get; private set; }
        public string JWT { get; private set; }
        public string sessionToken { get; private set; }

        [SerializeField] private Text IDLabel;
        [SerializeField] private Text UsernameLabel;
        [SerializeField] private Text ClientDataLabel;
        [SerializeField] private Dictionary<string, byte[]> dataLog = new Dictionary<string, byte[]>();

        // constructor
        public void SetPlayerDetails(int _id, string _username, string _JWT, string _sessionToken)
        {
            ID = _id;
            username = _username;
            JWT = _JWT;
            sessionToken = _sessionToken;
            UpdatePlayerLabels();
        }

        private void UpdatePlayerLabels()
        {
            IDLabel.text = ID.ToString();
            UsernameLabel.text = username;
        }

        public void AddPlayerData(byte[] message, string path)
        {
            ClientDataLabel.text = message.ToString();
            dataLog.Add(path, message);
            Console.Log($"Added Player data to the data log | ID: {ID} USERNAME: {username}");
        }

        public void DisplayDataLog()
        {
            Console.Log($"--- Displaying Data Log for User: {username} - ID: {ID} ---");
            Console.Log($"JWT: {JWT}");
            Console.Log($"sessionToken: {sessionToken}");

            if (dataLog.Count == 0)
            {
                Console.Log(" NO DATA FOUND FOR GIVEN CLIENT ");
                return;
            }

            foreach (KeyValuePair<string, byte[]> entry in dataLog)
            {
                Console.Log("path: " + entry.Key);
                Console.Log("data: " + entry.Value);
            }

            Console.Log("--- Finished Displaying Data Log ---");
        }
    }
}

