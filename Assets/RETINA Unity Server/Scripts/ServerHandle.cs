using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Wenzil.Console;


namespace RetinaNetworking.Server
{
    class ServerHandle
    {
        public static void WelcomeReceived(int _fromClient, Packet _packet)
        {
            // important to read data in the same order as it was sent
            // in this case - the welcome received message sent from the client was an int, then a string
            int _instanceID = _packet.ReadInt();
            int _clientID = _packet.ReadInt();

            Wenzil.Console.Console.Log($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected succesfully | Instance ID: {_fromClient} - username: {_clientID}");

            // double check that client claimed the right id
            if (_fromClient != _instanceID)
            {
                Wenzil.Console.Console.Log($"Client ID: \"{_clientID}\" (session id: {_fromClient}) has assumed the wrong session id ({_instanceID})");
            }

            // save the client ID
            Server.clients[_fromClient].ID = _clientID;

            Wenzil.Console.Console.Log($"Saved Client ID: {Server.clients[_fromClient].ID}");

            // TODO: need to check whether this is a valid client ID before adding the panel
            List<Player> connectedPlayers = ClientDatabase.Instance.GetConnectedPlayers();
            foreach (Player player in connectedPlayers)
            {

            }
            ClientDatabase.Instance.AddClientPanel(Server.clients[_fromClient]);
        }


        public static void UDPTestReceived(int _fromClient, Packet _packet)
        {
            string _msg = _packet.ReadString();

            Wenzil.Console.Console.Log($"Received UDP Packet | Client ID: {_fromClient} - Message: {_msg}");
        }


        public static void ExampleDataBytes(int _fromClient, Packet _packet)
        {
            byte[] msg = _packet.ReadBytes(_packet.UnreadLength());

            Wenzil.Console.Console.Log($"Client {_fromClient} (username: {Server.clients[_fromClient].username}) has sent some example data (bytes)");
            NestedData deserialized = JSONHandler.DecodeNestedByteArray(msg);

            // save the incoming data
            string path;
            DataSave.SaveByteData(msg, _fromClient, out path);

            // log the incoming data in the Client Panels
            //ClientDetailsUI.Instance.LogClientData(Server.clients[_fromClient], msg.ToString());
            ClientDatabase.Instance.LogClientData(Server.clients[_fromClient], msg, path);

            // send reception message back to client
            ServerSend.ExampleDataBytesReceived(_fromClient);
        }
    }
}
