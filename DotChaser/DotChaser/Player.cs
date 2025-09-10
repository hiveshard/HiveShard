using System.Numerics;

namespace DotChaser
{
    public class Player
    {
        public Player(Vector2 position, Vector2 direction, int id)
        {
            Position = position;
            Direction = direction;
            ID = id;
        }

        public Vector2 Direction { get; private set; }
        public Vector2 Position { get; private set; }
        public int ID { get; }
        public int Dots { get; private set; }
        public bool Alive { get; private set; } = true;

        public void ChangeDirection(Vector2 inputChange)
        {
            Direction = inputChange;
        }

        public void UpdatePosition(Vector2 newPosition)
        {
            Position = newPosition;
        }
        
        public void CollectDot()
        {
            Dots = Dots + 1;
        }

        public void Died()
        {
            Alive = false;
        }
    }
}