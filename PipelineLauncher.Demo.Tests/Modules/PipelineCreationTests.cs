using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using PipelineLauncher.Demo.Tests.Fakes;
using PipelineLauncher.Demo.Tests.Stages;
using PipelineLauncher.Pipelines;
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
            var stageSetup = new PipelineCreator(new FakeServicesRegistry.JobService())
                .BulkStage(new BulkJobStage1())
                .BulkStage(new BulkJobStage2())
                .BulkStage(new BulkJobStage3())
                .BulkStage(new BulkJobStage4());
                

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
            var stageSetup = new PipelineCreator(new FakeServicesRegistry.JobService())
                .BulkStage(new BulkJobStage1())
                .BulkStage(new BulkJobStage2())
                .BulkStage(new BulkJobStage3())
                .BulkStage(new BulkJobStageItemToString());

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
            var stageSetup = new PipelineCreator(new FakeServicesRegistry.JobService())
                .BulkStage<BulkJobStage1, Item>()
                .BulkStage<BulkJobStage2>()
                .BulkStage<BulkJobStage3>()
                .BulkStage<BulkJobStageItemToString, string>()
                .BulkStage<BulkJobStageStringToObject, object>();

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
