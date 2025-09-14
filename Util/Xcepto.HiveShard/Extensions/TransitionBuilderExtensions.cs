using System;
using Xcepto.HiveShard.Adapters;
using Xcepto.HiveShard.Builder;

namespace Xcepto.HiveShard.Extensions
{
    public static class TransitionBuilderExtensions
    {
        public static HiveShardAdapter RegisterHiveShard(this TransitionBuilder builder, Func<HiveShardBuilder, HiveShardAdapter> hiveShardBuilder)
        {
            return builder.RegisterAdapter(hiveShardBuilder(new HiveShardBuilder()));
        }
    }
}