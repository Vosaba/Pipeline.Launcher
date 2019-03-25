using System.Linq;
using System.Threading;
using PipelineLauncher.Exceptions;
using PipelineLauncher.PipelineJobs;

namespace PipelineLauncher.Jobs
{
    internal class ConditionAsyncJob<TInput, TOutput> : AsyncJob<TInput, TOutput>
    {
        private readonly AsyncJob<TInput, TOutput>[] _pipelineJobs;

        public ConditionAsyncJob(params AsyncJob<TInput, TOutput>[] pipelineJobs)
        {
            _pipelineJobs = pipelineJobs;
        }


        public override object InternalPerform(object param, CancellationToken cancellationToken)
        {
            try
            {
                var workParam = (TInput) param;

                var firstAcceptableJob = _pipelineJobs.FirstOrDefault(e => e.Condition(workParam));
                if(firstAcceptableJob != null)
                {
                    var result = firstAcceptableJob.PerformAsync(workParam, cancellationToken).Result;
                    Output.Add(result, cancellationToken);
                    return result;
                }

                return null;
            }
            catch (NonParamException e)
            {
                NonParamResult(e.Result, param, cancellationToken);
                return null;
            }
        }
    }

    internal class ConditionAsyncJob<TInput> : ConditionAsyncJob<TInput, TInput>
    {
    }
}
