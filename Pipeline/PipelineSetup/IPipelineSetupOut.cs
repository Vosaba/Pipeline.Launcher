using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Configurations;
using PipelineLauncher.Dto;
using PipelineLauncher.Jobs;
using PipelineLauncher.Stage;

namespace PipelineLauncher.PipelineSetup
{
    public interface IPipelineSetupOut<TOutput> : IPipelineSetup
    {
        new IStageOut<TOutput> Current { get; }

        #region Generic

        #region BulkStages

       IPipelineSetupOut<TNextOutput> BulkStage<TJob, TNextOutput>()
            where TJob : BulkJob<TOutput, TNextOutput>;

       IPipelineSetupOut<TOutput> BulkStage<TJob>()
            where TJob : BulkJob<TOutput, TOutput>;

       IPipelineSetupOut<TNextOutput> BulkStage<TJob, TJob2, TNextOutput>()
            where TJob : BulkJob<TOutput, TNextOutput>
            where TJob2 : BulkJob<TOutput, TNextOutput>;

       IPipelineSetupOut<TNextOutput> BulkStage<TJob, TJob2, TJob3, TNextOutput>()
            where TJob : BulkJob<TOutput, TNextOutput>
            where TJob2 : BulkJob<TOutput, TNextOutput>
            where TJob3 : BulkJob<TOutput, TNextOutput>;

       IPipelineSetupOut<TNextOutput> BulkStage<TJob, TJob2, TJob3, TJob4, TNextOutput>()
            where TJob : BulkJob<TOutput, TNextOutput>
            where TJob2 : BulkJob<TOutput, TNextOutput>
            where TJob3 : BulkJob<TOutput, TNextOutput>
            where TJob4 : BulkJob<TOutput, TNextOutput>;

        #endregion

        #region Stages

       IPipelineSetupOut<TOutput> Stage<TAsyncJob>()
            where TAsyncJob : Job<TOutput, TOutput>;

       IPipelineSetupOut<TNextOutput> Stage<TAsyncJob, TNextOutput>()
            where TAsyncJob : Job<TOutput, TNextOutput>;

       IPipelineSetupOut<TNextOutput> Stage<TAsyncJob, TAsyncJob2, TNextOutput>()
            where TAsyncJob : Job<TOutput, TNextOutput>
            where TAsyncJob2 : Job<TOutput, TNextOutput>;

       IPipelineSetupOut<TNextOutput> Stage<TAsyncJob, TAsyncJob2, TAsyncJob3, TNextOutput>()
            where TAsyncJob : Job<TOutput, TNextOutput>
            where TAsyncJob2 : Job<TOutput, TNextOutput>
            where TAsyncJob3 : Job<TOutput, TNextOutput>;

       IPipelineSetupOut<TNextOutput> Stage<TAsyncJob, TAsyncJob2, TAsyncJob3, TAsyncJob4, TNextOutput>()
            where TAsyncJob : Job<TOutput, TNextOutput>
            where TAsyncJob2 : Job<TOutput, TNextOutput>
            where TAsyncJob3 : Job<TOutput, TNextOutput>
            where TAsyncJob4 : Job<TOutput, TNextOutput>;

        #endregion

        #endregion

        #region Nongeneric

        #region BulkStages

       IPipelineSetupOut<TNextOutput> BulkStage<TNextOutput>(BulkJob<TOutput, TNextOutput> job);

       IPipelineSetupOut<TNextOutput> BulkStage<TNextOutput>(Func<IEnumerable<TOutput>, IEnumerable<TNextOutput>> func, BulkJobConfiguration bulkJobConfiguration = null);

       IPipelineSetupOut<TNextOutput> BulkStage<TNextOutput>(Func<IEnumerable<TOutput>, Task<IEnumerable<TNextOutput>>> func, BulkJobConfiguration bulkJobConfiguration = null);

       IPipelineSetupOut<TNextOutput> BulkStage<TNextOutput>(params BulkJob<TOutput, TNextOutput>[] jobs);

        #endregion

        #region Stages

       IPipelineSetupOut<TNextOutput> Stage<TNextOutput>(Job<TOutput, TNextOutput> job);

       IPipelineSetupOut<TNextOutput> Stage<TNextOutput>(Func<TOutput, TNextOutput> asyncFunc);

       IPipelineSetupOut<TNextOutput> Stage<TNextOutput>(Func<TOutput, StageOption<TOutput, TNextOutput>, TNextOutput> asyncFuncWithOption);

       IPipelineSetupOut<TNextOutput> Stage<TNextOutput>(Func<TOutput, Task<TNextOutput>> asyncFunc);

       IPipelineSetupOut<TNextOutput> Stage<TNextOutput>(Func<TOutput, StageOption<TOutput, TNextOutput>, Task<TNextOutput>> asyncFuncWithOption);

       IPipelineSetupOut<TNextOutput> Stage<TNextOutput>(params Job<TOutput, TNextOutput>[] jobS);

        #endregion

        #endregion

        #region Branches

       IPipelineSetupOut<TNextOutput> Broadcast<TNextOutput>(params (Predicate<TOutput> predicate,
            Func<IPipelineSetupOut<TOutput>,IPipelineSetupOut<TNextOutput>> branch)[] branches);

       IPipelineSetupOut<TNextOutput> Branch<TNextOutput>(params (Predicate<TOutput> predicate,
            Func<IPipelineSetupOut<TOutput>,IPipelineSetupOut<TNextOutput>> branch)[] branches);

       IPipelineSetupOut<TNextOutput> Branch<TNextOutput>(
            ConditionExceptionScenario conditionExceptionScenario,
            params (Predicate<TOutput> predicate, Func<IPipelineSetupOut<TOutput>,IPipelineSetupOut<TNextOutput>> branch)[] branches);

        #endregion
    }
}