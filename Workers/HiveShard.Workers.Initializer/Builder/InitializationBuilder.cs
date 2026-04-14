using System;
using System.Collections.Generic;
using System.Linq;
using HiveShard.Data;
using HiveShard.Initializer.Interfaces;
using HiveShard.Workers.Initializer.Data;namespace HiveShard.Workers.Initializer.Builder;

public class InitializationBuilder
{
    private readonly List<InitializerAdditionRequest> _initializers = new();
    public InitializerIsolatedEnvironment Build()
    {
        return new InitializerIsolatedEnvironment(_initializers.AsEnumerable());
    }

    public InitializationBuilder AddInitializer<TInitializer>()
        where TInitializer : IInitializer => 
        AddInitializer<TInitializer>(new InitializerEmitterIdentity(new EmitterIdentity(typeof(TInitializer).FullName!)));
    public InitializationBuilder AddInitializer<TInitializer>(
        InitializerEmitterIdentity identity)
        where TInitializer : IInitializer => AddInitializer<TInitializer>(identity, _ => { });

    public InitializationBuilder AddInitializer<TInitializer>(
        Action<InitializerBuilder> initializerBuilder)
        where TInitializer : IInitializer =>
        AddInitializer<TInitializer>(
            new InitializerEmitterIdentity(new EmitterIdentity(typeof(TInitializer).FullName!)), initializerBuilder);
    public InitializationBuilder AddInitializer<TInitializer>(
        InitializerEmitterIdentity identity, 
        Action<InitializerBuilder> initializerBuilder)
        where TInitializer: IInitializer
    {
        var builder = new InitializerBuilder(identity, typeof(TInitializer));
        initializerBuilder(builder);
        _initializers.Add(builder.Build());
        return this;
    }
}