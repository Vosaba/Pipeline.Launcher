using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using PipelineLauncher.Demo.Tests.Fakes;
using PipelineLauncher.Demo.Tests.Stages;
using PipelineLauncher.Dto;
using PipelineLauncher.Pipelines;
using PipelineLauncher.PipelineSetup;
using Xunit;
using Xunit.Abstractions;

namespace PipelineLauncher.Demo.Tests.Modules
{
    public static class PipelineExtensions
    {
        public static IPipelineSetup<TInput, int> MssCall<TInput>(this IPipelineSetup<TInput, Item> pipelineSetup, string someValue)
        {
            return pipelineSetup.AsyncStage(e => e.GetHashCode());
        }
    }
    public class PipelineCreationMultipleTests : PipelineTestsBase
    {
        public IPipelineCreator PipelineCreator = new PipelineCreator(new FakeServicesRegistry.JobService());

        public PipelineCreationMultipleTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public void Pipeline_Creation_Multiple_AsyncJobs()
        {
            //Test input 6 items
            List<Item> input = MakeInput(3);

            //Configure stages
            var pipelineSetup = PipelineCreator
                .WithToken(CancellationToken.None)
                .AsyncStage<AsyncStage1, Item, Item>()
                .Branch(
                    (item => item.Value == "Item#1->AsyncStage1->",
                        branch => branch
                            .AsyncStage<AsyncStage2>()
                        //.Stage(x =>
                        //{
                        //    var y = x.ToList();

                        //    y.Add(new Item("Item#NEW->"));
                        //    return y;
                        //})
                    ),
                    (item => true,
                        branch => branch
                            .AsyncStage<AsyncStage2>()
                            .Broadcast(
                                (item => true, //=> "Item#NEW->AsyncStage3->AsyncStage4->Stage4->AsyncStage1->",
                                    branch1 => branch1.AsyncStage(x =>
                                    {
                                        x.Value += "111111111111111111111111";
                                        return x;
                                    })),
                                (item => true,
                                    branch1 => branch1.AsyncStage(x =>
                                    {
                                        x.Value += "222222222222222222222222";
                                        return x;
                                    })))
                    ))

                //.AsyncStage(new AsyncStage3())
                //.AsyncStage(new AsyncStage4())
                //.AsyncStage(Task.FromResult) 
                .AsyncStage((Item item, AsyncJobOption<Item, Item> asyncJobOption) =>
                {
                    if (item.Value.StartsWith("Item#0"))
                    {
                        return asyncJobOption.Skip(item);
                    }
                    return item;
                })
                .Stage(new Stage3())
                .Stage(new Stage4());//.Cast<Item, Item, object>();
                //.AsyncStage(e => e.Value)
                //.MssCall("fff");

            Stopwatch stopWatch = new Stopwatch();
            //var skippedItems = new List<Item>();
            //Make pipeline from stageSetup
            //pipeline.SkippedItemReceivedEvent += delegate(SkippedItemEventArgs item) { skippedItems.Add(item.Item); };

            var pipeline = pipelineSetup.CreateAwaitable();

            //run
            stopWatch.Start();
            var result = pipeline.Process(input).ToArray();
            stopWatch.Stop();

            PrintOutputAndTime(stopWatch.ElapsedMilliseconds, result);
            stopWatch.Reset();

            //var pipeline2 = pipelineSetup.CreateAwaitable();

            //run
            stopWatch.Start();
            var result2 = result;//pipeline.Process(result).ToArray();
            stopWatch.Stop();

            //Total time 24032
            PrintOutputAndTime(stopWatch.ElapsedMilliseconds, result2);
            stopWatch.Reset();

            //stopWatch.Start();
            //var result3 = pipeline.Process(result2).ToArray();
            //stopWatch.Stop();

            ////Total time 24032
            //PrintOutputAndTime(stopWatch.ElapsedMilliseconds, result3);
            //stopWatch.Reset();

            //stopWatch.Start();
            //var result4 = pipeline.Process(result3).ToArray();
            //stopWatch.Stop();

            ////Total time 24032
            //PrintOutputAndTime(stopWatch.ElapsedMilliseconds, result4);
            //stopWatch.Reset();

            //stopWatch.Start();
            //var result5 = pipeline.Process(result4).ToArray();
            //stopWatch.Stop();

            ////Total time 24032
            //PrintOutputAndTime(stopWatch.ElapsedMilliseconds, result5);
            //stopWatch.Reset();
            //PrintOutputAndTime(stopWatch.ElapsedMilliseconds, result);


            //stopWatch.Reset();
            //stopWatch.Start();
            //result = pipeline.Run(input);

            //while (result.Count() < 12)
            //{

            //}
            //stopWatch.Stop();


            ////Total time 24032
            //PrintOutputAndTime(stopWatch.ElapsedMilliseconds, input);


        }

        [Fact]
        public void Pipeline_Creation_Multiple_SyncJobs()
        {
            //Test input 6 items
            List<Item> input = MakeInput(6);

            //Configure stages
            var stageSetup = new PipelineCreator(new FakeServicesRegistry.JobService())
                .Stage(new Stage1())
                .Stage(new Stage2())//, new Stage2Alternative())
                .Stage(new Stage4())
                .Stage(new Stage4());

            Stopwatch stopWatch = new Stopwatch();

            //Make pipeline from stageSetup
            var pipeline = stageSetup.CreateAwaitable();

            //run
            stopWatch.Start();
            var result = pipeline.Post(input);
            stopWatch.Stop();

            //Total time 24032
            PrintOutputAndTime(stopWatch.ElapsedMilliseconds, input);
        }

        //[Fact]
        //public void Pipeline_Creation_Multiple_Async_and_SyncJobs_generic()
        //{
        //    //Test input 6 items
        //    List<Item> input = MakeInput(6);

        //    //Configure stages
        //    var stageSetup = new PipelineFrom<Item>(new FakeServicesRegistry.JobService())
        //        .Stage<Stage1>()
        //        .Stage<Stage2>() //, Stage2Alternative, Item>()
        //        .AsyncStage(item => item)
        //        .AsyncStage<AsyncStage2, AsyncStage2Alternative, Item>();

        //    Stopwatch stopWatch = new Stopwatch();

        //    //Make pipeline from stageSetup
        //    var pipeline = stageSetup.From<Item>();

        //    //run
        //    stopWatch.Start();
        //    IEnumerable<Item> result = pipeline.Run(input);
        //    stopWatch.Stop();

        //    //Total time 24032
        //    PrintOutputAndTime(stopWatch.ElapsedMilliseconds, input);
        //}
    }
}
