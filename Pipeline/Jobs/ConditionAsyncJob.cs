using System.Linq;
using System.Threading;
using PipelineLauncher.Exceptions;
using PipelineLauncher.PipelineJobs;

namespace PipelineLauncher.Jobs
{
    internal class ConditionAsyncJob<TInput, TOutput> : AsyncJob<TInput, TOutput>
    {
        private readonly AsyncJobVariant<TInput, TOutput>[] _pipelineJobs;

        public ConditionAsyncJob(params AsyncJobVariant<TInput, TOutput>[] pipelineJobs)
        {
            _pipelineJobs = pipelineJobs;
        }


        public override object InternalExecute(object input, CancellationToken cancellationToken)
        {
            try
            {
                var param = (TInput) input;

                var firstAcceptableJob = _pipelineJobs.FirstOrDefault(e => e.Condition(param));
                if(firstAcceptableJob != null)
                {
                    var result = firstAcceptableJob.ExecuteAsync(param, cancellationToken).Result;
                    Output.Add(result, cancellationToken);
                    return result;
                }

                return null;
            }
            catch (NonParamException e)
            {
                NonOutputResult(e.Result, input, cancellationToken);
                return null;
            }
        }
    }

    internal class ConditionAsyncJob<TInput> : ConditionAsyncJob<TInput, TInput>
    {
    }
}
