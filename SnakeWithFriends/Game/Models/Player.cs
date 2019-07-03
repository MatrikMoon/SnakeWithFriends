using System;
using System.Collections.Generic;
using System.Linq;

namespace SnakeWithFriends.Game.Models
{
    [Serializable]
    class Player
    {
        public int id { get; set; }
        public PositionDirection headPosition;
        public int numberOfBodyPositions;
        public PositionDirection[] bodyPositions;
        public bool dead = true;

        //If true, the player will be removed from the list at a thread-safe time
        public bool flagForRemoval = false;

        public Player()
        {
            bodyPositions = new PositionDirection[0];
        }

        public void ChangeDirection(int direction) => headPosition.Direction = direction;

        public void AddLength() => numberOfBodyPositions += 1;

        public bool CollidesWithHead(int x, int y) => headPosition.X == x && headPosition.Y == y;

        public bool CollidesWithBody(int x, int y) => bodyPositions.Any(position => position.X == x && position.Y == y);

        public bool CollidesWithSelf() => CollidesWithBody(headPosition.X, headPosition.Y);

        public bool Collides(int x, int y) => CollidesWithHead(x, y) || CollidesWithBody(x, y);

        public void Kill()
        {
            dead = true;
            numberOfBodyPositions = 0;
            bodyPositions = new PositionDirection[0];
            headPosition = null;
        }

        /// <summary>
        /// Moves the player and the body in the currently facing direction
        /// </summary>
        public void MovePlayer()
        {
            //Move body
            var body = new List<PositionDirection>(bodyPositions);
            body.Add(new PositionDirection()
            {
                X = headPosition.X,
                Y = headPosition.Y,
                Direction = headPosition.Direction
            });

            //If we aren't at the max length yet, don't dequeue
            if (body.Count > numberOfBodyPositions) body.RemoveAt(0);
            bodyPositions = body.ToArray();

            //Move head
            switch (headPosition.Direction)
            {
                //North
                case 0:
                    headPosition.Y -= 1;
                    break;

                //East
                case 1:
                    headPosition.X += 1;
                    break;

                //South
                case 2:
                    headPosition.Y += 1;
                    break;

                //West
                case 3:
                    headPosition.X -= 1;
                    break;

                default:
                    break;
            }
        }

        public char[][] GetPlayerOverlay(int width, int height)
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

            //Add the head to the board
            var headCharacter = ' ';
            switch (headPosition.Direction)
            {
                //North
                case 0:
                    headCharacter = '^';
                    break;

                //East
                case 1:
                    headCharacter = '>';
                    break;

                //South
                case 2:
                    headCharacter = '_';
                    break;

                //West
                case 3:
                    headCharacter = '<';
                    break;

                default:
                    headCharacter = 'X';
                    break;
            }

            ret[headPosition.Y][headPosition.X] = headCharacter;

            //Add body positions to the board
            for (int i = 0; i < bodyPositions.Length; i++)
            {
                var bodyCharacter = ' ';
                switch (bodyPositions[i].Direction)
                {
                    //North
                    case 0:
                        bodyCharacter = '|';
                        break;

                    //East
                    case 1:
                        bodyCharacter = '-';
                        break;

                    //South
                    case 2:
                        bodyCharacter = '|';
                        break;

                    //West
                    case 3:
                        bodyCharacter = '-';
                        break;

                    default:
                        bodyCharacter = 'X';
                        break;
                }

                ret[bodyPositions[i].Y][bodyPositions[i].X] = bodyCharacter;
            }

            return ret;
        }
    }
}
