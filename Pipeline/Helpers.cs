using PipelineLauncher.PipelineSetup;
using System.Linq;
using PipelineLauncher.StageSetup;

namespace PipelineLauncher
{
    internal static class Helpers
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
}
