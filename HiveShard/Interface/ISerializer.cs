using System;

namespace HiveShard.Interface;

public interface ISerializer
{
    public string Serialize<T>(T obj);
    public T Deserialize<T>(string serializedObj);
    object Deserialize(string tcpMessagePayload, Type type);
}