using Snake.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake.Classes
{
    public class GameLogic
    {
        public int Rows { get; set; }
        public int Cols { get; set; }
        public GridValue[,] Grid {  get; set; }
        public Direction Dir {  get; set; }
        public int Score { get; set; }
        public bool GameOver { get; set; }
        private readonly LinkedList<Position> _snakePositions = new LinkedList<Position>();
        private readonly LinkedList<Direction> _dirChanges = new LinkedList<Direction>();
        private readonly Random _random = new Random();

        public GameLogic(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
            Grid = new GridValue[rows, cols];
            Dir = Direction.Right;
            AddSnake();
            AddFood();
        }

        private void AddSnake()
        {
            int r = Rows / 2;

            for(int c = 1; c <= 3; c++)
            {
                Grid[r, c] = GridValue.Snake;

                _snakePositions.AddFirst(new Position(r, c));
            }
        }

        private IEnumerable<Position> EmptyPositions()
        {
            for(int r = 0; r < Rows; r++)
            {
                for(int c = 0; c < Cols; c++)
                {
                    if (Grid[r, c] == GridValue.Empty)
                    {
                        yield return new Position(r, c);
                    }
                }
            }
        }

        private void AddFood()
        {
            List<Position> empty = new(EmptyPositions());

            if(empty.Count == 0)
            {
                return;
            }

            Position foodSpawn = empty[_random.Next(0, empty.Count)];
            Grid[foodSpawn.Row, foodSpawn.Col] = GridValue.Food;
        }

        public Position HeadPosition()
        {
            return _snakePositions.First!.Value;
        }

        public Position TailPosition()
        {
            return _snakePositions.Last!.Value;
        }

        public IEnumerable<Position> SnakePositions()
        {
            foreach(Position position in _snakePositions)
            {
                Debug.WriteLine($"Row: {position.Row}, Col: {position.Col}");
            }
            return _snakePositions;
        }

        public void AddHead(Position pos)
        {
            _snakePositions.AddFirst(pos);
            Grid[pos.Row, pos.Col] = GridValue.Snake;
        }

        public void RemoveTail() 
        {
            Position currTail = _snakePositions.Last.Value;
            Grid[currTail.Row, currTail.Col] = GridValue.Empty;
            _snakePositions.RemoveLast();
        }

        private Direction GetLastDirection()
        {
            if(_dirChanges.Count == 0)
            {
                return Dir;
            }

            return _dirChanges.Last.Value;
        }

        private bool CanChangeDirection(Direction newDir)
        {
            if(_dirChanges.Count == 2)
            {
                return false;
            }

            Direction lastDir = GetLastDirection();
            return newDir != lastDir && newDir != lastDir.Opposite();
        }

        public void ChangeDirection(Direction dir)
        {
            if (CanChangeDirection(dir))
            {
            _dirChanges.AddLast(dir);
            }
        }

        private bool isOutsideGrid(Position pos)
        {
            return pos.Row < 0 || pos.Row >= Rows || pos.Col < 0 || pos.Col >= Cols;
        }

        private GridValue WillHit(Position newHeadPos)
        {
            if (isOutsideGrid(newHeadPos))
            {
                return GridValue.Outside;
            }

            if(newHeadPos == TailPosition())
            {
                return GridValue.Empty;
            }

            return Grid[newHeadPos.Row, newHeadPos.Col];
        }

        public void Move()
        {
            if(_dirChanges.Count > 0)
            {
                Dir = _dirChanges.First.Value;
                _dirChanges.RemoveFirst();
            }

            Position newHeadPos = HeadPosition().Translate(Dir);
            GridValue hit = WillHit(newHeadPos);

            if (hit == GridValue.Outside || hit == GridValue.Snake)
            {
                GameOver = true;
            } 
            else if (hit == GridValue.Empty)
            {
                RemoveTail();
                AddHead(newHeadPos);
            }
            else if(hit == GridValue.Food)
            {
                AddHead(newHeadPos);
                Score++;
                AddFood();
            }
        }
    }
}
