using System;
using System.Collections.Generic;
using System.Linq;
using HiveShard.Data;
using HiveShard.Interface;
using HiveShard.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace HiveShard.Builder;

public class DecentralizedHiveShardBuilder
{
    private readonly IDeployment _deployment;
    private Chunk _minChunk = new(0,0);
    private Chunk _maxChunk = new(0,0);
    private readonly List<IsolatedEnvironment> _workers = [];
    private readonly ServiceCollection _topLevelServices = [];
    private readonly EventRepository _eventRepository = new();
    internal DecentralizedHiveShardBuilder(IDeployment deployment)
    {
        _deployment = deployment;
    }

    public DecentralizedHiveShardBuilder SetGridSize(Chunk min, Chunk max)
    {
        if (min.XCoord > max.XCoord || min.YCoord > max.YCoord)
            throw new InvalidOperationException("MinChunk has to be >= MaxChunk");
            
        _minChunk = min;
        _maxChunk = max;
        return this;
    }
    internal ServiceEnvironment Build(string environmentName)
    {
        return _deployment.Build(_minChunk, _maxChunk, _workers.AsEnumerable(), _eventRepository, environmentName);
    }

    private readonly ISet<Type> _isolatedEnvironment = new HashSet<Type>();

    public void RegisterWorker<TIsolatedEnvironment>(TIsolatedEnvironment environment)
    where TIsolatedEnvironment: IsolatedEnvironment
    {
        var type = typeof(TIsolatedEnvironment);
        if (_isolatedEnvironment.Contains(type) && environment.IsUnique)
            throw new InvalidOperationException($"Already registered one {type.Name}");
        _isolatedEnvironment.Add(type);
        _workers.Add(environment);
    }

    public DecentralizedHiveShardBuilder Events(Func<EventBuilder, EventBuilder> eventBuilder)
    {
        eventBuilder(new EventBuilder(_eventRepository));
        return this;
    }
}