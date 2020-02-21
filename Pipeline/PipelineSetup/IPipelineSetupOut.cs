using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.Stages;
using PipelineLauncher.StageSetup;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.PipelineStage;

namespace PipelineLauncher.PipelineSetup
{
    public interface IPipelineSetup
    {
        IStageSetup StageSetup { get; }
    }

    public interface IPipelineSetupOut<TOutput> : IPipelineSetup
    {
        ISourceStageSetup<TOutput> SourceStageSetup { get; }

        #region Generic

        #region BulkStages

       IPipelineSetupOut<TNextStageOutput> BulkStage<TBulkStage, TNextStageOutput>(PipelinePredicate<TOutput> predicate = null)
            where TBulkStage : class, IBulkStage<TOutput, TNextStageOutput>;

       IPipelineSetupOut<TOutput> BulkStage<TBulkStage>(PipelinePredicate<TOutput> predicate = null)
            where TBulkStage : class, IBulkStage<TOutput, TOutput>;

        #endregion

        #region Stages

       IPipelineSetupOut<TOutput> Stage<TStage>(PipelinePredicate<TOutput> predicate = null)
           where TStage : class, IStage<TOutput, TOutput>;

       IPipelineSetupOut<TNextStageOutput> Stage<TStage, TNextStageOutput>(PipelinePredicate<TOutput> predicate = null)
           where TStage : class, IStage<TOutput, TNextStageOutput>;

        #endregion

        #endregion

        #region Nongeneric

        #region BulkStages

       IPipelineSetupOut<TNextStageOutput> BulkStage<TNextStageOutput>(IBulkStage<TOutput, TNextStageOutput> baseStageBulkStage, PipelinePredicate<TOutput> predicate = null);

       IPipelineSetupOut<TNextStageOutput> BulkStage<TNextStageOutput>(Func<TOutput[], IEnumerable<TNextStageOutput>> bulkFunc, BulkStageConfiguration bulkStageConfiguration = null);

       IPipelineSetupOut<TNextStageOutput> BulkStage<TNextStageOutput>(Func<TOutput[], Task<IEnumerable<TNextStageOutput>>> bulkFunc, BulkStageConfiguration bulkStageConfiguration = null);

        #endregion

        #region Stages

       IPipelineSetupOut<TNextStageOutput> Stage<TNextStageOutput>(IStage<TOutput, TNextStageOutput> stage, PipelinePredicate<TOutput> predicate = null);

       IPipelineSetupOut<TNextStageOutput> Stage<TNextStageOutput>(Func<TOutput, TNextStageOutput> func);

       IPipelineSetupOut<TNextStageOutput> Stage<TNextStageOutput>(Func<TOutput, StageOption<TOutput, TNextStageOutput>, TNextStageOutput> funcWithOption);

       IPipelineSetupOut<TNextStageOutput> Stage<TNextStageOutput>(Func<TOutput, Task<TNextStageOutput>> func);

       IPipelineSetupOut<TNextStageOutput> Stage<TNextStageOutput>(Func<TOutput, StageOption<TOutput, TNextStageOutput>, Task<TNextStageOutput>> funcWithOption);

        #endregion

        #endregion

        #region Branches

       //IPipelineSetupOut<TNextStageOutput> Broadcast<TNextStageOutput>(params (PipelinePredicate<TOutput> predicate,
       //     Func<IPipelineSetupOut<TOutput>,IPipelineSetupOut<TNextStageOutput>> branch)[] branches);

       //IPipelineSetupOut<TNextStageOutput> Branch<TNextStageOutput>(params (PipelinePredicate<TOutput> predicate,
       //     Func<IPipelineSetupOut<TOutput>,IPipelineSetupOut<TNextStageOutput>> branch)[] branches);

       //IPipelineSetupOut<TNextStageOutput> Branch<TNextStageOutput>(ConditionExceptionScenario conditionExceptionScenario,
       //     params (PipelinePredicate<TOutput> predicate,
       //     Func<IPipelineSetupOut<TOutput>,IPipelineSetupOut<TNextStageOutput>> branch)[] branches);

        #endregion
    }
}