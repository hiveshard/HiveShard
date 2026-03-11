using System;
using HiveShard.Config;
using HiveShard.Data;
using HiveShard.Interface;
using HiveShard.Interface.Config;
using HiveShard.Interface.Logging;
using HiveShard.Interface.Providers;
using HiveShard.Interface.Repository;
using HiveShard.Provider;
using HiveShard.Repository;
using HiveShard.Serializer;

namespace HiveShard.Fabrics.InMemory.Builders;

public class InMemorySimpleFabricBuilder
{
    public InMemorySimpleFabric Build(IHiveShardTelemetry telemetryProvider)
    {
        ISerializer serializer = new NewtonsoftSerializer();
        return new InMemorySimpleFabric(telemetryProvider, new GlobalChunkConfig(new Chunk(0,0), new Chunk(0,0)), serializer);
    }
}