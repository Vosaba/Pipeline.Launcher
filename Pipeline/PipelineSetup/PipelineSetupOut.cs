using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.Abstractions.PipelineStage.Dto;
using PipelineLauncher.Stages;

namespace PipelineLauncher.PipelineSetup
{
    internal partial class PipelineSetup<TInput, TOutput>
    {
        #region Generic

        #region BulkStages

        IPipelineSetupOut<TNextOutput> IPipelineSetupOut<TOutput>.BulkStage<TBulkStage, TNextOutput>(PipelinePredicate<TOutput> predicate = null)
            => BulkStage<TBulkStage, TNextOutput>(predicate);

        IPipelineSetupOut<TOutput> IPipelineSetupOut<TOutput>.BulkStage<TBulkStage>(PipelinePredicate<TOutput> predicate = null) 
            => BulkStage<TBulkStage>(predicate);

        #endregion

        #region Stages

        IPipelineSetupOut<TOutput> IPipelineSetupOut<TOutput>.Stage<TStage>(PipelinePredicate<TOutput> predicate = null)
            => Stage<TStage>(predicate);

        IPipelineSetupOut<TNextOutput> IPipelineSetupOut<TOutput>.Stage<TStage, TNextOutput>(PipelinePredicate<TOutput> predicate = null)
            => Stage<TStage, TNextOutput>(predicate);

        #endregion

        #endregion

        #region Nongeneric

        #region BulkStages

        IPipelineSetupOut<TNextOutput> IPipelineSetupOut<TOutput>.BulkStage<TNextOutput>(BulkStage<TOutput, TNextOutput> baseStageBulkStage, PipelinePredicate<TOutput> predicate = null)
            => BulkStage<TNextOutput>(baseStageBulkStage, predicate);

        IPipelineSetupOut<TNextOutput> IPipelineSetupOut<TOutput>.BulkStage<TNextOutput>(Func<TOutput[], IEnumerable<TNextOutput>> bulkFunc, BulkStageConfiguration bulkStageConfiguration)
            => BulkStage(bulkFunc, bulkStageConfiguration);

        IPipelineSetupOut<TNextOutput> IPipelineSetupOut<TOutput>.BulkStage<TNextOutput>(Func<TOutput[], Task<IEnumerable<TNextOutput>>> bulkFunc, BulkStageConfiguration bulkStageConfiguration)
            => BulkStage(bulkFunc, bulkStageConfiguration);

        #endregion

        #region Stages

        IPipelineSetupOut<TNextOutput> IPipelineSetupOut<TOutput>.Stage<TNextOutput>(Stages.Stage<TOutput, TNextOutput> stage, PipelinePredicate<TOutput> predicate = null)
            => Stage<TNextOutput>(stage, predicate);

        IPipelineSetupOut<TNextOutput> IPipelineSetupOut<TOutput>.Stage<TNextOutput>(Func<TOutput, TNextOutput> func)
            => Stage<TNextOutput>(func);

        IPipelineSetupOut<TNextOutput> IPipelineSetupOut<TOutput>.Stage<TNextOutput>(Func<TOutput, StageOption<TOutput, TNextOutput>, TNextOutput> funcWithOption)
            => Stage<TNextOutput>(funcWithOption);

        IPipelineSetupOut<TNextOutput> IPipelineSetupOut<TOutput>.Stage<TNextOutput>(Func<TOutput, Task<TNextOutput>> func)
            => Stage<TNextOutput>(func);

        IPipelineSetupOut<TNextOutput> IPipelineSetupOut<TOutput>.Stage<TNextOutput>(Func<TOutput, StageOption<TOutput, TNextOutput>, Task<TNextOutput>> funcWithOption)
            => Stage<TNextOutput>(funcWithOption);

        #endregion

        #endregion

        #region Branches

        //public IPipelineSetupOut<TNextOutput> Broadcast<TNextOutput>(params (PipelinePredicate<TOutput> predicate, Func<IPipelineSetupOut<TOutput>, IPipelineSetupOut<TNextOutput>> branch)[] branches)
        //{
        //    throw new NotImplementedException();
        //}

        //public IPipelineSetupOut<TNextOutput> Branch<TNextOutput>(params (PipelinePredicate<TOutput> predicate, Func<IPipelineSetupOut<TOutput>, IPipelineSetupOut<TNextOutput>> branch)[] branches)
        //{
        //    throw new NotImplementedException();
        //}

        //public IPipelineSetupOut<TNextOutput> Branch<TNextOutput>(ConditionExceptionScenario conditionExceptionScenario,
        //    params (PipelinePredicate<TOutput> predicate, Func<IPipelineSetupOut<TOutput>, IPipelineSetupOut<TNextOutput>> branch)[] branches)
        //{
        //    return Branch(conditionExceptionScenario, branches);
        //}

        #endregion
    }
}