using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Fabric.Telemetry;
using HiveShard.Fabric.Ticker;
using HiveShard.Interface.Config;
using HiveShard.Interface.Logging;

namespace HiveShard.Fabrics.InMemory
{
    public class InMemoryTelemetryFabric: InMemorySimpleFabric, ITelemetryFabric
    {
        public InMemoryTelemetryFabric(IFabricLoggingProvider loggingProvider, IIdentityConfig identityConfig) : base(loggingProvider, identityConfig)
        {
        }

        public new void Register<T>(string topic, Action<Consumption<T>> action) => 
            base.Register<T>(topic, action);

        public new void Register<T>(string topic, Chunk chunk, Action<Consumption<T>> action) =>
            base.Register(topic, chunk, action);
        public new Task Send<T>(string topic, T message) => base.Send(topic, message);
        public new Task Send<T>(string topic, Chunk chunk, T message) => base.Send(topic, chunk, message);
    }
}