using System;
using HiveShard.Interface;
using HiveShard.Interface.Config;

namespace HiveShard.Config
{
    public class EnvironmentConfig: IEnvironmentConfig
    {
        public EnvironmentConfig(Guid environment)
        {
            Prefix = environment;
        }

        public Guid Prefix { get; }
    }
}