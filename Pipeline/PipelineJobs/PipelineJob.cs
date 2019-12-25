using System;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Configurations;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.Pipeline;
using PipelineLauncher.Abstractions.PipelineEvents;
using PipelineLauncher.Dto;
using PipelineLauncher.Exceptions;

namespace PipelineLauncher.PipelineJobs
{
    public abstract class Pipeline<TInput, TOutput> : PipelineBase<TInput, TOutput>, IPipelineJob<TInput, TOutput>
    {
        public abstract JobConfiguration Configuration { get; }

        public abstract Task<TOutput> ExecuteAsync(TInput input, CancellationToken cancellationToken);

        public async Task<PipelineItem<TOutput>> InternalExecute(PipelineItem<TInput> input, CancellationToken cancellationToken, ActionsSet actionsSet)
        {
            DiagnosticEventArgs diagnosticEventArgs = new DiagnosticEventArgs(GetType());
            actionsSet.DiagnosticAction?.Invoke(diagnosticEventArgs);

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
                            cancellationToken));
                        break;
                    case SkipItem<TInput> skipItem when typeof(TInput) != skipItem.OriginalItem.GetType():
                        result = skipItem.Return<TOutput>();
                        break;

                    case SkipItemTill<TInput> skipItemTill
                        when GetType() == skipItemTill.SkipTillType:
                        result = new PipelineItem<TOutput>(await ExecuteAsync((TInput) skipItemTill.OriginalItem,
                            cancellationToken));
                        break;
                    case SkipItemTill<TInput> skipItemTill
                        when GetType() != skipItemTill.SkipTillType:
                        result = skipItemTill.Return<TOutput>();
                        break;

                    default:
                        result = new PipelineItem<TOutput>(await ExecuteAsync(input.Item, cancellationToken));
                        break;
                }

                actionsSet.DiagnosticAction?.Invoke(diagnosticEventArgs.Finish());
                return result;
            }
            catch (NoneParamException<TOutput> e)
            {
                actionsSet.DiagnosticAction?.Invoke(diagnosticEventArgs.Finish(DiagnosticState.Skipped));
                return e.Item;
            }
            catch (Exception e)
            {
                actionsSet.DiagnosticAction?.Invoke(diagnosticEventArgs.Finish(DiagnosticState.ExceptionOccured));
                return new ExceptionItem<TOutput>(e, actionsSet.ReExecute, GetType(), input != null ? input.Item : default);
            }
        }
    }
}
