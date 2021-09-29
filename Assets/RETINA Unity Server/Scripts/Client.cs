using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using Wenzil.Console;

namespace RetinaNetworking.Server
{
    public class Client
    {
        // Size of the Data Buffer in Bytes (default 4MB)
        public static int dataBufferSize = 4096;

        public int ID;
        public string username;
        public string JWT;
        public string sessionToken;
        public TCP tcp;
        public UDP udp;

        // client constructor
        public Client(int _id, string _username, string _JWT, string _sessionToken)
        {
            ID = _id;
            username = _username;
            JWT = _JWT;
            sessionToken = _sessionToken;
            tcp = new TCP(ID);
            udp = new UDP(ID);
        }


        public class TCP
        {
            public TcpClient socket;

            private readonly int ID;
            private NetworkStream stream;
            private Packet receivedData;
            private byte[] receiveBuffer;


            // constructor
            public TCP(int _id)
            {
                ID = _id;
            }

            public void Connect(TcpClient _socket)
            {
                socket = _socket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                stream = socket.GetStream();

                receivedData = new Packet();

                receiveBuffer = new byte[dataBufferSize];

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

                // send welcome packet to the client
                ServerSend.Welcome(ID, "Welcome to the server!");
            }

            public void SendData(Packet _packet)
            {
                try
                {
                    //make sure socket has value
                    if (socket != null)
                    {
                        stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                    }
                }
                catch (Exception ex)
                {
                    Wenzil.Console.Console.Log($"Error Sending TCP Data Packet to Client {ID} : {ex.Message}");
                }
            }


            private void ReceiveCallback(IAsyncResult _result)
            {
                try
                {
                    int _byteLength = stream.EndRead(_result);

                    // no data received
                    if (_byteLength <= 0)
                    {
                        Server.clients[ID].Disconnect();
                        return;
                    }

                    // data received
                    byte[] _data = new byte[_byteLength];
                    Array.Copy(receiveBuffer, _data, _byteLength);

                    // Handle data
                    receivedData.Reset(HandleData(_data));

                    // start listening again
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);


                }
                catch (Exception ex)
                {
                    Wenzil.Console.Console.Log($"Error Receiving TCP Data: {ex.Message}");
                    Server.clients[ID].Disconnect();
                }
            }

            private bool IsStartOfPacket(Packet _packet)
            {
                // check if the received data has 4 or more unread bytes
                // if this is true - then we are at the start of a new packet
                // this is because every new packet starts with an int (which takes up 4 bytes)
                // this int represents the length of said packer
                if (_packet.UnreadLength() >= 4)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            private bool IsPacketEmpty(int _packetLength)
            {
                // The packet is empty if there is less than 1 byte contained
                if (_packetLength <= 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }


            private bool HandleData(byte[] _data)
            {
                int _packetLength = 0;

                receivedData.SetBytes(_data);

                // check if we're at a new packet
                if (IsStartOfPacket(receivedData))
                {
                    // if so - store this packet length
                    _packetLength = receivedData.ReadInt();

                    // check if empty - if so return and reset
                    if (IsPacketEmpty(_packetLength))
                    {
                        return true;
                    }
                }

                // this loop will keep running as long as receivedData contains more data packets which we can handle
                while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
                {
                    // read the packet byte array
                    byte[] _packetBytes = receivedData.ReadBytes(_packetLength);

                    // handle the packet
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet _packet = new Packet(_packetBytes))
                        {
                            int _packetID = _packet.ReadInt();

                            // get the appropriate packet handler and invoke it
                            Server.packetHandlers[_packetID](ID, _packet);
                        }
                    });

                    // reset the packet length to zero
                    _packetLength = 0;

                    // check if theres another packet contained in the receivedData
                    if (IsStartOfPacket(receivedData))
                    {
                        // store this packet length
                        _packetLength = receivedData.ReadInt();

                        // if the packet length is less than 1 byte - then reset the data
                        if (IsPacketEmpty(_packetLength))
                        {
                            return true;
                        }
                    }
                }

                // return to reset data if the packet length is 1 byte or less
                if (_packetLength <= 1)
                {
                    return true;
                }

                // otherwise, do not reset - because there is still a partial packet left - we need to wait for the next stream to finish unpacking it
                return false;
            }


            public void Disconnect()
            {
                socket.Close();
                stream = null;
                receivedData = null;
                receiveBuffer = null;
                socket = null;
            }
        }

        public class UDP
        {
            public IPEndPoint endPoint;

            private int ID;

            public UDP(int _id)
            {
                ID = _id;
            }

            public void Connect(IPEndPoint _endPoint)
            {
                endPoint = _endPoint;

                // UDP Test
                ServerSend.UPDTest(ID);
            }

            public void SendData(Packet _packet)
            {
                Server.SendUDPData(endPoint, _packet);
            }


            public void HandleData(Packet _packetData)
            {
                // remove the packet length from the start of the packer
                int _packetLength = _packetData.ReadInt();
                byte[] _packetBytes = _packetData.ReadBytes(_packetLength);

                // then handle the rest of the packet according to the packet id and our handler dict
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(_packetBytes))
                    {
                        int _packetID = _packet.ReadInt();
                        Server.packetHandlers[_packetID](ID, _packet);
                    }
                });

            }


            public void Disconnect()
            {
                endPoint = null;
            }
        }


        private void Disconnect()
        {
            Wenzil.Console.Console.Log($"{tcp.socket.Client.RemoteEndPoint} has disconnected.");

            ClientDatabase.Instance.RemoveClientPanel(this);

            tcp.Disconnect();
            udp.Disconnect();

            username = null;
        }
    }
}
