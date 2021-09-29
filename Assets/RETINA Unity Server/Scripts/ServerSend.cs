using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RetinaNetworking.Server
{
    class ServerSend
    {

        #region SendTCP
        private static void SendTCPData(int _toClient, Packet _packet)
        {
            // get the length of the message
            _packet.WriteLength();

            Server.clients[_toClient].tcp.SendData(_packet);

        }

        private static void SendTCPDataToAll(Packet _packet)
        {
            _packet.WriteLength();

            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.clients[i].tcp.SendData(_packet);
            }
        }

        private static void SendTCPDataToAllExcept(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();

            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                if (i != _exceptClient)
                {
                    Server.clients[i].tcp.SendData(_packet);
                }
            }
        }
        #endregion


        #region SendUDP
        private static void SendUDPData(int _toClient, Packet _packet)
        {
            // get the length of the message
            _packet.WriteLength();

            Server.clients[_toClient].udp.SendData(_packet);

        }

        private static void SendUDPDataToAll(Packet _packet)
        {
            _packet.WriteLength();

            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.clients[i].udp.SendData(_packet);
            }
        }

        private static void SendUDPDataToAllExcept(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();

            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                if (i != _exceptClient)
                {
                    Server.clients[i].udp.SendData(_packet);
                }
            }
        }

        #endregion

        #region Packets

        public static void Welcome(int _toClient, string _msg)
        {
            using (Packet _packet = new Packet((int)ServerPackets.welcome))
            {
                _packet.Write(_msg);
                _packet.Write(_toClient);

                SendTCPData(_toClient, _packet);
            }
        }


        public static void UPDTest(int _toClient)
        {
            using (Packet _packet = new Packet((int)ServerPackets.udpTest))
            {
                _packet.Write("Test packet for UDP");

                SendUDPData(_toClient, _packet);
            }
        }

        public static void ExampleDataBytesReceived(int _toClient)
        {
            using (Packet _packet = new Packet((int)ServerPackets.exampleDataBytesReceived))
            {
                _packet.Write("Received example data bytes!");

                SendTCPData(_toClient, _packet);
            }
        }

        public static void ExampleDataStringReceived(int _toClient)
        {
            using (Packet _packet = new Packet((int)ServerPackets.exampleDataStringReceived))
            {
                _packet.Write("Received example data string!");

                SendTCPData(_toClient, _packet);
            }
        }

        public static void SpawnPlayer(int _toClient, Player _player)
        {
            using (Packet _packet = new Packet((int)ServerPackets.spawnPlayer))
            {
                _packet.Write(_player.ID);
                _packet.Write(_player.username);

                // other relevant info sent here
            }
        }

        #endregion

    }
}
