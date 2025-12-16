using System;
using System.Collections.Generic;
using System.Linq;
using HiveShard.Initializer.Interfaces;
using HiveShard.Workers.Initializer.Data;

namespace HiveShard.Workers.Initializer.Builder
{
    public class InitializationBuilder
    {
        private readonly List<Type> _initializers = new();
        public InitializerIsolatedEnvironment Build()
        {
            return new InitializerIsolatedEnvironment(_initializers.AsEnumerable());
        }

        public InitializationBuilder AddInitializer<TInitializer>()
        where TInitializer: IInitializer
        {
            _initializers.Add(typeof(TInitializer));
            return this;
        }
    }
}