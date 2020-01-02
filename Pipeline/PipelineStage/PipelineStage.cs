using System;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Configurations;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.Pipeline;
using PipelineLauncher.Abstractions.PipelineEvents;
using PipelineLauncher.Dto;
using PipelineLauncher.Exceptions;

namespace PipelineLauncher.PipelineStage
{
    public abstract class PipelineStage<TInput, TOutput> : PipelineBaseStage<TInput, TOutput>, IPipelineStage<TInput, TOutput>
    {
        public abstract StageConfiguration Configuration { get; }

        public abstract Task<TOutput> ExecuteAsync(TInput input, CancellationToken cancellationToken);

        public async Task<PipelineItem<TOutput>> InternalExecute(PipelineItem<TInput> input, PipelineStageContext context)
        {
            DiagnosticItem diagnosticItem = new DiagnosticItem(GetType());
            context.ActionsSet?.DiagnosticAction?.Invoke(diagnosticItem);

            try
            {
                PipelineItem<TOutput> result;
                switch (input)
                {
                    case RemoveItem<TInput> removeItem:
                        result = removeItem.Return<TOutput>();
                        break;

                    case ExceptionItem<TInput> exceptionItem:
                        result = exceptionItem.Return<TOutput>();
                        break;

                    case SkipItem<TInput> skipItem when typeof(TInput) == skipItem.OriginalItem.GetType():
                        result = new PipelineItem<TOutput>(await ExecuteAsync((TInput) skipItem.OriginalItem,
                            context.CancellationToken));
                        break;
                    case SkipItem<TInput> skipItem when typeof(TInput) != skipItem.OriginalItem.GetType():
                        result = skipItem.Return<TOutput>();
                        break;

                    case SkipItemTill<TInput> skipItemTill
                        when GetType() == skipItemTill.SkipTillType:
                        result = new PipelineItem<TOutput>(await ExecuteAsync((TInput) skipItemTill.OriginalItem,
                            context.CancellationToken));
                        break;
                    case SkipItemTill<TInput> skipItemTill
                        when GetType() != skipItemTill.SkipTillType:
                        result = skipItemTill.Return<TOutput>();
                        break;

                    default:
                        result = new PipelineItem<TOutput>(await ExecuteAsync(input.Item, context.CancellationToken));
                        break;
                }

                context.ActionsSet?.DiagnosticAction?.Invoke(diagnosticItem.Finish());
                return result;
            }
            catch (NoneParamException<TOutput> e)
            {
                context.ActionsSet?.DiagnosticAction?.Invoke(diagnosticItem.Finish(DiagnosticState.Skipped));
                return e.Item;
            }
            catch (Exception e)
            {
                context.ActionsSet?.DiagnosticAction?.Invoke(diagnosticItem.Finish(DiagnosticState.ExceptionOccured, e.Message));
                return new ExceptionItem<TOutput>(e, context.ActionsSet?.ReExecute, GetType(), input != null ? input.Item : default);
            }
        }
    }
}
