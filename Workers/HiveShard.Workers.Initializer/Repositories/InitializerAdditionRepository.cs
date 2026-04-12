using System;
using System.Collections.Concurrent;
using HiveShard.Workers.Initializer.Data;

namespace HiveShard.Workers.Initializer.Repositories;

public class InitializerAdditionRepository
{
    private readonly ConcurrentQueue<InitializerAdditionRequest> _initializersToBeAdded = new();

    public void AddInitializer(InitializerAdditionRequest type)
    {
        _initializersToBeAdded.Enqueue(type);
    }

    public bool TryGetInitializer(out InitializerAdditionRequest initializer) => 
        _initializersToBeAdded.TryDequeue(out initializer);
}