using System.Collections.Generic;
using System.Diagnostics;
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
            var stageSetup = new PipelineFrom<Item>()
                .Stage(new Stage1())
                .Stage(new Stage2())
                .Stage(new Stage3())
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

        [Fact]
        public void Pipeline_Creation_Type_Diff()
        {
            //Test input 6 items
            List<Item> input = MakeInput(6);

            //Configure stages
            var stageSetup = new PipelineFrom<Item>()
                .Stage(new Stage1())
                .Stage(new Stage2())
                .Stage(new Stage3())
                .Stage(new Stage_Item_To_String());

            Stopwatch stopWatch = new Stopwatch();

            //Make pipeline from stageSetup
            var pipeline = stageSetup.From();

            //run
            stopWatch.Start();
            IEnumerable<string> result = pipeline.Run(input);
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
            var stageSetup = new PipelineFrom<Item>(new FakeServicesRegistry.JobService())
                .Stage<Stage1>()
                .Stage<Stage2>()
                .Stage<Stage3>()
                .Stage<Stage_Item_To_String, string>()
                .Stage<Stage_String_To_Object, object>();

            Stopwatch stopWatch = new Stopwatch();

            //Make pipeline from stageSetup
            var pipeline = stageSetup.From();

            //run
            stopWatch.Start();
            IEnumerable<object> result = pipeline.Run(input);
            stopWatch.Stop();

            //Total time 24032
            PrintOutputAndTime(stopWatch.ElapsedMilliseconds, input);
        }
    }
}
