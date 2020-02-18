using System;
using System.Collections.Generic;
using PipelineLauncher.PipelineSetup;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineRunner;
using PipelineLauncher.Abstractions.PipelineStage;
using PipelineLauncher.Abstractions.PipelineStage.Dto;
using PipelineLauncher.PipelineRunner;
using PipelineLauncher.PipelineStage;
using PipelineLauncher.StageSetup;

namespace PipelineLauncher
{
    internal static partial class Helpers
    {
        public static IStageSetupIn<TInput> GetFirstStage<TInput>(this IPipelineSetup pipelineSetup)
        {
            IStageSetup stageSetup = pipelineSetup.Current;

            while (stageSetup.Previous != null)
            {
                stageSetup = stageSetup.Previous;
            }

            return (IStageSetupIn<TInput>)stageSetup;
        }

        public static void DestroyStageBlocks(this IStageSetup stageSetup)
        {
            if (stageSetup == null)
            {
                return;
            }

            stageSetup.DestroyBlock();

            if (stageSetup.Next == null || !stageSetup.Next.Any()) return;
            foreach (var nextStage in stageSetup.Next)
            {
                DestroyStageBlocks(nextStage);
            }
        }

        public static Predicate<PipelineStageItem<TOutput>> GetPredicate<TOutput>(
            this PipelinePredicate<TOutput> predicate, 
            ITargetBlock<PipelineStageItem<TOutput>> target)
        {
            return x =>
            {
                if (x == null)
                {
                    return false;
                }

                if (x.Item == null)
                {
                    return true;
                }

                try
                {
                    var result = predicate(x.Item);

                    switch (result)
                    {
                        case PredicateResult.Keep:
                            return true;
                        case PredicateResult.Skip:
                            target.Post(new SkipStageItem<TOutput>(x.Item, predicate.GetType()));
                            return false;
                        case PredicateResult.Remove:
                            target.Post(new RemoveStageItem<TOutput>(x.Item, predicate.GetType()));
                            return false;
                        default:
                            target.Post(new ExceptionStageItem<TOutput>(new ArgumentOutOfRangeException(), null, predicate.GetType(), x.Item));
                            return false;
                    }
                }
                catch (Exception ex)
                {
                    target.Post(new ExceptionStageItem<TOutput>(ex, null, predicate.GetType(), x.Item));
                    return false;
                }
            };
        }
    }

    internal static partial class Helpers
    {
        internal static class Strings
        {
            public static string RetryOnAwaitable = 
                $"{nameof(ActionsSet.Retry)} action cannot be used on '{nameof(IAwaitablePipelineRunner<object, object>)}', " +
                $"try use '{nameof(IAwaitablePipelineRunner<object, object>.SetupExceptionHandler)}' " +
                $"on one of '{nameof(IAwaitablePipelineRunner<object, object>)}' or '{nameof(IPipelineCreator)}' to perform that action.";

            public static string RetriesMaxCountReached =
                $"{nameof(ActionsSet.Retry)} was called" + " more than '{0}' times.";
        }
    }
}
