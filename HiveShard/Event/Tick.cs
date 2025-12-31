using System;
using System.Collections.Generic;
using HiveShard.Data;
using HiveShard.Interface;
using Newtonsoft.Json;

namespace HiveShard.Event
{
    public class Tick: IEvent
    {
        [JsonConstructor]
        public Tick(long tickNumber, IEnumerable<TopicPartitionOffset> chunkOffsets, DateTime tickDateTime, string tickEventType)
        {
            TickNumber = tickNumber;
            ChunkOffsets = chunkOffsets;
            TickDateTime = tickDateTime;
            TickEventType = tickEventType;
        }

        public long TickNumber { get; }
        public DateTime TickDateTime { get; }
        public IEnumerable<TopicPartitionOffset> ChunkOffsets { get; }
        public string TickEventType { get; }
    }
}