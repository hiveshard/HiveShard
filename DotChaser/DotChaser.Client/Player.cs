using System.Numerics;

namespace DotChaser.Client;

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

    public void ChangeDirection(Vector2 inputChange)
    {
        Direction = inputChange;
    }

    public void UpdatePosition(Vector2 newPosition)
    {
        Position = newPosition;
    }
}