using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wenzil.Console;

namespace RetinaNetworking.Server
{
    public class ClientDatabase : MonoBehaviour
    {
        public static ClientDatabase Instance;

        public GameObject ClientDetailsPanelPrefab;

        private List<Player> connectedPlayers = new List<Player>();
        private List<Player> disconnectedPlayers = new List<Player>();

        /// <summary>
        /// Singleton declaration to make this accessible from anywhere
        /// </summary>
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Console.Log("ClientDetailsUI instance already exists - destroying object");
                Destroy(this);
            }
        }


        public List<Player> GetConnectedPlayers()
        {
            return connectedPlayers;
        }

        /// <summary>
        /// Creates a new panel for a freshly connected client
        /// </summary>
        public void AddClientPanel(Client client)
        {
            Console.Log($"Attempting to add client panel | ID: {client.ID} - Name: {client.username}");

            // check if given client has an associated panel.
            foreach (Player player in connectedPlayers)
            {
                // if the panel already exists -  return
                if (player.username == client.username && player.ID == client.ID)
                {
                    return;
                }
            }

            try
            {
                GameObject panel = Instantiate(ClientDetailsPanelPrefab, this.transform);

                Player player = panel.GetComponent<Player>();
                connectedPlayers.Add(player);

                // shift to the right depending on the position in the player panels list
                RectTransform panelTransform = panel.GetComponent<RectTransform>();

                int index = connectedPlayers.IndexOf(player);
                panelTransform.localPosition += Vector3.right * index * panelTransform.sizeDelta.x;

                // ID and username text fields
                player.SetPlayerDetails(client.ID, client.username, client.JWT, client.sessionToken);
                
                Console.Log($"Successfully added panel for the following client: {client.ID} - {client.username}");
            }
            catch (System.Exception ex)
            {
                Console.Log($"Error adding client panel to the dashboard: {ex}");
            }
        }

        /// <summary>
        /// Removes Client panel upon client disconnection
        /// TODO: add the removed panel to a list of disconnected clients
        /// </summary>
        public void RemoveClientPanel(Client client)
        {
            Console.Log($"Attempting to remove client panel | ID: {client.ID} - Name: {client.username}");

            try
            {
                // check if given client has an associated panel.
                foreach(Player player in connectedPlayers)
                {
                    if (player.username == client.username && player.ID == client.ID)
                    {
                        Console.Log($"Found player panel for the given client - Destroying Panel");
                        Destroy(player.gameObject);
                        connectedPlayers.Remove(player);
                        disconnectedPlayers.Add(player);

                        RearrangePanels();
                    }
                }
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Rearrange panels so there is no blank space between them
        /// TODO: Order them in increasing order according to the Client ID
        /// </summary>
        private void RearrangePanels()
        {
            foreach(Player player in connectedPlayers)
            {
                // shift to the right depending on the position in the player panels list
                RectTransform panelTransform = player.GetComponent<RectTransform>();

                int index = connectedPlayers.IndexOf(player);

                // reset x position
                panelTransform.localPosition = new Vector3(-960, panelTransform.localPosition.y, panelTransform.localPosition.z);
                // push in the x direction
                panelTransform.localPosition += Vector3.right * (index * panelTransform.sizeDelta.x);

                Console.Log("*** rectPos: " + panelTransform.position);
                Console.Log("*** rectLocalPos: " + panelTransform.localPosition);
            }
        }

        /// <summary>
        /// Logs incoming client data as a byte array
        /// </summary>
        public void LogClientData(Client client, byte[] message, string path)
        {
            Console.Log($"Attempting to log incoming client data | ID: {client.ID} - Name: {client.username} - msg: {message}");

            try
            {
                // check if given client has an associated panel.
                foreach (Player player in connectedPlayers)
                {
                    // if the panel is available, add the data
                    if (player.username == client.username && player.ID == client.ID)
                    {
                        player.AddPlayerData(message, path);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Console.Log($"Error logging client data to the panel: {ex}");
            }
        }
    }
}
