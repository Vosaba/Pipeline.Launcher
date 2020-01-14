using System;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineEvents;
using PipelineLauncher.Abstractions.PipelineStage;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.Abstractions.PipelineStage.Dto;
using PipelineLauncher.Exceptions;

namespace PipelineLauncher.PipelineStage
{
    public abstract class PipelineStage<TInput, TOutput> : PipelineBaseStage<TInput, TOutput>, IPipelineStage<TInput, TOutput>
    {
        public abstract StageConfiguration Configuration { get; }

        public abstract Task<TOutput> ExecuteAsync(TInput input, CancellationToken cancellationToken);

        public async Task<PipelineStageItem<TOutput>> InternalExecute(PipelineStageItem<TInput> input, PipelineStageContext context)
        {
            context.ActionsSet.Processed?.Invoke(1);

            Func<int[]> getItemsHashCode = null;
            if (context.ActionsSet?.DiagnosticAction != null)
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

                context.ActionsSet?.DiagnosticAction?.Invoke(new DiagnosticItem(getItemsHashCode, GetType(), DiagnosticState.Enter));
            }

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
                        context.ActionsSet?.DiagnosticAction?.Invoke(
                            new DiagnosticItem(
                                () => context.ActionsSet.GetItemsHashCode(new[] { skipItem.OriginalItem }),
                                GetType(), DiagnosticState.Process));
                        result = new PipelineStageItem<TOutput>(await ExecuteAsync((TInput) skipItem.OriginalItem,
                            context.CancellationToken));
                        break;
                    case SkipStageItem<TInput> skipItem when typeof(TInput) != skipItem.OriginalItem.GetType():
                        result = skipItem.Return<TOutput>();
                        break;

                    case SkipStageItemTill<TInput> skipItemTill
                        when GetType() == skipItemTill.SkipTillType:
                        context.ActionsSet?.DiagnosticAction?.Invoke(
                            new DiagnosticItem(
                                () => context.ActionsSet.GetItemsHashCode(new [] { skipItemTill.OriginalItem }),
                                GetType(), DiagnosticState.Process));
                        result = new PipelineStageItem<TOutput>(await ExecuteAsync((TInput) skipItemTill.OriginalItem,
                            context.CancellationToken));
                        break;
                    case SkipStageItemTill<TInput> skipItemTill
                        when GetType() != skipItemTill.SkipTillType:
                        result = skipItemTill.Return<TOutput>();
                        break;

                    default:
                        context.ActionsSet?.DiagnosticAction?.Invoke(
                            new DiagnosticItem(getItemsHashCode, GetType(), DiagnosticState.Process));
                        result = new PipelineStageItem<TOutput>(await ExecuteAsync(input.Item, context.CancellationToken));
                        break;
                }

                context.ActionsSet.Processed?.Invoke(-1);
                return result;
            }
            catch (NoneParamException<TOutput> e)
            {
                context.ActionsSet?.DiagnosticAction?.Invoke(new DiagnosticItem(getItemsHashCode, GetType(), DiagnosticState.Skip));
                return e.StageItem;
            }
            catch (Exception e)
            {
                context.ActionsSet?.Processed(0);
                context.ActionsSet?.DiagnosticAction?.Invoke(new DiagnosticItem(getItemsHashCode, GetType(), DiagnosticState.ExceptionOccured, e.Message));
                return new ExceptionStageItem<TOutput>(e, context.ActionsSet?.ReExecute, GetType(), input != null ? input.Item : default);
            }
        }
    }
}
