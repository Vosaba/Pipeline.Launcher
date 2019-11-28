using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using PipelineLauncher.Demo.Tests.Fakes;
using PipelineLauncher.Demo.Tests.Stages;
using PipelineLauncher.Pipelines;
using PipelineLauncher.Stages;
using Xunit;
using Xunit.Abstractions;

namespace PipelineLauncher.Demo.Tests.Modules
{
    public class PipelineCreationMultipleTests : PipelineTestsBase
    {
        public PipelineCreationMultipleTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public void Pipeline_Creation_Multiple_AsyncJobs()
        {
            //Test input 6 items
            List<Item> input = MakeInput(3);

            //Configure stages
            StageSetupOut<Item, Item> stageSetup = new Pipeline(new FakeServicesRegistry.JobService())
                .WithCancellationToken(CancellationToken.None)
                .AsyncStage<AsyncStage1, Item, Item>()
                .Branch(
                    (item => item.Value == "Item#1->AsyncStage1->",
                        branch => branch.AsyncStage<AsyncStage2>()
                                        //.AsyncStage(e => e.Value)

                                        //.AsyncStage(new AsyncStage1())
                                        .Stage(x =>
                                        {
                                            var y = x.ToList();

                                            y.Add(new Item("Item#NEW->"));
                                             return y;
                                        })),
                    (item => true,
                        branch => branch.AsyncStage(new AsyncStage2Alternative())
                                        .AsyncStage(x =>
                                        {
                                            Thread.Sleep(5000);
                                            return x;
                                        })))
                //.Stage(new Stage2())
                .AsyncStage(new AsyncStage3())
                //.AsyncStage((item) => item.Value)
                .AsyncStage(new AsyncStage4())
                .Stage(new Stage4());

            Stopwatch stopWatch = new Stopwatch();

            //Make pipeline from stageSetup
            var pipeline = stageSetup.From();

            //run
            stopWatch.Start();
            var result = pipeline.RunSync(input);
            //while (result.Count() < 3)
            //{

            //}
            stopWatch.Stop();



            //Total time 24032
            PrintOutputAndTime(stopWatch.ElapsedMilliseconds, result);


            stopWatch.Start();
            result = pipeline.RunSync(input);
            //while (result.Count() < 3)
            //{

            //}
            stopWatch.Stop();



            //Total time 24032
            PrintOutputAndTime(stopWatch.ElapsedMilliseconds, result);
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
            var stageSetup = new Pipeline(new FakeServicesRegistry.JobService())
                .Stage(new Stage1())
                .Stage(new Stage2())//, new Stage2Alternative())
                .Stage(new Stage4())
                .Stage(new Stage4());

            Stopwatch stopWatch = new Stopwatch();

            //Make pipeline from stageSetup
            var pipeline = stageSetup.From();

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
