using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.PipelineSetup;
using PipelineLauncher.Stages;

namespace PipelineLauncher.Extensions.Stages
{
    public static class Stages
    {
        public static IPipelineSetup<TInput, TNextOutput> SingleThreadStage<TInput, TOutput, TNextOutput>(
            this IPipelineSetup<TInput, TOutput> pipelineSetup, Func<TOutput, TNextOutput> func)
            => pipelineSetup.Stage(func, new StageConfiguration {MaxDegreeOfParallelism = 1});

        public static IPipelineSetup<TInput, TNextOutput> SingleThreadStage<TInput, TOutput, TNextOutput>(this IPipelineSetup<TInput, TOutput> pipelineSetup, Func<TOutput, StageOption<TOutput, TNextOutput>, TNextOutput> funcWithOption)
            => pipelineSetup.Stage(funcWithOption, new StageConfiguration { MaxDegreeOfParallelism = 1 });

        public static IPipelineSetup<TInput, TNextOutput> SingleThreadStage<TInput, TOutput, TNextOutput>(this IPipelineSetup<TInput, TOutput> pipelineSetup, Func<TOutput, Task<TNextOutput>> func)
            => pipelineSetup.Stage(func, new StageConfiguration { MaxDegreeOfParallelism = 1 });

        public static IPipelineSetup<TInput, TNextOutput> SingleThreadStage<TInput, TOutput, TNextOutput>(this IPipelineSetup<TInput, TOutput> pipelineSetup, Func<TOutput, StageOption<TOutput, TNextOutput>, Task<TNextOutput>> funcWithOption)
            => pipelineSetup.Stage(funcWithOption, new StageConfiguration { MaxDegreeOfParallelism = 1 });

        public static IPipelineSetup<TInput, TNextOutput> SingleThreadBulkStage<TInput, TOutput, TNextOutput>(this IPipelineSetup<TInput, TOutput> pipelineSetup, Func<TOutput[], IEnumerable<TNextOutput>> func)
            => pipelineSetup.BulkStage(func, new BulkStageConfiguration { MaxDegreeOfParallelism = 1 });

        public static IPipelineSetup<TInput, TNextOutput> SingleThreadBulkStage<TInput, TOutput, TNextOutput>(this IPipelineSetup<TInput, TOutput> pipelineSetup, Func<TOutput[], Task<IEnumerable<TNextOutput>>> func)
            => pipelineSetup.BulkStage(func, new BulkStageConfiguration { MaxDegreeOfParallelism = 1 });
    }
}
