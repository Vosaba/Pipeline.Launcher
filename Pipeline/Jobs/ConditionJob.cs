using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PipelineLauncher.Exceptions;

namespace PipelineLauncher.Jobs
{
    internal class ConditionJob<TInput, TOutput> : Job<TInput, TOutput>
    {
        private readonly Job<TInput, TOutput>[] _pipelineJobs;

        public ConditionJob(params Job<TInput, TOutput>[] pipelineJobs)
        {
            _pipelineJobs = pipelineJobs;
        }

        public override IEnumerable<object> InternalPerform(object[] @params, CancellationToken cancellationToken)
        {
            var workParams = @params.Cast<TInput>().ToArray();

            foreach (var job in _pipelineJobs)
            {
                var acceptableParam = workParams.Where(e => job.Condition(e)).ToArray();

                if (acceptableParam.Any())
                {
                    var result = job.PerformAsync(acceptableParam, cancellationToken).Result;

                    var internalResult = result as TOutput[] ?? result.ToArray();
                    foreach (var res in internalResult)
                    {
                        Output.Add(res, cancellationToken);
                    }

                    yield return internalResult.Cast<object>();
                }
            }
        }
    }

    internal class ConditionJob<TInput> : ConditionJob<TInput, TInput>
    {
    }
}