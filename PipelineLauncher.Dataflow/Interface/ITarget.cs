using System;

namespace PipelineLauncher.Dataflow
{
    public interface ITarget : IComplete
    {

    }

    public interface ITargetOut<TOut> : ITarget
    {
        void LinkTo(ITargetIn<TOut> target);

        void LinkTo(ITargetIn<TOut> target, Action<TOut, ITargetIn<TOut>> filterMethod);
    }

    public interface ITargetIn<TIn> : ITryAdd<TIn>, ITarget
    {
    }

    public interface ITarget<TIn, TOut> : ITargetIn<TIn>, ITargetOut<TOut>
    {

    }
}