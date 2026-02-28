using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using HiveShard.Config;
using HiveShard.Interface;
using HiveShard.Interface.Config;
using HiveShard.Interface.Logging;
using HiveShardEE.API.Contracts.Data;
using HiveShardEE.API.Contracts.Requests;
using HiveShardEE.API.Contracts.Responses;

namespace HiveShard.Telemetry.HiveShardEE
{
    // ReSharper disable once InconsistentNaming
    public class HiveShardEETelemetry: IHiveShardTelemetry
    {
        private readonly HttpClient _client;
        private readonly TelemetryConfig _config;
        private readonly ISerializer _serializer; 
        private readonly ConcurrentQueue<SystemLog> _messages = new();
        private IIdentityConfig _scopedIdentity = new IdentityConfig(Guid.Empty, "unscoped");
        private int? _environment;
        
        public HiveShardEETelemetry(TelemetryConfig config, ISerializer serializer)
        {
            _config = config;
            _serializer = serializer;
            _client = new HttpClient()
            {
                DefaultRequestHeaders =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", _config.Token) 
                }
            };
        }

        public void LogWarning(string message)
        {
            _messages.Enqueue(new SystemLog(
                _scopedIdentity.GetServiceType(), 
                _scopedIdentity.GetIdentity(), 
                "Warning", 
                message, 
                DateTime.UtcNow
            ));
        }

        public void LogDebug(string message)
        {
            _messages.Enqueue(new SystemLog(
                _scopedIdentity.GetServiceType(), 
                _scopedIdentity.GetIdentity(), 
                "Debug", 
                message, 
                DateTime.UtcNow
            ));
        }

        public void LogException(Exception exception)
        {
            _messages.Enqueue(new SystemLog(
                _scopedIdentity.GetServiceType(), 
                _scopedIdentity.GetIdentity(), 
                "Exception", 
                exception.ToString(), 
                DateTime.UtcNow
            ));
        }

        public IHiveShardTelemetry GetScopedLogger<T>(IIdentityConfig identityConfig)
        {
            throw new NotImplementedException();
        }

        private async Task Flush()
        {
            if(_messages.IsEmpty)
                return;
            
            _environment ??= await CreateEnvironment();
            
            List<SystemLog> logs = [];
            while (_messages.TryDequeue(out var log))
                logs.Add(log);

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    string path = $"api/{_config.Organization}/{_config.Project}/{_config.EnvironmentType}/{_environment}/system-log/ingest";
                    var postAsJsonAsync = await _client.PostAsJsonAsync(new Uri(_config.ApiEndpoint, path),
                        new SystemLogIngestRequest(logs));
                    if (postAsJsonAsync.StatusCode != HttpStatusCode.OK)
                        throw new Exception($"Failed to flush logs to HiveShardEE: {postAsJsonAsync.StatusCode}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                await Task.Delay(200);
            }

        }

        public void Dispose()
        {
            Flush().GetAwaiter().GetResult();
        }
        
        private async Task<int> CreateEnvironment()
        {
            var response = await _client.PostAsJsonAsync(new Uri(_config.ApiEndpoint, $"api/{_config.Organization}/{_config.Project}/env/create"),
                new CreateEnvironmentRequest(_config.EnvironmentType));

            if (response.StatusCode is not HttpStatusCode.OK)
                throw new Exception($"Failed to create a HiveShardEE environment: {response.StatusCode}");

            var content = await response.Content.ReadAsStringAsync();
            var createEnvironmentResponse = _serializer.Deserialize<CreateEnvironmentResponse>(content);

            return createEnvironmentResponse.EnvInstance;
        }
    }
}