using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PipelineLauncher.Demo.Tests.Fakes;
using PipelineLauncher.Demo.Tests.Stages;
using PipelineLauncher.Pipelines;
using Xunit;
using Xunit.Abstractions;

namespace PipelineLauncher.Demo.Tests.Modules
{
    public class PipelineCreationMultipleTests : PipelineTestsBase
    {
        public PipelineCreationMultipleTests(ITestOutputHelper output)
            : base(output){}

        [Fact]
        public void Pipeline_Creation_Multiple_AsyncJobs()
        {
            //Test input 6 items
            List<Item> input = MakeInput(3);

            //Configure stages
            var stageSetup = new PipelineFrom<Item>()
                .AsyncStage(new AsyncStage1())
                .Branch((item => item.Value == "Item#1->AsyncStage1->", d => d.AsyncStage(new AsyncStage2()).Stage(new Stage2())),
                   (item => true, d => d.AsyncStage(new AsyncStage2Alternative())))
                //.Stage(new Stage2())
                .AsyncStage(new AsyncStage3())
                //.AsyncStage((item) => item.Value)
                .AsyncStage(new AsyncStage4());
            // .Stage(new Stage4());

            Stopwatch stopWatch = new Stopwatch();

            //Make pipeline from stageSetup
            var pipeline = stageSetup.From();

            //run
            stopWatch.Start();
            var result = pipeline.Run(input);
            while (result.Count() < 3)
            {

            }
            stopWatch.Stop();

           

            //Total time 24032
            PrintOutputAndTime(stopWatch.ElapsedMilliseconds, input);

            PrintOutputAndTime(stopWatch.ElapsedMilliseconds, result);


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
            var stageSetup = new PipelineFrom<Item>()
                .Stage(new Stage1())
                .Stage(new Stage2())//, new Stage2Alternative())
                .Stage(new Stage4())
                .Stage(new Stage4());

            Stopwatch stopWatch = new Stopwatch();

            //Make pipeline from stageSetup
            var pipeline = stageSetup.From();

            //run
            stopWatch.Start();
            IEnumerable<Item> result = pipeline.Run(input);
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
