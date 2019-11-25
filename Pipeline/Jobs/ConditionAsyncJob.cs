using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Dto;
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


        public override async Task<PipelineItem<TOutput>> InternalExecute(PipelineItem<TInput> input, CancellationToken cancellationToken)
        {
            try
            {
                var param =  input;

                var firstAcceptableJob = _pipelineJobs.FirstOrDefault(e => e.Condition(param.Item));
                if(firstAcceptableJob != null)
                {
                    var result = await firstAcceptableJob.ExecuteAsync(param.Item, cancellationToken);
                    //Output.Add(result, cancellationToken);
                    return new PipelineItem<TOutput>(result);
                }

                //return null;
            }
            catch (NonParamException<TOutput> e)
            {
                //NonOutputResult(e.Item, input, cancellationToken);
                //return null;
            }

            throw new Exception();
        }
    }

    internal class ConditionAsyncJob<TInput> : ConditionAsyncJob<TInput, TInput>
    {
    }
}
