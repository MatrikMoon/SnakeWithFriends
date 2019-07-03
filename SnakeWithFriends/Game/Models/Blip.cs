using System;

namespace SnakeWithFriends.Game.Models
{
    [Serializable]
    class Blip
    {
        public int id { get; set; }
        public PositionDirection position;

        public char[][] GetBlipOverlay(int width, int height)
        {
            var ret = new char[height][];

            for (int y = 0; y < height; y++)
            {
                ret[y] = new char[width];
                for (int x = 0; x < width; x++)
                {
                    //Fill the board withwhitespace
                    ret[y][x] = ' ';
                }
            }

            //Add the blip to the board
            ret[position.Y][position.X] = 'o';

            return ret;
        }
    }
}
