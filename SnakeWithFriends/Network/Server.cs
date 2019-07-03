using SnakeWithFriends.Network.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SnakeWithFriends.Network
{
    // State object for reading client data asynchronously  
    public class NetworkPlayer
    {
        public int id;
        public Socket workSocket = null;
        public const int BufferSize = 1024;
        public byte[] buffer = new byte[BufferSize];
        public List<byte> accumulatedBytes = new List<byte>();

        //If true, the player will be removed from the list at a thread-safe time
        public bool flagForRemoval = false;
    }

    class Server
    {
        public event Action<NetworkPlayer, Packet> PacketRecieved;
        public event Action<NetworkPlayer> PlayerConnected;
        public event Action<NetworkPlayer> PlayerDisconnected;

        private List<NetworkPlayer> players = new List<NetworkPlayer>();
        private List<NetworkPlayer> playersInQueue = new List<NetworkPlayer>();
        private Socket server;
        private int port;
        private bool enabled;
        private Random rand = new Random();

        private static ManualResetEvent accpeting = new ManualResetEvent(false);

        public Server(int port)
        {
            this.port = port;
        }

        public void Start()
        {
            enabled = true;

            IPAddress ipAddress = IPAddress.Any;
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            server = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            server.Bind(localEndPoint);
            server.Listen(100);

            while (enabled)
            {
                // Set the event to nonsignaled state.  
                accpeting.Reset();

                // Start an asynchronous socket to listen for connections.  
                Console.WriteLine("Waiting for a connection...");
                server.BeginAccept(new AsyncCallback(AcceptCallback), server);

                // Wait until a connection is made before continuing.  
                accpeting.WaitOne();
            }
        }

        public void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            accpeting.Set();

            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            NetworkPlayer player = new NetworkPlayer();
            player.id = rand.Next(int.MaxValue);
            player.workSocket = handler;
            playersInQueue.Add(player);

            handler.BeginReceive(player.buffer, 0, NetworkPlayer.BufferSize, 0, new AsyncCallback(ReadCallback), player);
        }

        public void ReadCallback(IAsyncResult ar)
        {
            try
            {
                NetworkPlayer player = (NetworkPlayer)ar.AsyncState;
                Socket handler = player.workSocket;

                // Read data from the client socket.   
                int bytesRead = handler.EndReceive(ar);

                if (bytesRead > 0)
                {
                    var currentBytes = new byte[bytesRead];
                    Buffer.BlockCopy(player.buffer, 0, currentBytes, 0, bytesRead);

                    player.accumulatedBytes.AddRange(currentBytes);
                    if (player.accumulatedBytes.Count >= Packet.packetHeaderSize)
                    {
                        //If we're not at the start of a packet, increment our position until we are, or we run out of bytes
                        var accumulatedBytes = player.accumulatedBytes.ToArray();
                        while (!Packet.StreamIsAtPacket(accumulatedBytes) && accumulatedBytes.Length >= Packet.packetHeaderSize)
                        {
                            player.accumulatedBytes.RemoveAt(0);
                            accumulatedBytes = player.accumulatedBytes.ToArray();
                        }

                        if (Packet.PotentiallyValidPacket(accumulatedBytes))
                        {
                            PacketRecieved?.Invoke(player, Packet.FromBytes(accumulatedBytes));
                            player.accumulatedBytes.Clear();
                        }
                    }

                    // Not all data received. Get more.  
                    handler.BeginReceive(player.buffer, 0, NetworkPlayer.BufferSize, 0, new AsyncCallback(ReadCallback), player);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void SendGameState(Game.Models.Player[] playerModels, Game.Models.Blip[] blips)
        {
            //Send gamestate to all connected players
            players.ForEach(x =>
            {
                if (x.id != 0) //If not the local player...
                {
                    var gameState = new GameState();
                    gameState.blips = blips.ToArray();
                    gameState.players = playerModels.ToArray();
                    var packet = new Packet(gameState);

                    Send(x.id, packet.ToBytes());
                }
            });

            //Remove the players that were marked for removal
            players.RemoveAll(x => x.flagForRemoval);

            //Add players who are waiting in the queue
            players.AddRange(playersInQueue);
            playersInQueue.Clear();
            players.ForEach(x => PlayerConnected?.Invoke(x));
        }

        public void Send(int playerId, byte[] data)
        {
            var player = players.First(x => x.id == playerId);

            try
            {
                //Get the socket for the specified playerId
                var socket = player.workSocket;

                // Begin sending the data to the remote device.  
                socket.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), player);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());

                player.flagForRemoval = true;
                PlayerDisconnected?.Invoke(player);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            NetworkPlayer player = (NetworkPlayer)ar.AsyncState;

            try
            {
                // Retrieve the socket from the state object.  
                var handler = player.workSocket;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());

                player.flagForRemoval = true;
                PlayerDisconnected?.Invoke(player);
            }
        }
    }
}
