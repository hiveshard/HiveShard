using System;
using HiveShard.Interface;

namespace HiveShard.Edge.Events
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