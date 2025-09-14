using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HiveShard.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace Xcepto.HiveShard.Adapters
{
    public class HiveShardAdapter: XceptoAdapter
    {
        public void SendEdgeMessage<T>(T message)
        where T: IEvent
        {
            throw new System.NotImplementedException();
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