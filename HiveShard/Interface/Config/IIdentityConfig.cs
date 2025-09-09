using System;

namespace HiveShard.Interface.Config
{
    public interface IIdentityConfig
    {
        public Guid GetIdentity();
        public string GetIdentityString();
    }
}