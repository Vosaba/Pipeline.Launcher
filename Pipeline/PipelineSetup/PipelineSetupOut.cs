using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Configurations;
using PipelineLauncher.Dto;
using PipelineLauncher.Jobs;

namespace PipelineLauncher.PipelineSetup
{
    internal partial class PipelineSetup<TInput, TOutput>
    {
        #region Generic

        #region BulkStages

        IPipelineSetupOut<TNextOutput> IPipelineSetupOut<TOutput>.BulkStage<TBulkJob, TNextOutput>()
            => BulkStage<TBulkJob, TNextOutput>();

        IPipelineSetupOut<TOutput> IPipelineSetupOut<TOutput>.BulkStage<TBulkJob>() 
            => BulkStage<TBulkJob>();

        #endregion

        #region Stages

        IPipelineSetupOut<TOutput> IPipelineSetupOut<TOutput>.Stage<TJob>()
            => Stage<TJob>();

        IPipelineSetupOut<TNextOutput> IPipelineSetupOut<TOutput>.Stage<TJob, TNextOutput>()
            => Stage<TJob, TNextOutput>();

        #endregion

        #endregion

        #region Nongeneric

        #region BulkStages

        IPipelineSetupOut<TNextOutput> IPipelineSetupOut<TOutput>.BulkStage<TNextOutput>(Bulk<TOutput, TNextOutput> bulk)
            => BulkStage<TNextOutput>(bulk);

        IPipelineSetupOut<TNextOutput> IPipelineSetupOut<TOutput>.BulkStage<TNextOutput>(Func<IEnumerable<TOutput>, IEnumerable<TNextOutput>> bulkFunc, BulkJobConfiguration bulkJobConfiguration)
            => BulkStage(bulkFunc, bulkJobConfiguration);

        IPipelineSetupOut<TNextOutput> IPipelineSetupOut<TOutput>.BulkStage<TNextOutput>(Func<IEnumerable<TOutput>, Task<IEnumerable<TNextOutput>>> bulkFunc, BulkJobConfiguration bulkJobConfiguration)
            => BulkStage(bulkFunc, bulkJobConfiguration);

        #endregion

        #region Stages

        IPipelineSetupOut<TNextOutput> IPipelineSetupOut<TOutput>.Stage<TNextOutput>(Job<TOutput, TNextOutput> job)
            => Stage<TNextOutput>(job);

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