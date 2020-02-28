using System.Collections.Generic;
using PipelineLauncher.Demo.Tests.Items;
using PipelineLauncher.Demo.Tests.Stages.Single;
using Xunit;
using Xunit.Abstractions;

namespace PipelineLauncher.Demo.Tests.PipelineTest.NonAwaitablePipelineRunner
{

    public class BasicStagesTests : NonAwaitableTestBase
    {
        public BasicStagesTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void Simple_Running_Stages()
        {
            // Test input 6 items
            List<Item> items = MakeItemsInput(6);

            // Configure stages
            var pipelineSetup = PipelineCreator
                .Stage<StageS, Item>()
                .Stage<StageS1>()
                .Stage<StageSItemToItem2, Item2>()
                .Stage<StageSItem2ToItem, Item>()
                .Stage<StageS3>();

            // Make pipeline from stageSetup
            var pipelineRunner = pipelineSetup.Create();

            PostItemsAndPrintProcessedWithDefaultConditionToStop(pipelineRunner, items);
        }
    }
}
