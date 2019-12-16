using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Configurations;
using PipelineLauncher.Exceptions;

namespace PipelineLauncher.Jobs
{
    internal class ConditionBulkJob<TInput, TOutput> : BulkJob<TInput, TOutput>
    {
        private readonly BulkJob<TInput, TOutput>[] _jobs;

        public ConditionBulkJob(params BulkJob<TInput, TOutput>[] jobs)
        {
            _jobs = jobs;
        }

        public override BulkJobConfiguration Configuration => throw new System.NotImplementedException();

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

    internal class ConditionBulkJob<TInput> : ConditionBulkJob<TInput, TInput>
    {
    }
}