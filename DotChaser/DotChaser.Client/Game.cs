using System.Collections.Concurrent;
using System.Numerics;

namespace DotChaser.Client;

public class Game
{
    Dictionary<Vector2, int> grid = new();
    public IEnumerable<Player> Players => _players.Values;
    private readonly Dictionary<int, Player> _players = new();

    public Game(int height, int width)
    {
        _players.Add(1000, new Player(new Vector2(3, 3), new Vector2(0, 0), 1000));
        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                if (_players.Values.Any(p => p.Direction == new Vector2(x, y)))
                    grid[new Vector2(x, y)] = 1000;
                else
                    grid[new Vector2(x, y)] = 0;
            }
        }
    }


    public void Simulate(ConcurrentQueue<Vector2> inputChanges)
    {
        foreach (var inputChange in inputChanges)
        {
            var player = _players[1000];
            player.ChangeDirection(inputChange);
        }

        foreach (var player in _players.Values)
        {
            var newPosition = player.Position + player.Direction;
            grid[player.Position] = 0;
            grid[newPosition] = player.ID;
            player.UpdatePosition(newPosition);
        }
    }
}