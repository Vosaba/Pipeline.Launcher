using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.Abstractions.PipelineStage.Dto;
using PipelineLauncher.Stages;
using PipelineLauncher.StageSetup;

namespace PipelineLauncher.PipelineSetup
{
    public interface IPipelineSetupOut<TOutput> : IPipelineSetup
    {
        new IStageSetupOut<TOutput> Current { get; }

        #region Generic

        #region BulkStages

       IPipelineSetupOut<TNextOutput> BulkStage<TBulkStage, TNextOutput>(PipelinePredicate<TOutput> predicate = null)
            where TBulkStage : BulkStage<TOutput, TNextOutput>;

       IPipelineSetupOut<TOutput> BulkStage<TBulkStage>(PipelinePredicate<TOutput> predicate = null)
            where TBulkStage : BulkStage<TOutput, TOutput>;

        #endregion

        #region Stages

       IPipelineSetupOut<TOutput> Stage<TStage>(PipelinePredicate<TOutput> predicate = null)
           where TStage : Stages.Stage<TOutput, TOutput>;

       IPipelineSetupOut<TNextOutput> Stage<TStage, TNextOutput>(PipelinePredicate<TOutput> predicate = null)
           where TStage : Stages.Stage<TOutput, TNextOutput>;

        #endregion

        #endregion

        #region Nongeneric

        #region BulkStages

       IPipelineSetupOut<TNextOutput> BulkStage<TNextOutput>(BulkStage<TOutput, TNextOutput> baseStageBulkStage, PipelinePredicate<TOutput> predicate = null);

       IPipelineSetupOut<TNextOutput> BulkStage<TNextOutput>(Func<TOutput[], IEnumerable<TNextOutput>> bulkFunc, BulkStageConfiguration bulkStageConfiguration = null);

       IPipelineSetupOut<TNextOutput> BulkStage<TNextOutput>(Func<TOutput[], Task<IEnumerable<TNextOutput>>> bulkFunc, BulkStageConfiguration bulkStageConfiguration = null);

        #endregion

        #region Stages

       IPipelineSetupOut<TNextOutput> Stage<TNextOutput>(Stage<TOutput, TNextOutput> stage, PipelinePredicate<TOutput> predicate = null);

       IPipelineSetupOut<TNextOutput> Stage<TNextOutput>(Func<TOutput, TNextOutput> func);

       IPipelineSetupOut<TNextOutput> Stage<TNextOutput>(Func<TOutput, StageOption<TOutput, TNextOutput>, TNextOutput> funcWithOption);

       IPipelineSetupOut<TNextOutput> Stage<TNextOutput>(Func<TOutput, Task<TNextOutput>> func);

       IPipelineSetupOut<TNextOutput> Stage<TNextOutput>(Func<TOutput, StageOption<TOutput, TNextOutput>, Task<TNextOutput>> funcWithOption);

        #endregion

        #endregion

        #region Branches

       //IPipelineSetupOut<TNextOutput> Broadcast<TNextOutput>(params (PipelinePredicate<TOutput> predicate,
       //     Func<IPipelineSetupOut<TOutput>,IPipelineSetupOut<TNextOutput>> branch)[] branches);

       //IPipelineSetupOut<TNextOutput> Branch<TNextOutput>(params (PipelinePredicate<TOutput> predicate,
       //     Func<IPipelineSetupOut<TOutput>,IPipelineSetupOut<TNextOutput>> branch)[] branches);

       //IPipelineSetupOut<TNextOutput> Branch<TNextOutput>(ConditionExceptionScenario conditionExceptionScenario,
       //     params (PipelinePredicate<TOutput> predicate,
       //     Func<IPipelineSetupOut<TOutput>,IPipelineSetupOut<TNextOutput>> branch)[] branches);

        #endregion
    }
}