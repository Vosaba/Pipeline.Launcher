using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.Pipeline;
using System;
using System.Diagnostics;
using System.Threading;

namespace PipelineLauncher.PipelineJobs
{
    [DebuggerDisplay("Name = d")]
    public abstract class PipelineJob<TInput, TOutput>:  IPipelineJob<TInput, TOutput>
    {
        protected void NonOutputResult(PipelineItem<TInput> item, object output, CancellationToken cancellationToken)
        {
            switch (item)
            {
                //case RemoveResult _:
                //    Output.Add(output, cancellationToken);
                //    break;
                //case SkipResult _:
                //    Output.Add(new StageSkipObject(output), cancellationToken);
                //    break;
                //case SkipToResult skipTo:
                //    Output.Add(new StageSkipObject(output, skipTo.JobType), cancellationToken);
                //    break;
            }
        }

        protected static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }

        public Type AcceptedType => typeof(TInput);

        public virtual bool Condition(TInput input) => true;

        public virtual int MaxDegreeOfParallelism => Environment.ProcessorCount;
    }
}