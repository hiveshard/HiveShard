using System;

namespace HiveShard.Environment
{
    public static class HiveShardEnv
    {
        public static string GetEnv(string key)
        {
            DotNetEnv.Env.TraversePath().Load();
            var value = System.Environment.GetEnvironmentVariable(key);
            if (value is null)
                throw new Exception($"{key} environment variable missing");
            return value;
        }
    }
}