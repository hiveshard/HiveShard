using System;
using System.Collections.Generic;
using HiveShard.Data;
using HiveShard.Workers.Initializer.Data;

namespace HiveShard.Workers.Initializer.Builder;

public class InitializerBuilder
{
    private readonly InitializerEmitterIdentity _identity;
    private readonly Type _type;
    private readonly List<InitializerDependency> _initializerDependencies = new();

    public InitializerBuilder(InitializerEmitterIdentity identity, Type type)
    {
        _identity = identity;
        _type = type;
    }
    internal InitializerAdditionRequest Build()
    {
        return new InitializerAdditionRequest(_identity, _type, _initializerDependencies);
    }

    public InitializerBuilder WithDependency<T>(T instance)
        where T : class
    {
        _initializerDependencies.Add(new InitializerDependency(typeof(T), instance));
        return this;
    }
}