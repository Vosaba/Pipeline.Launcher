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

       IPipelineSetupOut<TNextOutput> BulkStage<TBulkJob, TNextOutput>()
            where TBulkJob : Bulk<TOutput, TNextOutput>;

       IPipelineSetupOut<TOutput> BulkStage<TBulkJob>()
            where TBulkJob : Bulk<TOutput, TOutput>;

        #endregion

        #region Stages

       IPipelineSetupOut<TOutput> Stage<TJob>()
           where TJob : Job<TOutput, TOutput>;

       IPipelineSetupOut<TNextOutput> Stage<TJob, TNextOutput>()
           where TJob : Job<TOutput, TNextOutput>;

        #endregion

        #endregion

        #region Nongeneric

        #region BulkStages

       IPipelineSetupOut<TNextOutput> BulkStage<TNextOutput>(Bulk<TOutput, TNextOutput> bulk);

       IPipelineSetupOut<TNextOutput> BulkStage<TNextOutput>(Func<IEnumerable<TOutput>, IEnumerable<TNextOutput>> bulkFunc, BulkJobConfiguration bulkJobConfiguration = null);

       IPipelineSetupOut<TNextOutput> BulkStage<TNextOutput>(Func<IEnumerable<TOutput>, Task<IEnumerable<TNextOutput>>> bulkFunc, BulkJobConfiguration bulkJobConfiguration = null);

        #endregion

        #region Stages

       IPipelineSetupOut<TNextOutput> Stage<TNextOutput>(Job<TOutput, TNextOutput> job);

       IPipelineSetupOut<TNextOutput> Stage<TNextOutput>(Func<TOutput, TNextOutput> func);

       IPipelineSetupOut<TNextOutput> Stage<TNextOutput>(Func<TOutput, StageOption<TOutput, TNextOutput>, TNextOutput> funcWithOption);

       IPipelineSetupOut<TNextOutput> Stage<TNextOutput>(Func<TOutput, Task<TNextOutput>> func);

       IPipelineSetupOut<TNextOutput> Stage<TNextOutput>(Func<TOutput, StageOption<TOutput, TNextOutput>, Task<TNextOutput>> funcWithOption);

        #endregion

        #endregion

        #region Branches

       //IPipelineSetupOut<TNextOutput> Broadcast<TNextOutput>(params (Predicate<TOutput> predicate,
       //     Func<IPipelineSetupOut<TOutput>,IPipelineSetupOut<TNextOutput>> branch)[] branches);

       //IPipelineSetupOut<TNextOutput> Branch<TNextOutput>(params (Predicate<TOutput> predicate,
       //     Func<IPipelineSetupOut<TOutput>,IPipelineSetupOut<TNextOutput>> branch)[] branches);

       //IPipelineSetupOut<TNextOutput> Branch<TNextOutput>(ConditionExceptionScenario conditionExceptionScenario,
       //     params (Predicate<TOutput> predicate,
       //     Func<IPipelineSetupOut<TOutput>,IPipelineSetupOut<TNextOutput>> branch)[] branches);

        #endregion
    }
}