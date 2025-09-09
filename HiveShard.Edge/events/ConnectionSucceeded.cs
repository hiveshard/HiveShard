using System;
using HiveShard.Interface;

namespace HiveShard.Edge.events
{
    public class ConnectionSucceeded: IEvent
    {
        public ConnectionSucceeded(Uri edge)
        {
            Edge = edge;
        }

        public Uri Edge { get; }
    }
}