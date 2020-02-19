using PipelineLauncher.Abstractions.PipelineEvents;
using System.Threading;

namespace PipelineLauncher.Abstractions.PipelineRunner
{
    public interface IPipelineRunnerBase<in TInput, out TOutput>
    {
        event ItemReceivedEventHandler<TOutput> ItemReceivedEvent;
        event ExceptionItemsReceivedEventHandler ExceptionItemsReceivedEvent;
        event SkippedItemReceivedEventHandler SkippedItemReceivedEvent;
        event DiagnosticEventHandler DiagnosticEvent;

        public IPipelineRunnerBase<TInput, TOutput> SetupCancellationToken(CancellationToken cancellationToken);
    }
}
