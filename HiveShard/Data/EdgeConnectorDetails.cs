using System;
using System.Collections.Generic;

namespace HiveShard.Data
{
    public class EdgeConnectorDetails
    {
        public EdgeConnectorDetails(Uri uri, IEnumerable<Chunk> chunks)
        {
            Uri = uri;
            Chunks = chunks;
        }

        public Uri Uri { get; }
        public IEnumerable<Chunk> Chunks { get; }
    }
}
