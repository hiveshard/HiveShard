using System;
using System.Collections.Generic;

namespace HiveShard.Workers.Initializer.Data;

public class InitializerIsolatedEnvironment: IsolatedEnvironment
{
    public IEnumerable<InitializerAdditionRequest> Initializers { get; }

    public InitializerIsolatedEnvironment(IEnumerable<InitializerAdditionRequest> initializers)
    {
        Initializers = initializers;
    }

    public override bool IsUnique => true;
}