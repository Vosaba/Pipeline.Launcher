using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineEvents;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.PipelineRunner;
using PipelineLauncher.Abstractions.PipelineRunner.Configurations;
using PipelineLauncher.PipelineStage;

namespace PipelineLauncher.PipelineRunner
{
    internal class AwaitablePipelineRunner<TInput, TOutput> : PipelineRunnerBase<TInput, TOutput>, IAwaitablePipelineRunner<TInput, TOutput>
    {
        private readonly AwaitablePipelineCreationConfig _pipelineCreationConfig;
        private readonly Action _destroyTaskStages;

        internal AwaitablePipelineRunner(
            Func<StageCreationContext, ITargetBlock<PipelineStageItem<TInput>>> retrieveFirstBlock,
            Func<StageCreationContext, ISourceBlock<PipelineStageItem<TOutput>>> retrieveLastBlock,
            Action destroyTaskStages,
            AwaitablePipelineCreationConfig pipelineCreationConfig)
            : base(retrieveFirstBlock, retrieveLastBlock, new StageCreationContext(PipelineType.Awaitable, !pipelineCreationConfig.IgnoreTimeOuts))
        {
            _destroyTaskStages = destroyTaskStages;
            _pipelineCreationConfig = pipelineCreationConfig;

            if (_pipelineCreationConfig.ThrowExceptionOccured)
            {
                ExceptionItemsReceivedEvent += AwaitablePipelineRunner_ExceptionItemsReceivedEvent;
            }
        }

        public IEnumerable<TOutput> Process(TInput input)
        {
            return Process(new[] { input });
        }

        public IEnumerable<TOutput> Process(IEnumerable<TInput> input)
        {
            var result = ProcessAsync(input)
                .ConfigureAwait(false)
                .GetAwaiter();

            return result.GetResult();
        }

        public Task<IEnumerable<TOutput>> ProcessAsync(TInput input)
        {
            return ProcessAsync(new[] { input });
        }

        public async Task<IEnumerable<TOutput>> ProcessAsync(IEnumerable<TInput> input)
        {
            var result = new ConcurrentBag<TOutput>();

            _destroyTaskStages();

            var lastBlock = RetrieveLastBlock(StageCreationContext);
            var firstBlock = RetrieveFirstBlock(StageCreationContext);

            var posted = input.Select(x => new PipelineStageItem<TInput>(x)).All(x => firstBlock.Post(x));
            firstBlock.Complete();

            while (await lastBlock.OutputAvailableAsync())
            {
                var item = await lastBlock.ReceiveAsync();
                if (item.Item != null)
                {
                    result.Add(item.Item);
                }

                SortingMethod(item);
            }

            return result;
        }

        //public IAsyncEnumerable<TOutput> ProcessAsyncEnumerable(TInput input)
        //{
        //    return ProcessAsyncEnumerable(new[] { input });
        //}

        //public async IAsyncEnumerable<TOutput> ProcessAsyncEnumerable(IEnumerable<TInput> input)
        //{
        //    _destroyTaskStages();

        //    var lastBlock = RetrieveLastBlock(StageCreationContext);
        //    var firstBlock = RetrieveFirstBlock(StageCreationContext);

        //    var posted = input.Select(x => new PipelineStageItem<TInput>(x)).All(x => firstBlock.Post(x));
        //    firstBlock.Complete();

        //    while (await lastBlock.OutputAvailableAsync())
        //    {
        //        var item = await lastBlock.ReceiveAsync();
        //        if (item.Item != null)
        //        {
        //            yield return item.Item;
        //        }

        //        SortingMethod(item);
        //    }
        //}

        public Task GetCompletionTaskFor(TInput input)
        {
            return GetCompletionTaskFor(new[] { input });
        }

        public Task GetCompletionTaskFor(IEnumerable<TInput> input)
        {
            _destroyTaskStages();

            var lastBlock = RetrieveLastBlock(StageCreationContext);
            var sortingBlock = GenerateSortingBlock(lastBlock);
            var firstBlock = RetrieveFirstBlock(StageCreationContext);

            var posted = input.Select(x => new PipelineStageItem<TInput>(x)).All(x => firstBlock.Post(x));
            firstBlock.Complete();

            lastBlock.Completion.ContinueWith(x =>
            {
                sortingBlock.Complete();
            });

            return sortingBlock.Completion;
        }

        public IAwaitablePipelineRunner<TInput, TOutput> SetupInstantExceptionHandler(Action<ExceptionItemsEventArgs> exceptionHandler)
        {
            StageCreationContext.SetupInstantExceptionHandler(exceptionHandler);
            return this;
        }

        private void AwaitablePipelineRunner_ExceptionItemsReceivedEvent(ExceptionItemsEventArgs items)
        {
            throw items.Exception;
        }

        IAwaitablePipelineRunner<TInput, TOutput> IAwaitablePipelineRunner<TInput, TOutput>.SetupCancellationToken(CancellationToken cancellationToken)
            => (IAwaitablePipelineRunner<TInput, TOutput>)SetupCancellationToken(cancellationToken);
    }
}