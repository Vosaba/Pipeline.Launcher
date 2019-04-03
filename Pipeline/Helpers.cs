using PipelineLauncher.Collections;
using PipelineLauncher.Stages;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PipelineLauncher.Abstractions.Collections;
using PipelineLauncher.Abstractions.Pipeline;

namespace PipelineLauncher
{
    internal static class Helpers
    {
        public static IStage GetFirstStage<TInput, TOutput>(this IStageSetup<TInput, TOutput> stageSetup)
        {
            IStage stage = stageSetup.Current;

            while (stage.Previous != null)
            {
                stage = stage.Previous;
            }

            return stage;
        }

        //public static IEnumerable<IEnumerable<IPipelineJob>> ToEnumerable(this IStageSetup stageSetup)
        //{
        //    return stageSetup.ToTree();
        //    //var current = stageSetup.GetFirstStage();

        //    //while (current != null)
        //    //{
        //    //    yield return new []{current.Job};
        //    //    current = current.Next?.First();
        //    //}
        //}

        //public static IEnumerable<IEnumerable<IPipelineJob>> ToTree(this IStageSetup stageSetup)
        //{

        //    var root = stageSetup.GetFirstStage();

        //    var t = new List<List<IPipelineJob>>();

        //   ToTree(root, t);

        //    return t;
        //    //while (root != null)
        //    //{
        //    //    yield return new[] { root.Job };
        //    //    root = root.Next?.First();
        //    //}
        //}

        //public static List<IPipelineJob> ToTree(this IStage root, List<List<IPipelineJob>> jobRows, List<IPipelineJob> rowJobs = null)
        //{
        //    if (rowJobs == null)
        //    {
        //        rowJobs = new List<IPipelineJob>() { root.Job };
        //        jobRows.Add(rowJobs);
        //    }
        //    else
        //    {
        //        rowJobs.Add(root.Job);
        //    }


        //    if (root.Next != null)
        //    {
        //        var isNewCase = false;
        //        IPipelineJob[] savedJobCase = new IPipelineJob[rowJobs.Count];
        //        rowJobs.CopyTo(savedJobCase);

        //        foreach (var next in root.Next)
        //        {
        //            if (isNewCase)
        //            {
        //                //TODO: make one ref per each jobs into different cases
        //                IPipelineJob[] newJobCase = new IPipelineJob[savedJobCase.Length];
        //                savedJobCase.ToList().CopyTo(newJobCase);

        //                var nj = newJobCase.ToList();

        //                jobRows.Add(nj);
        //                ToTree(next, jobRows, nj);
        //            }
        //            else
        //            {
        //                ToTree(next, jobRows, rowJobs);
        //            }

        //            isNewCase = true;
        //            //return ToTree(next, rowJobs);
        //        }
        //    }

        //    return rowJobs;


        //    //List<List<IPipelineJob>> steps = new List<List<IPipelineJob>>();
        //    //if (root != null && root.Next != null)
        //    //{
        //    //    var rootJob = new List<IPipelineJob>() {root.Job};

        //    //    foreach (var next in root.Next)
        //    //    {
        //    //        rowJobs.Add(new List<IPipelineJob>() {next.Job});
        //    //        return ToTree(next, rowJobs);
        //    //    }
        //    //}

        //    //while (root != null)
        //    //{
        //    //    yield return new[] { root.Job };
        //    //    root = root.Next?.First();
        //    //}
        //}

        //public static IEnumerable<IPipelineJob> ToTree(this IStage root)
        //{
        //    var steps = new[] {}
        //    foreach (var next in root.Next)
        //    {
        //        yield return new[] { root.Job };
        //    }

        //    while (root != null)
        //    {
        //        yield return new[] { root.Job };
        //        root = root.Next?.First();
        //    }
        //}


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
