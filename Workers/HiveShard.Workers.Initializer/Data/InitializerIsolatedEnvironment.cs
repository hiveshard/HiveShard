using System;
using System.Collections.Generic;

namespace HiveShard.Workers.Initializer.Data
{
    public class InitializerIsolatedEnvironment: IsolatedEnvironment
    {
        public IEnumerable<Type> Initializers { get; }

        public InitializerIsolatedEnvironment(IEnumerable<Type> initializers)
        {
            Initializers = initializers;
        }

        public override bool IsUnique => true;
    }
}