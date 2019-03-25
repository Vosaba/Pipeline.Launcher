using PipelineLauncher.Attributes;
using PipelineLauncher.Jobs;
using PipelineLauncher.PipelineJobs;
using PipelineLauncher.Pipelines;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using PipelineLauncher.Abstractions.Pipeline;
using PipelineLauncher.Abstractions.Services;

namespace PipelineLauncher.Stages
{
    public class StageSetup<TInput, TOutput> : IStageSetup
    {
        private readonly IStage _stage;
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

        /// <summary>
        /// Gets the current stage.
        /// </summary>
        public IStage Current => _stage;

        internal StageSetup(IStage stage, IJobService jobService)
        {
            _stage = stage;
            _jobService = jobService;
        }

        #region Generic Stages

        public StageSetup<TOutput, TNexTOutput> Stage<TPipelineJob, TNexTOutput>() 
            where TPipelineJob : PipelineJob<TOutput, TNexTOutput>
            => CreateNextStage<TNexTOutput>(GetJobService.GetJobInstance<TPipelineJob>());

        public StageSetup<TOutput, TNexTOutput> Stage<TPipelineJob, TPipelineJob2, TNexTOutput>()
            where TPipelineJob : PipelineJob<TOutput, TNexTOutput>
            where TPipelineJob2 : PipelineJob<TOutput, TNexTOutput>
            => Stage(GetJobService.GetJobInstance<TPipelineJob>(), GetJobService.GetJobInstance<TPipelineJob2>());

        public StageSetup<TOutput, TNexTOutput> Stage<TPipelineJob, TPipelineJob2, TPipelineJob3, TNexTOutput>()
            where TPipelineJob : PipelineJob<TOutput, TNexTOutput>
            where TPipelineJob2 : PipelineJob<TOutput, TNexTOutput>
            where TPipelineJob3 : PipelineJob<TOutput, TNexTOutput>
            => Stage(GetJobService.GetJobInstance<TPipelineJob>(), GetJobService.GetJobInstance<TPipelineJob2>(), GetJobService.GetJobInstance<TPipelineJob3>());

        public StageSetup<TOutput, TNexTOutput> Stage<TPipelineJob, TPipelineJob2, TPipelineJob3, TPipelineJob4, TNexTOutput>()
            where TPipelineJob : PipelineJob<TOutput, TNexTOutput>
            where TPipelineJob2 : PipelineJob<TOutput, TNexTOutput>
            where TPipelineJob3 : PipelineJob<TOutput, TNexTOutput>
            where TPipelineJob4 : PipelineJob<TOutput, TNexTOutput>
            => Stage(GetJobService.GetJobInstance<TPipelineJob>(), GetJobService.GetJobInstance<TPipelineJob2>(), GetJobService.GetJobInstance<TPipelineJob3>(), GetJobService.GetJobInstance<TPipelineJob4>());

        public StageSetup<TOutput, TOutput> Stage<TPipelineJob>() where TPipelineJob : PipelineJob<TOutput, TOutput>
            => CreateNextStage<TOutput>(GetJobService.GetJobInstance<TPipelineJob>());

        public StageSetup<TOutput, TNexTOutput> MapAs<TNexTOutput>() where TNexTOutput : class 
            => Stage(output => output as TNexTOutput);

        #endregion

        #region Nongeneric Stages

        public StageSetup<TOutput, TNexTOutput> Stage<TNexTOutput>(Job<TOutput, TNexTOutput> job)
            => CreateNextStage<TNexTOutput>(job);

        public StageSetup<TOutput, TNexTOutput> Stage<TNexTOutput>(AsyncJob<TOutput, TNexTOutput> asyncJob)
            => CreateNextStage<TNexTOutput>(asyncJob);

        public StageSetup<TOutput, TNexTOutput> Stage<TNexTOutput>(Func<TOutput, TNexTOutput> func)
            => Stage(new AsyncLambdaJob<TOutput, TNexTOutput>(func));

        public StageSetup<TOutput, TNexTOutput> Stage<TNexTOutput>(Func<IEnumerable<TOutput>, IEnumerable<TNexTOutput>> func)
            => Stage(new LambdaJob<TOutput, TNexTOutput>(func));

        public StageSetup<TOutput, TNexTOutput> Stage<TNexTOutput>(params PipelineJob<TOutput, TNexTOutput>[] pipelineJobs)
        {
            switch (pipelineJobs.FirstOrDefault())
            {
                case Job<TOutput, TNexTOutput> _:
                    return Stage(pipelineJobs.Cast<Job<TOutput, TNexTOutput>>().ToArray());
                case AsyncJob<TOutput, TNexTOutput> _:
                    return Stage(pipelineJobs.Cast<AsyncJob<TOutput, TNexTOutput>>().ToArray());
                default: 
                    throw new ArgumentException($"Jobs can't be recognized: '{pipelineJobs}'");
            }
        }

        public StageSetup<TOutput, TNexTOutput> Stage<TNexTOutput>(params AsyncJob<TOutput, TNexTOutput>[] asyncJobs)
            => Stage(new ConditionAsyncJob<TOutput, TNexTOutput>(asyncJobs));

        public StageSetup<TOutput, TNexTOutput> Stage<TNexTOutput>(params Job<TOutput, TNexTOutput>[] jobs)
            => Stage(new ConditionJob<TOutput, TNexTOutput>(jobs));

        #endregion

        public IPipeline<TFirstInput, TOutput> From<TFirstInput>(CancellationToken cancellationToken)
        {
            var firstJobType = this.GetFirstStage().Job.GetType();

            if (firstJobType.BaseType != null && firstJobType.BaseType.GenericTypeArguments[0] == typeof(TFirstInput))
            {
                return new BasicPipeline<TFirstInput, TOutput>(this.ToEnumerable(), cancellationToken);
            }

            if (firstJobType.BaseType != null)
            {
                throw new Exception(
                    $"Stages config expects '{firstJobType.BaseType.GenericTypeArguments[0].Name}', but was recived '{typeof(TFirstInput).Name}'");
            }
            else
            {
                throw new Exception(
                    $"Stages config didn't expected '{typeof(TFirstInput).Name}' as input");
            }
                
        }

        public IPipeline<TFirstInput, TOutput> From<TFirstInput>()
            => From<TFirstInput>(CancellationToken.None);

        private StageSetup<TOutput, TNexTOutput> CreateNextStage<TNexTOutput>(IPipelineJob job)
        {
            var attributes = job.GetType().GetCustomAttributes(typeof(PipelineFilterAttribute), true);

            if (attributes.Length > 0)
            {
                return CreateFilterStage<TNexTOutput>(job, attributes);
            }

            _stage.Next = new Stage(job)
            {
                Previous = _stage
            };

            // Wrap the new stage with a setup
            return new StageSetup<TOutput, TNexTOutput>(_stage.Next, _jobService);

        }

        private StageSetup<TOutput, TNexTOutput> CreateFilterStage<TNexTOutput>(IPipelineJob job, object[] attributes)
        {
            var attribute = attributes[0] as PipelineFilterAttribute;

            _stage.Next = new Stage(new PipelineFilterJobAsync<TOutput>(attribute))
            {
                Previous = _stage,
                Next = new Stage(job)
                {
                    Previous = _stage
                }
            };

            // Wrap the new stage with a setup
            return new StageSetup<TOutput, TNexTOutput>(_stage.Next.Next, _jobService);
        }
    }
}
