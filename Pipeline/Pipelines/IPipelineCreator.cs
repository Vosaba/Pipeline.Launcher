using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Configurations;
using PipelineLauncher.Abstractions.PipelineEvents;
using PipelineLauncher.Dto;
using PipelineLauncher.Jobs;
using PipelineLauncher.PipelineSetup;

namespace PipelineLauncher.Pipelines
{
    public interface IPipelineCreator
    {
        IPipelineCreator WithToken(CancellationToken cancellationToken);

        IPipelineCreator WithDiagnostic(Action<DiagnosticEventArgs> diagnosticHandler);

        IPipelineSetup<TInput, TInput> Prepare<TInput>();

        IPipelineSetup<TInput, TInput> BulkPrepare<TInput>(BulkJobConfiguration jobConfiguration = null);

        #region Generic

        #region BulkStages

        IPipelineSetup<TInput, TOutput> BulkStage<TBulkJob, TInput, TOutput>()
            where TBulkJob : Bulk<TInput, TOutput>;

        IPipelineSetup<TInput, TInput> BulkStage<TBulkJob, TInput>()
            where TBulkJob : Bulk<TInput, TInput>;

        #endregion

        #region Stages

        IPipelineSetup<TInput, TOutput> Stage<TJob, TInput, TOutput>()
            where TJob : Job<TInput, TOutput>;

        IPipelineSetup<TInput, TInput> Stage<TJob, TInput>()
            where TJob : Job<TInput, TInput>;

        #endregion

        #endregion

        #region Nongeneric

        #region BulkStages

        IPipelineSetup<TInput, TOutput> BulkStage<TInput, TOutput>(Bulk<TInput, TOutput> bulk);

        IPipelineSetup<TInput, TOutput> BulkStage<TInput, TOutput>(Func<IEnumerable<TInput>, IEnumerable<TOutput>> bulkFunc, BulkJobConfiguration bulkJobConfiguration = null);

        IPipelineSetup<TInput, TOutput> BulkStage<TInput, TOutput>(Func<IEnumerable<TInput>, Task<IEnumerable<TOutput>>> bulkFunc, BulkJobConfiguration bulkJobConfiguration = null);

        #endregion

        #region Stages

        IPipelineSetup<TInput, TOutput> Stage<TInput, TOutput>(Job<TInput, TOutput> job);

        IPipelineSetup<TInput, TOutput> Stage<TInput, TOutput>(Func<TInput, TOutput> func);

        IPipelineSetup<TInput, TOutput> Stage<TInput, TOutput>(Func<TInput, StageOption<TInput, TOutput>,  TOutput> funcWithOption);

        IPipelineSetup<TInput, TOutput> Stage<TInput, TOutput>(Func<TInput, Task<TOutput>> func);

        IPipelineSetup<TInput, TOutput> Stage<TInput, TOutput>(Func<TInput, StageOption<TInput, TOutput>, Task<TOutput>> funcWithOption);

        #endregion

        #endregion
    }
}