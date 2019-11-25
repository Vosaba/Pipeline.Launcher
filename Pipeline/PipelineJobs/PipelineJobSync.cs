using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Pipeline;

namespace PipelineLauncher.PipelineJobs
{
    public abstract class PipelineJobSync<TInput, TOutput> : PipelineJob<TInput, TOutput>, IPipelineJobSync
    {
        public abstract Task<IEnumerable<TOutput>> ExecuteAsync(IEnumerable<TInput> input, CancellationToken cancellationToken);

        public virtual IEnumerable<object> InternalExecute(IEnumerable<object> input, CancellationToken cancellationToken)
        {
            var output = ExecuteAsync(input.Cast<TInput>().ToArray(), cancellationToken).Result;

            var castedOutput = output as TOutput[] ?? output.ToArray();
            foreach (var cOutput in castedOutput)
            {
                Output.Add(cOutput, cancellationToken);
            }

            return castedOutput.Cast<object>();
        }
    }
}