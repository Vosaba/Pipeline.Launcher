﻿using PipelineLauncher.Abstractions.Dto;
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
        private readonly PipelineCreationContext _pipelineCreationContext;

        public PipelineCreator()
        {
            _pipelineCreationContext = new PipelineCreationContext();
        }

        public PipelineCreator(IStageService stageService)
        {
            _pipelineCreationContext = new PipelineCreationContext(stageService);
        }

        public PipelineCreator(Func<Type, IPipelineStage> stageResolveFunc)
        {
            _pipelineCreationContext = new PipelineCreationContext(stageResolveFunc);
        }

        //public IPipelineCreator WithCancellationToken(CancellationToken cancellationToken)
        //{
        //    _pipelineCreationContext.SetupCancellationToken(cancellationToken);
        //    return this;
        //}

        public IPipelineCreator WithStageService(IStageService stageService)
        {
            _pipelineCreationContext.SetupStageService(stageService);
            return this;
        }

        public IPipelineCreator WithStageService(Func<Type, IPipelineStage> stageService)
        {
            _pipelineCreationContext.SetupStageService(stageService);
            return this;
        }

        //public IPipelineCreator WithDiagnostic(Action<DiagnosticItem> diagnosticHandler)
        //{
        //    _pipelineCreationContext.SetupDiagnosticAction(diagnosticHandler);
        //    return this;
        //}

        //public IPipelineCreator WithExceptionHandler(Action<ExceptionItemsEventArgs> exceptionHandler)
        //{
        //    _pipelineCreationContext.SetupExceptionHandler(exceptionHandler);
        //    return this;
        //}

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
            where TBulkStage : BulkStage<TInput, TOutput>
            => CreateNextBulkStage<TInput, TOutput>(_pipelineCreationContext.StageService.GetStageInstance<TBulkStage>());

        public IPipelineSetup<TInput, TInput> BulkStage<TBulkStage, TInput>()
            where TBulkStage : BulkStage<TInput, TInput>
            => CreateNextBulkStage<TInput, TInput>(_pipelineCreationContext.StageService.GetStageInstance<TBulkStage>());

        #endregion

        #region Stages

        public IPipelineSetup<TInput, TOutput> Stage<TStage, TInput, TOutput>()
            where TStage : Stages.Stage<TInput, TOutput>
            => CreateNextStage<TInput, TOutput>(_pipelineCreationContext.StageService.GetStageInstance<TStage>());

        public IPipelineSetup<TInput, TInput> Stage<TStage, TInput>()
            where TStage : Stages.Stage<TInput, TInput>
            => CreateNextStage<TInput, TInput>(_pipelineCreationContext.StageService.GetStageInstance<TStage>());

        #endregion

        #endregion

        #region Nongeneric

        #region BulkStages

        public IPipelineSetup<TInput, TOutput> BulkStage<TInput, TOutput>(BulkStage<TInput, TOutput> bulkStage)
            => CreateNextBulkStage<TInput, TOutput>(bulkStage);

        public IPipelineSetup<TInput, TOutput> BulkStage<TInput, TOutput>(Func<TInput[], IEnumerable<TOutput>> bulkFunc, BulkStageConfiguration bulkStageConfiguration)
            => BulkStage(new LambdaBulkStage<TInput, TOutput>(bulkFunc, bulkStageConfiguration));

        public IPipelineSetup<TInput, TOutput> BulkStage<TInput, TOutput>(Func<TInput[], Task<IEnumerable<TOutput>>> bulkFunc, BulkStageConfiguration bulkStageConfiguration)
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

        private PipelineSetup<TInput, TOutput> CreateNextBulkStage<TInput, TOutput>(IBulkStage<TInput, TOutput> bulkStageA)
        {
            IPropagatorBlock<PipelineStageItem<TInput>, PipelineStageItem<TOutput>> MakeNextBlock(StageCreationContext options)
            {
                var bulkStage = new PipelineBulkStage<TInput, TOutput>(bulkStageA);

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
                    RePostMessages = items => rePostBlock?.Post(items);
                }
                else
                {
                    RePostMessages = items => throw new PipelineUsageException(Helpers.Strings.RetryOnAwaitable);
                }

                var nextBlock = new TransformManyBlock<IEnumerable<PipelineStageItem<TInput>>, PipelineStageItem<TOutput>>(
                    async delegate (IEnumerable<PipelineStageItem<TInput>> items)
                    {
                        return await bulkStage.BaseExecute(items, options.GetPipelineStageContext(() => RePostMessages(items)));
                    },
                    new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = bulkStage.Configuration.MaxDegreeOfParallelism,
                        MaxMessagesPerTask = bulkStage.Configuration.MaxMessagesPerTask,
                        CancellationToken = options.CancellationToken,
                        SingleProducerConstrained = bulkStage.Configuration.SingleProducerConstrained,
                        EnsureOrdered = bulkStage.Configuration.EnsureOrdered
                    });

                buffer.LinkTo(nextBlock, new DataflowLinkOptions() { PropagateCompletion = false });
                rePostBlock = nextBlock;

                buffer.Completion.ContinueWith(x =>
                {
                    nextBlock.Complete();
                });//, _pipelineCreationContext.CancellationToken);

                return DataflowBlock.Encapsulate(buffer, nextBlock);
            }

            return CreateNextBlock(MakeNextBlock);
        }

        private PipelineSetup<TInput, TOutput> CreateNextStage<TInput, TOutput>(IStage<TInput, TOutput> stageA)
        {
            IPropagatorBlock<PipelineStageItem<TInput>, PipelineStageItem<TOutput>> MakeNextBlock(StageCreationContext options)
            {
                var stage = new PipelineStage<TInput, TOutput>(stageA);

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
                        return await stage.BaseExecute(item, options.GetPipelineStageContext(() => RePostMessage(item)));
                    },
                    new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = stage.Configuration.MaxDegreeOfParallelism,
                        MaxMessagesPerTask = stage.Configuration.MaxMessagesPerTask,
                        CancellationToken = options.CancellationToken,
                        SingleProducerConstrained = stage.Configuration.SingleProducerConstrained,
                        EnsureOrdered = stage.Configuration.EnsureOrdered
                    });

                rePostBlock = nextBlock;

                return nextBlock;
            }

            return CreateNextBlock(MakeNextBlock);
        }

        private PipelineSetup<TInput, TOutput> CreateNextBlock<TInput, TOutput>(
            Func<StageCreationContext, IPropagatorBlock<PipelineStageItem<TInput>, PipelineStageItem<TOutput>>> executionBlock)
        {
            return AppendStage(new StageSetup<TInput, TOutput>(executionBlock)
            {
                PreviousStageSetup = null,
            });
        }

        private PipelineSetup<TInput, TOutput> AppendStage<TInput, TOutput>(IStageSetup<TInput, TOutput> stageSetup)
        {
            return new PipelineSetup<TInput, TOutput>(stageSetup, _pipelineCreationContext);
        }
    }
}
