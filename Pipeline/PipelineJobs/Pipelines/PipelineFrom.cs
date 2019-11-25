using PipelineLauncher.Abstractions.Services;
//using PipelineLauncher.Dataflow;
using PipelineLauncher.Jobs;
using PipelineLauncher.Stages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;

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

        public StageSetupOut<TInput, TOutput> Stage<TJob, TOutput>()
            where TJob : Job<TInput, TOutput>
            => CreateNextStage<TOutput>(GetJobService.GetJobInstance<TJob>());

        public StageSetupOut<TInput, TInput> Stage<TJob>()
            where TJob : Job<TInput, TInput>
            => CreateNextStage<TInput>(GetJobService.GetJobInstance<TJob>());

        public StageSetupOut<TInput, TOutput> Stage<TJob, TJob2, TOutput>()
            where TJob : Job<TInput, TOutput>
            where TJob2 : Job<TInput, TOutput>
            => Stage(GetJobService.GetJobInstance<TJob>(), GetJobService.GetJobInstance<TJob2>());

        public StageSetupOut<TInput, TOutput> Stage<TJob, TJob2, TJob3, TOutput>()
            where TJob : Job<TInput, TOutput>
            where TJob2 : Job<TInput, TOutput>
            where TJob3 : Job<TInput, TOutput>
            => Stage(GetJobService.GetJobInstance<TJob>(), GetJobService.GetJobInstance<TJob2>(), GetJobService.GetJobInstance<TJob3>());

        public StageSetupOut<TInput, TOutput> Stage<TJob, TJob2, TJob3, TJob4, TOutput>()
            where TJob : Job<TInput, TOutput>
            where TJob2 : Job<TInput, TOutput>
            where TJob3 : Job<TInput, TOutput>
            where TJob4 : Job<TInput, TOutput>
            => Stage(GetJobService.GetJobInstance<TJob>(), GetJobService.GetJobInstance<TJob2>(), GetJobService.GetJobInstance<TJob3>(), GetJobService.GetJobInstance<TJob4>());

        public StageSetupOut<TInput, TOutput> AsyncStage<TAsyncJob, TOutput>()
            where TAsyncJob : AsyncJob<TInput, TOutput>
            => CreateNextStageAsync<TOutput>(GetJobService.GetJobInstance<TAsyncJob>());

        public StageSetupOut<TInput, TInput> AsyncStage<TAsyncJob>()
            where TAsyncJob : AsyncJob<TInput, TInput>
            => CreateNextStageAsync<TInput>(GetJobService.GetJobInstance<TAsyncJob>());

        public StageSetupOut<TInput, TOutput> AsyncStage<TAsyncJob, TAsyncJob2, TOutput>()
            where TAsyncJob : AsyncJob<TInput, TOutput>
            where TAsyncJob2 : AsyncJob<TInput, TOutput>
            => AsyncStage(GetJobService.GetJobInstance<TAsyncJob>(), GetJobService.GetJobInstance<TAsyncJob2>());

        public StageSetupOut<TInput, TOutput> AsyncStage<TAsyncJob, TAsyncJob2, TAsyncJob3, TOutput>()
            where TAsyncJob : AsyncJob<TInput, TOutput>
            where TAsyncJob2 : AsyncJob<TInput, TOutput>
            where TAsyncJob3 : AsyncJob<TInput, TOutput>
            => AsyncStage(GetJobService.GetJobInstance<TAsyncJob>(), GetJobService.GetJobInstance<TAsyncJob2>(), GetJobService.GetJobInstance<TAsyncJob3>());

        public StageSetupOut<TInput, TOutput> AsyncStage<TAsyncJob, TAsyncJob2, TAsyncJob3, TAsyncJob4, TOutput>()
            where TAsyncJob : AsyncJob<TInput, TOutput>
            where TAsyncJob2 : AsyncJob<TInput, TOutput>
            where TAsyncJob3 : AsyncJob<TInput, TOutput>
            where TAsyncJob4 : AsyncJob<TInput, TOutput>
            => AsyncStage(GetJobService.GetJobInstance<TAsyncJob>(), GetJobService.GetJobInstance<TAsyncJob2>(), GetJobService.GetJobInstance<TAsyncJob3>(), GetJobService.GetJobInstance<TAsyncJob4>());

        #endregion

        #region Nongeneric Stages

        public StageSetupOut<TInput, TOutput> Stage<TOutput>(Job<TInput, TOutput> job)
            => CreateNextStage<TOutput>(job);

        public StageSetupOut<TInput, TOutput> Stage<TOutput>(Func<IEnumerable<TInput>, IEnumerable<TOutput>> func)
            => Stage(new LambdaJob<TInput, TOutput>(func));

        public StageSetupOut<TInput, TOutput> Stage<TOutput>(params Job<TInput, TOutput>[] jobs)
            => Stage(new ConditionJob<TInput, TOutput>(jobs));

        public StageSetupOut<TInput, TOutput> AsyncStage<TOutput>(AsyncJob<TInput, TOutput> asyncJob)
            => CreateNextStageAsync<TOutput>(asyncJob);

        public StageSetupOut<TInput, TOutput> AsyncStage<TOutput>(Func<TInput, TOutput> func)
            => AsyncStage(new AsyncLambdaJob<TInput, TOutput>(func));

        public StageSetupOut<TInput, TOutput> AsyncStage<TOutput>(params AsyncJob<TInput, TOutput>[] asyncJobs)
            => AsyncStage(new ConditionAsyncJob<TInput, TOutput>(asyncJobs));

        #endregion

        private StageSetupOut<TInput, TOutput> CreateNextStage<TOutput>(Job<TInput, TOutput> job)
        {
            var nextBuffer = new BatchBlock<TInput>(int.MaxValue);
            var newcurrent = CreateNextBlock(nextBuffer);

            var g = new TransformManyBlock<IEnumerable<TInput>, TOutput>(e => job.Execute(e.ToArray()), new ExecutionDataflowBlockOptions(){MaxDegreeOfParallelism =5});

            nextBuffer.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    //((IDataflowBlock)_step2A).Fault(t.Exception);
                    //((IDataflowBlock)_step2B).Fault(t.Exception);
                }
                else
                {
                    //g.Complete();
                }
            });
            //ar nextBlock = new TransformBlock<TOutput[], IEnumerable<TNexTOutput>>(e => job.Execute(e));

            return newcurrent.CreateNextBlock(g);

            //return CreateNextBlock<TOutput>(t);
        }

        private StageSetupOut<TInput, TOutput> CreateNextStageAsync<TOutput>(AsyncJob<TInput, TOutput> asyncJob)
        {
            var nextBlock = new TransformBlock<TInput, TOutput>(e => asyncJob.Execute(e));

            return CreateNextBlock<TOutput>(nextBlock);
        }

        private StageSetupOut<TInput, TOutput> CreateNextBlock<TOutput>(IPropagatorBlock<TInput, TOutput> executionBlock)
        {

            return AppendStage(
                new Stage<TInput, TOutput>(executionBlock)
                {
                    Previous = null
                });
        }

        public StageSetupOut<TInput, TOutput> AppendStage<TOutput>(IStage<TInput, TOutput> stage)
        {
            return new StageSetup<TInput, TInput, TOutput>(stage, _jobService);
        }
    }
}
