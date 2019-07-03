using SnakeWithFriends.Game;
using System;
using System.Threading;

namespace SnakeWithFriends
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Host a game, or join one? (h/j)");

            char key = ' ';
            while (key != 'q' && key != 'h' && key != 'j')
            {
                key = Console.ReadKey().KeyChar;

                switch (key)
                {
                    case 'h':
                        Console.Write("\b");
                        new Server().Start();
                        break;
                    case 'j':
                        Console.Write("\b");
                        new Client().Start();
                        break;
                    default:
                        Console.Write("\b");
                        break;
                }
            }

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
