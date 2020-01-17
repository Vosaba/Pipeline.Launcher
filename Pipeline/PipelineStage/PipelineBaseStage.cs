using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineEvents;
using PipelineLauncher.Abstractions.PipelineStage;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.Abstractions.PipelineStage.Dto;

namespace PipelineLauncher.PipelineStage
{
    public abstract class PipelineBaseStage<TInput, TOutput>
    {
        public virtual StageBaseConfiguration Configuration { get; }

        public abstract Task<IEnumerable<PipelineStageItem<TOutput>>> InternalExecute(IEnumerable<PipelineStageItem<TInput>> input, PipelineStageContext context);

        public async Task<IEnumerable<PipelineStageItem<TOutput>>> BaseExecute(IEnumerable<PipelineStageItem<TInput>> input, PipelineStageContext context)
        {
            var inputArray = input.ToArray();

            Func<int[]> getItemsHashCode = null;
            if (context.ActionsSet?.DiagnosticHandler != null)
            {
                int[] itemsHashCode = null;
                getItemsHashCode = () =>
                {
                    if (itemsHashCode == null)
                    {
                        itemsHashCode = context.ActionsSet.GetItemsHashCode(inputArray.Select(e => e.Item).Cast<object>().ToArray());
                    }

                    return itemsHashCode;
                };

                context.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(getItemsHashCode, GetType(), DiagnosticState.Enter));
            }

            try
            {
                var result = await InternalExecute(inputArray, context);

                context.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(getItemsHashCode, GetType(), DiagnosticState.Process));

                return result;
            }
            catch (Exception ex)
            {
                if (context.ActionsSet?.ExceptionHandler != null)
                {
                    var shouldBeReExecuted = false;

                    context.ActionsSet?.ExceptionHandler(
                        new ExceptionItemsEventArgs(
                            inputArray.Cast<object>().ToArray(), 
                            GetType(), 
                            ex, 
                            () => { shouldBeReExecuted = true; }));

                    if (shouldBeReExecuted)
                    {
                        return await InternalExecute(inputArray, context);
                    }
                }

                context.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(getItemsHashCode, GetType(), DiagnosticState.ExceptionOccured, ex.Message));

                return new[] { new ExceptionStageItem<TOutput>(ex, context.ActionsSet?.Retry, GetType(), inputArray.Select(e => e.Item)) };
            }
        }

        public async Task<PipelineStageItem<TOutput>> BaseExecute(PipelineStageItem<TInput> input, PipelineStageContext context)
        {
            return (await BaseExecute(new[] {input}, context)).First();
        }
    }
}