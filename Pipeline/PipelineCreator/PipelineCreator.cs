using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Configurations;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.Pipeline;
using PipelineLauncher.Abstractions.PipelineEvents;
using PipelineLauncher.Abstractions.Services;
using PipelineLauncher.Blocks;
using PipelineLauncher.Dto;
using PipelineLauncher.PipelineSetup;
using PipelineLauncher.PipelineStage;
using PipelineLauncher.Stages;
using PipelineLauncher.StageSetup;

namespace PipelineLauncher
{
    public class PipelineCreator : IPipelineCreator
    {
        private readonly PipelineSetupContext _pipelineSetupContext;

        public PipelineCreator(IStageService stageService = null)
        {
            _pipelineSetupContext = new PipelineSetupContext(stageService);
        }

        public IPipelineCreator WithToken(CancellationToken cancellationToken)
        {
            _pipelineSetupContext.SetupCancellationToken(cancellationToken);
            return this;
        }

        public IPipelineCreator WithStageService(IStageService stageService)
        {
            _pipelineSetupContext.SetupStageService(stageService);
            return this;
        }

        public IPipelineCreator WithStageService(Func<Type, IPipeline> stageService)
        {
            _pipelineSetupContext.SetupStageService(stageService);
            return this;
        }

        public IPipelineCreator WithDiagnostic(Action<DiagnosticItem> diagnosticHandler)
        {
            _pipelineSetupContext.SetupDiagnosticAction(diagnosticHandler);
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
            IPropagatorBlock<PipelineItem<TInput>, PipelineItem<TOutput>> MakeNextBlock(StageCreationOptions options)
            {
                IPropagatorBlock<PipelineItem<TInput>, PipelineItem<TInput>[]> buffer;
                if (options.UseTimeOuts)
                {
                    buffer = new BatchBlockEx<PipelineItem<TInput>>(bulkStage.Configuration.BatchItemsCount, bulkStage.Configuration.BatchItemsTimeOut); //TODO
                }
                else
                {
                    buffer = new BatchBlock<PipelineItem<TInput>>(bulkStage.Configuration.BatchItemsCount); //TODO
                }

                TransformManyBlock<IEnumerable<PipelineItem<TInput>>, PipelineItem<TOutput>> rePostBlock = null;

                void RePostMessages(IEnumerable<PipelineItem<TInput>> messages)
                {
                    rePostBlock?.Post(messages);
                }

                var nextBlock = new TransformManyBlock<IEnumerable<PipelineItem<TInput>>, PipelineItem<TOutput>>(
                    async e => await bulkStage
                        .InternalExecute(e, _pipelineSetupContext.GetPipelineStageContext(() => RePostMessages(e))),
                    new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = bulkStage.Configuration.MaxDegreeOfParallelism,
                        MaxMessagesPerTask = bulkStage.Configuration.MaxMessagesPerTask,
                        CancellationToken = _pipelineSetupContext.CancellationToken
                    });

                buffer.LinkTo(nextBlock, new DataflowLinkOptions() { PropagateCompletion = true });
                rePostBlock = nextBlock;

                buffer.Completion.ContinueWith(x => { nextBlock.Complete(); }, _pipelineSetupContext.CancellationToken);

                return DataflowBlock.Encapsulate(buffer, nextBlock);
            }

            return CreateNextBlock(MakeNextBlock, bulkStage.Configuration);
        }

        private PipelineSetup<TInput, TOutput> CreateNextStage<TInput, TOutput>(PipelineStage<TInput, TOutput> stage)
        {
            IPropagatorBlock<PipelineItem<TInput>, PipelineItem<TOutput>> MakeNextBlock(StageCreationOptions options)
            {


                TransformBlock<PipelineItem<TInput>, PipelineItem<TOutput>> rePostBlock = null;
                void RePostMessage(PipelineItem<TInput> message)
                {
                    rePostBlock?.Post(message);
                }

                var nextBlock = new TransformBlock<PipelineItem<TInput>, PipelineItem<TOutput>>(
                    async e => await stage.InternalExecute(e, _pipelineSetupContext.GetPipelineStageContext(() => RePostMessage(e))),
                    new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = stage.Configuration.MaxDegreeOfParallelism,
                        MaxMessagesPerTask = stage.Configuration.MaxMessagesPerTask,
                        CancellationToken = _pipelineSetupContext.CancellationToken
                    });

                rePostBlock = nextBlock;

                return nextBlock;
            }

            return CreateNextBlock(MakeNextBlock, stage.Configuration);
        }

        private PipelineSetup<TInput, TOutput> CreateNextBlock<TInput, TOutput>(Func<StageCreationOptions, IPropagatorBlock<PipelineItem<TInput>, PipelineItem<TOutput>>> executionBlock, PipelineBaseConfiguration pipelineBaseConfiguration)
        {
            return AppendStage(
                new StageSetup<TInput, TOutput>(executionBlock)
                {
                    Previous = null,
                    PipelineBaseConfiguration = pipelineBaseConfiguration
                }); ;
        }

        private PipelineSetup<TInput, TOutput> AppendStage<TInput, TOutput>(IStageSetup<TInput, TOutput> stageSetup)
        {
            return new PipelineSetup<TInput, TOutput>(
                stageSetup, _pipelineSetupContext);
        }
    }
}
