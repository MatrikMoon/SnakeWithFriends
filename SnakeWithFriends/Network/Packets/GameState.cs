using System;

namespace SnakeWithFriends.Network.Packets
{
    [Serializable]
    class GameState
    {
        public Game.Models.Player[] players;
        public Game.Models.Blip[] blips;
    }
}
