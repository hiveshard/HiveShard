using HiveShard.Data;
using HiveShard.Interface;

namespace HiveShard.Xcepto.Tests.Shards;

public class StateShard: IHiveShard
{
    public Chunk Chunk { get; private set; }

    public void Initialize(Chunk chunk)
    {
        Chunk = chunk;
    }
}