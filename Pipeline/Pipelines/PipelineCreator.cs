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
        private readonly IJobService _BulkJobService;
        private CancellationToken _cancellationToken = default;

        private IJobService GetBulkJobService
        {
            get
            {
                if (_BulkJobService == null)
                {
                    throw new Exception($"'{nameof(IJobService)}' isn't provided, if you need to use Generic stage setups, provide service.");
                }

                return _BulkJobService;
            }
        }

        public PipelineCreator(IJobService BulkJobService)
        {
            _BulkJobService = BulkJobService;
        }

        public IPipelineCreator WithToken(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            return this;
        }

        #region Generic Stages

        public IPipelineSetup<TInput, TOutput> BulkStage<TBulkJob, TInput, TOutput>()
            where TBulkJob : BulkJob<TInput, TOutput>
            => CreateNextStage<TInput, TOutput>(GetBulkJobService.GetJobInstance<TBulkJob>());

        public IPipelineSetup<TInput, TInput> BulkStage<TBulkJob, TInput>()
            where TBulkJob : BulkJob<TInput, TInput>
            => CreateNextStage<TInput, TInput>(GetBulkJobService.GetJobInstance<TBulkJob>());

        public IPipelineSetup<TInput, TOutput> BulkStage<TBulkJob, TBulkJob2, TInput, TOutput>()
            where TBulkJob : BulkJob<TInput, TOutput>
            where TBulkJob2 : BulkJob<TInput, TOutput>
            => BulkStage(GetBulkJobService.GetJobInstance<TBulkJob>(), GetBulkJobService.GetJobInstance<TBulkJob2>());

        public IPipelineSetup<TInput, TOutput> BulkStage<TBulkJob, TBulkJob2, TBulkJob3, TInput, TOutput>()
            where TBulkJob : BulkJob<TInput, TOutput>
            where TBulkJob2 : BulkJob<TInput, TOutput>
            where TBulkJob3 : BulkJob<TInput, TOutput>
            => BulkStage(GetBulkJobService.GetJobInstance<TBulkJob>(), GetBulkJobService.GetJobInstance<TBulkJob2>(), GetBulkJobService.GetJobInstance<TBulkJob3>());

        public IPipelineSetup<TInput, TOutput> BulkStage<TBulkJob, TBulkJob2, TBulkJob3, TBulkJob4, TInput, TOutput>()
            where TBulkJob : BulkJob<TInput, TOutput>
            where TBulkJob2 : BulkJob<TInput, TOutput>
            where TBulkJob3 : BulkJob<TInput, TOutput>
            where TBulkJob4 : BulkJob<TInput, TOutput>
            => BulkStage(GetBulkJobService.GetJobInstance<TBulkJob>(), GetBulkJobService.GetJobInstance<TBulkJob2>(), GetBulkJobService.GetJobInstance<TBulkJob3>(), GetBulkJobService.GetJobInstance<TBulkJob4>());

        public IPipelineSetup<TInput, TOutput> Stage<TJob, TInput, TOutput>()
            where TJob : Job<TInput, TOutput>
            => CreateNextStageAsync<TInput, TOutput>(GetBulkJobService.GetJobInstance<TJob>());

        public IPipelineSetup<TInput, TInput> Stage<TJob, TInput>()
            where TJob : Job<TInput, TInput>
            => CreateNextStageAsync<TInput, TInput>(GetBulkJobService.GetJobInstance<TJob>());

        public IPipelineSetup<TInput, TOutput> Stage<TJob, TJob2, TInput, TOutput>()
            where TJob : Job<TInput, TOutput>
            where TJob2 : Job<TInput, TOutput>
            => Stage(GetBulkJobService.GetJobInstance<TJob>(), GetBulkJobService.GetJobInstance<TJob2>());

        public IPipelineSetup<TInput, TOutput> Stage<TJob, TJob2, TJob3, TInput, TOutput>()
            where TJob : Job<TInput, TOutput>
            where TJob2 : Job<TInput, TOutput>
            where TJob3 : Job<TInput, TOutput>
            => Stage(GetBulkJobService.GetJobInstance<TJob>(), GetBulkJobService.GetJobInstance<TJob2>(), GetBulkJobService.GetJobInstance<TJob3>());

        public IPipelineSetup<TInput, TOutput> Stage<TJob, TJob2, TJob3, TJob4, TInput, TOutput>()
            where TJob : Job<TInput, TOutput>
            where TJob2 : Job<TInput, TOutput>
            where TJob3 : Job<TInput, TOutput>
            where TJob4 : Job<TInput, TOutput>
            => Stage(GetBulkJobService.GetJobInstance<TJob>(), GetBulkJobService.GetJobInstance<TJob2>(), GetBulkJobService.GetJobInstance<TJob3>(), GetBulkJobService.GetJobInstance<TJob4>());

        #endregion

        #region Nongeneric Stages

        public IPipelineSetup<TInput, TOutput> BulkStage<TInput, TOutput>(BulkJob<TInput, TOutput> bulkJob)
            => CreateNextStage<TInput, TOutput>(bulkJob);

        public IPipelineSetup<TInput, TOutput> BulkStage<TInput, TOutput>(Func<IEnumerable<TInput>, IEnumerable<TOutput>> bulkFunc, BulkJobConfiguration bulkJobConfiguration)
            => BulkStage(new LambdaBulkJob<TInput, TOutput>(bulkFunc, bulkJobConfiguration));

        public IPipelineSetup<TInput, TOutput> BulkStage<TInput, TOutput>(Func<IEnumerable<TInput>, Task<IEnumerable<TOutput>>> bulkFunc, BulkJobConfiguration bulkJobConfiguration)
            => BulkStage(new LambdaBulkJob<TInput, TOutput>(bulkFunc, bulkJobConfiguration));

        public IPipelineSetup<TInput, TOutput> BulkStage<TInput, TOutput>(params BulkJob<TInput, TOutput>[] bulkJobs)
            => BulkStage(new ConditionBulkJob<TInput, TOutput>(bulkJobs));

        public IPipelineSetup<TInput, TOutput> Stage<TInput, TOutput>(Job<TInput, TOutput> job)
            => CreateNextStageAsync<TInput, TOutput>(job);

        public IPipelineSetup<TInput, TOutput> Stage<TInput, TOutput>(Func<TInput, TOutput> func)
            => Stage(new LambdaJob<TInput, TOutput>(func));

        public IPipelineSetup<TInput, TOutput> Stage<TInput, TOutput>(Func<TInput, StageOption<TInput, TOutput>, TOutput> funcWithOption)
            => Stage(new LambdaJob<TInput, TOutput>(funcWithOption));


        public IPipelineSetup<TInput, TOutput> Stage<TInput, TOutput>(Func<TInput, Task<TOutput>> func)
            => Stage(new LambdaJob<TInput, TOutput>(func));

        public IPipelineSetup<TInput, TOutput> Stage<TInput, TOutput>(Func<TInput, StageOption<TInput, TOutput>, Task<TOutput>> funcWithOption)
            => Stage(new LambdaJob<TInput, TOutput>(funcWithOption));


        public IPipelineSetup<TInput, TOutput> Stage<TInput, TOutput>(params Job<TInput, TOutput>[] jobs)
            => Stage(new ConditionJob<TInput, TOutput>(jobs));

        #endregion

        private PipelineSetup<TInput, TOutput> CreateNextStage<TInput, TOutput>(PipelineBulkJob<TInput, TOutput> BulkJob)
        {
            IPropagatorBlock<PipelineItem<TInput>, PipelineItem<TOutput>> MakeNextBlock()
            {

                var buffer = new BatchBlockEx<PipelineItem<TInput>>(BulkJob.Configuration.BatchItemsCount, BulkJob.Configuration.BatchItemsTimeOut); //TODO

                TransformManyBlock<IEnumerable<PipelineItem<TInput>>, PipelineItem<TOutput>> rePostBlock = null;

                void RePostMessages(IEnumerable<PipelineItem<TInput>> messages)
                {
                    rePostBlock?.Post(messages);
                }

                var nextBlock = new TransformManyBlock<IEnumerable<PipelineItem<TInput>>, PipelineItem<TOutput>>(
                    async e => await BulkJob.InternalExecute(e, () => RePostMessages(e), _cancellationToken),
                    new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = BulkJob.Configuration.MaxDegreeOfParallelism,
                        MaxMessagesPerTask = BulkJob.Configuration.MaxMessagesPerTask,
                        CancellationToken = _cancellationToken
                    });

                buffer.LinkTo(nextBlock, new DataflowLinkOptions() { PropagateCompletion = true });
                rePostBlock = nextBlock;

                buffer.Completion.ContinueWith(x => { nextBlock.Complete(); }, _cancellationToken);

                return DataflowBlock.Encapsulate(buffer, nextBlock);
            }

            return CreateNextBlock(MakeNextBlock, BulkJob.Configuration);
        }

        private PipelineSetup<TInput, TOutput> CreateNextStageAsync<TInput, TOutput>(PipelineJob<TInput, TOutput> Job)
        {
            IPropagatorBlock<PipelineItem<TInput>, PipelineItem<TOutput>> MakeNextBlock()
            {
                TransformBlock<PipelineItem<TInput>, PipelineItem<TOutput>> rePostBlock = null;
                void RePostMessage(PipelineItem<TInput> message)
                {
                    rePostBlock?.Post(message);
                }

                var nextBlock = new TransformBlock<PipelineItem<TInput>, PipelineItem<TOutput>>(
                    async e => await Job.InternalExecute(e, () => RePostMessage(e), _cancellationToken),
                    new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = Job.Configuration.MaxDegreeOfParallelism,
                        MaxMessagesPerTask = Job.Configuration.MaxMessagesPerTask,
                        CancellationToken = _cancellationToken
                    });

                rePostBlock = nextBlock;

                return nextBlock;
            }

            return CreateNextBlock(MakeNextBlock, Job.Configuration);
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
            return new FirstStageSetup<TInput, TInput, TOutput>(stage, _BulkJobService);
        }
    }
}
