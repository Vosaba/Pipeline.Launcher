using PipelineLauncher.Abstractions.PipelineEvents;
using PipelineLauncher.Abstractions.PipelineStage;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.Abstractions.PipelineStage.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Dto;

namespace PipelineLauncher.PipelineStage
{
    public abstract class PipelineBulkStage<TInput, TOutput> : PipelineBaseStage<IEnumerable<PipelineStageItem<TInput>>, IEnumerable<PipelineStageItem<TOutput>>>, IPipelineBulkStage<TInput, TOutput>
    {
        public new abstract BulkStageConfiguration Configuration { get; }

        public abstract Task<IEnumerable<TOutput>> ExecuteAsync(TInput[] input, CancellationToken cancellationToken);

        protected override async Task<IEnumerable<PipelineStageItem<TOutput>>> InternalExecute(IEnumerable<PipelineStageItem<TInput>> input, PipelineStageContext context)
        {
            var inputArray = input.ToArray();

            var itemsToProcess = new List<TInput>();
            var result = new List<PipelineStageItem<TOutput>>();

            var removeAndExceptionItems = inputArray.Where(x =>
            {
                var type = x.GetType();
                return type == typeof(RemoveStageItem<TInput>) || type == typeof(ExceptionStageItem<TInput>);
            }).Cast<NoneResultStageItem<TInput>>().Select(x => x.Return<TOutput>()).ToArray();

            if (removeAndExceptionItems.Any())
            {
                result.AddRange(removeAndExceptionItems);
            }

            var skipItems = inputArray.Where(x =>
            {
                var type = x.GetType();
                return type == typeof(SkipStageItem<TInput>);
            }).Cast<SkipStageItem<TInput>>().ToArray();

            itemsToProcess.AddRange(skipItems.Where(x => typeof(TInput) == x.OriginalItem.GetType()).Select(x => (TInput)x.OriginalItem));

            var skipItemsWithoutProcess = skipItems.Where(x => typeof(TInput) != x.OriginalItem.GetType()).Select(x => x.Return<TOutput>()).ToArray();

            if (skipItemsWithoutProcess.Any())
            {
                result.AddRange(skipItemsWithoutProcess);
            }
            
            var skipTillItems = inputArray.Where(x =>
            {
                var type = x.GetType();
                return type == typeof(SkipStageItemTill<TInput>);
            }).Cast<SkipStageItemTill<TInput>>().ToArray();

            itemsToProcess.AddRange(skipTillItems.Where(x => GetType() == x.SkipTillType).Select(x => (TInput)x.OriginalItem));

            var skipTillWithoutProcess = skipTillItems.Where(x => GetType() != x.SkipTillType).Select(x => x.Return<TOutput>()).ToArray();

            if (skipTillWithoutProcess.Any())
            {
                result.AddRange(skipTillWithoutProcess);
            }

            itemsToProcess.AddRange(inputArray.Where(x => x.GetType() == typeof(PipelineStageItem<TInput>)).Select(x => x.Item));

            if (itemsToProcess.Any())
            {
                result.AddRange((await ExecuteAsync(itemsToProcess.ToArray(), context.CancellationToken)).Select(x => new PipelineStageItem<TOutput>(x)));
            }

            return result;
        }

        protected override IEnumerable<PipelineStageItem<TOutput>> GetExceptionItem(IEnumerable<PipelineStageItem<TInput>> input, Exception ex, PipelineStageContext context)
        {
            return new[] { new ExceptionStageItem<TOutput>(ex, context.ActionsSet?.Retry, GetType(), GetOriginalItems(input)) };
        }

        protected override object[] GetOriginalItems(IEnumerable<PipelineStageItem<TInput>> input)
        {
            return input.Select(x => x.Item).Cast<object>().ToArray();
        }
    }
}