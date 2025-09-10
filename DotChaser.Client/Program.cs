using System.Numerics;

namespace DotChaser.Client;

class Program
{
    private const int Height = 5;
    private const int Width = 10;
    
    static void Main(string[] args)
    {
        Vector2 playerPosition = new Vector2(3,3);
        Render(playerPosition);
        while (true)
        {
            var input = Console.ReadKey();
            if (input.Key == ConsoleKey.A)
                playerPosition = new Vector2(Math.Clamp(playerPosition.X - 1, 0, Width - 1), playerPosition.Y);
            if (input.Key == ConsoleKey.D)
                playerPosition = new Vector2(Math.Clamp(playerPosition.X + 1, 0, Width - 1), playerPosition.Y);
            if (input.Key == ConsoleKey.W)
                playerPosition = new Vector2(playerPosition.X, Math.Clamp(playerPosition.Y + 1, 0, Height - 1));
            if (input.Key == ConsoleKey.S)
                playerPosition = new Vector2(playerPosition.X, Math.Clamp(playerPosition.Y - 1, 0, Height - 1));
            Render(playerPosition);
        }
    }
    
    private static void Render(Vector2 playerPosition)
    {
        Console.Clear();
        for (int y = Height - 1; y >= 0; y--)
        {
            for (int x = 0; x < Width; x++)
            {
                if(playerPosition == new Vector2(x,y))
                    Console.Write(" O");
                else
                    Console.Write(" X");
            }
            Console.Write("\n");
        }
    }
}