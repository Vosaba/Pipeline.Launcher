using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineStage;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.Abstractions.Services;
using PipelineLauncher.Abstractions.Stages;
using PipelineLauncher.Blocks;
using PipelineLauncher.PipelineSetup;
using PipelineLauncher.PipelineStage;
using PipelineLauncher.Stages;
using PipelineLauncher.StageSetup;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.PipelineEvents;
using PipelineLauncher.Exceptions;

namespace PipelineLauncher
{
    public class PipelineCreator : IPipelineCreator
    {
        private readonly PipelineSetupContext _pipelineSetupContext;

        public PipelineCreator()
        {
            _pipelineSetupContext = new PipelineSetupContext();
        }

        public PipelineCreator(IStageService stageService)
        {
            _pipelineSetupContext = new PipelineSetupContext(stageService);
        }

        public PipelineCreator(Func<Type, IStage> stageResolveFunc)
        {
            _pipelineSetupContext = new PipelineSetupContext(stageResolveFunc);
        }

        public IPipelineCreator WithCancellationToken(CancellationToken cancellationToken)
        {
            _pipelineSetupContext.SetupCancellationToken(cancellationToken);
            return this;
        }

        public IPipelineCreator WithStageService(IStageService stageService)
        {
            _pipelineSetupContext.SetupStageService(stageService);
            return this;
        }

        public IPipelineCreator WithStageService(Func<Type, IStage> stageService)
        {
            _pipelineSetupContext.SetupStageService(stageService);
            return this;
        }

        public IPipelineCreator WithDiagnostic(Action<DiagnosticItem> diagnosticHandler)
        {
            _pipelineSetupContext.SetupDiagnosticAction(diagnosticHandler);
            return this;
        }

        public IPipelineCreator WithExceptionHandler(Action<ExceptionItemsEventArgs> exceptionHandler)
        {
            _pipelineSetupContext.SetupExceptionHandler(exceptionHandler);
            return this;
        }

        public IPipelineSetup<TInput, TInput> Prepare<TInput>()
            => Stage<TInput, TInput>(x => x);

        public IPipelineSetup<TInput, TInput> BulkPrepare<TInput>(BulkStageConfiguration stageConfiguration)
            => BulkStage<TInput, TInput>(x => x, stageConfiguration);

        #region Generic Stages

        #region BulkStages

        public IPipelineSetup<TInput, TOutput> BulkStage<TBulkStage, TInput, TOutput>()
            where TBulkStage : BulkStage<TInput, TOutput>
            => CreateNextBulkStage<TInput, TOutput>(_pipelineSetupContext.StageService.GetStageInstance<TBulkStage>());

        public IPipelineSetup<TInput, TInput> BulkStage<TBulkStage, TInput>()
            where TBulkStage : BulkStage<TInput, TInput>
            => CreateNextBulkStage<TInput, TInput>(_pipelineSetupContext.StageService.GetStageInstance<TBulkStage>());

        #endregion

        #region Stages

        public IPipelineSetup<TInput, TOutput> Stage<TStage, TInput, TOutput>()
            where TStage : Stages.Stage<TInput, TOutput>
            => CreateNextStage<TInput, TOutput>(_pipelineSetupContext.StageService.GetStageInstance<TStage>());

        public IPipelineSetup<TInput, TInput> Stage<TStage, TInput>()
            where TStage : Stages.Stage<TInput, TInput>
            => CreateNextStage<TInput, TInput>(_pipelineSetupContext.StageService.GetStageInstance<TStage>());

        #endregion

        #endregion

        #region Nongeneric

        #region BulkStages

        public IPipelineSetup<TInput, TOutput> BulkStage<TInput, TOutput>(BulkStage<TInput, TOutput> bulkStage)
            => CreateNextBulkStage<TInput, TOutput>(bulkStage);

        public IPipelineSetup<TInput, TOutput> BulkStage<TInput, TOutput>(Func<IEnumerable<TInput>, IEnumerable<TOutput>> bulkFunc, BulkStageConfiguration bulkStageConfiguration)
            => BulkStage(new LambdaBulkStage<TInput, TOutput>(bulkFunc, bulkStageConfiguration));

        public IPipelineSetup<TInput, TOutput> BulkStage<TInput, TOutput>(Func<IEnumerable<TInput>, Task<IEnumerable<TOutput>>> bulkFunc, BulkStageConfiguration bulkStageConfiguration)
            => BulkStage(new LambdaBulkStage<TInput, TOutput>(bulkFunc, bulkStageConfiguration));

        #endregion

        #region Stages

        public IPipelineSetup<TInput, TOutput> Stage<TInput, TOutput>(Stages.Stage<TInput, TOutput> stage)
            => CreateNextStage<TInput, TOutput>(stage);

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

        private PipelineSetup<TInput, TOutput> CreateNextBulkStage<TInput, TOutput>(PipelineBulkStage<TInput, TOutput> bulkStage)
        {
            IPropagatorBlock<PipelineStageItem<TInput>, PipelineStageItem<TOutput>> MakeNextBlock(StageCreationOptions options)
            {
                IPropagatorBlock<PipelineStageItem<TInput>, PipelineStageItem<TInput>[]> buffer;
                if (options.UseTimeOuts)
                {
                    buffer = new BatchBlockEx<PipelineStageItem<TInput>>(bulkStage.Configuration.BatchItemsCount, bulkStage.Configuration.BatchItemsTimeOut); //TODO
                }
                else
                {
                    buffer = new BatchBlock<PipelineStageItem<TInput>>(bulkStage.Configuration.BatchItemsCount); //TODO
                }

                TransformManyBlock<IEnumerable<PipelineStageItem<TInput>>, PipelineStageItem<TOutput>> rePostBlock = null;

                Action<IEnumerable<PipelineStageItem<TInput>>> RePostMessages;
                if (options.PipelineType == PipelineType.Normal)
                {
                    RePostMessages = items =>  rePostBlock?.Post(items);
                }
                else
                {
                    RePostMessages = items => throw new PipelineUsageException(Helpers.Strings.RetryOnAwaitable);
                }

                var nextBlock = new TransformManyBlock<IEnumerable<PipelineStageItem<TInput>>, PipelineStageItem<TOutput>>(
                    async delegate(IEnumerable<PipelineStageItem<TInput>> items)
                    {
                        return await bulkStage.InternalExecute(items, _pipelineSetupContext.GetPipelineStageContext(() => RePostMessages(items)));
                    },
                    new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = bulkStage.Configuration.MaxDegreeOfParallelism,
                        MaxMessagesPerTask = bulkStage.Configuration.MaxMessagesPerTask,
                        CancellationToken = _pipelineSetupContext.CancellationToken,
                        SingleProducerConstrained = bulkStage.Configuration.SingleProducerConstrained
                    });

                buffer.LinkTo(nextBlock, new DataflowLinkOptions() { PropagateCompletion = false });
                rePostBlock = nextBlock;

                buffer.Completion.ContinueWith(x => 
                { 
                       nextBlock.Complete(); 
                });//, _pipelineSetupContext.CancellationToken);

                return DataflowBlock.Encapsulate(buffer, nextBlock);
            }

            return CreateNextBlock(MakeNextBlock, bulkStage.Configuration);
        }

        private PipelineSetup<TInput, TOutput> CreateNextStage<TInput, TOutput>(PipelineStage<TInput, TOutput> stage)
        {
            IPropagatorBlock<PipelineStageItem<TInput>, PipelineStageItem<TOutput>> MakeNextBlock(StageCreationOptions options)
            {
                TransformBlock<PipelineStageItem<TInput>, PipelineStageItem<TOutput>> rePostBlock = null;

                Action<PipelineStageItem<TInput>> RePostMessage;
                if (options.PipelineType == PipelineType.Normal)
                {
                    RePostMessage = item => rePostBlock?.Post(item);
                }
                else
                {
                    RePostMessage = item => throw new PipelineUsageException(Helpers.Strings.RetryOnAwaitable);
                }

                var nextBlock = new TransformBlock<PipelineStageItem<TInput>, PipelineStageItem<TOutput>>(
                    async item =>
                    {
                        return await stage.InternalExecute(item, _pipelineSetupContext.GetPipelineStageContext(() => RePostMessage(item)));
                    },
                    new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = stage.Configuration.MaxDegreeOfParallelism,
                        MaxMessagesPerTask = stage.Configuration.MaxMessagesPerTask,
                        CancellationToken = _pipelineSetupContext.CancellationToken,
                        SingleProducerConstrained = stage.Configuration.SingleProducerConstrained
                    });

                rePostBlock = nextBlock;

                return nextBlock;
            }

            return CreateNextBlock(MakeNextBlock, stage.Configuration);
        }

        private PipelineSetup<TInput, TOutput> CreateNextBlock<TInput, TOutput>(Func<StageCreationOptions, IPropagatorBlock<PipelineStageItem<TInput>, PipelineStageItem<TOutput>>> executionBlock, StageBaseConfiguration stageConfiguration)
        {
            return AppendStage(
                new StageSetup<TInput, TOutput>(executionBlock)
                {
                    Previous = null,
                    PipelineBaseConfiguration = stageConfiguration
                });
        }

        private PipelineSetup<TInput, TOutput> AppendStage<TInput, TOutput>(IStageSetup<TInput, TOutput> stageSetup)
        {
            return new PipelineSetup<TInput, TOutput>(
                stageSetup, _pipelineSetupContext);
        }
    }
}
