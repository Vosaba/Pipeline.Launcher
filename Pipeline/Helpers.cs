using PipelineLauncher.PipelineSetup;
using System.Linq;
using PipelineLauncher.Abstractions.PipelineRunner;
using PipelineLauncher.Abstractions.PipelineStage.Dto;
using PipelineLauncher.PipelineRunner;
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
    }

    internal static partial class Helpers
    {
        internal static class Strings
        {
            public static string RetryOnAwaitable = 
                $"{nameof(ActionsSet.Retry)} action cannot be used in case of '{nameof(IAwaitablePipelineRunner<object, object>)}', " +
                $"try use '{nameof(IAwaitablePipelineRunner<object, object>.SetupExceptionHandler)}' " +
                $"on one of '{nameof(IAwaitablePipelineRunner<object, object>)}' or '{nameof(IPipelineCreator)}' to perform that action.";

            public static string RetriesMaxCountReached =
                $"You called {nameof(ActionsSet.Retry)} action more than '{0}' times.";
        }
    }
}
