using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.Abstractions.Services;
using PipelineLauncher.Blocks;
using PipelineLauncher.PipelineSetup;
using PipelineLauncher.PipelineStage;
using PipelineLauncher.Stages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Stages;
using PipelineLauncher.Exceptions;
using PipelineLauncher.Extensions;
using PipelineLauncher.PipelineSetup.StageSetup;

namespace PipelineLauncher
{
    public class PipelineCreator : IPipelineCreator
    {
        private readonly PipelineCreationContext _pipelineCreationContext;

        public PipelineCreator()
        {
            _pipelineCreationContext = new PipelineCreationContext();
        }

        public PipelineCreator(IStageService stageService)
        {
            _pipelineCreationContext = new PipelineCreationContext(stageService);
        }

        public PipelineCreator(Func<Type, IStage> stageResolveFunc)
        {
            _pipelineCreationContext = new PipelineCreationContext(stageResolveFunc);
        }

        public IPipelineCreator WithStageService(IStageService stageService)
        {
            _pipelineCreationContext.SetupStageService(stageService);
            return this;
        }

        public IPipelineCreator WithStageService(Func<Type, IStage> stageService)
        {
            _pipelineCreationContext.SetupStageService(stageService);
            return this;
        }

        public IPipelineCreator UseDefaultServiceResolver(bool useDefaultServiceResolver)
        {
            _pipelineCreationContext.SetupConfiguration(useDefaultServiceResolver);
            return this;
        }

        public IPipelineSetup<TInput, TInput> Prepare<TInput>()
            => Stage<TInput, TInput>(x => x);

        public IPipelineSetup<TInput, TInput> BulkPrepare<TInput>(BulkStageConfiguration stageConfiguration = null)
            => BulkStage<TInput, TInput>(x => x, stageConfiguration);

        #region Generic Stages

        #region BulkStages

        public IPipelineSetup<TInput, TOutput> BulkStage<TBulkStage, TInput, TOutput>()
            where TBulkStage : class, IBulkStage<TInput, TOutput>
            => CreateBulkStage<TInput, TOutput>(_pipelineCreationContext.StageService.GetStageInstance<TBulkStage>());

        public IPipelineSetup<TInput, TInput> BulkStage<TBulkStage, TInput>()
            where TBulkStage : class, IBulkStage<TInput, TInput>
            => CreateBulkStage<TInput, TInput>(_pipelineCreationContext.StageService.GetStageInstance<TBulkStage>());

        #endregion

        #region Stages

        public IPipelineSetup<TInput, TOutput> Stage<TStage, TInput, TOutput>()
            where TStage : class, IStage<TInput, TOutput>
            => CreateStage<TInput, TOutput>(_pipelineCreationContext.StageService.GetStageInstance<TStage>());

        public IPipelineSetup<TInput, TInput> Stage<TStage, TInput>()
            where TStage : class, IStage<TInput, TInput>
            => CreateStage<TInput, TInput>(_pipelineCreationContext.StageService.GetStageInstance<TStage>());

        #endregion

        #endregion

        #region Nongeneric

        #region BulkStages

        public IPipelineSetup<TInput, TOutput> BulkStage<TInput, TOutput>(IBulkStage<TInput, TOutput> bulkStage)
            => CreateBulkStage<TInput, TOutput>(bulkStage);

        public IPipelineSetup<TInput, TOutput> BulkStage<TInput, TOutput>(Func<TInput[], IEnumerable<TOutput>> func, BulkStageConfiguration bulkStageConfiguration)
            => BulkStage(new LambdaBulkStage<TInput, TOutput>(func, bulkStageConfiguration));

        public IPipelineSetup<TInput, TOutput> BulkStage<TInput, TOutput>(Func<TInput[], Task<IEnumerable<TOutput>>> func, BulkStageConfiguration bulkStageConfiguration)
            => BulkStage(new LambdaBulkStage<TInput, TOutput>(func, bulkStageConfiguration));

        #endregion

        #region Stages

        public IPipelineSetup<TInput, TOutput> Stage<TInput, TOutput>(IStage<TInput, TOutput> stage)
            => CreateStage<TInput, TOutput>(stage);

        public IPipelineSetup<TInput, TOutput> Stage<TInput, TOutput>(Func<TInput, TOutput> func)
            => Stage(new LambdaStage<TInput, TOutput>(func));

        public IPipelineSetup<TInput, TOutput> Stage<TInput, TOutput>(Func<TInput, StageOption<TInput, TOutput>, TOutput> funcWithOption)
            => Stage(new LambdaStage<TInput, TOutput>(funcWithOption));

        public IPipelineSetup<TInput, TOutput> Stage<TInput, TOutput>(Func<TInput, Task<TOutput>> func)
            => Stage(new LambdaStage<TInput, TOutput>(func));

        public IPipelineSetup<TInput, TOutput> Stage<TInput, TOutput>(Func<TInput, StageOption<TInput, TOutput>, Task<TOutput>> funcWithOption)
            => Stage(new LambdaStage<TInput, TOutput>(funcWithOption));

        #endregion

        #endregion

        private PipelineSetup<TInput, TOutput> CreateBulkStage<TInput, TOutput>(IBulkStage<TInput, TOutput> bulkStage)
        {
            IPropagatorBlock<PipelineStageItem<TInput>, PipelineStageItem<TOutput>> CreateExecutionBlock(StageCreationContext stageCreationContext)
            {
                var pipelineBulkStage = new PipelineBulkStage<TInput, TOutput>(bulkStage);
                TransformManyBlock<IEnumerable<PipelineStageItem<TInput>>, PipelineStageItem<TOutput>> nextBlock = null;

                IPropagatorBlock<PipelineStageItem<TInput>, PipelineStageItem<TInput>[]> batchPrepareBlock;
                if (stageCreationContext.UseTimeOut)
                {
                    batchPrepareBlock = new BatchBlockWithTimeOut<PipelineStageItem<TInput>>(pipelineBulkStage.Configuration.BatchSize, pipelineBulkStage.Configuration.BatchTimeOut);
                }
                else
                {
                    batchPrepareBlock = new BatchBlock<PipelineStageItem<TInput>>(pipelineBulkStage.Configuration.BatchSize);
                }

                Action<IEnumerable<PipelineStageItem<TInput>>> postItem;
                if (stageCreationContext.PipelineType == PipelineType.Normal)
                {
                    postItem = items => nextBlock?.Post(items);
                }
                else
                {
                    postItem = items => throw new PipelineUsageException(Helpers.Strings.RetryOnAwaitable);
                }

                nextBlock = new TransformManyBlock<IEnumerable<PipelineStageItem<TInput>>, PipelineStageItem<TOutput>>(
                    async items => await pipelineBulkStage.BaseExecute(items, stageCreationContext.GetPipelineStageContext(() => postItem(items))),
                    new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = pipelineBulkStage.Configuration.MaxDegreeOfParallelism,
                        MaxMessagesPerTask = pipelineBulkStage.Configuration.MaxMessagesPerTask,
                        CancellationToken = stageCreationContext.CancellationToken,
                        SingleProducerConstrained = pipelineBulkStage.Configuration.SingleProducerConstrained,
                        EnsureOrdered = pipelineBulkStage.Configuration.EnsureOrdered
                    });

                batchPrepareBlock.LinkTo(nextBlock, new DataflowLinkOptions { PropagateCompletion = false });
                batchPrepareBlock.Completion.ContinueWith(x => nextBlock.Complete());

                return DataflowBlock.Encapsulate(batchPrepareBlock, nextBlock);
            }

            return CreatePipelineSetup(CreateExecutionBlock);
        }

        private PipelineSetup<TInput, TOutput> CreateStage<TInput, TOutput>(IStage<TInput, TOutput> stage)
        {
            IPropagatorBlock<PipelineStageItem<TInput>, PipelineStageItem<TOutput>> CreateExecutionBlock(StageCreationContext stageCreationContext)
            {
                var pipelineStage = new PipelineStage<TInput, TOutput>(stage);

                TransformBlock<PipelineStageItem<TInput>, PipelineStageItem<TOutput>> nextExecutionBlock = null;

                Action<PipelineStageItem<TInput>> postItem;

                if (stageCreationContext.PipelineType == PipelineType.Normal)
                {
                    postItem = item => nextExecutionBlock?.Post(item);
                }
                else
                {
                    postItem = item => throw new PipelineUsageException(Helpers.Strings.RetryOnAwaitable);
                }

                nextExecutionBlock = new TransformBlock<PipelineStageItem<TInput>, PipelineStageItem<TOutput>>(
                    async item =>
                    {
                        return await pipelineStage.BaseExecute(item, stageCreationContext.GetPipelineStageContext(() => postItem(item)));
                    },
                    new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = pipelineStage.Configuration.MaxDegreeOfParallelism,
                        MaxMessagesPerTask = pipelineStage.Configuration.MaxMessagesPerTask,
                        CancellationToken = stageCreationContext.CancellationToken,
                        SingleProducerConstrained = pipelineStage.Configuration.SingleProducerConstrained,
                        EnsureOrdered = pipelineStage.Configuration.EnsureOrdered
                    });

                return nextExecutionBlock;
            }

            return CreatePipelineSetup(CreateExecutionBlock);
        }

        private PipelineSetup<TInput, TOutput> CreatePipelineSetup<TInput, TOutput>(Func<StageCreationContext, IPropagatorBlock<PipelineStageItem<TInput>, PipelineStageItem<TOutput>>> executionBlockCreator)
            => AppendStage(new StageSetup<TInput, TOutput>(executionBlockCreator, null) { PreviousStageSetup = null });

        private PipelineSetup<TInput, TOutput> AppendStage<TInput, TOutput>(IStageSetup<TInput, TOutput> stageSetup)
            => new PipelineSetup<TInput, TOutput>(stageSetup, _pipelineCreationContext);
    }
}
