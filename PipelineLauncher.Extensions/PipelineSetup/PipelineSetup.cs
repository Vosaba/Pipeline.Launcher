using System;
using System.Collections.Generic;
using PipelineLauncher.Extensions.Models;
using PipelineLauncher.PipelineSetup;
using System.Linq;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;

namespace PipelineLauncher.Extensions.PipelineSetup
{
    public static class PipelineSetup
    {
        public static IPipelineSetup<TPipelineInput, (TPipelineOutput input, TData data)> WrapInputAsTuple<TPipelineInput, TPipelineOutput, TData>(this IPipelineSetup<TPipelineInput, TPipelineOutput> pipelineSetup, TData data) =>
            pipelineSetup.Stage(input => (input, data));

        public static IPipelineSetup<TPipelineInput, WrappedInput<TPipelineOutput, TData>> WrapInput<TPipelineInput, TPipelineOutput, TData>(this IPipelineSetup<TPipelineInput, TPipelineOutput> pipelineSetup, TData data) =>
            pipelineSetup.Stage(input => new WrappedInput<TPipelineOutput, TData>(input, data));

        public static IPipelineSetup<TPipelineInput, TPipelineOutput> UnwrapInput<TPipelineInput, TPipelineOutput, TData>(this IPipelineSetup<TPipelineInput, (TPipelineOutput input, TData data)> pipelineSetup) =>
            pipelineSetup.Stage(wrappedInput => wrappedInput.input);

        public static IPipelineSetup<TPipelineInput, TPipelineOutput> UnwrapInput<TPipelineInput, TPipelineOutput, TData>(this IPipelineSetup<TPipelineInput, WrappedInput<TPipelineOutput, TData>> pipelineSetup) =>
            pipelineSetup.Stage(wrappedInput => wrappedInput.Input);

        public static IPipelineSetup<TPipelineInput, GroupedInput<TKey, TPipelineOutput>> GroupInput<TPipelineInput, TPipelineOutput, TKey>(this IPipelineSetup<TPipelineInput, TPipelineOutput> pipelineSetup, Func<TPipelineOutput, TKey> keySelector, BulkStageConfiguration bulkStageConfiguration = null) =>
            pipelineSetup
                .BulkStage<GroupedInput<TKey, TPipelineOutput>>(
                    input => input
                        .GroupBy(keySelector)
                        .Select(x => new GroupedInput<TKey, TPipelineOutput>(x.Key, x.ToArray()))
                        .ToArray(),
                    bulkStageConfiguration ?? Configurations.BulkStage.TakeWhileInputAvailable);

        public static IPipelineSetup<TPipelineInput, GroupedInput<TKey, TPipelineOutput>> GroupInput<TPipelineInput, TPipelineOutput, TKey>(this IPipelineSetup<TPipelineInput, TPipelineOutput> pipelineSetup, Func<TPipelineOutput, TKey> keySelector, IEqualityComparer<TKey> comparer, BulkStageConfiguration bulkStageConfiguration = null) =>
            pipelineSetup
                .BulkStage<GroupedInput<TKey, TPipelineOutput>>(
                    input => input
                        .GroupBy(keySelector, comparer)
                        .Select(x => new GroupedInput<TKey, TPipelineOutput>(x.Key, x.ToArray()))
                        .ToArray(),
                    bulkStageConfiguration ?? Configurations.BulkStage.TakeWhileInputAvailable);

        public static IPipelineSetup<TPipelineInput, TPipelineOutput> UngroupInput<TPipelineInput, TPipelineOutput, TKey>(this IPipelineSetup<TPipelineInput, GroupedInput<TKey, TPipelineOutput>> pipelineSetup, BulkStageConfiguration bulkStageConfiguration = null) =>
            pipelineSetup
                .BulkStage<TPipelineOutput>(
                    input => input
                        .SelectMany(x => x.Group)
                        .ToArray(),
                    bulkStageConfiguration ?? Configurations.BulkStage.TakeUntilSizeIsFilled(1));
    }
}
