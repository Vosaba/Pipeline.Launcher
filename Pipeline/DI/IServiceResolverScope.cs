using System;

namespace PipelineLauncher.DI
{
    /// <summary>
    /// Provides service instances in separate scope    
    /// </summary>
    public interface IServiceResolverScope : IServiceResolver, IDisposable
    {
    }
}