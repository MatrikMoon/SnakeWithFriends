using System;
using System.Collections.Generic;
using System.Text;

namespace SnakeWithFriends.Misc
{
    class Types
    {
        public enum PacketType
        {
            Ping,
            Ack,
            Connect,
            Disconnect,
            Move,
            Status
        }
    }
}
