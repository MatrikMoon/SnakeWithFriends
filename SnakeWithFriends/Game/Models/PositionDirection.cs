using System;
using System.Collections.Generic;
using System.Text;

namespace SnakeWithFriends.Game.Models
{
    [Serializable]
    class PositionDirection
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Direction { get; set; }
    }
}
