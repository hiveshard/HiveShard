using System.Numerics;
using DotChaser.Maps;
using DotChaser.Tests.Providers;

namespace DotChaser.Tests.Test;

public class PlayerBlockedBySquares
{
    [Test]
    public void BlockStopsPlayer()
    {
        var player = new Player(new Vector2(2, 0), new Vector2(-1, 0), 1000);
        Game game = new Game([player], new BlockedLinearMap(), new TestOutputProvider());
        game.Simulate();
        game.Simulate();
        
        game.Render();

        Assert.That(game.ValidatePosition(new Vector2(2, 0), 1000));
    }
}