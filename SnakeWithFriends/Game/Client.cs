using SnakeWithFriends.Network;
using SnakeWithFriends.Network.Packets;
using System;
using System.Collections.Generic;
using System.Threading;
using static SnakeWithFriends.Network.Packets.Event;

namespace SnakeWithFriends.Game
{
    class Client
    {
        private Network.Client client;
        private Models.Player[] players;
        private Models.Blip[] blips;
        private Screen screen;

        private bool dead = true;

        public void Start()
        {
            Console.WriteLine("Client started");
            client = new Network.Client(4445);
            client.PacketRecieved += Client_PacketRecieved;
            client.Start();

            ConsoleKey? key = null;
            while (key != ConsoleKey.Q)
            {
                if (Console.KeyAvailable)
                {
                    key = Console.ReadKey(true).Key;

                    //If we're dead, check for the respawn key press
                    if (dead)
                    {
                        if (key == ConsoleKey.R)
                        {
                            var respawnRequest = new Event();
                            respawnRequest.eventType = (int)EventType.Request;
                            respawnRequest.specificEvent = (int)Request.Spawn;
                            var newPacket = new Packet(respawnRequest);

                            client.Send(newPacket.ToBytes());
                        }
                    }
                    else
                    {
                        switch (key)
                        {
                            case ConsoleKey.W:
                                client.SendDirectionChange(0);
                                break;

                            case ConsoleKey.D:
                                client.SendDirectionChange(1);
                                break;

                            case ConsoleKey.S:
                                client.SendDirectionChange(2);
                                break;

                            case ConsoleKey.A:
                                client.SendDirectionChange(3);
                                break;

                            default:
                                break;
                        }
                    }
                }

                Thread.Sleep(10);
            }

            client.Shutdown();
        }

        private void Client_PacketRecieved(Packet packet)
        {
            if (packet.Type == PacketType.Connect)
            {
                var connect = packet.SpecificPacket as Connect;
                client.PlayerId = connect.playerId;
                screen = new Screen(connect.screenWidth, connect.screenHeight);
            }
            else if (packet.Type == PacketType.GameState)
            {
                var gameState = packet.SpecificPacket as GameState;
                players = gameState.players;
                blips = gameState.blips;

                screen.Draw(new List<Models.Player>(players), new List<Models.Blip>(blips));
                if (dead) screen.DrawMessage(">>> You are dead. <<<");
            }
            else if (packet.Type == PacketType.Event)
            {
                var specificPacket = packet.SpecificPacket as Event;
                if ((EventType)specificPacket.eventType == EventType.Event)
                {
                    if ((Events)specificPacket.specificEvent == Events.Death)
                    {
                        dead = true;
                    }
                }
                else if ((EventType)specificPacket.eventType == EventType.Resopnse)
                {
                    if ((Response)specificPacket.specificEvent == Response.SpawnCompleted)
                    {
                        dead = false;
                    }
                }
            }
        }
    }
}
