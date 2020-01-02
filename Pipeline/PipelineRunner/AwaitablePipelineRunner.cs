using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineEvents;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.PipelineRunner;
using PipelineLauncher.Abstractions.PipelineRunner.Configurations;
using PipelineLauncher.Abstractions.PipelineStage;

namespace PipelineLauncher.PipelineRunner
{
    internal class AwaitablePipelineRunner<TInput, TOutput> : PipelineRunnerBase<TInput, TOutput>, IAwaitablePipelineRunner<TInput, TOutput>
    {
        private readonly Action _destroyTaskStages;
        private readonly ConcurrentBag<TOutput> _processedItems = new ConcurrentBag<TOutput>();

        protected sealed override StageCreationOptions CreationOptions => new StageCreationOptions(PipelineType.Awaitable, !PipelineConfig.IgnoreTimeOuts);
        protected readonly AwaitablePipelineConfig PipelineConfig;


        internal AwaitablePipelineRunner(
            Func<StageCreationOptions, bool, ITargetBlock<PipelineStageItem<TInput>>> retrieveFirstBlock,
            Func<StageCreationOptions, bool, ISourceBlock<PipelineStageItem<TOutput>>> retrieveLastBlock,
            CancellationToken cancellationToken,
            Action destroyTaskStages,
            AwaitablePipelineConfig pipelineConfig)
            : base(retrieveFirstBlock, retrieveLastBlock, cancellationToken)
        {
            _destroyTaskStages = destroyTaskStages;
            PipelineConfig = pipelineConfig ?? new AwaitablePipelineConfig();

            ItemReceivedEvent += AwaitablePipeline_ItemReceivedEvent;

            if (PipelineConfig.ThrowExceptionOccured)
            {
                ExceptionItemsReceivedEvent += AwaitablePipelineRunner_ExceptionItemsReceivedEvent;
            }
        }

        public IEnumerable<TOutput> Process(TInput input)
        {
            return Process(new []{input});
        }

        public IEnumerable<TOutput> Process(IEnumerable<TInput> input)
        {
            _processedItems.Clear();
            _destroyTaskStages();

            var lastBlock = RetrieveLastBlock(CreationOptions, false);
            var sortingBlock = GenerateSortingBlock(lastBlock);

            var firstBlock = RetrieveFirstBlock(CreationOptions, false);
            var posted = input.Select(x => new PipelineStageItem<TInput>(x)).All(x => firstBlock.Post(x));
            firstBlock.Complete();

            lastBlock.Completion.ContinueWith(x =>
            {
                sortingBlock.Complete();
            });

            sortingBlock.Completion.Wait();

            return _processedItems;
        }


        private void AwaitablePipeline_ItemReceivedEvent(TOutput item)
        {
            _processedItems.Add(item);
        }

        private void AwaitablePipelineRunner_ExceptionItemsReceivedEvent(ExceptionItemsEventArgs items)
        {
            throw items.Exception;
        }
    }
}