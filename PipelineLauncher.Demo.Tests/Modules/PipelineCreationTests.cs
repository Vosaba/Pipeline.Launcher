using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using PipelineLauncher.Demo.Tests.Fakes;
using PipelineLauncher.Demo.Tests.Stages;
using Xunit;
using Xunit.Abstractions;

namespace PipelineLauncher.Demo.Tests.Modules
{
    public class PipelineCreationTests: PipelineTestsBase
    {
        public PipelineCreationTests(ITestOutputHelper output)
            : base(output){}

        [Fact]
        public void Pipeline_Creation()
        {
            //Test input 6 items
            List<Item> input = MakeInput(6);

            //Configure stages
            var stageSetup = new PipelineCreator(new FakeServicesRegistry.StageService())
                .BulkStage(new BulkStageStage1())
                .BulkStage(new BulkStageStage2())
                .BulkStage(new BulkStageStage3())
                .BulkStage(new BulkStageStage4());
                

            Stopwatch stopWatch = new Stopwatch();

            //Make pipeline from stageSetup
            var pipeline = stageSetup.CreateAwaitable();

            //run
            stopWatch.Start();
            var result = pipeline.Process(input);
            stopWatch.Stop();

            //Total time 24032
            PrintOutputAndTime(stopWatch.ElapsedMilliseconds, input);
        }

        [Fact]
        public void Pipeline_Creation_Type_Diff()
        {
            //Test input 6 items
            List<Item> input = MakeInput(6);

            //Configure stages
            var stageSetup = new PipelineCreator(new FakeServicesRegistry.StageService())
                .BulkStage(new BulkStageStage1())
                .BulkStage(new BulkStageStage2())
                .BulkStage(new BulkStageStage3())
                .BulkStage(new BulkStageStageItemToString());

            Stopwatch stopWatch = new Stopwatch();

            //Make pipeline from stageSetup
            var pipeline = stageSetup.CreateAwaitable();

            //run
            stopWatch.Start();
            var result = pipeline.Process(input);
            stopWatch.Stop();

            //Total time 24032
            PrintOutputAndTime(stopWatch.ElapsedMilliseconds, input);
        }


        [Fact]
        public void Pipeline_Creation_Generic()
        {
            //Test input 6 items
            List<Item> input = MakeInput(6);

            //Configure stages
            var stageSetup = new PipelineCreator(new FakeServicesRegistry.StageService())
                .BulkStage<BulkStageStage1, Item>()
                .BulkStage<BulkStageStage2>()
                .BulkStage<BulkStageStage3>()
                .BulkStage<BulkStageStageItemToString, string>()
                .BulkStage<BulkStageStageStringToObject, object>();

            Stopwatch stopWatch = new Stopwatch();

            //Make pipeline from stageSetup
            var pipeline = stageSetup.CreateAwaitable();

            //run
            stopWatch.Start();
            var result = pipeline.Process(input);
            stopWatch.Stop();

            //Total time 24032
            PrintOutputAndTime(stopWatch.ElapsedMilliseconds, input);
        }
    }
}
