using System;

namespace HiveShard.Telemetry.HiveShardEE;

public class TelemetryConfig
{
    public TelemetryConfig(Uri apiEndpoint, string token, string organization, string project, string environmentType)
    {
        ApiEndpoint = apiEndpoint;
        Token = token;
        Organization = organization;
        Project = project;
        EnvironmentType = environmentType;
    }

    public Uri ApiEndpoint { get; }
    public string Token { get; }
    public string EnvironmentType { get; }
    public string Organization { get; }
    public string Project { get; }
}