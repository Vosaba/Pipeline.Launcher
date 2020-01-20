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

        public abstract Task<IEnumerable<TOutput>> ExecuteAsync(IEnumerable<TInput> input, CancellationToken cancellationToken);

        public override async Task<IEnumerable<PipelineStageItem<TOutput>>> InternalExecute(IEnumerable<PipelineStageItem<TInput>> input, PipelineStageContext context)
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
                //context.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(() => context.ActionsSet.GetItemsHashCode(removeAndExceptionItems.Cast<object>().ToArray()), GetType(), DiagnosticState.Skip));
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
                //context.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(() => context.ActionsSet.GetItemsHashCode(skipItemsWithoutProcess.Cast<object>().ToArray()), GetType(), DiagnosticState.Skip));
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
                //context.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(() => context.ActionsSet.GetItemsHashCode(skipTillWithoutProcess.Cast<object>().ToArray()), GetType(), DiagnosticState.Skip));
            }

            itemsToProcess.AddRange(inputArray.Where(x => x.GetType() == typeof(PipelineStageItem<TInput>)).Select(x => x.Item));

            if (itemsToProcess.Any())
            {
                result.AddRange((await ExecuteAsync(itemsToProcess, context.CancellationToken)).Select(x => new PipelineStageItem<TOutput>(x)));
                //context.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(() => context.ActionsSet.GetItemsHashCode(itemsToProcess.Cast<object>().ToArray()), GetType(), DiagnosticState.Process));
            }

            return result;
        }

        protected override IEnumerable<PipelineStageItem<TOutput>> GetExceptionItem(IEnumerable<PipelineStageItem<TInput>> input, Exception ex, PipelineStageContext context)
        {
            return new[] { new ExceptionStageItem<TOutput>(ex, context.ActionsSet?.Retry, GetType(), GetOriginalItems(input)) };
        }

        protected override int[] GetItemsHashCode(IEnumerable<PipelineStageItem<TInput>> input, PipelineStageContext context)
        {
            return input.Select(x => context.ActionsSet.GetItemsHashCode(x)).ToArray();
        }
    }
}