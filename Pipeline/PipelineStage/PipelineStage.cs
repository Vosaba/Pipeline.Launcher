using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.PipelineStage;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.Abstractions.PipelineStage.Dto;
using PipelineLauncher.Exceptions;

namespace PipelineLauncher.PipelineStage
{
    public abstract class PipelineStage<TInput, TOutput> : PipelineBaseStage<PipelineStageItem<TInput>, PipelineStageItem<TOutput>>, IPipelineStage<TInput, TOutput>
    {
        public new abstract StageConfiguration Configuration { get; }

        public abstract Task<TOutput> ExecuteAsync(TInput input, CancellationToken cancellationToken);

        protected override async Task<PipelineStageItem<TOutput>> InternalExecute(PipelineStageItem<TInput> input, PipelineStageContext context)
        {
            try
            {
                PipelineStageItem<TOutput> result;
                switch (input)
                {
                    case RemoveStageItem<TInput> removeItem:
                        result = removeItem.Return<TOutput>();
                        break;

                    case ExceptionStageItem<TInput> exceptionItem:
                        result = exceptionItem.Return<TOutput>();
                        break;

                    case SkipStageItem<TInput> skipItem when typeof(TInput) == skipItem.OriginalItem.GetType():
                        //context.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(() => context.ActionsSet.GetItemsHashCode(new[] { skipItem.OriginalItem }), GetType(), DiagnosticState.Process));
                        result = new PipelineStageItem<TOutput>(await ExecuteAsync((TInput)skipItem.OriginalItem, context.CancellationToken));
                        break;
                    case SkipStageItem<TInput> skipItem when typeof(TInput) != skipItem.OriginalItem.GetType():
                        result = skipItem.Return<TOutput>();
                        break;

                    case SkipStageItemTill<TInput> skipItemTill
                        when GetType() == skipItemTill.SkipTillType:
                        //context.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(() => context.ActionsSet.GetItemsHashCode(new[] { skipItemTill.OriginalItem }), GetType(), DiagnosticState.Process));
                        result = new PipelineStageItem<TOutput>(await ExecuteAsync((TInput)skipItemTill.OriginalItem, context.CancellationToken));
                        break;
                    case SkipStageItemTill<TInput> skipItemTill
                        when GetType() != skipItemTill.SkipTillType:
                        result = skipItemTill.Return<TOutput>();
                        break;

                    default:
                        result = new PipelineStageItem<TOutput>(await ExecuteAsync(input.Item, context.CancellationToken));
                        break;
                }

                return result;
            }
            catch (NoneParamException<TOutput> e)
            {
                //context.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(getItemsHashCode, GetType(), DiagnosticState.Skip));
                return e.StageItem;
            }
        }

        protected override PipelineStageItem<TOutput> GetExceptionItem(PipelineStageItem<TInput> input, Exception ex, PipelineStageContext context)
        {
            return new ExceptionStageItem<TOutput>(ex, context.ActionsSet?.Retry, GetType(), GetOriginalItems(input));
        }

        protected override int[] GetItemsHashCode(PipelineStageItem<TInput> input, PipelineStageContext context)
        {
            return new[] { context.ActionsSet.GetItemsHashCode(input.Item) };
        }

        protected override object[] GetOriginalItems(PipelineStageItem<TInput> input)
        {
            return new object[] { input.Item };
        }
    }
}
