using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.Stages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.PipelineStage;

namespace PipelineLauncher.PipelineSetup
{
    internal partial class PipelineSetup<TPipelineInput, TStageOutput>// : PipelineSetup<TPipelineInput>, IPipelineSetup<TPipelineInput, TStageOutput>
    {
        #region Generic

        #region BulkStages

        IPipelineSetupOut<TNextStageOutput> IPipelineSetupOut<TStageOutput>.BulkStage<TBulkStage, TNextStageOutput>(PipelinePredicate<TStageOutput> predicate = null)
            => BulkStage<TBulkStage, TNextStageOutput>(predicate);

        IPipelineSetupOut<TStageOutput> IPipelineSetupOut<TStageOutput>.BulkStage<TBulkStage>(PipelinePredicate<TStageOutput> predicate = null) 
            => BulkStage<TBulkStage>(predicate);

        #endregion

        #region Stages

        IPipelineSetupOut<TStageOutput> IPipelineSetupOut<TStageOutput>.Stage<TStage>(PipelinePredicate<TStageOutput> predicate = null)
            => Stage<TStage>(predicate);

        IPipelineSetupOut<TNextStageOutput> IPipelineSetupOut<TStageOutput>.Stage<TStage, TNextStageOutput>(PipelinePredicate<TStageOutput> predicate = null)
            => Stage<TStage, TNextStageOutput>(predicate);

        #endregion

        #endregion

        #region Nongeneric

        #region BulkStages

        IPipelineSetupOut<TNextStageOutput> IPipelineSetupOut<TStageOutput>.BulkStage<TNextStageOutput>(IBulkStage<TStageOutput, TNextStageOutput> baseStageBulkStage, PipelinePredicate<TStageOutput> predicate = null)
            => BulkStage<TNextStageOutput>(baseStageBulkStage, predicate);

        IPipelineSetupOut<TNextStageOutput> IPipelineSetupOut<TStageOutput>.BulkStage<TNextStageOutput>(Func<TStageOutput[], IEnumerable<TNextStageOutput>> bulkFunc, BulkStageConfiguration bulkStageConfiguration)
            => BulkStage(bulkFunc, bulkStageConfiguration);

        IPipelineSetupOut<TNextStageOutput> IPipelineSetupOut<TStageOutput>.BulkStage<TNextStageOutput>(Func<TStageOutput[], Task<IEnumerable<TNextStageOutput>>> bulkFunc, BulkStageConfiguration bulkStageConfiguration)
            => BulkStage(bulkFunc, bulkStageConfiguration);

        #endregion

        #region Stages

        IPipelineSetupOut<TNextStageOutput> IPipelineSetupOut<TStageOutput>.Stage<TNextStageOutput>(IStage<TStageOutput, TNextStageOutput> stage, PipelinePredicate<TStageOutput> predicate = null)
            => Stage<TNextStageOutput>(stage, predicate);

        IPipelineSetupOut<TNextStageOutput> IPipelineSetupOut<TStageOutput>.Stage<TNextStageOutput>(Func<TStageOutput, TNextStageOutput> func)
            => Stage<TNextStageOutput>(func);

        IPipelineSetupOut<TNextStageOutput> IPipelineSetupOut<TStageOutput>.Stage<TNextStageOutput>(Func<TStageOutput, StageOption<TStageOutput, TNextStageOutput>, TNextStageOutput> funcWithOption)
            => Stage<TNextStageOutput>(funcWithOption);

        IPipelineSetupOut<TNextStageOutput> IPipelineSetupOut<TStageOutput>.Stage<TNextStageOutput>(Func<TStageOutput, Task<TNextStageOutput>> func)
            => Stage<TNextStageOutput>(func);

        IPipelineSetupOut<TNextStageOutput> IPipelineSetupOut<TStageOutput>.Stage<TNextStageOutput>(Func<TStageOutput, StageOption<TStageOutput, TNextStageOutput>, Task<TNextStageOutput>> funcWithOption)
            => Stage<TNextStageOutput>(funcWithOption);

        #endregion

        #endregion

        #region Branches

        //public IPipelineSetupOut<TNextStageOutput> Broadcast<TNextStageOutput>(params (PipelinePredicate<TStageOutput> predicate, Func<IPipelineSetupOut<TStageOutput>, IPipelineSetupOut<TNextStageOutput>> branch)[] branches)
        //{
        //    throw new NotImplementedException();
        //}

        //public IPipelineSetupOut<TNextStageOutput> Branch<TNextStageOutput>(params (PipelinePredicate<TStageOutput> predicate, Func<IPipelineSetupOut<TStageOutput>, IPipelineSetupOut<TNextStageOutput>> branch)[] branches)
        //{
        //    throw new NotImplementedException();
        //}

        //public IPipelineSetupOut<TNextStageOutput> Branch<TNextStageOutput>(ConditionExceptionScenario conditionExceptionScenario,
        //    params (PipelinePredicate<TStageOutput> predicate, Func<IPipelineSetupOut<TStageOutput>, IPipelineSetupOut<TNextStageOutput>> branch)[] branches)
        //{
        //    return Branch(conditionExceptionScenario, branches);
        //}

        #endregion
    }
}