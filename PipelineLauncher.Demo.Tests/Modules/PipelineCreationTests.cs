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
                .BulkStage(new BulkStage1())
                .BulkStage(new BulkStage2())
                .BulkStage(new BulkStage3())
                .BulkStage(new BulkStage4());
                

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

        [Fact]
        public void Pipeline_Creation_Type_Diff()
        {
            //Test input 6 items
            List<Item> input = MakeInput(6);

            //Configure stages
            var stageSetup = new PipelineCreator(new FakeServicesRegistry.JobService())
                .BulkStage(new BulkStage1())
                .BulkStage(new BulkStage2())
                .BulkStage(new BulkStage3())
                .BulkStage(new BulkStage_Item_To_String());

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


        [Fact]
        public void Pipeline_Creation_Generic()
        {
            //Test input 6 items
            List<Item> input = MakeInput(6);

            //Configure stages
            var stageSetup = new PipelineCreator(new FakeServicesRegistry.JobService())
                .BulkStage<BulkStage1, Item>()
                .BulkStage<BulkStage2>()
                .BulkStage<BulkStage3>()
                .BulkStage<BulkStage_Item_To_String, string>()
                .BulkStage<BulkStage_String_To_Object, object>();

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
    }
}
