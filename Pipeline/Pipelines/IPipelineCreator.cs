using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Dto;
using PipelineLauncher.Jobs;
using PipelineLauncher.PipelineSetup;

namespace PipelineLauncher.Pipelines
{
    public interface IPipelineCreator
    {
        IPipelineCreator WithToken(CancellationToken cancellationToken);

        IPipelineSetup<TInput, TOutput> BulkStage<TBulkJob, TInput, TOutput>()
            where TBulkJob : BulkJob<TInput, TOutput>;

        IPipelineSetup<TInput, TInput> BulkStage<TBulkJob, TInput>()
            where TBulkJob : BulkJob<TInput, TInput>;

        IPipelineSetup<TInput, TOutput> BulkStage<TBulkJob, TBulkJob2, TInput, TOutput>()
            where TBulkJob : BulkJob<TInput, TOutput>
            where TBulkJob2 : BulkJob<TInput, TOutput>;

        IPipelineSetup<TInput, TOutput> BulkStage<TBulkJob, TBulkJob2, TBulkJob3, TInput, TOutput>()
            where TBulkJob : BulkJob<TInput, TOutput>
            where TBulkJob2 : BulkJob<TInput, TOutput>
            where TBulkJob3 : BulkJob<TInput, TOutput>;

        IPipelineSetup<TInput, TOutput> BulkStage<TBulkJob, TBulkJob2, TBulkJob3, TBulkJob4, TInput, TOutput>()
            where TBulkJob : BulkJob<TInput, TOutput>
            where TBulkJob2 : BulkJob<TInput, TOutput>
            where TBulkJob3 : BulkJob<TInput, TOutput>
            where TBulkJob4 : BulkJob<TInput, TOutput>;

        IPipelineSetup<TInput, TOutput> Stage<TJob, TInput, TOutput>()
            where TJob : Job<TInput, TOutput>;

        IPipelineSetup<TInput, TInput> Stage<TJob, TInput>()
            where TJob : Job<TInput, TInput>;

        IPipelineSetup<TInput, TOutput> Stage<TJob, TJob2, TInput, TOutput>()
            where TJob : Job<TInput, TOutput>
            where TJob2 : Job<TInput, TOutput>;

        IPipelineSetup<TInput, TOutput> Stage<TJob, TJob2, TJob3, TInput, TOutput>()
            where TJob : Job<TInput, TOutput>
            where TJob2 : Job<TInput, TOutput>
            where TJob3 : Job<TInput, TOutput>;

        IPipelineSetup<TInput, TOutput> Stage<TJob, TJob2, TJob3, TJob4, TInput, TOutput>()
            where TJob : Job<TInput, TOutput>
            where TJob2 : Job<TInput, TOutput>
            where TJob3 : Job<TInput, TOutput>
            where TJob4 : Job<TInput, TOutput>;

        IPipelineSetup<TInput, TOutput> BulkStage<TInput, TOutput>(BulkJob<TInput, TOutput> bulkJob);

        IPipelineSetup<TInput, TOutput> BulkStage<TInput, TOutput>(Func<IEnumerable<TInput>, IEnumerable<TOutput>> bulkFunc);

        IPipelineSetup<TInput, TOutput> BulkStage<TInput, TOutput>(Func<IEnumerable<TInput>, Task<IEnumerable<TOutput>>> bulkFunc);

        IPipelineSetup<TInput, TOutput> BulkStage<TInput, TOutput>(params BulkJob<TInput, TOutput>[] bulkJobs);

        IPipelineSetup<TInput, TOutput> Stage<TInput, TOutput>(Job<TInput, TOutput> job);

        IPipelineSetup<TInput, TOutput> Stage<TInput, TOutput>(Func<TInput, TOutput> func);

        IPipelineSetup<TInput, TOutput> Stage<TInput, TOutput>(Func<TInput, StageOption<TInput, TOutput>,  TOutput> funcWithOption);

        IPipelineSetup<TInput, TOutput> Stage<TInput, TOutput>(Func<TInput, Task<TOutput>> func);

        IPipelineSetup<TInput, TOutput> Stage<TInput, TOutput>(Func<TInput, StageOption<TInput, TOutput>, Task<TOutput>> funcWithOption);

        IPipelineSetup<TInput, TOutput> Stage<TInput, TOutput>(params Job<TInput, TOutput>[] jobs);
    }
}