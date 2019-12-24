using PipelineLauncher.Abstractions.PipelineEvents;
using System.Collections.Generic;

namespace PipelineLauncher.Pipelines
{
    public interface IPipelineRunner<in TInput, out TOutput> 
    {
        event ItemReceivedEventHandler<TOutput> ItemReceivedEvent;
        event ExceptionItemsReceivedEventHandler ExceptionItemsReceivedEvent;
        event SkippedItemReceivedEventHandler SkippedItemReceivedEvent;

        bool Post(TInput input);
        bool Post(IEnumerable<TInput> input);
    }
}
