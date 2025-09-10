using System.Numerics;

namespace DotChaser
{
    public class Monster
    {
        public Monster(Vector2 position, Vector2 direction)
        {
            Position = position;
            Direction = direction;
        }

        public Vector2 Position { get; private set; }
        public Vector2 Direction { get; private set; }

        public void UpdatePosition(Vector2 newPosition)
        {
            Position = newPosition;
        }

        public void UpdateDirection(Vector2 possibleDirection)
        {
            Direction = possibleDirection;
        }
    }
}