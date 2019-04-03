using PipelineLauncher.Attributes;
using PipelineLauncher.Jobs;
using PipelineLauncher.PipelineJobs;
using PipelineLauncher.Stages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PipelineLauncher.Abstractions.Pipeline;
using PipelineLauncher.Abstractions.Services;
using PipelineLauncher.Dataflow;

namespace PipelineLauncher.Pipelines
{
    public class PipelineFrom<TInput>
    {
        private readonly IJobService _jobService;
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

        public PipelineFrom()
        {
        }

        public PipelineFrom(IJobService jobService)
        {
            _jobService = jobService;
        }

        #region Generic Stages

        public StageSetup<TInput, TOutput> Stage<TJob, TOutput>()
            where TJob : Job<TInput, TOutput>
            => CreateNextStage<TOutput>(GetJobService.GetJobInstance<TJob>());

        public StageSetup<TInput, TInput> Stage<TJob>()
            where TJob : Job<TInput, TInput>
            => CreateNextStage<TInput>(GetJobService.GetJobInstance<TJob>());

        public StageSetup<TInput, TOutput> Stage<TJob, TJob2, TOutput>()
            where TJob : Job<TInput, TOutput>
            where TJob2 : Job<TInput, TOutput>
            => Stage(GetJobService.GetJobInstance<TJob>(), GetJobService.GetJobInstance<TJob2>());

        public StageSetup<TInput, TOutput> Stage<TJob, TJob2, TJob3, TOutput>()
            where TJob : Job<TInput, TOutput>
            where TJob2 : Job<TInput, TOutput>
            where TJob3 : Job<TInput, TOutput>
            => Stage(GetJobService.GetJobInstance<TJob>(), GetJobService.GetJobInstance<TJob2>(), GetJobService.GetJobInstance<TJob3>());

        public StageSetup<TInput, TOutput> Stage<TJob, TJob2, TJob3, TJob4, TOutput>()
            where TJob : Job<TInput, TOutput>
            where TJob2 : Job<TInput, TOutput>
            where TJob3 : Job<TInput, TOutput>
            where TJob4 : Job<TInput, TOutput>
            => Stage(GetJobService.GetJobInstance<TJob>(), GetJobService.GetJobInstance<TJob2>(), GetJobService.GetJobInstance<TJob3>(), GetJobService.GetJobInstance<TJob4>());

        public StageSetup<TInput, TOutput> AsyncStage<TAsyncJob, TOutput>()
            where TAsyncJob : AsyncJob<TInput, TOutput>
            => CreateNextStageAsync<TOutput>(GetJobService.GetJobInstance<TAsyncJob>());

        public StageSetup<TInput, TInput> AsyncStage<TAsyncJob>()
            where TAsyncJob : AsyncJob<TInput, TInput>
            => CreateNextStageAsync<TInput>(GetJobService.GetJobInstance<TAsyncJob>());

        public StageSetup<TInput, TOutput> AsyncStage<TAsyncJob, TAsyncJob2, TOutput>()
            where TAsyncJob : AsyncJob<TInput, TOutput>
            where TAsyncJob2 : AsyncJob<TInput, TOutput>
            => AsyncStage(GetJobService.GetJobInstance<TAsyncJob>(), GetJobService.GetJobInstance<TAsyncJob2>());

        public StageSetup<TInput, TOutput> AsyncStage<TAsyncJob, TAsyncJob2, TAsyncJob3, TOutput>()
            where TAsyncJob : AsyncJob<TInput, TOutput>
            where TAsyncJob2 : AsyncJob<TInput, TOutput>
            where TAsyncJob3 : AsyncJob<TInput, TOutput>
            => AsyncStage(GetJobService.GetJobInstance<TAsyncJob>(), GetJobService.GetJobInstance<TAsyncJob2>(), GetJobService.GetJobInstance<TAsyncJob3>());

        public StageSetup<TInput, TOutput> AsyncStage<TAsyncJob, TAsyncJob2, TAsyncJob3, TAsyncJob4, TOutput>()
            where TAsyncJob : AsyncJob<TInput, TOutput>
            where TAsyncJob2 : AsyncJob<TInput, TOutput>
            where TAsyncJob3 : AsyncJob<TInput, TOutput>
            where TAsyncJob4 : AsyncJob<TInput, TOutput>
            => AsyncStage(GetJobService.GetJobInstance<TAsyncJob>(), GetJobService.GetJobInstance<TAsyncJob2>(), GetJobService.GetJobInstance<TAsyncJob3>(), GetJobService.GetJobInstance<TAsyncJob4>());

        #endregion

        #region Nongeneric Stages

        public StageSetup<TInput, TOutput> Stage<TOutput>(Job<TInput, TOutput> job)
            => CreateNextStage<TOutput>(job);

        public StageSetup<TInput, TOutput> Stage<TOutput>(Func<IEnumerable<TInput>, IEnumerable<TOutput>> func)
            => Stage(new LambdaJob<TInput, TOutput>(func));

        public StageSetup<TInput, TOutput> Stage<TOutput>(params Job<TInput, TOutput>[] jobs)
            => Stage(new ConditionJob<TInput, TOutput>(jobs));

        public StageSetup<TInput, TOutput> AsyncStage<TOutput>(AsyncJob<TInput, TOutput> asyncJob)
            => CreateNextStageAsync<TOutput>(asyncJob);

        public StageSetup<TInput, TOutput> AsyncStage<TOutput>(Func<TInput, TOutput> func)
            => AsyncStage(new AsyncLambdaJob<TInput, TOutput>(func));

        public StageSetup<TInput, TOutput> AsyncStage<TOutput>(params AsyncJob<TInput, TOutput>[] asyncJobs)
            => AsyncStage(new ConditionAsyncJob<TInput, TOutput>(asyncJobs));

        #endregion

        //public static IPipeline<TInput, TOutput> To<TOutput>(IStageSetup stageSetup, CancellationToken cancellationToken)
        //{
        //    var lastJobType = stageSetup.Current.Job.GetType();

        //    if (lastJobType.BaseType != null && lastJobType.BaseType.GenericTypeArguments[1] != typeof(TOutput))
        //    {
        //        throw new Exception($"PipelineSetup result should be: {lastJobType.BaseType.GenericTypeArguments[1].Name}");
        //    }

        //    var firstJobType = stageSetup.GetFirstStage().Job.GetType();

        //    if (firstJobType.BaseType != null && firstJobType.BaseType.GenericTypeArguments[0] != typeof(TInput))
        //    {
        //        throw new Exception($"Pipeline input should be: {firstJobType.BaseType.GenericTypeArguments[0].Name}");
        //    }

        //    return new BasicPipeline<TInput, TOutput>(stageSetup.GetFirstStage(), cancellationToken);
        //}

        //public static IPipeline<TInput, TOutput> To<TOutput>(IStageSetup stageSetup)
        //    => To<TOutput>(stageSetup, CancellationToken.None);

        private StageSetup<TInput, TOutput> CreateNextStage<TOutput>(Job<TInput, TOutput> job)
        {
            var nextBlock = new TransformManyToManyBlock<TInput, TOutput>((e, target) =>
            {
                foreach (var result in job.Execute(e))
                {
                    target.TryAdd(result);
                }

                target.CompleteAdding();
            });


            return CreateNextBlock<TOutput>(nextBlock);
        }

        private StageSetup<TInput, TOutput> CreateNextStageAsync<TOutput>(AsyncJob<TInput, TOutput> asyncJob)
        {
            var nextBlock = new TransformBlock<TInput, TOutput>(e => asyncJob.Execute(e));

            return CreateNextBlock<TOutput>(nextBlock);
        }

        private StageSetup<TInput, TOutput> CreateNextBlock<TOutput>(ITarget<TInput, TOutput> executionBlock)
        {

            return AppendStage(
                new Stage<TInput, TOutput>(executionBlock)
                {
                    Previous = null
                });
        }

        public StageSetup<TInput, TOutput> AppendStage<TOutput>(IStage<TInput, TOutput> stage)
        {


            return new StageSetup<TInput, TOutput>(stage, _jobService);
        }

        //private StageSetup<TInput, TOutput> CreateFilterStage<TOutput>(IPipelineJob job, object[] attributes)
        //{
        //    var attribute = attributes[0] as PipelineFilterAttribute;

        //    var stage = new Stage(new PipelineFilterJobAsync<TOutput>(attribute));

        //    stage.Next = new[]{
        //        new Stage(job)
        //        {
        //            Previous = new []{stage}
        //        }
        //    };

        //    // Wrap the new stage with a setup
        //    return new StageSetup<TInput, TOutput>(stage.Next.First(), _jobService);
        //}
    }
}
