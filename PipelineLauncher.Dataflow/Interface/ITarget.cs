using System;

namespace PipelineLauncher.Dataflow
{
    public interface ITarget<in TIn> : ITryAdd<TIn>, IComplete
    {
    }

    public interface ITarget<TIn, TOut> : ITarget<TIn>
    {
        void LinkTo(ITarget<TOut> target);

        void LinkTo(ITarget<TOut> target, Action<TOut, ITarget<TOut>> filterMethod);
    }
}