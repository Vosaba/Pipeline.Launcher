using System;
using System.Collections;
using System.Collections.Generic;

namespace PipelineLauncher.Dataflow
{
    public interface IDataBlockCollection<TIn> : IEnumerable<TIn>, ICollection, IDisposable, ITryAdd<TIn>, ITryTake<TIn>, IConsumingEnumerable<TIn>, IComplete
    {
    }
}