using System;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Pipeline;
using PipelineLauncher.Exceptions;

namespace PipelineLauncher.PipelineJobs
{
    public abstract class PipelineJobAsync<TInput, TOutput> : PipelineJob<TInput, TOutput>, IPipelineJobAsync
    {
        public abstract Task<TOutput> ExecuteAsync(TInput input, CancellationToken cancellationToken);

        public virtual object InternalExecute(object input, CancellationToken cancellationToken)
        {
            try
            {
                var result = ExecuteAsync((TInput)input, cancellationToken).Result;
                Output.Add(result, cancellationToken);
                return result;
            }
            catch (NonParamException e)
            {
                NonOutputResult(e.Result, input, cancellationToken);
                return null;
            }
        }
    }
}
