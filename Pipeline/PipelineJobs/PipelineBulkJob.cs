using PipelineLauncher.Abstractions.Configurations;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Dto;

namespace PipelineLauncher.PipelineJobs
{
    public abstract class PipelineBulk<TInput, TOutput> : PipelineBase<TInput, TOutput>, IPipelineBulkJob<TInput, TOutput>
    {
        public abstract BulkJobConfiguration Configuration { get; }

        public abstract Task<IEnumerable<TOutput>> ExecuteAsync(IEnumerable<TInput> input, CancellationToken cancellationToken);

        public async Task<IEnumerable<PipelineItem<TOutput>>> InternalExecute(IEnumerable<PipelineItem<TInput>> input, CancellationToken cancellationToken, ActionsSet actionsSet)
        {
            var inputArray = input.ToArray();
            var result = new List<PipelineItem<TOutput>>();

            try
            {
                var removeAndExceptionItems = inputArray.Where(x =>
                {
                    var type = x.GetType();
                    return type == typeof(RemoveItem<TInput>) || type == typeof(ExceptionItem<TInput>);
                }).Cast<NoneResultItem<TInput>>().Select(x => x.Return<TOutput>());

                result.AddRange(removeAndExceptionItems);

                var skipItems = inputArray.Where(x =>
                {
                    var type = x.GetType();
                    return type == typeof(SkipItem<TInput>);
                }).Cast<SkipItem<TInput>>().ToArray();

                var executedSkipItems = skipItems.Any() ? await ExecuteAsync(skipItems
                    .Where(x => typeof(TInput) == x.OriginalItem.GetType()).Select(x => (TInput)x.OriginalItem), cancellationToken) : Array.Empty<TOutput>();

                result.AddRange(executedSkipItems.Select(x => new PipelineItem<TOutput>(x)));

                result.AddRange(skipItems.Where(x => typeof(TInput) != x.OriginalItem.GetType()).Select(x => x.Return<TOutput>()));

                var skipTillItems = inputArray.Where(x =>
                {
                    var type = x.GetType();
                    return type == typeof(SkipItemTill<TInput>);
                }).Cast<SkipItemTill<TInput>>().ToArray();

                var executedSkipTillItems = skipTillItems.Any() ? await ExecuteAsync(skipTillItems
                    .Where(x => GetType() == x.SkipTillType).Select(x => (TInput)x.OriginalItem), cancellationToken) : Array.Empty<TOutput>();

                result.AddRange(executedSkipTillItems.Select(x => new PipelineItem<TOutput>(x)));

                result.AddRange(skipTillItems.Where(x => GetType() != x.SkipTillType).Select(x => x.Return<TOutput>()));

                var executedItems = await ExecuteAsync(inputArray.Where(x => x.GetType() == typeof(PipelineItem<TInput>)).Select(x => x.Item), cancellationToken);
                result.AddRange(executedItems.Select(x => new PipelineItem<TOutput>(x)));

                return result;
            }
            catch (Exception ex)
            {
                return new[] { new ExceptionItem<TOutput>(ex, actionsSet.ReExecute, GetType(), inputArray.Select(e => e.Item)) };
            }
        }
    }
}