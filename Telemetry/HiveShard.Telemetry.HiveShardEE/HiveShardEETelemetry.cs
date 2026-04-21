using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using HiveShard.Config;
using HiveShard.Data;
using HiveShard.Interface;
using HiveShard.Interface.Config;
using HiveShard.Interface.Logging;
using HiveShard.Telemetry.HiveShardEE.Extensions;
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
        private readonly ConcurrentQueue<Cause> _causes = new();
        private IIdentityConfig _scopedIdentity = new IdentityConfig(Guid.Empty, "unscoped");
        private int? _environment;
        private ServiceEnvironment _serviceEnvironment;
        
        
        public HiveShardEETelemetry(TelemetryConfig config, ISerializer serializer, ServiceEnvironment serviceEnvironment)
        {
            _config = config;
            _serializer = serializer;
            _serviceEnvironment = serviceEnvironment;
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

        public void Cause(TransitionCause cause)
        {
            _causes.Enqueue(cause.ToCause());
        }

        public IHiveShardTelemetry GetScopedLogger<T>(IIdentityConfig identityConfig)
        {
            throw new NotImplementedException();
        }

        private async Task Flush()
        {
            if(_messages.IsEmpty)
                return;

            if (_environment is null)
            {
                _environment = await CreateEnvironment();

                if (_environment is not null)
                {
                    await PushInfrastructure(_environment.Value);
                }
                else
                {
                    Console.WriteLine("[Telemetry WARNING] Environment not available. Skipping remote push.");
                }
            }

            await PushLogs(_environment);
            await PushCauses(_environment);
        }

        private async Task PushLogs(int? instance)
        {
            List<SystemLog> logs = [];
            while (_messages.TryDequeue(out var log))
            {
                Console.WriteLine(log.Message);
                logs.Add(log);
            }

            if (instance is null)
                return;
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    string path = $"api/{_config.Organization}/{_config.Project}/{_config.EnvironmentType}/{instance}/system-log/ingest";
                    var postAsJsonAsync = await _client.PostAsJsonAsync(new Uri(_config.ApiEndpoint, path),
                        new SystemLogIngestRequest(logs));
                    if (postAsJsonAsync.StatusCode != HttpStatusCode.OK)
                        throw new Exception($"Failed to flush logs to HiveShardEE: {postAsJsonAsync.StatusCode}");
                    break;
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
            try
            {
                Flush().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Telemetry WARNING] Flush failed: {e}");
            }
        }
        
        private async Task<int?> CreateEnvironment()
        {
            var content = "";
            Exception? lastException = null;
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    var response = await _client.PostAsJsonAsync(new Uri(_config.ApiEndpoint, $"api/{_config.Organization}/{_config.Project}/env/create"),
                        new CreateEnvironmentRequest(_config.EnvironmentType));

                    if (response.StatusCode is not HttpStatusCode.OK)
                        throw new Exception($"Failed to create a HiveShardEE environment: {response.StatusCode}");

                    content = await response.Content.ReadAsStringAsync();
                    break;
                }
                catch (Exception e)
                {
                    lastException = e;
                }
                await Task.Delay(200);
            }
            
            if (string.IsNullOrWhiteSpace(content))
            {
                if (lastException is not null)
                {
                    Console.WriteLine($"[Telemetry WARNING]: {lastException.Message}");
                }

                Console.WriteLine("[Telemetry WARNING] Environment creation failed (no response).");
                return null;
            }

            try
            {
                var createEnvironmentResponse = _serializer.Deserialize<CreateEnvironmentResponse>(content);
                return createEnvironmentResponse?.EnvInstance;
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Telemetry WARNING] Failed to parse environment response: {e.Message}");
                return null;
            }
        }

        private async Task PushInfrastructure(int instance)
        {
            var shardWorkers = _serviceEnvironment.Inner
                .Where(x => x.Identifier.CompartmentType == CompartmentType.ShardWorker)
                .Select(x => new ShardWorker(x.Identifier.Id))
                .ToList();

            var shardWorkerResponse = await _client.PostAsJsonAsync(new Uri(_config.ApiEndpoint, $"api/{_config.Organization}/{_config.Project}/{_config.EnvironmentType}/{instance}/infrastructure/full"),
                new PublishFullInfrastructureRequest([], shardWorkers, 0, 0));

            shardWorkerResponse.EnsureSuccessStatusCode();
        }

        private async Task PushCauses(int? instance)
        {
            List<Cause> causes = [];
            while (_causes.TryDequeue(out var cause))
            {
                Console.WriteLine($"[Cause] {cause.InboundEventType} => {cause.ShardType} => {cause.OutboundEventType}");
                causes.Add(cause);
            }

            if (instance is null)
                return;

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    string path = $"api/{_config.Organization}/{_config.Project}/{_config.EnvironmentType}/{instance}/edge/ingest";
                    var postAsJsonAsync = await _client.PostAsJsonAsync(new Uri(_config.ApiEndpoint, path),
                        new EdgeIngestRequest(causes));
                    if (postAsJsonAsync.StatusCode != HttpStatusCode.OK)
                        throw new Exception($"Failed to flush causes to HiveShardEE: {postAsJsonAsync.StatusCode}");
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                await Task.Delay(200);
            }
        }
    }
}