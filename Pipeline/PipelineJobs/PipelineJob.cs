using PipelineLauncher.Abstractions.Configurations;
using PipelineLauncher.Abstractions.Pipeline;
using System;
using System.Diagnostics;

namespace PipelineLauncher.PipelineJobs
{
    [DebuggerDisplay("Name = d")]
    public abstract class PipelineJob<TInput, TOutput>:  IPipelineJob<TInput, TOutput>
    {
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


        public virtual bool Condition(TInput input) => true;
    }
}