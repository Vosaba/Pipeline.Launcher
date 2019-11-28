using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PipelineLauncher.Pipelines
{
    public class PipelineItemEventArgs
    {
        public Type StageType { get; }
        public string StageName => StageType.FullName;

        public PipelineItemEventArgs(Type stageType)
        {
            StageType = stageType;
        }
    }

    public class ExceptionItemsEventArgs : PipelineItemEventArgs
    {
        public object[] Items { get; }
        public Exception Exception { get; }

        public Action ReProcess { get; }

        public ExceptionItemsEventArgs(object[] items, Type stageType, Exception exception, Action reProcess)
            : base(stageType)
        {
            Items = items;
            ReProcess = reProcess;
            Exception = exception;
        }
    }

    public class SkippedItemEventArgs : PipelineItemEventArgs
    {
        public object Item { get; }

        public SkippedItemEventArgs(object item, Type stageType) 
            : base(stageType)
        {
            Item = item;
        }
    }

    public delegate void ExceptionItemsReceivedEventHandler(ExceptionItemsEventArgs items);
    public delegate void SkippedItemReceivedEventHandler(SkippedItemEventArgs item);
    public delegate void ItemReceivedEventHandler<in TItem>(TItem item);

    public interface IPipeline<in TInput, TOutput> 
    {
        event ItemReceivedEventHandler<TOutput> ItemReceivedEvent;
        event ExceptionItemsReceivedEventHandler ExceptionItemsReceivedEvent;
        event SkippedItemReceivedEventHandler SkippedItemReceivedEvent;

        bool Post(TInput input);
        bool Post(IEnumerable<TInput> input);
    }

    public interface IAwaitablePipeline<in TInput, TOutput> : IPipeline<TInput, TOutput>
    {
        Task AwaitableTask();

        IEnumerable<TOutput> RunSync(TInput input);
        IEnumerable<TOutput> RunSync(IEnumerable<TInput> input);
    }
}
