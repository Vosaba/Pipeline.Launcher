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
            DiagnosticEventArgs diagnosticEventArgs = default;

            if (actionsSet.DiagnosticAction != null)
            {
                diagnosticEventArgs = new DiagnosticEventArgs(GetType());
            }

            try
            {
                switch (input)
                {
                    case RemoveItem<TInput> removeItem:
                        return removeItem.Return<TOutput>();

                    case ExceptionItem<TInput> exceptionItem:
                        return exceptionItem.Return<TOutput>();

                    case SkipItem<TInput> skipItem when typeof(TInput) == skipItem.OriginalItem.GetType():
                        return new PipelineItem<TOutput>(await ExecuteAsync((TInput) skipItem.OriginalItem,
                            cancellationToken));
                    case SkipItem<TInput> skipItem when typeof(TInput) != skipItem.OriginalItem.GetType():
                        return skipItem.Return<TOutput>();

                    case SkipItemTill<TInput> skipItemTill
                        when GetType() == skipItemTill.SkipTillType:
                        return new PipelineItem<TOutput>(await ExecuteAsync((TInput) skipItemTill.OriginalItem,
                            cancellationToken));
                    case SkipItemTill<TInput> skipItemTill
                        when GetType() != skipItemTill.SkipTillType:
                        return skipItemTill.Return<TOutput>();

                    default:
                        return new PipelineItem<TOutput>(await ExecuteAsync(input.Item, cancellationToken));
                }
            }
            catch (NoneParamException<TOutput> e)
            {
                actionsSet.DiagnosticAction?.Invoke(diagnosticEventArgs.Finish(DiagnosticFinishReason.NoneParamException));
                return e.Item;
            }
            catch (Exception e)
            {
                actionsSet.DiagnosticAction?.Invoke(diagnosticEventArgs.Finish(DiagnosticFinishReason.Exception));
                return new ExceptionItem<TOutput>(e, actionsSet.ReExecute, GetType(), input != null ? input.Item : default);
            }
        }
    }
}
