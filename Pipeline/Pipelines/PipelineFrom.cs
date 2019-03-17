﻿using PipelineLauncher.Attributes;
using PipelineLauncher.Jobs;
using PipelineLauncher.PipelineJobs;
using PipelineLauncher.Stages;
using System;
using System.Collections.Generic;
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

        public StageSetup<TInput, TOutput> Stage<TPipelineJob, TOutput>() where TPipelineJob : PipelineJob<TInput, TOutput>
            => CreateNewStage<TOutput>(GetJobService.GetJobInstance<TPipelineJob>());


        public StageSetup<TInput, TInput> Stage<TPipelineJob>() where TPipelineJob : PipelineJob<TInput, TInput>
            => CreateNewStage<TInput>(GetJobService.GetJobInstance<TPipelineJob>());

        #endregion

        #region Nongeneric Stages

        public StageSetup<TInput, TOutput> Stage<TOutput>(Job<TInput, TOutput> job)
            => CreateNewStage<TOutput>(job);

        public StageSetup<TInput, TOutput> Stage<TOutput>(AsyncJob<TInput, TOutput> asyncJob)
            => CreateNewStage<TOutput>(asyncJob);

        public StageSetup<TInput, TOutput> Stage<TOutput>(Func<TInput, TOutput> func)
            => Stage(new AsyncLambdaJob<TInput, TOutput>(func));

        public StageSetup<TInput, TOutput> Stage<TOutput>(Func<IEnumerable<TInput>, IEnumerable<TOutput>> func)
            => Stage(new LambdaJob<TInput, TOutput>(func));

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
