using System;
using HiveShard.Interface;
using HiveShard.Interface.Config;

namespace HiveShard.Config
{
    public class IdentityConfig: IIdentityConfig
    {
        private Guid _id;
        private string _serviceType;

        public IdentityConfig(Guid id, string serviceType)
        {
            _serviceType = serviceType;
            _id = id;
        }

        public Guid GetIdentity() => _id;

        public string GetIdentityString() => $"{_serviceType}-{_id}";
    }
}