using System;

namespace HiveShard.Shard.Data
{
    public class Serving
    {
        public Serving(Action<string, long> depositor)
        {
            _depositor = depositor;
        }

        private Action<string, long> _depositor;

        public void DepositBlocking(string messageValue, long offsetValue)
        {
            _depositor(messageValue, offsetValue);
        }
    }
}