using System;
using System.Threading.Tasks;
using HiveShard.Interface;
using Xcepto.Interfaces;

namespace Xcepto.HiveShard;

public class GenericEntryPoint: IEntryPoint
{
    private Func<IIsolatedEntryPoint> _isolatedEntryPointProvider;

    public Task Start()
    {
        return _isolatedEntryPointProvider().Start();
    }

    public void UpdateStartMethod(Func<IIsolatedEntryPoint> isolatedEntryPoint)
    {
        _isolatedEntryPointProvider = isolatedEntryPoint;
    }
}