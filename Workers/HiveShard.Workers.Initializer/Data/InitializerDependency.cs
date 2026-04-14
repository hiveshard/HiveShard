using System;

namespace HiveShard.Workers.Initializer.Data;

public class InitializerDependency
{
    public InitializerDependency(Type type, object instance)
    {
        Type = type;
        Instance = instance;
    }

    public Type Type { get; }
    public object Instance { get; }
}