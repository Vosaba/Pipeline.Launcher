using PipelineLauncher.Dto;
using PipelineLauncher.PipelineEvents;
using System.Collections.Generic;
using System.Threading.Tasks;

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

    public interface IAwaitablePipelineRunner<in TInput, out TOutput> : IPipelineRunner<TInput, TOutput>
    {
        IEnumerable<TOutput> Process(TInput input);
        IEnumerable<TOutput> Process(IEnumerable<TInput> input);

        IAsyncEnumerable<TOutput> Process(IEnumerable<TInput> input, bool f);
    }
}
