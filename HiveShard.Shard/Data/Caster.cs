using System;
using HiveShard.Data;

namespace HiveShard.Shard.Data
{
    public class Caster
    {
        public Caster(Consumption<object> consumption, Action<object> handler)
        {
            Consumption = consumption;
            Handler = handler;
        }

        public Consumption<object> Consumption { get; }
        public Action<object> Handler { get; }
    }
}