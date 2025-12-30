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
        public CompletedTick(string emitterIdentity, long tick, string eventType,
            IEnumerable<TopicPartitionOffset> topicPartitionOffsets)
        {
            TopicPartitionOffsets = topicPartitionOffsets;
            EmitterIdentity = emitterIdentity;
            Tick = tick;
            EventType = eventType;
        }

        public static CompletedTick From<T>(IEventEmitterType emitter, long tick, 
            IEnumerable<TopicPartitionOffset> topicPartitionOffsets)
            where T : IEvent => From(typeof(T), emitter, tick, topicPartitionOffsets);
        
        public static CompletedTick From(Type eventType, IEventEmitterType emitter, long tick,
            IEnumerable<TopicPartitionOffset> topicPartitionOffsets) =>
            From(eventType.FullName!, emitter, tick, topicPartitionOffsets);
        
        public static CompletedTick From(string eventType, IEventEmitterType emitter, long tick,
            IEnumerable<TopicPartitionOffset> topicPartitionOffsets)
        {
            if (emitter.InitializationTickOnly && tick != 2)
                throw new InvalidOperationException();

            return new CompletedTick(emitter.Identity, tick, eventType, topicPartitionOffsets);
        }
        public string EmitterIdentity { get; }
        public long Tick { get; }
        public string EventType { get; }
        public IEnumerable<TopicPartitionOffset> TopicPartitionOffsets { get; }
    }
}