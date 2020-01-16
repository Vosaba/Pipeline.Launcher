using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineStage;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.Abstractions.PipelineStage.Dto;
using PipelineLauncher.Abstractions.Stages;

namespace PipelineLauncher.PipelineStage
{
    public abstract class PipelineBaseStage<TInput, TOutput> //:  IStage<TInput, TOutput>
    {
        // public abstract StageBaseConfiguration Configuration { get; }

        public Task<TOutput> InternalExecute(TInput input, PipelineStageContext context)
        {
            Func<int[]> getItemsHashCode = null;
            if (context.ActionsSet?.DiagnosticHandler != null)
            {
                int[] itemsHashCode = null;
                getItemsHashCode = () =>
                {
                    if (itemsHashCode == null)
                    {
                        itemsHashCode = context.ActionsSet.GetItemsHashCode(new object[] { input.Item });
                    }

                    return itemsHashCode;
                };

                context.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(getItemsHashCode, GetType(), DiagnosticState.Enter));
            }


        }
    }
}