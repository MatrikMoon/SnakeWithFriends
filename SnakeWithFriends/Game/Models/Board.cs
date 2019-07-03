using System;
using System.Collections.Generic;
using System.Text;

namespace SnakeWithFriends.Game.Models
{
    class Board
    {
        public char[][] RawBoard { get; private set; }

        public int Height { get; private set; }
        public int Width { get; private set; }

        public Board(int width, int height)
        {
            Width = width;
            Height = height;

            //Draw the initial border
            RawBoard = new char[Height][];
            for (int y = 0; y < Height; y++)
            {
                RawBoard[y] = new char[Width];
                for (int x = 0; x < Width; x++)
                {
                    //Put a plus sign on the corners
                    if (x == 0 && y == 0 ||
                        x == 0 && y == Height - 1 ||
                        x == Width - 1 && y == 0 ||
                        x == Width - 1 && y == Height - 1)
                    {
                        RawBoard[y][x] = '+';
                    }

                    //Put vertical pipes on the left and right side
                    else if (x == 0 || x == Width - 1)
                    {
                        RawBoard[y][x] = '|';
                    }

                    //Put horizontal dasehs on the top and bottom
                    else if (y == 0 || y == Height - 1)
                    {
                        RawBoard[y][x] = '-';
                    }

                    //Fill the rest with whitespace
                    else RawBoard[y][x] = ' ';
                }
            }
        }

        public Board AddPlayer(Player player)
        {
            RawBoard = AppendToBoard(player.GetPlayerOverlay(Width, Height));
            return this;
        }

        public Board AddBlip(Blip blip)
        {
            RawBoard = AppendToBoard(blip.GetBlipOverlay(Width, Height));
            return this;
        }

        private char[][] AppendToBoard(char[][] item)
        {
            var ret = new char[RawBoard.Length][];

            for (int y = 0; y < RawBoard.Length; y++)
            {
                //Default to item1's contents
                ret[y] = RawBoard[y];

                for (int x = 0; x < RawBoard[y].Length; x++)
                {
                    //If the content of item2 is non-whitespace, overwrite item1
                    if (item[y][x] != ' ') ret[y][x] = item[y][x];
                }
            }
            return ret;
        }
    }
}
