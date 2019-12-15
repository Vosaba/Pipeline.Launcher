using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Configurations;
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

        public override JobConfiguration Configuration => throw new System.NotImplementedException();

        public override async Task<IEnumerable<TOutput>> ExecuteAsync(IEnumerable<TInput> input, CancellationToken cancellationToken)
        {
            List<TOutput> result = new List<TOutput>();
            var enumerable = input as TInput[] ?? input.ToArray();

            foreach (var job in _jobs)
            {
                var acceptableParam = enumerable.Where(e => job.Condition(e)).ToArray();

                if (acceptableParam.Any())
                {
                    result.AddRange(await job.ExecuteAsync(acceptableParam, cancellationToken));
                }
            }

            return result;
        }
    }

    internal class ConditionJob<TInput> : ConditionJob<TInput, TInput>
    {
    }
}