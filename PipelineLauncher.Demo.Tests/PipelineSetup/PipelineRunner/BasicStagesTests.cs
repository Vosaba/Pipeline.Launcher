using PipelineLauncher.Demo.Tests.Items;
using PipelineLauncher.Demo.Tests.Stages.Single;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace PipelineLauncher.Demo.Tests.PipelineSetup.PipelineRunner
{

    public class BasicStagesTests : PipelineRunnerTestBase
    {
        public BasicStagesTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void Simple_Running_Stages()
        {
            // Test input 6 items
            List<Item> items = MakeItemsInput(6);

            // Configure stages
            var pipelineSetup = PipelineCreator
                .Stage<Stage, Item>()
                .Stage<Stage_1>()
                .Stage<Stage_Item_To_Item2, Item2>()
                .Stage<Stage_Item2_To_Item, Item>()
                .Stage<Stage_3>();

            // Make pipeline from stageSetup
            var pipelineRunner = pipelineSetup.Create();

            PostItemsAndPrintProcessedWithDefaultConditionToStop(pipelineRunner, items);
        }
    }
}
