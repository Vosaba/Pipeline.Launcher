using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.Abstractions.PipelineStage.Dto;
using PipelineLauncher.Stages;

namespace PipelineLauncher.PipelineSetup
{
    internal partial class PipelineSetup<TInput, TOutput>
    {
        #region Generic

        #region BulkStages

        IPipelineSetupOut<TNextOutput> IPipelineSetupOut<TOutput>.BulkStage<TBulkStage, TNextOutput>()
            => BulkStage<TBulkStage, TNextOutput>();

        IPipelineSetupOut<TOutput> IPipelineSetupOut<TOutput>.BulkStage<TBulkStage>() 
            => BulkStage<TBulkStage>();

        #endregion

        #region Stages

        IPipelineSetupOut<TOutput> IPipelineSetupOut<TOutput>.Stage<TStage>()
            => Stage<TStage>();

        IPipelineSetupOut<TNextOutput> IPipelineSetupOut<TOutput>.Stage<TStage, TNextOutput>()
            => Stage<TStage, TNextOutput>();

        #endregion

        #endregion

        #region Nongeneric

        #region BulkStages

        IPipelineSetupOut<TNextOutput> IPipelineSetupOut<TOutput>.BulkStage<TNextOutput>(BulkStage<TOutput, TNextOutput> baseStageBulkStage)
            => BulkStage<TNextOutput>(baseStageBulkStage);

        IPipelineSetupOut<TNextOutput> IPipelineSetupOut<TOutput>.BulkStage<TNextOutput>(Func<IEnumerable<TOutput>, IEnumerable<TNextOutput>> bulkFunc, BulkStageConfiguration bulkStageConfiguration)
            => BulkStage(bulkFunc, bulkStageConfiguration);

        IPipelineSetupOut<TNextOutput> IPipelineSetupOut<TOutput>.BulkStage<TNextOutput>(Func<IEnumerable<TOutput>, Task<IEnumerable<TNextOutput>>> bulkFunc, BulkStageConfiguration bulkStageConfiguration)
            => BulkStage(bulkFunc, bulkStageConfiguration);

        #endregion

        #region Stages

        IPipelineSetupOut<TNextOutput> IPipelineSetupOut<TOutput>.Stage<TNextOutput>(Stages.Stage<TOutput, TNextOutput> stage)
            => Stage<TNextOutput>(stage);

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

        //public IPipelineSetupOut<TNextOutput> Broadcast<TNextOutput>(params (Predicate<TOutput> predicate, Func<IPipelineSetupOut<TOutput>, IPipelineSetupOut<TNextOutput>> branch)[] branches)
        //{
        //    throw new NotImplementedException();
        //}

        //public IPipelineSetupOut<TNextOutput> Branch<TNextOutput>(params (Predicate<TOutput> predicate, Func<IPipelineSetupOut<TOutput>, IPipelineSetupOut<TNextOutput>> branch)[] branches)
        //{
        //    throw new NotImplementedException();
        //}

        //public IPipelineSetupOut<TNextOutput> Branch<TNextOutput>(ConditionExceptionScenario conditionExceptionScenario,
        //    params (Predicate<TOutput> predicate, Func<IPipelineSetupOut<TOutput>, IPipelineSetupOut<TNextOutput>> branch)[] branches)
        //{
        //    return Branch(conditionExceptionScenario, branches);
        //}

        #endregion
    }
}