using System;
using PipelineLauncher.Abstractions.PipelineEvents;
using System.Threading;

namespace PipelineLauncher.Abstractions.PipelineRunner
{
    //public interface ITypedHandler
    //{
    //    bool TryToHandleExceptionItems(ExceptionItemsEventArgs exceptionItemsEventArgs);
    //    bool TryToHandleSkippedItem(SkippedItemEventArgs skippedItemEventArgs);
    //}

    public interface ITypedHandler<TItem>//: ITypedHandler
    {
        event ExceptionItemsReceivedEventHandler<TItem> ExceptionItemsReceivedEvent;
        event SkippedItemReceivedEventHandler<TItem> SkippedItemReceivedEvent;
    }

    public interface IPipelineRunnerBase<in TInput, out TOutput>
    {
        event ItemReceivedEventHandler<TOutput> ItemReceivedEvent;
        event ExceptionItemsReceivedEventHandler ExceptionItemsReceivedEvent;
        event SkippedItemReceivedEventHandler SkippedItemReceivedEvent;
        event DiagnosticEventHandler DiagnosticEvent;

        IPipelineRunnerBase<TInput, TOutput> SetupCancellationToken(CancellationToken cancellationToken);
        ITypedHandler<T> TypedHandler<T>();

        IPipelineRunnerBase<TInput, TOutput> WithModifier(Action<IPipelineRunnerBase<TInput, TOutput>> modifier);
    }
}
