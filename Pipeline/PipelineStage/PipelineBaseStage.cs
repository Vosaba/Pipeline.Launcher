using System;
using PipelineLauncher.Abstractions.PipelineStage;
using PipelineLauncher.Abstractions.Stages;

namespace PipelineLauncher.PipelineStage
{
    public abstract class PipelineBaseStage<TInput, TOutput>:  IStage<TInput, TOutput>
    {
        //protected static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        //{
        //    while (toCheck != null && toCheck != typeof(object))
        //    {
        //        var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
        //        if (generic == cur)
        //        {
        //            return true;
        //        }
        //        toCheck = toCheck.BaseType;
        //    }
        //    return false;
        //}


        //public virtual bool Condition(TInput input) => true;
    }
}