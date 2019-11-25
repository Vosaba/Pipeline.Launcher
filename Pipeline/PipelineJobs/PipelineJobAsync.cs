using System;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.Pipeline;
using PipelineLauncher.Attributes;
using PipelineLauncher.Exceptions;

namespace PipelineLauncher.PipelineJobs
{
    public abstract class PipelineJobAsync<TInput, TOutput> : PipelineJob<TInput, TOutput>, IPipelineJobAsync<TInput, TOutput>
    {
        public abstract Task<TOutput> ExecuteAsync(TInput input, CancellationToken cancellationToken);

        public virtual async Task<PipelineItem<TOutput>> InternalExecute(PipelineItem<TInput> input, CancellationToken cancellationToken)
        {
            try
            {
                switch (input)
                {
                    case RemoveItem<TInput> removeItem:
                        return removeItem.Return<TOutput>();

                    case ExceptionItem<TInput> exceptionItem:
                        return exceptionItem.Return<TOutput>();

                    case SkipItem<TInput> skipItem when typeof(TInput) == skipItem.OriginalItem.GetType():
                        return new PipelineItem<TOutput>(await ExecuteAsync((TInput)skipItem.OriginalItem, cancellationToken));
                    case SkipItem<TInput> skipItem when typeof(TInput) != skipItem.OriginalItem.GetType():
                        return skipItem.Return<TOutput>();

                    case SkipItemTill<TInput> skipItemTill 
                        when GetType() == skipItemTill.JobType:
                        return new PipelineItem<TOutput>(await ExecuteAsync((TInput)skipItemTill.OriginalItem, cancellationToken));
                    case SkipItemTill<TInput> skipItemTill 
                        when GetType() != skipItemTill.JobType:
                        return skipItemTill.Return<TOutput>();

                    default:
                        return new PipelineItem<TOutput>(await ExecuteAsync(input.Item, cancellationToken));
                }
            }
            catch (NonParamException<TOutput> e)
            {
                return e.Item;
            }
            catch (Exception e)
            {
                return new ExceptionItem<TOutput>(e, input != null ? input.Item : default);
            }
        }
    }
}
