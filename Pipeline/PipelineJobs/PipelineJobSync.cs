using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Pipeline;

namespace PipelineLauncher.PipelineJobs
{
    public abstract class PipelineJobSync<TInput, TOutput> : PipelineJob<TInput, TOutput>, IPipelineJobSync
    {
        public abstract Task<IEnumerable<TOutput>> PerformAsync(TInput[] param, CancellationToken cancellationToken);

        public IEnumerable<object> InternalPerform(object[] param, CancellationToken cancellationToken)
        {
            var result = PerformAsync(param.Cast<TInput>().ToArray(), cancellationToken).Result;

            var internalResult = result as TOutput[] ?? result.ToArray();
            foreach (var res in internalResult)
            {
                Output.Add(res, cancellationToken);
            }

            return internalResult.Cast<object>();
        }
    }
}