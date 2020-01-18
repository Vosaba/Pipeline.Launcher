﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineEvents;
using PipelineLauncher.Abstractions.PipelineRunner;
using PipelineLauncher.Abstractions.PipelineRunner.Configurations;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.Abstractions.PipelineStage.Dto;
using PipelineLauncher.PipelineRunner;
using PipelineLauncher.Stages;
using PipelineLauncher.StageSetup;

namespace PipelineLauncher.PipelineSetup
{
    public interface IPipelineSetup
    {
        //Action<DiagnosticItem> DiagnosticHandler { get; }
        IStageSetup Current { get; }
    }

    public interface IPipelineSetup<TInput, TOutput> : IPipelineSetupOut<TOutput>
    {
        #region Generic

        #region BulkStages

        new IPipelineSetup<TInput, TNextOutput> BulkStage<TBulkStage, TNextOutput>()
            where TBulkStage : BulkStage<TOutput, TNextOutput>;

        new IPipelineSetup<TInput, TOutput> BulkStage<TBulkStage>()
            where TBulkStage : BulkStage<TOutput, TOutput>;

        #endregion

        #region Stages

        new IPipelineSetup<TInput, TOutput> Stage<TStage>()
           where TStage : Stages.Stage<TOutput, TOutput>;

        new IPipelineSetup<TInput, TNextOutput> Stage<TStage, TNextOutput>()
           where TStage : Stages.Stage<TOutput, TNextOutput>;

        #endregion

        #endregion

        #region Nongeneric

        #region BulkStages

        new IPipelineSetup<TInput, TNextOutput> BulkStage<TNextOutput>(BulkStage<TOutput, TNextOutput> bulkStage);

        new IPipelineSetup<TInput, TNextOutput> BulkStage<TNextOutput>(Func<IEnumerable<TOutput>, IEnumerable<TNextOutput>> bulkFunc, BulkStageConfiguration bulkStageConfiguration = null);

        new IPipelineSetup<TInput, TNextOutput> BulkStage<TNextOutput>(Func<IEnumerable<TOutput>, Task<IEnumerable<TNextOutput>>> bulkFunc, BulkStageConfiguration bulkStageConfiguration = null);

        #endregion

        #region Stages

        new IPipelineSetup<TInput, TNextOutput> Stage<TNextOutput>(Stages.Stage<TOutput, TNextOutput> stage);

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

        IPipelineSetup<TInput, TOutput> RemoveDuplicates();

        #region Native



        #endregion

        IPipelineSetup<TInput, TNextOutput> MergeWith<TNextOutput>(IPipelineSetup<TOutput, TNextOutput> pipelineSetup);

        IAwaitablePipelineRunner<TInput, TOutput> CreateAwaitable(AwaitablePipelineConfig pipelineConfig = null);

        IPipelineRunner<TInput, TOutput> Create(PipelineConfig pipelineConfig = null);
    }
}