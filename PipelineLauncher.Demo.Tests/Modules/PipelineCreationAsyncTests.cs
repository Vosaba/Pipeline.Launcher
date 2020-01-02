using System.Collections.Generic;
using System.Diagnostics;
using PipelineLauncher.Demo.Tests.Fakes;
using PipelineLauncher.Demo.Tests.Stages;
using Xunit;
using Xunit.Abstractions;

namespace PipelineLauncher.Demo.Tests.Modules
{
    public class PipelineCreationAsyncTests: PipelineTestsBase
    {
        public PipelineCreationAsyncTests(ITestOutputHelper output)
            : base(output){}

        //[Fact]
        //public void Pipeline_Creation_Async()
        //{
        //    //Test input 6 items
        //    List<Item> input = MakeInput(6);

        //    //Configure stages
        //    var stageSetup = new PipelineFrom<Item>(new FakeServicesRegistry.JobService())
        //        .AsyncStage<AsyncStage1>()
        //        .AsyncStage<AsyncStage2>()
        //        .AsyncStage<AsyncStage3>()
        //        .AsyncStage<AsyncStage4>();

        //    Stopwatch stopWatch = new Stopwatch();

        //    //Make pipeline from stageSetup
        //    var pipeline = stageSetup.From<Item>();

        //    //run
        //    stopWatch.Start();
        //    IEnumerable<Item> result = pipeline.Run(input);
        //    stopWatch.Stop();

        //    //Total time 6049
        //    PrintOutputAndTime(stopWatch.ElapsedMilliseconds, input);
        //}

        //[Fact]
        //public void Pipeline_Creation_FirstSync_Async()
        //{
        //    //Test input 6 items
        //    List<Item> input = MakeInput(6);

        //    //Configure stages
        //    var stageSetup = new PipelineFrom<Item>(new FakeServicesRegistry.JobService())
        //        .Stage<Stage1>()
        //        .AsyncStage<AsyncStage2>()
        //        .AsyncStage<AsyncStage3>()
        //        .AsyncStage<AsyncStage4>();

        //    Stopwatch stopWatch = new Stopwatch();

        //    //Make pipeline from stageSetup
        //    var pipeline = stageSetup.From<Item>();

        //    //run
        //    stopWatch.Start();
        //    IEnumerable<Item> result = pipeline.Run(input);
        //    stopWatch.Stop();

        //    //Total time 10026
        //    PrintOutputAndTime(stopWatch.ElapsedMilliseconds, input);
        //}

        //[Fact]
        //public void Pipeline_Creation_FirstANDThirdSync_Async()
        //{
        //    //Test input 6 items
        //    List<Item> input = MakeInput(6);

        //    //Configure stages
        //    var stageSetup = new PipelineFrom<Item>(new FakeServicesRegistry.JobService())
        //        .Stage<Stage1>()
        //        .AsyncStage<AsyncStage2>()
        //        .Stage<Stage3>()
        //        .AsyncStage<AsyncStage4>();

        //    Stopwatch stopWatch = new Stopwatch();

        //    //Make pipeline from stageSetup
        //    var pipeline = stageSetup.From<Item>();

        //    //run
        //    stopWatch.Start();
        //    IEnumerable<Item> result = pipeline.Run(input);
        //    stopWatch.Stop();

        //    //Total time 14031
        //    PrintOutputAndTime(stopWatch.ElapsedMilliseconds, input);
        //}
    }
}
