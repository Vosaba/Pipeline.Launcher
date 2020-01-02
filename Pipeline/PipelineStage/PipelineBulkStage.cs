using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineEvents;
using PipelineLauncher.Abstractions.PipelineStage;
using PipelineLauncher.Abstractions.PipelineStage.Configuration;
using PipelineLauncher.Abstractions.PipelineStage.Dto;

namespace PipelineLauncher.PipelineStage
{
    public abstract class PipelineBulkStage<TInput, TOutput> : PipelineBaseStage<TInput, TOutput>, IPipelineBulkStage<TInput, TOutput>
    {
        public abstract BulkStageConfiguration Configuration { get; }

        public abstract Task<IEnumerable<TOutput>> ExecuteAsync(IEnumerable<TInput> input, CancellationToken cancellationToken);

        public async Task<IEnumerable<PipelineStageItem<TOutput>>> InternalExecute(IEnumerable<PipelineStageItem<TInput>> input, PipelineStageContext context)
        {
            DiagnosticItem diagnosticItem = new DiagnosticItem(GetType());
            context.ActionsSet?.DiagnosticAction?.Invoke(diagnosticItem);

            var inputArray = input.ToArray();
            var result = new List<PipelineStageItem<TOutput>>();

            try
            {
                var removeAndExceptionItems = inputArray.Where(x =>
                {
                    var type = x.GetType();
                    return type == typeof(RemoveStageItem<TInput>) || type == typeof(ExceptionStageItem<TInput>);
                }).Cast<NoneResultStageItem<TInput>>().Select(x => x.Return<TOutput>());

                result.AddRange(removeAndExceptionItems);

                var skipItems = inputArray.Where(x =>
                {
                    var type = x.GetType();
                    return type == typeof(SkipStageItem<TInput>);
                }).Cast<SkipStageItem<TInput>>().ToArray();

                var executedSkipItems = skipItems.Any() ? await ExecuteAsync(skipItems
                    .Where(x => typeof(TInput) == x.OriginalItem.GetType()).Select(x => (TInput)x.OriginalItem), context.CancellationToken) : Array.Empty<TOutput>();

                result.AddRange(executedSkipItems.Select(x => new PipelineStageItem<TOutput>(x)));

                result.AddRange(skipItems.Where(x => typeof(TInput) != x.OriginalItem.GetType()).Select(x => x.Return<TOutput>()));

                var skipTillItems = inputArray.Where(x =>
                {
                    var type = x.GetType();
                    return type == typeof(SkipStageItemTill<TInput>);
                }).Cast<SkipStageItemTill<TInput>>().ToArray();

                var executedSkipTillItems = skipTillItems.Any() ? await ExecuteAsync(skipTillItems
                    .Where(x => GetType() == x.SkipTillType).Select(x => (TInput)x.OriginalItem), context.CancellationToken) : Array.Empty<TOutput>();

                result.AddRange(executedSkipTillItems.Select(x => new PipelineStageItem<TOutput>(x)));

                result.AddRange(skipTillItems.Where(x => GetType() != x.SkipTillType).Select(x => x.Return<TOutput>()));

                var executedItems = await ExecuteAsync(inputArray.Where(x => x.GetType() == typeof(PipelineStageItem<TInput>))
                    .Select(x => x.Item), context.CancellationToken);
                result.AddRange(executedItems.Select(x => new PipelineStageItem<TOutput>(x)));

                context.ActionsSet?.DiagnosticAction?.Invoke(diagnosticItem.Finish());
                return result;
            }
            catch (Exception ex)
            {
                context.ActionsSet?.DiagnosticAction?.Invoke(diagnosticItem.Finish(DiagnosticState.ExceptionOccured, ex.Message));
                return new[] { new ExceptionStageItem<TOutput>(ex, context.ActionsSet?.ReExecute, GetType(), inputArray.Select(e => e.Item)) };
            }
        }
    }
}