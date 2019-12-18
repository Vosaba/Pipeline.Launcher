using PipelineLauncher.PipelineSetup;
using PipelineLauncher.Stage;
using System.Linq;

namespace PipelineLauncher
{
    internal static class Helpers
    {
        public static IStageIn<TInput> GetFirstStage<TInput>(this IPipelineSetup stageSetup)
        {
            IStage stage = stageSetup.Current;

            while (stage.Previous != null)
            {
                stage = stage.Previous;
            }

            return (IStageIn<TInput>)stage;
        }

        public static void DestroyStageBlocks(this IStage stage)
        {
            if (stage == null)
            {
                return;
            }

            stage.DestroyBlock();

            if (stage.Next == null || !stage.Next.Any()) return;
            foreach (var nextStage in stage.Next)
            {
                DestroyStageBlocks(nextStage);
            }
        }
    }
}
