using PipelineLauncher.Abstractions.Configurations;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.Services;
using PipelineLauncher.Blocks;
using PipelineLauncher.Dto;
using PipelineLauncher.Jobs;
using PipelineLauncher.PipelineJobs;
using PipelineLauncher.PipelineSetup;
using PipelineLauncher.Stage;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace PipelineLauncher.Pipelines
{
    public class PipelineCreator : IPipelineCreator
    {
        private readonly IJobService _jobService;
        private CancellationToken _cancellationToken = default;

        private IJobService GetJobService
        {
            get
            {
                if (_jobService == null)
                {
                    throw new Exception($"'{nameof(IJobService)}' isn't provided, if you need to use Generic stage setups, provide service.");
                }

                return _jobService;
            }
        }

        public PipelineCreator(IJobService jobService = null)
        {
            _jobService = jobService;
        }

        public IPipelineCreator WithToken(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            return this;
        }

        public IPipelineSetup<TInput, TInput> Prepare<TInput>()
            => Stage<TInput, TInput>(x => x);

        public IPipelineSetup<TInput, TInput> BulkPrepare<TInput>(BulkJobConfiguration jobConfiguration)
            => BulkStage<TInput, TInput>(x => x, jobConfiguration);

        #region Generic Stages

        #region BulkStages

        public IPipelineSetup<TInput, TOutput> BulkStage<TBulkJob, TInput, TOutput>()
            where TBulkJob : Bulk<TInput, TOutput>
            => CreateNextBulkStage<TInput, TOutput>(GetJobService.GetJobInstance<TBulkJob>());

        public IPipelineSetup<TInput, TInput> BulkStage<TBulkJob, TInput>()
            where TBulkJob : Bulk<TInput, TInput>
            => CreateNextBulkStage<TInput, TInput>(GetJobService.GetJobInstance<TBulkJob>());

        #endregion

        #region Stages

        public IPipelineSetup<TInput, TOutput> Stage<TJob, TInput, TOutput>()
            where TJob : Job<TInput, TOutput>
            => CreateNextStage<TInput, TOutput>(GetJobService.GetJobInstance<TJob>());

        public IPipelineSetup<TInput, TInput> Stage<TJob, TInput>()
            where TJob : Job<TInput, TInput>
            => CreateNextStage<TInput, TInput>(GetJobService.GetJobInstance<TJob>());

        #endregion

        #endregion

        #region Nongeneric

        #region BulkStages

        public IPipelineSetup<TInput, TOutput> BulkStage<TInput, TOutput>(Bulk<TInput, TOutput> bulk)
            => CreateNextBulkStage<TInput, TOutput>(bulk);

        public IPipelineSetup<TInput, TOutput> BulkStage<TInput, TOutput>(Func<IEnumerable<TInput>, IEnumerable<TOutput>> bulkFunc, BulkJobConfiguration bulkJobConfiguration)
            => BulkStage(new LambdaBulk<TInput, TOutput>(bulkFunc, bulkJobConfiguration));

        public IPipelineSetup<TInput, TOutput> BulkStage<TInput, TOutput>(Func<IEnumerable<TInput>, Task<IEnumerable<TOutput>>> bulkFunc, BulkJobConfiguration bulkJobConfiguration)
            => BulkStage(new LambdaBulk<TInput, TOutput>(bulkFunc, bulkJobConfiguration));

        #endregion

        #region Stages

        public IPipelineSetup<TInput, TOutput> Stage<TInput, TOutput>(Job<TInput, TOutput> job)
            => CreateNextStage<TInput, TOutput>(job);

        public IPipelineSetup<TInput, TOutput> Stage<TInput, TOutput>(Func<TInput, TOutput> func)
            => Stage(new LambdaJob<TInput, TOutput>(func));

        public IPipelineSetup<TInput, TOutput> Stage<TInput, TOutput>(Func<TInput, StageOption<TInput, TOutput>, TOutput> funcWithOption)
            => Stage(new LambdaJob<TInput, TOutput>(funcWithOption));

        public IPipelineSetup<TInput, TOutput> Stage<TInput, TOutput>(Func<TInput, Task<TOutput>> func)
            => Stage(new LambdaJob<TInput, TOutput>(func));

        public IPipelineSetup<TInput, TOutput> Stage<TInput, TOutput>(Func<TInput, StageOption<TInput, TOutput>, Task<TOutput>> funcWithOption)
            => Stage(new LambdaJob<TInput, TOutput>(funcWithOption));

        #endregion

        #endregion

        private PipelineSetup<TInput, TOutput> CreateNextBulkStage<TInput, TOutput>(PipelineBulk<TInput, TOutput> bulkJob)
        {
            IPropagatorBlock<PipelineItem<TInput>, PipelineItem<TOutput>> MakeNextBlock()
            {

                var buffer = new BatchBlockEx<PipelineItem<TInput>>(bulkJob.Configuration.BatchItemsCount, bulkJob.Configuration.BatchItemsTimeOut); //TODO

                TransformManyBlock<IEnumerable<PipelineItem<TInput>>, PipelineItem<TOutput>> rePostBlock = null;

                void RePostMessages(IEnumerable<PipelineItem<TInput>> messages)
                {
                    rePostBlock?.Post(messages);
                }

                var nextBlock = new TransformManyBlock<IEnumerable<PipelineItem<TInput>>, PipelineItem<TOutput>>(
                    async e => await bulkJob.InternalExecute(e, () => RePostMessages(e), _cancellationToken),
                    new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = bulkJob.Configuration.MaxDegreeOfParallelism,
                        MaxMessagesPerTask = bulkJob.Configuration.MaxMessagesPerTask,
                        CancellationToken = _cancellationToken
                    });

                buffer.LinkTo(nextBlock, new DataflowLinkOptions() { PropagateCompletion = true });
                rePostBlock = nextBlock;

                buffer.Completion.ContinueWith(x => { nextBlock.Complete(); }, _cancellationToken);

                return DataflowBlock.Encapsulate(buffer, nextBlock);
            }

            return CreateNextBlock(MakeNextBlock, bulkJob.Configuration);
        }

        private PipelineSetup<TInput, TOutput> CreateNextStage<TInput, TOutput>(Pipeline<TInput, TOutput> job)
        {
            IPropagatorBlock<PipelineItem<TInput>, PipelineItem<TOutput>> MakeNextBlock()
            {
                TransformBlock<PipelineItem<TInput>, PipelineItem<TOutput>> rePostBlock = null;
                void RePostMessage(PipelineItem<TInput> message)
                {
                    rePostBlock?.Post(message);
                }

                var nextBlock = new TransformBlock<PipelineItem<TInput>, PipelineItem<TOutput>>(
                    async e => await job.InternalExecute(e, () => RePostMessage(e), _cancellationToken),
                    new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = job.Configuration.MaxDegreeOfParallelism,
                        MaxMessagesPerTask = job.Configuration.MaxMessagesPerTask,
                        CancellationToken = _cancellationToken
                    });

                rePostBlock = nextBlock;

                return nextBlock;
            }

            return CreateNextBlock(MakeNextBlock, job.Configuration);
        }

        private PipelineSetup<TInput, TOutput> CreateNextBlock<TInput, TOutput>(Func<IPropagatorBlock<PipelineItem<TInput>, PipelineItem<TOutput>>> executionBlock, PipelineBaseConfiguration pipelineBaseConfiguration)
        {
            return AppendStage(
                new Stage<TInput, TOutput>(executionBlock, _cancellationToken)
                {
                    Previous = null,
                    PipelineBaseConfiguration = pipelineBaseConfiguration
                }); ;
        }

        private PipelineSetup<TInput, TOutput> AppendStage<TInput, TOutput>(IStage<TInput, TOutput> stage)
        {
            return new PipelineHeadSetup<TInput, TInput, TOutput>(stage, _jobService);
        }
    }
}
