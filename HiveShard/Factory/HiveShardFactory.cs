using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HiveShard.Builder;
using HiveShard.Data;
using HiveShard.Interface;

namespace HiveShard.Factory;

public static class HiveShardFactory
{
    public static ServiceEnvironment Create<TDeployment>(Action<DecentralizedHiveShardBuilder> builder, 
        string? environmentName = null)
    where TDeployment: IDeployment, new()
    {
        environmentName ??= DetermineEnvironmentName();
        var builderInstance = new DecentralizedHiveShardBuilder(new TDeployment());
        builder(builderInstance);
        
        return builderInstance.Build(environmentName);
    }

    private static MethodBase? FindExternalCaller()
    {
        var trace = new StackTrace();

        for (int i = 0; i < trace.FrameCount; i++)
        {
            var method = trace.GetFrame(i)?.GetMethod();
            var type = method?.DeclaringType;

            if (type != typeof(HiveShardFactory))
                return method;
        }

        return null;
    }
    
    private static string DetermineEnvironmentName()
    {
        try
        {
            var method = FindExternalCaller();

            var type = method.DeclaringType;

            if (type?.DeclaringType != null &&
                type.GetCustomAttribute<CompilerGeneratedAttribute>() != null)
            {
                type = type.DeclaringType;
            }

            string methodName = method.Name == "MoveNext"
                ? type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .FirstOrDefault(m => m.GetCustomAttribute<AsyncStateMachineAttribute>()?.StateMachineType == method.DeclaringType)
                    ?.Name
                : method.Name;

            string className = type?.Name;
            string namespaceName = type?.Namespace;

            return $"{namespaceName}.{className}.{methodName}";
        }
        catch (Exception)
        {
            return "unnamed";
        }
    }
}