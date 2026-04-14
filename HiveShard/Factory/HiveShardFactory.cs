using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HiveShard.Builder;
using HiveShard.Data;
using HiveShard.Exceptions;
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

        var serviceEnvironment = builderInstance.Build(environmentName);
        ValidateEnvironment(serviceEnvironment);
        return serviceEnvironment;
    }

    private static void ValidateEnvironment(ServiceEnvironment serviceEnvironment)
    {
        ValidateEventRegistrations(serviceEnvironment);
        ValidateEmitters(serviceEnvironment);
    }

    private static void ValidateEventRegistrations(ServiceEnvironment serviceEnvironment)
    {
        var events = serviceEnvironment.EventRepository;
        if (events.GetTotalOrder().Length <= 0)
            throw new HiveShardValidationException("No events registered!", HiveShardValidationExceptionCase.NoEvents);
    }
    private static void ValidateEmitters(ServiceEnvironment serviceEnvironment)
    {
        var events = serviceEnvironment.EventRepository;
        foreach (var emitter in serviceEnvironment.Inner.SelectMany(x=>x.ContainedEmitters))
        {
            var topicsOfEmitter = events.GetTopicsOfEmitter(emitter);
            if (topicsOfEmitter.Length == 0)
            {
                throw new HiveShardValidationException(
                    $"Emitter {emitter.EmitterIdentityString} does not emit any events!",
                    HiveShardValidationExceptionCase.EmitterWithoutEvents
                );
            }
        }
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