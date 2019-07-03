using System;

namespace SnakeWithFriends.Network.Packets
{
    [Serializable]
    class Connect
    {
        public int playerId;
        public int screenWidth;
        public int screenHeight;
    }
}
