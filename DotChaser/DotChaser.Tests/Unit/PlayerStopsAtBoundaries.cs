using System.Numerics;
using DotChaser.Maps;
using DotChaser.Tests.Providers;

namespace DotChaser.Tests;

public class PlayerStopsAtBoundaries
{
    [Test]
    public void LeftBoundaryStopsPlayer()
    {
        var player = new Player(Vector2.Zero, new Vector2(-1, 0), 1000);
        Game game = new Game([player], new LinearMap(), new TestOutputProvider());
        game.Simulate();
        
        game.Render();

        Assert.That(game.ValidatePosition(new Vector2(0, 0), 1000));
    }
}