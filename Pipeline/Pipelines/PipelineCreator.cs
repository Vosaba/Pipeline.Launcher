using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.Pipeline;
using PipelineLauncher.Abstractions.Services;
using PipelineLauncher.Blocks;
using PipelineLauncher.Jobs;
using PipelineLauncher.PipelineJobs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Dto;
using PipelineLauncher.PipelineSetup;
using PipelineLauncher.Stage;

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

        public PipelineCreator(IJobService jobService)
        {
            _jobService = jobService;
        }

        public IPipelineCreator WithToken(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            return this;
        }

        #region Generic Stages

        public IPipelineSetup<TInput, TOutput> Stage<TJob, TInput, TOutput>()
            where TJob : Job<TInput, TOutput>
            => CreateNextStage<TInput, TOutput>(GetJobService.GetJobInstance<TJob>());

        public IPipelineSetup<TInput, TInput> Stage<TJob, TInput>()
            where TJob : Job<TInput, TInput>
            => CreateNextStage<TInput, TInput>(GetJobService.GetJobInstance<TJob>());

        public IPipelineSetup<TInput, TOutput> Stage<TJob, TJob2, TInput, TOutput>()
            where TJob : Job<TInput, TOutput>
            where TJob2 : Job<TInput, TOutput>
            => Stage(GetJobService.GetJobInstance<TJob>(), GetJobService.GetJobInstance<TJob2>());

        public IPipelineSetup<TInput, TOutput> Stage<TJob, TJob2, TJob3, TInput, TOutput>()
            where TJob : Job<TInput, TOutput>
            where TJob2 : Job<TInput, TOutput>
            where TJob3 : Job<TInput, TOutput>
            => Stage(GetJobService.GetJobInstance<TJob>(), GetJobService.GetJobInstance<TJob2>(), GetJobService.GetJobInstance<TJob3>());

        public IPipelineSetup<TInput, TOutput> Stage<TJob, TJob2, TJob3, TJob4, TInput, TOutput>()
            where TJob : Job<TInput, TOutput>
            where TJob2 : Job<TInput, TOutput>
            where TJob3 : Job<TInput, TOutput>
            where TJob4 : Job<TInput, TOutput>
            => Stage(GetJobService.GetJobInstance<TJob>(), GetJobService.GetJobInstance<TJob2>(), GetJobService.GetJobInstance<TJob3>(), GetJobService.GetJobInstance<TJob4>());

        public IPipelineSetup<TInput, TOutput> AsyncStage<TAsyncJob, TInput, TOutput>()
            where TAsyncJob : AsyncJob<TInput, TOutput>
            => CreateNextStageAsync<TInput, TOutput>(GetJobService.GetJobInstance<TAsyncJob>());

        public IPipelineSetup<TInput, TInput> AsyncStage<TAsyncJob, TInput>()
            where TAsyncJob : AsyncJob<TInput, TInput>
            => CreateNextStageAsync<TInput, TInput>(GetJobService.GetJobInstance<TAsyncJob>());

        public IPipelineSetup<TInput, TOutput> AsyncStage<TAsyncJob, TAsyncJob2, TInput, TOutput>()
            where TAsyncJob : AsyncJob<TInput, TOutput>
            where TAsyncJob2 : AsyncJob<TInput, TOutput>
            => AsyncStage(GetJobService.GetJobInstance<TAsyncJob>(), GetJobService.GetJobInstance<TAsyncJob2>());

        public IPipelineSetup<TInput, TOutput> AsyncStage<TAsyncJob, TAsyncJob2, TAsyncJob3, TInput, TOutput>()
            where TAsyncJob : AsyncJob<TInput, TOutput>
            where TAsyncJob2 : AsyncJob<TInput, TOutput>
            where TAsyncJob3 : AsyncJob<TInput, TOutput>
            => AsyncStage(GetJobService.GetJobInstance<TAsyncJob>(), GetJobService.GetJobInstance<TAsyncJob2>(), GetJobService.GetJobInstance<TAsyncJob3>());

        public IPipelineSetup<TInput, TOutput> AsyncStage<TAsyncJob, TAsyncJob2, TAsyncJob3, TAsyncJob4, TInput, TOutput>()
            where TAsyncJob : AsyncJob<TInput, TOutput>
            where TAsyncJob2 : AsyncJob<TInput, TOutput>
            where TAsyncJob3 : AsyncJob<TInput, TOutput>
            where TAsyncJob4 : AsyncJob<TInput, TOutput>
            => AsyncStage(GetJobService.GetJobInstance<TAsyncJob>(), GetJobService.GetJobInstance<TAsyncJob2>(), GetJobService.GetJobInstance<TAsyncJob3>(), GetJobService.GetJobInstance<TAsyncJob4>());

        #endregion

        #region Nongeneric Stages

        public IPipelineSetup<TInput, TOutput> Stage<TInput, TOutput>(Job<TInput, TOutput> job)
            => CreateNextStage<TInput, TOutput>(job);

        public IPipelineSetup<TInput, TOutput> Stage<TInput, TOutput>(Func<IEnumerable<TInput>, IEnumerable<TOutput>> func)
            => Stage(new LambdaJob<TInput, TOutput>(func));

        public IPipelineSetup<TInput, TOutput> Stage<TInput, TOutput>(Func<IEnumerable<TInput>, Task<IEnumerable<TOutput>>> func)
            => Stage(new LambdaJob<TInput, TOutput>(func));

        public IPipelineSetup<TInput, TOutput> Stage<TInput, TOutput>(params Job<TInput, TOutput>[] jobs)
            => Stage(new ConditionJob<TInput, TOutput>(jobs));

        public IPipelineSetup<TInput, TOutput> AsyncStage<TInput, TOutput>(AsyncJob<TInput, TOutput> asyncJob)
            => CreateNextStageAsync<TInput, TOutput>(asyncJob);

        public IPipelineSetup<TInput, TOutput> AsyncStage<TInput, TOutput>(Func<TInput, TOutput> func)
            => AsyncStage(new AsyncLambdaJob<TInput, TOutput>(func));

        public IPipelineSetup<TInput, TOutput> AsyncStage<TInput, TOutput>(Func<TInput, AsyncJobOption<TInput, TOutput>, TOutput> asyncFuncWithOption)
            => AsyncStage(new AsyncLambdaJob<TInput, TOutput>(asyncFuncWithOption));


        public IPipelineSetup<TInput, TOutput> AsyncStage<TInput, TOutput>(Func<TInput, Task<TOutput>> func)
            => AsyncStage(new AsyncLambdaJob<TInput, TOutput>(func));

        public IPipelineSetup<TInput, TOutput> AsyncStage<TInput, TOutput>(Func<TInput, AsyncJobOption<TInput, TOutput>, Task<TOutput>> asyncFuncWithOption)
            => AsyncStage(new AsyncLambdaJob<TInput, TOutput>(asyncFuncWithOption));


        public IPipelineSetup<TInput, TOutput> AsyncStage<TInput, TOutput>(params AsyncJob<TInput, TOutput>[] asyncJobs)
            => AsyncStage(new ConditionAsyncJob<TInput, TOutput>(asyncJobs));

        #endregion

        private PipelineSetup<TInput, TOutput> CreateNextStage<TInput, TOutput>(IPipelineJobSync<TInput, TOutput> job)
        {
            IPropagatorBlock<PipelineItem<TInput>, PipelineItem<TOutput>> MakeNextBlock()
            {

                var buffer = new BatchBlockEx<PipelineItem<TInput>>(job.Configuration.BatchItemsCount, job.Configuration.BatchItemsTimeOut); //TODO

                TransformManyBlock<IEnumerable<PipelineItem<TInput>>, PipelineItem<TOutput>> rePostBlock = null;

                void RePostMessages(IEnumerable<PipelineItem<TInput>> messages)
                {
                    rePostBlock?.Post(messages);
                }

                var nextBlock = new TransformManyBlock<IEnumerable<PipelineItem<TInput>>, PipelineItem<TOutput>>(
                    async e => await job.InternalExecute(e, () => RePostMessages(e), _cancellationToken),
                    new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = job.Configuration.MaxDegreeOfParallelism,
                        MaxMessagesPerTask = job.Configuration.MaxMessagesPerTask
                    });

                buffer.LinkTo(nextBlock, new DataflowLinkOptions() { PropagateCompletion = true });
                rePostBlock = nextBlock;

                buffer.Completion.ContinueWith(x => { nextBlock.Complete(); }, _cancellationToken);

                return DataflowBlock.Encapsulate(buffer, nextBlock);
            }

            return CreateNextBlock(MakeNextBlock);
        }

        private PipelineSetup<TInput, TOutput> CreateNextStageAsync<TInput, TOutput>(PipelineJobAsync<TInput, TOutput> asyncJob)
        {
            IPropagatorBlock<PipelineItem<TInput>, PipelineItem<TOutput>> MakeNextBlock()
            {
                TransformBlock<PipelineItem<TInput>, PipelineItem<TOutput>> rePostBlock = null;
                void RePostMessage(PipelineItem<TInput> message)
                {
                    rePostBlock?.Post(message);
                }

                var nextBlock = new TransformBlock<PipelineItem<TInput>, PipelineItem<TOutput>>(
                    async e => await asyncJob.InternalExecute(e, () => RePostMessage(e), _cancellationToken),
                    new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = asyncJob.Configuration.MaxDegreeOfParallelism,
                        MaxMessagesPerTask = asyncJob.Configuration.MaxMessagesPerTask
                    });

                rePostBlock = nextBlock;

                return nextBlock;
            }

            return CreateNextBlock(MakeNextBlock);
        }

        private PipelineSetup<TInput, TOutput> CreateNextBlock<TInput, TOutput>(Func<IPropagatorBlock<PipelineItem<TInput>, PipelineItem<TOutput>>> executionBlock)
        {
            return AppendStage(
                new Stage<TInput, TOutput>(executionBlock, _cancellationToken)
                {
                    Previous = null
                });
        }

        private PipelineSetup<TInput, TOutput> AppendStage<TInput, TOutput>(IStage<TInput, TOutput> stage)
        {
            return new FirstStageSetup<TInput, TInput, TOutput>(stage, _jobService);
        }
    }
}
