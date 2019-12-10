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

        IPipelineSetup<TInput, TOutput> AsyncStage<TAsyncJob, TInput, TOutput>()
            where TAsyncJob : AsyncJob<TInput, TOutput>;

        IPipelineSetup<TInput, TInput> AsyncStage<TAsyncJob, TInput>()
            where TAsyncJob : AsyncJob<TInput, TInput>;

        IPipelineSetup<TInput, TOutput> AsyncStage<TAsyncJob, TAsyncJob2, TInput, TOutput>()
            where TAsyncJob : AsyncJob<TInput, TOutput>
            where TAsyncJob2 : AsyncJob<TInput, TOutput>;

        IPipelineSetup<TInput, TOutput> AsyncStage<TAsyncJob, TAsyncJob2, TAsyncJob3, TInput, TOutput>()
            where TAsyncJob : AsyncJob<TInput, TOutput>
            where TAsyncJob2 : AsyncJob<TInput, TOutput>
            where TAsyncJob3 : AsyncJob<TInput, TOutput>;

        IPipelineSetup<TInput, TOutput> AsyncStage<TAsyncJob, TAsyncJob2, TAsyncJob3, TAsyncJob4, TInput, TOutput>()
            where TAsyncJob : AsyncJob<TInput, TOutput>
            where TAsyncJob2 : AsyncJob<TInput, TOutput>
            where TAsyncJob3 : AsyncJob<TInput, TOutput>
            where TAsyncJob4 : AsyncJob<TInput, TOutput>;

        IPipelineSetup<TInput, TOutput> Stage<TInput, TOutput>(Job<TInput, TOutput> job);

        IPipelineSetup<TInput, TOutput> Stage<TInput, TOutput>(Func<IEnumerable<TInput>, IEnumerable<TOutput>> func);

        IPipelineSetup<TInput, TOutput> Stage<TInput, TOutput>(Func<IEnumerable<TInput>, Task<IEnumerable<TOutput>>> func);

        IPipelineSetup<TInput, TOutput> Stage<TInput, TOutput>(params Job<TInput, TOutput>[] jobs);

        IPipelineSetup<TInput, TOutput> AsyncStage<TInput, TOutput>(AsyncJob<TInput, TOutput> asyncJob);

        IPipelineSetup<TInput, TOutput> AsyncStage<TInput, TOutput>(Func<TInput, TOutput> func);

        IPipelineSetup<TInput, TOutput> AsyncStage<TInput, TOutput>(Func<TInput, AsyncJobOption<TInput, TOutput>,  TOutput> asyncFuncWithOption);

        IPipelineSetup<TInput, TOutput> AsyncStage<TInput, TOutput>(Func<TInput, Task<TOutput>> func);

        IPipelineSetup<TInput, TOutput> AsyncStage<TInput, TOutput>(Func<TInput, AsyncJobOption<TInput, TOutput>, Task<TOutput>> asyncFuncWithOption);

        IPipelineSetup<TInput, TOutput> AsyncStage<TInput, TOutput>(params AsyncJob<TInput, TOutput>[] asyncJobs);
    }
}