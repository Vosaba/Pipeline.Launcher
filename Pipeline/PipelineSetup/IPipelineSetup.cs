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
        IPipelineSetup<TInput, TNextOutput> BulkStage<TJob, TNextOutput>()
            where TJob : BulkJob<TOutput, TNextOutput>;

        IPipelineSetup<TInput, TOutput> BulkStage<TJob>()
            where TJob : BulkJob<TOutput, TOutput>;

        IPipelineSetup<TInput, TNextOutput> BulkStage<TJob, TJob2, TNextOutput>()
            where TJob : BulkJob<TOutput, TNextOutput>
            where TJob2 : BulkJob<TOutput, TNextOutput>;

        IPipelineSetup<TInput, TNextOutput> BulkStage<TJob, TJob2, TJob3, TNextOutput>()
            where TJob : BulkJob<TOutput, TNextOutput>
            where TJob2 : BulkJob<TOutput, TNextOutput>
            where TJob3 : BulkJob<TOutput, TNextOutput>;

        IPipelineSetup<TInput, TNextOutput> BulkStage<TJob, TJob2, TJob3, TJob4, TNextOutput>()
            where TJob : BulkJob<TOutput, TNextOutput>
            where TJob2 : BulkJob<TOutput, TNextOutput>
            where TJob3 : BulkJob<TOutput, TNextOutput>
            where TJob4 : BulkJob<TOutput, TNextOutput>;

        #endregion

        #region Async

        IPipelineSetup<TInput, TOutput> Stage<TAsyncJob>()
           where TAsyncJob : Job<TOutput, TOutput>;

        IPipelineSetup<TInput, TNextOutput> Stage<TAsyncJob, TNextOutput>()
           where TAsyncJob : Job<TOutput, TNextOutput>;

        IPipelineSetup<TInput, TNextOutput> Stage<TAsyncJob, TAsyncJob2, TNextOutput>()
           where TAsyncJob : Job<TOutput, TNextOutput>
           where TAsyncJob2 : Job<TOutput, TNextOutput>;

        IPipelineSetup<TInput, TNextOutput> Stage<TAsyncJob, TAsyncJob2, TAsyncJob3, TNextOutput>()
           where TAsyncJob : Job<TOutput, TNextOutput>
           where TAsyncJob2 : Job<TOutput, TNextOutput>
           where TAsyncJob3 : Job<TOutput, TNextOutput>;

        IPipelineSetup<TInput, TNextOutput> Stage<TAsyncJob, TAsyncJob2, TAsyncJob3, TAsyncJob4, TNextOutput>()
           where TAsyncJob : Job<TOutput, TNextOutput>
           where TAsyncJob2 : Job<TOutput, TNextOutput>
           where TAsyncJob3 : Job<TOutput, TNextOutput>
           where TAsyncJob4 : Job<TOutput, TNextOutput>;

        #endregion
        
        #endregion

        #region Nongeneric Stages

        #region Sync

        IPipelineSetup<TInput, TNextOutput> BulkStage<TNextOutput>(BulkJob<TOutput, TNextOutput> job);

        IPipelineSetup<TInput, TNextOutput> BulkStage<TNextOutput>(Func<IEnumerable<TOutput>, IEnumerable<TNextOutput>> func);

        IPipelineSetup<TInput, TNextOutput> BulkStage<TNextOutput>(Func<IEnumerable<TOutput>, Task<IEnumerable<TNextOutput>>> func);

        IPipelineSetup<TInput, TNextOutput> BulkStage<TNextOutput>(params BulkJob<TOutput, TNextOutput>[] jobs);

        #endregion

        #region Async

        IPipelineSetup<TInput, TNextOutput> Stage<TNextOutput>(Job<TOutput, TNextOutput> job);

        IPipelineSetup<TInput, TNextOutput> Stage<TNextOutput>(Func<TOutput, TNextOutput> asyncFunc);

        IPipelineSetup<TInput, TNextOutput> Stage<TNextOutput>(Func<TOutput, StageOption<TOutput, TNextOutput>, TNextOutput> asyncFuncWithOption);

        IPipelineSetup<TInput, TNextOutput> Stage<TNextOutput>(Func<TOutput, Task<TNextOutput>> asyncFunc);

        IPipelineSetup<TInput, TNextOutput> Stage<TNextOutput>(Func<TOutput, StageOption<TOutput, TNextOutput>, Task<TNextOutput>> asyncFuncWithOption);

        IPipelineSetup<TInput, TNextOutput> Stage<TNextOutput>(params Job<TOutput, TNextOutput>[] jobS);

        #endregion

        #endregion

        #region Branch

        IPipelineSetup<TInput, TNextOutput> Broadcast<TNextOutput>(params (Predicate<TOutput> predicate,
            Func<IPipelineSetup<TInput, TOutput>, IPipelineSetup<TInput, TNextOutput>> branch)[] branches);

        IPipelineSetup<TInput, TNextOutput> Branch<TNextOutput>(params (Predicate<TOutput> predicate,
            Func<IPipelineSetup<TInput, TOutput>, IPipelineSetup<TInput, TNextOutput>> branch)[] branches);

        IPipelineSetup<TInput, TNextOutput> Branch<TNextOutput>(
            ConditionExceptionScenario conditionExceptionScenario,
            params (Predicate<TOutput> predicate, Func<IPipelineSetup<TInput, TOutput>, IPipelineSetup<TInput, TNextOutput>> branch)[] branches);

        #endregion

        IAwaitablePipelineRunner<TInput, TOutput> CreateAwaitable();

        IPipelineRunner<TInput, TOutput> Create();
    }
}