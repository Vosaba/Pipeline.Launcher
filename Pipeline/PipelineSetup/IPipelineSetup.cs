using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineLauncher.Dto;
using PipelineLauncher.Jobs;
using PipelineLauncher.Pipelines;
using PipelineLauncher.Stage;

namespace PipelineLauncher.PipelineSetup
{
    public interface IPipelineSetup
    {
        IStage Current { get; }
    }

    public interface IPipelineSetup<TInput, TOutput> : IPipelineSetup
    {
        new IStageOut<TOutput> Current { get; }

        #region Generic Stages

        #region Sync
        IPipelineSetup<TInput, TNextOutput> Stage<TJob, TNextOutput>()
            where TJob : Job<TOutput, TNextOutput>;

        IPipelineSetup<TInput, TOutput> Stage<TJob>()
            where TJob : Job<TOutput, TOutput>;

        IPipelineSetup<TInput, TNextOutput> Stage<TJob, TJob2, TNextOutput>()
            where TJob : Job<TOutput, TNextOutput>
            where TJob2 : Job<TOutput, TNextOutput>;

        IPipelineSetup<TInput, TNextOutput> Stage<TJob, TJob2, TJob3, TNextOutput>()
            where TJob : Job<TOutput, TNextOutput>
            where TJob2 : Job<TOutput, TNextOutput>
            where TJob3 : Job<TOutput, TNextOutput>;

        IPipelineSetup<TInput, TNextOutput> Stage<TJob, TJob2, TJob3, TJob4, TNextOutput>()
            where TJob : Job<TOutput, TNextOutput>
            where TJob2 : Job<TOutput, TNextOutput>
            where TJob3 : Job<TOutput, TNextOutput>
            where TJob4 : Job<TOutput, TNextOutput>;

        #endregion

        #region Async

        IPipelineSetup<TInput, TOutput> AsyncStage<TAsyncJob>()
           where TAsyncJob : AsyncJob<TOutput, TOutput>;

        IPipelineSetup<TInput, TNextOutput> AsyncStage<TAsyncJob, TNextOutput>()
           where TAsyncJob : AsyncJob<TOutput, TNextOutput>;

        IPipelineSetup<TInput, TNextOutput> AsyncStage<TAsyncJob, TAsyncJob2, TNextOutput>()
           where TAsyncJob : AsyncJob<TOutput, TNextOutput>
           where TAsyncJob2 : AsyncJob<TOutput, TNextOutput>;

        IPipelineSetup<TInput, TNextOutput> AsyncStage<TAsyncJob, TAsyncJob2, TAsyncJob3, TNextOutput>()
           where TAsyncJob : AsyncJob<TOutput, TNextOutput>
           where TAsyncJob2 : AsyncJob<TOutput, TNextOutput>
           where TAsyncJob3 : AsyncJob<TOutput, TNextOutput>;

        IPipelineSetup<TInput, TNextOutput> AsyncStage<TAsyncJob, TAsyncJob2, TAsyncJob3, TAsyncJob4, TNextOutput>()
           where TAsyncJob : AsyncJob<TOutput, TNextOutput>
           where TAsyncJob2 : AsyncJob<TOutput, TNextOutput>
           where TAsyncJob3 : AsyncJob<TOutput, TNextOutput>
           where TAsyncJob4 : AsyncJob<TOutput, TNextOutput>;

        #endregion
        
        #endregion

        #region Nongeneric Stages

        #region Sync

        IPipelineSetup<TInput, TNextOutput> Stage<TNextOutput>(Job<TOutput, TNextOutput> job);

        IPipelineSetup<TInput, TNextOutput> Stage<TNextOutput>(Func<IEnumerable<TOutput>, IEnumerable<TNextOutput>> func);

        IPipelineSetup<TInput, TNextOutput> Stage<TNextOutput>(Func<IEnumerable<TOutput>, Task<IEnumerable<TNextOutput>>> func);

        IPipelineSetup<TInput, TNextOutput> Stage<TNextOutput>(params Job<TOutput, TNextOutput>[] jobs);

        #endregion

        #region Async

        IPipelineSetup<TInput, TNextOutput> AsyncStage<TNextOutput>(AsyncJob<TOutput, TNextOutput> asyncJob);

        IPipelineSetup<TInput, TNextOutput> AsyncStage<TNextOutput>(Func<TOutput, TNextOutput> asyncFunc);

        IPipelineSetup<TInput, TNextOutput> AsyncStage<TNextOutput>(Func<TOutput, AsyncJobOption<TOutput, TNextOutput>, TNextOutput> asyncFuncWithOption);

        IPipelineSetup<TInput, TNextOutput> AsyncStage<TNextOutput>(Func<TOutput, Task<TNextOutput>> asyncFunc);

        IPipelineSetup<TInput, TNextOutput> AsyncStage<TNextOutput>(Func<TOutput, AsyncJobOption<TOutput, TNextOutput>, Task<TNextOutput>> asyncFuncWithOption);

        IPipelineSetup<TInput, TNextOutput> AsyncStage<TNextOutput>(params AsyncJob<TOutput, TNextOutput>[] asyncJobs);

        #endregion

        #endregion

        #region Branch

        IPipelineSetup<TInput, TNextOutput> Broadcast<TNextOutput>(params (Predicate<TOutput> predicate,
            Func<IPipelineSetup<TInput, TOutput>, IPipelineSetup<TInput, TNextOutput>> branch)[] branches);

        IPipelineSetup<TInput, TNextOutput> Branch<TNextOutput>(params (Predicate<TOutput> predicate,
            Func<IPipelineSetup<TInput, TOutput>, IPipelineSetup<TInput, TNextOutput>> branch)[] branches);

        #endregion

        IAwaitablePipelineRunner<TInput, TOutput> CreateAwaitable();

        IPipelineRunner<TInput, TOutput> Create();
    }
}