using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PipelineLauncher.Exceptions;

namespace PipelineLauncher.Jobs
{
    internal class ConditionJob<TInput, TOutput> : Job<TInput, TOutput>
    {
        private readonly Job<TInput, TOutput>[] _jobs;

        public ConditionJob(params Job<TInput, TOutput>[] jobs)
        {
            _jobs = jobs;
        }

        public override IEnumerable<object> InternalExecute(IEnumerable<object> input, CancellationToken cancellationToken)
        {
            var workParams = input.Cast<TInput>().ToArray();

            foreach (var job in _jobs)
            {
                var acceptableParam = workParams.Where(e => job.Condition(e)).ToArray();

                if (acceptableParam.Any())
                {
                    var result = job.ExecuteAsync(acceptableParam, cancellationToken).Result;

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