using System;
using System.Collections.Concurrent;

namespace HiveShard.Workers.Initializer.Repositories;

public class InitializerAdditionRepository
{
    private ConcurrentQueue<Type> _initializersToBeAdded = new();

    public void AddInitializer(Type type)
    {
        _initializersToBeAdded.Enqueue(type);
    }

    public bool TryGetInitializer(out Type initializer) => _initializersToBeAdded.TryDequeue(out initializer);
}