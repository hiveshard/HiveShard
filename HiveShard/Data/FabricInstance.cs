using System;
using HiveShard.Interface.Config;

namespace HiveShard.Data
{
    public class FabricInstance
    {
        public string IdentityString { get; }
        public string TypeString { get; }

        public FabricInstance(IIdentityConfig identityConfig, Type type)
        {
            IdentityString = identityConfig.GetIdentityString();
            TypeString = type.FullName;
        }
    }
}