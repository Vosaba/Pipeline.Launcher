using PipelineLauncher.Abstractions.Configurations;
using PipelineLauncher.Dto;
using PipelineLauncher.Jobs;
using PipelineLauncher.Pipelines;
using PipelineLauncher.Stage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PipelineLauncher.PipelineSetup
{
    public interface IPipelineSetup
    {
        IStage Current { get; }
    }

    public interface IPipelineSetup<TInput, TOutput> : IPipelineSetupOut<TOutput>
    {
        #region Generic

        #region BulkStages

        new IPipelineSetup<TInput, TNextOutput> BulkStage<TBulkJob, TNextOutput>()
            where TBulkJob : Bulk<TOutput, TNextOutput>;

        new IPipelineSetup<TInput, TOutput> BulkStage<TBulkJob>()
            where TBulkJob : Bulk<TOutput, TOutput>;

        #endregion

        #region Stages

        new IPipelineSetup<TInput, TOutput> Stage<TJob>()
           where TJob : Job<TOutput, TOutput>;

        new IPipelineSetup<TInput, TNextOutput> Stage<TJob, TNextOutput>()
           where TJob : Job<TOutput, TNextOutput>;

        #endregion

        #endregion

        #region Nongeneric

        #region BulkStages

        new IPipelineSetup<TInput, TNextOutput> BulkStage<TNextOutput>(Bulk<TOutput, TNextOutput> bulk);

        new IPipelineSetup<TInput, TNextOutput> BulkStage<TNextOutput>(Func<IEnumerable<TOutput>, IEnumerable<TNextOutput>> bulkFunc, BulkJobConfiguration bulkJobConfiguration = null);

        new IPipelineSetup<TInput, TNextOutput> BulkStage<TNextOutput>(Func<IEnumerable<TOutput>, Task<IEnumerable<TNextOutput>>> bulkFunc, BulkJobConfiguration bulkJobConfiguration = null);

        #endregion

        #region Stages

        new IPipelineSetup<TInput, TNextOutput> Stage<TNextOutput>(Job<TOutput, TNextOutput> job);

        new IPipelineSetup<TInput, TNextOutput> Stage<TNextOutput>(Func<TOutput, TNextOutput> func);

        new IPipelineSetup<TInput, TNextOutput> Stage<TNextOutput>(Func<TOutput, StageOption<TOutput, TNextOutput>, TNextOutput> funcWithOption);

        new IPipelineSetup<TInput, TNextOutput> Stage<TNextOutput>(Func<TOutput, Task<TNextOutput>> func);

        new IPipelineSetup<TInput, TNextOutput> Stage<TNextOutput>(Func<TOutput, StageOption<TOutput, TNextOutput>, Task<TNextOutput>> funcWithOption);

        #endregion

        #endregion

        #region Branches

        IPipelineSetup<TInput, TNextOutput> Broadcast<TNextOutput>(params (Predicate<TOutput> predicate,
            Func<IPipelineSetup<TInput, TOutput>, IPipelineSetup<TInput, TNextOutput>> branch)[] branches);

        IPipelineSetup<TInput, TNextOutput> Branch<TNextOutput>(params (Predicate<TOutput> predicate,
            Func<IPipelineSetup<TInput, TOutput>, IPipelineSetup<TInput, TNextOutput>> branch)[] branches);

        IPipelineSetup<TInput, TNextOutput> Branch<TNextOutput>(ConditionExceptionScenario conditionExceptionScenario,
            params (Predicate<TOutput> predicate,
            Func<IPipelineSetup<TInput, TOutput>, IPipelineSetup<TInput, TNextOutput>> branch)[] branches);

        #endregion

        IAwaitablePipelineRunner<TInput, TOutput> CreateAwaitable(AwaitablePipelineConfig pipelineConfig = null);

        IPipelineRunner<TInput, TOutput> Create();
    }
}