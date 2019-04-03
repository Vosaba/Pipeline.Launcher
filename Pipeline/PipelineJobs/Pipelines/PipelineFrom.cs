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
            => CreateNewStage<TOutput>(GetJobService.GetJobInstance<TJob>());

        public StageSetup<TInput, TInput> Stage<TJob>()
            where TJob : Job<TInput, TInput>
            => CreateNewStage<TInput>(GetJobService.GetJobInstance<TJob>());

        public StageSetup<TInput, TOutput> Stage<TJob, TJob2, TOutput>()
            where TJob : JobVariant<TInput, TOutput>
            where TJob2 : JobVariant<TInput, TOutput>
            => Stage(GetJobService.GetJobInstance<TJob>(), GetJobService.GetJobInstance<TJob2>());

        public StageSetup<TInput, TOutput> Stage<TJob, TJob2, TJob3, TOutput>()
            where TJob : JobVariant<TInput, TOutput>
            where TJob2 : JobVariant<TInput, TOutput>
            where TJob3 : JobVariant<TInput, TOutput>
            => Stage(GetJobService.GetJobInstance<TJob>(), GetJobService.GetJobInstance<TJob2>(), GetJobService.GetJobInstance<TJob3>());

        public StageSetup<TInput, TOutput> Stage<TJob, TJob2, TJob3, TJob4, TOutput>()
            where TJob : JobVariant<TInput, TOutput>
            where TJob2 : JobVariant<TInput, TOutput>
            where TJob3 : JobVariant<TInput, TOutput>
            where TJob4 : JobVariant<TInput, TOutput>
            => Stage(GetJobService.GetJobInstance<TJob>(), GetJobService.GetJobInstance<TJob2>(), GetJobService.GetJobInstance<TJob3>(), GetJobService.GetJobInstance<TJob4>());

        public StageSetup<TInput, TOutput> AsyncStage<TAsyncJob, TOutput>()
            where TAsyncJob : AsyncJob<TInput, TOutput>
            => CreateNewStage<TOutput>(GetJobService.GetJobInstance<TAsyncJob>());

        public StageSetup<TInput, TInput> AsyncStage<TAsyncJob>()
            where TAsyncJob : AsyncJob<TInput, TInput>
            => CreateNewStage<TInput>(GetJobService.GetJobInstance<TAsyncJob>());

        public StageSetup<TInput, TOutput> AsyncStage<TAsyncJob, TAsyncJob2, TOutput>()
            where TAsyncJob : AsyncJobVariant<TInput, TOutput>
            where TAsyncJob2 : AsyncJobVariant<TInput, TOutput>
            => AsyncStage(GetJobService.GetJobInstance<TAsyncJob>(), GetJobService.GetJobInstance<TAsyncJob2>());

        public StageSetup<TInput, TOutput> AsyncStage<TAsyncJob, TAsyncJob2, TAsyncJob3, TOutput>()
            where TAsyncJob : AsyncJobVariant<TInput, TOutput>
            where TAsyncJob2 : AsyncJobVariant<TInput, TOutput>
            where TAsyncJob3 : AsyncJobVariant<TInput, TOutput>
            => AsyncStage(GetJobService.GetJobInstance<TAsyncJob>(), GetJobService.GetJobInstance<TAsyncJob2>(), GetJobService.GetJobInstance<TAsyncJob3>());

        public StageSetup<TInput, TOutput> AsyncStage<TAsyncJob, TAsyncJob2, TAsyncJob3, TAsyncJob4, TOutput>()
            where TAsyncJob : AsyncJobVariant<TInput, TOutput>
            where TAsyncJob2 : AsyncJobVariant<TInput, TOutput>
            where TAsyncJob3 : AsyncJobVariant<TInput, TOutput>
            where TAsyncJob4 : AsyncJobVariant<TInput, TOutput>
            => AsyncStage(GetJobService.GetJobInstance<TAsyncJob>(), GetJobService.GetJobInstance<TAsyncJob2>(), GetJobService.GetJobInstance<TAsyncJob3>(), GetJobService.GetJobInstance<TAsyncJob4>());

        #endregion

        #region Nongeneric Stages

        public StageSetup<TInput, TOutput> Stage<TOutput>(Job<TInput, TOutput> job)
            => CreateNewStage<TOutput>(job);

        public StageSetup<TInput, TOutput> Stage<TOutput>(Func<IEnumerable<TInput>, IEnumerable<TOutput>> func)
            => Stage(new LambdaJob<TInput, TOutput>(func));

        public StageSetup<TInput, TOutput> Stage<TOutput>(params JobVariant<TInput, TOutput>[] jobs)
            => Stage(new ConditionJob<TInput, TOutput>(jobs));

        public StageSetup<TInput, TOutput> AsyncStage<TOutput>(AsyncJob<TInput, TOutput> asyncJob)
            => CreateNewStage<TOutput>(asyncJob);

        public StageSetup<TInput, TOutput> AsyncStage<TOutput>(Func<TInput, TOutput> func)
            => AsyncStage(new AsyncLambdaJob<TInput, TOutput>(func));

        public StageSetup<TInput, TOutput> AsyncStage<TOutput>(params AsyncJobVariant<TInput, TOutput>[] asyncJobs)
            => AsyncStage(new ConditionAsyncJob<TInput, TOutput>(asyncJobs));

        #endregion

        public static IPipeline<TInput, TOutput> To<TOutput>(IStageSetup stageSetup, CancellationToken cancellationToken)
        {
            var lastJobType = stageSetup.Current.Job.GetType();

            if (lastJobType.BaseType != null && lastJobType.BaseType.GenericTypeArguments[1] != typeof(TOutput))
            {
                throw new Exception($"PipelineSetup result should be: {lastJobType.BaseType.GenericTypeArguments[1].Name}");
            }

            var firstJobType = stageSetup.GetFirstStage().Job.GetType();

            if (firstJobType.BaseType != null && firstJobType.BaseType.GenericTypeArguments[0] != typeof(TInput))
            {
                throw new Exception($"Pipeline input should be: {firstJobType.BaseType.GenericTypeArguments[0].Name}");
            }

            return new BasicPipeline<TInput, TOutput>(stageSetup.ToEnumerable(), cancellationToken);
        }

        public static IPipeline<TInput, TOutput> To<TOutput>(IStageSetup stageSetup)
            => To<TOutput>(stageSetup, CancellationToken.None);

        private StageSetup<TInput, TOutput> CreateNewStage<TOutput>(IPipelineJob job)
        {
            var attributes = job.GetType().GetCustomAttributes(typeof(PipelineFilterAttribute), true);

            if (attributes.Length > 0)
            {
                return CreateFilterStage<TOutput>(job, attributes);
            }

            return new StageSetup<TInput, TOutput>(new Stage(job), _jobService);
        }

        private StageSetup<TInput, TOutput> CreateFilterStage<TOutput>(IPipelineJob job, object[] attributes)
        {
            var attribute = attributes[0] as PipelineFilterAttribute;

            var stage = new Stage(new PipelineFilterJobAsync<TOutput>(attribute));

            stage.Next = new Stage(job)
            {
                Previous = stage
            };

            // Wrap the new stage with a setup
            return new StageSetup<TInput, TOutput>(stage.Next, _jobService);
        }
    }
}
