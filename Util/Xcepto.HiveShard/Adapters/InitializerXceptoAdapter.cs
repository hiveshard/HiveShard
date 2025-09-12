using System;
using System.Threading;
using System.Threading.Tasks;
using HiveShard.Interface;
using HiveShard.Shard;
using HiveShard.Worker;
using HiveShard.Worker.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Xcepto.HiveShard.Adapters
{
    public class InitializerXceptoAdapter<T>: XceptoAdapter
    where T: class, IInitializer
    {
        protected override async Task Initialize(IServiceProvider serviceProvider)
        {
            var initializer = serviceProvider.GetRequiredService<T>();
            await initializer.Initialize();
        }

        protected override Task AddServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<T>();
            return Task.CompletedTask;
        }
    }
}