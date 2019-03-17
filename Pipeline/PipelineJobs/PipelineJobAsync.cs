using System;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Pipeline;
using PipelineLauncher.Exceptions;

namespace PipelineLauncher.PipelineJobs
{
    public abstract class PipelineJobAsync<TInput, TOutput> : PipelineJob<TInput, TOutput>, IPipelineJobAsync
    {
        public abstract Task<TOutput> PerformAsync(TInput param, CancellationToken cancellationToken);

        public object InternalPerform(object param, CancellationToken cancellationToken)
        {
            try
            {
                var result = PerformAsync((TInput)param, cancellationToken).Result;
                Output.Add(result, cancellationToken);
                return result;
            }
            catch (NonParamException e)
            {
                NonParamResult(e.Result, param, cancellationToken);
                return null;
            }
        }

        public virtual int MaxDegreeOfParallelism => Environment.ProcessorCount;
    }
}
