using PipelineLauncher.Collections;
using PipelineLauncher.Stages;
using System.Collections.Generic;
using System.Threading;
using PipelineLauncher.Abstractions.Collections;
using PipelineLauncher.Abstractions.Pipeline;

namespace PipelineLauncher
{
    internal static class Helpers
    {
        public static IStage GetFirstStage(this IStageSetup stageSetup)
        {
            var stage = stageSetup.Current;

            while (stage.Previous != null)
            {
                stage = stage.Previous;
            }

            return stage;
        }

        public static IEnumerable<IPipelineJob> ToEnumerable(this IStageSetup stageSetup)
        {

            var current = stageSetup.GetFirstStage();

            while (current != null)
            {
                yield return current.Job;
                current = current.Next;
            }
        }

        public static IQueue<object> ToQueue<T>(this IEnumerable<T> source, CancellationToken cancellationToken)
        {
            var queue = new BlockingQueue<object>();

            foreach (var item in source)
            {
                queue.Add(item, cancellationToken);
            }

            queue.CompleteAdding();

            return queue;
        }
    }
}
