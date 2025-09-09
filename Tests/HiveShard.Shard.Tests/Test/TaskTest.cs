namespace HiveShard.Shard.Tests.Test;

[TestFixture]
public class TaskTest
{
    [Test]
    public void AwaitThrowsException()
    {
        Assert.CatchAsync<Exception>(async() =>
        {
            await Task.Run(() =>
            {
                throw new Exception();
            });
        });
    }
    
    [Test]
    public void WhenAllThrowsException()
    {
        Assert.ThrowsAsync<Exception>(async () =>
        {
            Task task = Task.Run(() =>
            {
                throw new Exception();
            });

            await Task.WhenAll(task);
        });
    }
    
    [Test]
    public void DontAwaitNoException()
    {
        Assert.DoesNotThrow(() =>
        {
            _ = Task.Run(() =>
            {
                throw new Exception();
            }); 
        });
    }
}