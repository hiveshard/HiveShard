using System;
using HiveShard.Interface;
using Newtonsoft.Json;

namespace HiveShard.Serializer
{
    public class NewtonsoftSerializer : ISerializer
    {
        public string Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public T Deserialize<T>(string serializedObj)
        {
            return JsonConvert.DeserializeObject<T>(serializedObj);
        }

        public object Deserialize(string tcpMessagePayload, Type type)
        {
            return JsonConvert.DeserializeObject(tcpMessagePayload, type);
        }
    }
}