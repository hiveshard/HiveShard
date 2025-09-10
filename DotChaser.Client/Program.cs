using System.Collections.Concurrent;
using System.Numerics;

namespace DotChaser.Client;

class Program
{
    private const int Height = 5;
    private const int Width = 10;


    private static ConcurrentQueue<Vector2> _inputChanges = new();
    static async Task Main(string[] args)
    {
        _ = Task.Run(() =>
        {
            while (true)
            {
                Vector2 direction = InputDirection();
                _inputChanges.Enqueue(direction);
            }
        });

        DateTime lastSimulationStep = DateTime.Now;
        Game game = new Game(Height, Width);
        while (true)
        {
            if(DateTime.Now - lastSimulationStep > TimeSpan.FromSeconds(1))
            {
                game.Simulate(_inputChanges);
                lastSimulationStep = DateTime.Now;
            }
            Render(game);
            await Task.Delay(100);
        }
    }
    
    private static Vector2 InputDirection()
    {
        var input = Console.ReadKey();
        if (input.Key == ConsoleKey.A)
            return new Vector2(-1,0);
        if (input.Key == ConsoleKey.D)
            return new Vector2(1,0);
        if (input.Key == ConsoleKey.W)
            return new Vector2(0,1);
        if (input.Key == ConsoleKey.S)
            return new Vector2(0,-1);
        return new Vector2(0,0);
    }
    
    private static void Render(Game game)
    {
        Console.Clear();
        for (int y = Height - 1; y >= 0; y--)
        {
            for (int x = 0; x < Width; x++)
            {
                if (game.Players.Any(p => p.Position == new Vector2(x, y)))
                    Console.Write(" O");
                else
                    Console.Write(" X");
            }
            Console.Write("\n");
        }
    }
}