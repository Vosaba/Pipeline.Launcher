using PipelineLauncher.Abstractions.PipelineEvents;

namespace PipelineLauncher.Pipelines
{
    public interface IPipelineRunnerBase<in TInput, out TOutput>
    {
        event ItemReceivedEventHandler<TOutput> ItemReceivedEvent;
        event ExceptionItemsReceivedEventHandler ExceptionItemsReceivedEvent;
        event SkippedItemReceivedEventHandler SkippedItemReceivedEvent;
    }
}
