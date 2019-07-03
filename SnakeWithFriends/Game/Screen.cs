using SnakeWithFriends.Game.Models;
using System;
using System.Collections.Generic;

namespace SnakeWithFriends.Game
{
    class Screen
    {
        private int width;
        private int height;

        public Screen(int width, int height)
        {
            this.width = width;
            this.height = height;

            Console.SetWindowSize(((width + 1) * 2), height + 1); //Extra size for viewing pleasure
        }

        public void Draw(List<Player> players, List<Blip> blips)
        {
            Console.CursorVisible = false;
            Console.SetCursorPosition(0, 0);
            Console.Clear();
            Console.Write(ToString(players, blips));
        }

        public void DrawMessage(string message)
        {
            Console.SetCursorPosition(width - (message.Length / 2), height / 2);
            Console.Write(message);
        }

        public string ToString(List<Player> players, List<Blip> blips)
        {
            var board = new Board(width, height);
            players.ForEach(x => {
                if (!x.dead) board = board.AddPlayer(x);
            });
            blips.ForEach(x => board = board.AddBlip(x));

            string ret = "";
            foreach (char[] y in board.RawBoard)
            {
                foreach (char x in y)
                {
                    ret += x;
                    ret += ' ';
                }
                ret += "\n";
            }

            return ret;
        }
    }
}
