using System.Collections.Concurrent;

namespace HiveShard.Workers.Initialization.Tests.Repositories;

public class TestRepository
{
    private ConcurrentQueue<int> _ints = new();

    public void AddInt(int integer)
    {
        _ints.Enqueue(integer);
    }

    public bool TryGet(out int integer) => _ints.TryDequeue(out integer);
}