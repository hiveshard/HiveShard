using HiveShard.Data;

namespace HiveShard.Edge.events
{
    public class EdgeUnbindingRequest
    {
        public EdgeUnbindingRequest(Client client)
        {
            Client = client;
        }

        public Client Client { get; }
    }
}