using System.Collections.Concurrent;
using System.Numerics;
using DotChaser.Client.Providers;

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
        var player = new Player(new Vector2(3, 3), new Vector2(0, 0), 1000);
        Game game = new Game(Height, Width, [player], new ClientOutputProvider());
        
        while (true)
        {
            if(DateTime.Now - lastSimulationStep > TimeSpan.FromSeconds(1))
            {
                foreach (var inputChange in _inputChanges)
                {
                    game.ChangePlayerDirection(inputChange);
                }
                game.Simulate();
                lastSimulationStep = DateTime.Now;
            }
            game.Render();
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
}