using PipelineLauncher.Abstractions.PipelineEvents;
using PipelineLauncher.Abstractions.PipelineStage;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.Abstractions.PipelineStage.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Dto;

namespace PipelineLauncher.PipelineStage
{
    public abstract class PipelineBulkStage<TInput, TOutput> : PipelineBaseStage<IEnumerable<PipelineStageItem<TInput>>, IEnumerable<PipelineStageItem<TOutput>>>, IPipelineBulkStage<TInput, TOutput>
    {
        public abstract BulkStageConfiguration Configuration { get; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        protected override StageBaseConfiguration BaseConfiguration => Configuration;

        public abstract Task<IEnumerable<TOutput>> ExecuteAsync(TInput[] input, CancellationToken cancellationToken);

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        protected override async Task<IEnumerable<PipelineStageItem<TOutput>>> InternalExecute(IEnumerable<PipelineStageItem<TInput>> input, PipelineStageContext context)
        {
            var inputArray = input as PipelineStageItem<TInput>[] ?? input.ToArray();

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

            itemsToProcess.AddRange(skipItems.Where(x => typeof(TInput) == x.OriginalItem.GetType() && x.ReadyToProcess).Select(x => (TInput)x.OriginalItem));

            var skipItemsWithoutProcess = skipItems.Where(x => typeof(TInput) != x.OriginalItem.GetType() || !x.ReadyToProcess).Select(x => x.Return<TOutput>()).ToArray();

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

            if (skipTillWithoutProcess.Any() || skipItemsWithoutProcess.Any())
            {
                context.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(skipTillWithoutProcess.Concat(skipItemsWithoutProcess).Select(x => x.OriginalItem).ToArray(), GetType(), DiagnosticState.Skip));
            }

            itemsToProcess.AddRange(inputArray.Where(x => x.GetType() == typeof(PipelineStageItem<TInput>)).Select(x => x.Item));

            if (itemsToProcess.Any())
            {
                result.AddRange((await ExecuteAsync(itemsToProcess.ToArray(), context.CancellationToken)).Select(x => new PipelineStageItem<TOutput>(x)));
            }

            return result;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        protected override IEnumerable<PipelineStageItem<TOutput>> GetExceptionItem(IEnumerable<PipelineStageItem<TInput>> input, Exception ex, PipelineStageContext context)
        {
            return new[] { new ExceptionStageItem<TOutput>(ex, context.ActionsSet?.Retry, GetType(), GetOriginalItems(input)) };
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        protected override object[] GetOriginalItems(IEnumerable<PipelineStageItem<TInput>> input)
        {
            var pipelineStageItems = input as PipelineStageItem<TInput>[] ?? input.ToArray();

            var skipItems = pipelineStageItems
                .Where(x =>
                {
                    var type = x.GetType();
                    return type == typeof(SkipStageItem<TInput>);
                })
                .Cast<SkipStageItem<TInput>>()
                .Where(x => typeof(TInput) == x.OriginalItem.GetType() && x.ReadyToProcess)
                .Select(x => x.OriginalItem);

            var skipTillItems = pipelineStageItems
                .Where(x =>
                {
                    var type = x.GetType();
                    return type == typeof(SkipStageItemTill<TInput>);
                })
                .Cast<SkipStageItemTill<TInput>>()
                .Where(x => GetType() == x.SkipTillType)
                .Select(e => e.OriginalItem);

            var resultItems = pipelineStageItems
                .Where(x => x.GetType() == typeof(PipelineStageItem<TInput>))
                .Select(x => x.Item)
                .Cast<object>();

            return resultItems.Concat(skipItems).Concat(skipTillItems).ToArray();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        protected override object[] GetOriginalItems(IEnumerable<PipelineStageItem<TOutput>> output)
        {
            var pipelineStageItems = output as PipelineStageItem<TOutput>[] ?? output.ToArray();

            //var exceptionItems = pipelineStageItems
            //    .Where(x =>
            //    {
            //        var type = x.GetType();
            //        return type == typeof(ExceptionStageItem<TOutput>);
            //    })
            //    .Cast<ExceptionStageItem<TOutput>>()
            //    .Select(x => x.FailedItems)
            //    .SelectMany(x => x); ;

            //var skipItems = pipelineStageItems
            //    .Where(x =>
            //    {
            //        var type = x.GetType();
            //        return type == typeof(SkipStageItemTill<TOutput>) || type == typeof(SkipStageItem<TOutput>);
            //    })
            //    .Cast<NoneResultStageItem<TOutput>>()
            //    .Select(x => x.OriginalItem);

            var resultItems = pipelineStageItems
                .Where(x => x.GetType() == typeof(PipelineStageItem<TOutput>))
                .Select(x => x.Item)
                .Cast<object>();

            return resultItems.ToArray();
        }
    }
}