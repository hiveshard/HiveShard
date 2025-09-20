using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HiveShard.Interface;
using Microsoft.Extensions.DependencyInjection;
using Xcepto.HiveShard.States;

namespace Xcepto.HiveShard.Adapters
{
    public class HiveShardAdapter: XceptoAdapter
    {
        public void SendEdgeMessage<T>(T message)
        where T: IEvent
        {
            AddStep(new SendEdgeMessageState<T>($"Send message of type {typeof(T).Name}", message));
        }
        
        public void ExpectEdgeMessage<T>(Predicate<T> predicate)
            where T: IEvent
        {
            throw new System.NotImplementedException();
        }

        public void SendShardMessage<T>(T message)
        where T: IEvent
        {
            throw new System.NotImplementedException();
        }
        
        public void ExpectShardMessage<T>(Predicate<T> predicate)
            where T: IEvent
        {
            throw new System.NotImplementedException();
        }
    }
}