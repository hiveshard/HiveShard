using HiveShard.Data;

namespace HiveShard.Edge.events
{
    public class EdgeBindingRequest
    {
        public EdgeBindingRequest(Client client)
        {
            Client = client;
        }

        public Client Client { get; }
    }
}