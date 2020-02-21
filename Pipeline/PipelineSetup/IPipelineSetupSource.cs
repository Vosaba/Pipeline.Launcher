using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineStage;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.PipelineSetup.StageSetup;
using PipelineLauncher.Stages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PipelineLauncher.PipelineSetup
{
    public interface IPipelineSetup
    {
        IStageSetup StageSetup { get; }
    }

    public interface IPipelineSetupSource<TOutput> : IPipelineSetup
    {
        ISourceStageSetup<TOutput> SourceStageSetup { get; }

        #region Generic

        #region BulkStages

       IPipelineSetupSource<TNextStageOutput> BulkStage<TBulkStage, TNextStageOutput>(PipelinePredicate<TOutput> predicate = null)
            where TBulkStage : class, IBulkStage<TOutput, TNextStageOutput>;

       IPipelineSetupSource<TOutput> BulkStage<TBulkStage>(PipelinePredicate<TOutput> predicate = null)
            where TBulkStage : class, IBulkStage<TOutput, TOutput>;

        #endregion

        #region Stages

       IPipelineSetupSource<TOutput> Stage<TStage>(PipelinePredicate<TOutput> predicate = null)
           where TStage : class, IStage<TOutput, TOutput>;

       IPipelineSetupSource<TNextStageOutput> Stage<TStage, TNextStageOutput>(PipelinePredicate<TOutput> predicate = null)
           where TStage : class, IStage<TOutput, TNextStageOutput>;

        #endregion

        #endregion

        #region Nongeneric

        #region BulkStages

       IPipelineSetupSource<TNextStageOutput> BulkStage<TNextStageOutput>(IBulkStage<TOutput, TNextStageOutput> baseStageBulkStage, PipelinePredicate<TOutput> predicate = null);

       IPipelineSetupSource<TNextStageOutput> BulkStage<TNextStageOutput>(Func<TOutput[], IEnumerable<TNextStageOutput>> bulkFunc, BulkStageConfiguration bulkStageConfiguration = null);

       IPipelineSetupSource<TNextStageOutput> BulkStage<TNextStageOutput>(Func<TOutput[], Task<IEnumerable<TNextStageOutput>>> bulkFunc, BulkStageConfiguration bulkStageConfiguration = null);

        #endregion

        #region Stages

       IPipelineSetupSource<TNextStageOutput> Stage<TNextStageOutput>(IStage<TOutput, TNextStageOutput> stage, PipelinePredicate<TOutput> predicate = null);

       IPipelineSetupSource<TNextStageOutput> Stage<TNextStageOutput>(Func<TOutput, TNextStageOutput> func);

       IPipelineSetupSource<TNextStageOutput> Stage<TNextStageOutput>(Func<TOutput, StageOption<TOutput, TNextStageOutput>, TNextStageOutput> funcWithOption);

       IPipelineSetupSource<TNextStageOutput> Stage<TNextStageOutput>(Func<TOutput, Task<TNextStageOutput>> func);

       IPipelineSetupSource<TNextStageOutput> Stage<TNextStageOutput>(Func<TOutput, StageOption<TOutput, TNextStageOutput>, Task<TNextStageOutput>> funcWithOption);

        #endregion

        #endregion

        #region Branches

       //IPipelineSetupSource<TNextStageOutput> Broadcast<TNextStageOutput>(params (PipelinePredicate<TOutput> predicate,
       //     Func<IPipelineSetupSource<TOutput>,IPipelineSetupSource<TNextStageOutput>> branch)[] branches);

       //IPipelineSetupSource<TNextStageOutput> Branch<TNextStageOutput>(params (PipelinePredicate<TOutput> predicate,
       //     Func<IPipelineSetupSource<TOutput>,IPipelineSetupSource<TNextStageOutput>> branch)[] branches);

       //IPipelineSetupSource<TNextStageOutput> Branch<TNextStageOutput>(ConditionExceptionScenario conditionExceptionScenario,
       //     params (PipelinePredicate<TOutput> predicate,
       //     Func<IPipelineSetupSource<TOutput>,IPipelineSetupSource<TNextStageOutput>> branch)[] branches);

        #endregion
    }
}