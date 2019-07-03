using SnakeWithFriends.Game.Models;
using SnakeWithFriends.Network;
using SnakeWithFriends.Network.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using static SnakeWithFriends.Network.Packets.Event;

namespace SnakeWithFriends.Game
{
    class Server
    {
        int width = 50;
        //int width = 100;
        int height = 30;
        //int height = 60;

        private Timer tickTimer = new Timer();
        private Random random = new Random();
        private List<Player> players = new List<Player>();
        private List<Blip> blips = new List<Blip>();
        private Screen screen;
        private Network.Server server;

        public void Start()
        {
            Console.WriteLine("Server starting...");

            StartServer();

            server = new Network.Server(4445);
            server.PacketRecieved += Server_PacketRecieved;
            server.PlayerConnected += Server_PlayerConnected;
            server.PlayerDisconnected += Server_PlayerDisconnected;
            server.Start();


            /*var gameState = new GameState();
            gameState.players = new Player[3];
            gameState.blips = new Blip[4];

            var packet = new Network.Packet(gameState);
            var packetString = packet.ToBase64();
            var packetBytes = packet.ToBytes();

            Console.WriteLine(sizeof(int));
            Console.WriteLine(packetString);
            Console.WriteLine();
            Console.WriteLine((Network.Packet.FromBytes(packetBytes).SpecificPacket as GameState).players);
            Console.WriteLine();*/

            /*using (var memStream = new MemoryStream(testBytes))
            {
                while (memStream.Length > 0)
                {
                    var testPackcet = Network.Packet.FromStream(memStream);
                    if (testPackcet != null)
                    {
                        Console.WriteLine($"FOUND CONNECT WITH VALUE: {((Connect)testPackcet.SpecificPacket).playerId}");
                    }
                    else memStream.ReadByte();
                }
            }*/
        }

        private void Server_PlayerDisconnected(NetworkPlayer networkPlayer)
        {
            var player = players.FirstOrDefault(x => x.id == networkPlayer.id);
            if (player != null) player.flagForRemoval = true;
        }

        private void Server_PlayerConnected(NetworkPlayer networkPlayer)
        {
            //Let the player know what their ID is
            var connect = new Connect();
            connect.playerId = networkPlayer.id;
            connect.screenWidth = width;
            connect.screenHeight = height;
            var packet = new Packet(connect);
            
            server.Send(networkPlayer.id, packet.ToBytes());
        }

        private void SpawnPlayer(NetworkPlayer networkPlayer)
        {
            if (!players.Any(x => x.id == networkPlayer.id))
            {
                var newPlayer = new Player();
                newPlayer.id = networkPlayer.id;
                players.Add(newPlayer);
            }

            var player = players.First(x => x.id == networkPlayer.id);
            player.headPosition = GetFreePosition();
            player.dead = false;
        }

        private void SpawnLocalPlayer(Player player)
        {
            player.headPosition = GetFreePosition();
            player.dead = false;
        }

        private void Server_PacketRecieved(NetworkPlayer player, Packet packet)
        {
            if (packet.Type == PacketType.Move)
            {
                players.First(x => x.id == player.id).ChangeDirection((packet.SpecificPacket as Move).newDirection);
            }
            else if (packet.Type == PacketType.Event)
            {
                var specificEvent = packet.SpecificPacket as Event;
                if ((EventType)specificEvent.eventType == EventType.Request)
                {
                    if ((Request)specificEvent.specificEvent == Request.Spawn)
                    {
                        //Do nothing if the player isn't dead
                        if (players.FirstOrDefault(x => x.id == player.id)?.dead ?? true)
                        {
                            SpawnPlayer(player);

                            var spawnResponse = new Event();
                            spawnResponse.eventType = (int)EventType.Resopnse;
                            spawnResponse.specificEvent = (int)Response.SpawnCompleted;
                            var newPacket = new Packet(spawnResponse);

                            server.Send(player.id, newPacket.ToBytes());
                        }
                    }
                }
            }
        }

        private void StartServer()
        {
            screen = new Screen(width, height);

            //Set up local player
            var localPlayer = new Player();
            localPlayer.id = 0;
            localPlayer.dead = false;
            localPlayer.headPosition = new PositionDirection()
            {
                X = width / 2,
                Y = height / 2,
                Direction = 0
            };
            //localPlayer.numberOfBodyPositions = 10;
            players.Add(localPlayer);

            //Add first blip
            AddBlip();

            //First draw
            screen.Draw(players, blips);

            tickTimer.Elapsed += DoTick;
            tickTimer.Interval = 100;
            tickTimer.Start();
        }

        private void DoTick(object sender, ElapsedEventArgs e)
        {
            var localPlayer = players.First(x => x.id == 0);

            //Check for local keypresses
            CheckLocalKeys(localPlayer);

            //Move all players
            foreach (var player in players)
            {
                if (!player.dead) player.MovePlayer();
            }

            //Check for collisions
            players.ForEach(x =>
            {
                if (!x.dead)
                {
                    blips.RemoveAll(y =>
                    {
                        bool collides = x.Collides(y.position.X, y.position.Y);
                        if (collides) x.AddLength();
                        return collides;
                    });
                }
            });

            var deadList = new List<Player>();
            players.ForEach(testingPlayer =>
            {
                if (!testingPlayer.dead)
                {
                    bool dead = false;

                    //Wall collisions
                    if (testingPlayer.headPosition.X == 0 ||
                        testingPlayer.headPosition.Y == 0 ||
                        testingPlayer.headPosition.X == width - 1 ||
                        testingPlayer.headPosition.Y == height - 1)
                    {
                        dead = true;
                    }

                    //Player collisions
                    //Note: Head to head collisions kill both players
                    if (players.Any(otherPlayer => !otherPlayer.dead && testingPlayer != otherPlayer && otherPlayer.Collides(testingPlayer.headPosition.X, testingPlayer.headPosition.Y)))
                    {
                        dead = true;
                    }

                    if (testingPlayer.CollidesWithSelf())
                    {
                        dead = true;
                    }

                    if (dead) deadList.Add(testingPlayer);
                }
            });

            deadList.ForEach(x =>
            {
                x.Kill();

                //If the player died, send the event to the player
                if (x.id != 0)
                {
                    var eventPacket = new Event();
                    eventPacket.eventType = (int)EventType.Event;
                    eventPacket.specificEvent = (int)Events.Death;
                    var packet = new Packet(eventPacket);

                    server.Send(x.id, packet.ToBytes());
                }
            });
            deadList.Clear();

            //Maybe add blips
            if (random.NextDouble() > 0.0) AddBlip();

            //Remove players who are disconnected
            players.RemoveAll(x => x.flagForRemoval);

            //Send game state to clients
            server.SendGameState(players.ToArray(), blips.ToArray());

            //Draw the screen
            screen.Draw(players, blips);

            if (players.First(x => x.id == 0).dead)
            {
                screen.DrawMessage("You are died.........");
            }
        }

        private void AddBlip()
        {
            var blip = new Blip();
            blip.position = GetFreePosition();
            blips.Add(blip);
        }

        private PositionDirection GetFreePosition()
        {
            PositionDirection ret = null;

            bool validPosition = false;
            while (!validPosition)
            {
                ret = new PositionDirection()
                {
                    X = random.Next(1, width - 1), //Index 0 is the border, second paramter is inclusive
                    Y = random.Next(1, height - 1),
                    Direction = random.Next(0, 3)
                };

                validPosition = players.TrueForAll(x => x.dead || !x.Collides(ret.X, ret.Y));
            }
            return ret;
        }

        private void CheckLocalKeys(Player localPlayer)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true).Key;
                if (localPlayer.dead)
                {
                    if (key == ConsoleKey.R)
                    {
                        SpawnLocalPlayer(localPlayer);
                    }
                }
                else
                {
                    switch (key)
                    {
                        case ConsoleKey.W:
                            localPlayer.ChangeDirection(0);
                            break;

                        case ConsoleKey.D:
                            localPlayer.ChangeDirection(1);
                            break;

                        case ConsoleKey.S:
                            localPlayer.ChangeDirection(2);
                            break;

                        case ConsoleKey.A:
                            localPlayer.ChangeDirection(3);
                            break;

                        default:
                            break;
                    }
                }
            }
        }
    }
}
