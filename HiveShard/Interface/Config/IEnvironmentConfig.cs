using System;

namespace HiveShard.Interface.Config
{
    public interface IEnvironmentConfig
    {
        public Guid Prefix { get; }
    }
}