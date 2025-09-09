using System;
using System.Collections.Generic;
using HiveShard.Data;
using HiveShard.Interface;
using Newtonsoft.Json;

namespace HiveShard.Event
{
    public class CompletedTick: IEvent
    {
        [JsonConstructor]
        public CompletedTick(HiveShardIdentity hiveShardIdentity, long number, IEnumerable<TopicPartitionOffset> produceOffsets, DateTime lastTickTime)
        {
            HiveShardIdentity = hiveShardIdentity;
            Number = number;
            ProduceOffsets = produceOffsets;
            LastTickTime = lastTickTime;
        }

        public HiveShardIdentity HiveShardIdentity { get; }
        public long Number { get; }
        public DateTime LastTickTime { get; }
        public IEnumerable<TopicPartitionOffset> ProduceOffsets { get; }
    }
}