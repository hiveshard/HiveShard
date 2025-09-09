using System;
using HiveShard.Interface;

namespace HiveShard.Fabrics.Tcp.Data
{
    public class TcpMessage
    {
        public TcpMessage(string assemblyType, string payload)
        {
            AssemblyType = assemblyType;
            Payload = payload;
        }

        public string AssemblyType { get; }
        public string Payload { get; }


        public string TypeFullName()
        {
            Type type = Type.GetType(AssemblyType);
            if (type is null || type.FullName is null)
                throw new Exception("invalid type in message");
            return type.FullName;
        }

        public T GetExpectedPayload<T>(ISerializer serializer)
        {
            Type type = Type.GetType(AssemblyType);
            if (type is null)
                throw new Exception("invalid type in message");
            if (typeof(T) != type)
                throw new Exception("expected type mismatched payload type");

            return serializer.Deserialize<T>(Payload);
        }
    }
}