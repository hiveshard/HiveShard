using System;
using HiveShard.Interface;

namespace HiveShard.Workers.Edge.Events;

public class EdgeBoundNotification: IEvent
{
    public EdgeBoundNotification(Uri uri)
    {
        Uri = uri;
    }

    public Uri Uri { get; }
}