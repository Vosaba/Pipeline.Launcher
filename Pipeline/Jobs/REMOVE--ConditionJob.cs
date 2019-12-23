using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Configurations;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Exceptions;
using PipelineLauncher.PipelineJobs;

namespace PipelineLauncher.Jobs
{
    //internal class ConditionJob<TInput, TOutput> : Job<TInput, TOutput>
    //{
    //    private readonly Job<TInput, TOutput>[] _pipelineJobS;

    //    public ConditionJob(params Job<TInput, TOutput>[] pipelineJobS)
    //    {
    //        _pipelineJobS = pipelineJobS;
    //    }

    //    public override JobConfiguration Configuration => throw new NotImplementedException();

    //    public new async Task<PipelineItem<TOutput>> InternalExecute(PipelineItem<TInput> input, Action reExecute, CancellationToken cancellationToken)
    //    {
    //        try
    //        {
    //            var param =  input;

    //            var firstAcceptableJob = _pipelineJobS.FirstOrDefault(e => e.Condition(param.Item));
    //            if(firstAcceptableJob != null)
    //            {
    //                var result = await firstAcceptableJob.ExecuteAsync(param.Item, cancellationToken);
    //                //Output.Add(result, cancellationToken);
    //                return new PipelineItem<TOutput>(result);
    //            }

    //            //return null;
    //        }
    //        catch (NonParamException<TOutput> e)
    //        {
    //            //NonOutputResult(e.Item, input, cancellationToken);
    //            //return null;
    //        }

    //        throw new Exception();
    //    }
    //}

    //internal class ConditionJob<TInput> : ConditionJob<TInput, TInput>
    //{
    //}
}
