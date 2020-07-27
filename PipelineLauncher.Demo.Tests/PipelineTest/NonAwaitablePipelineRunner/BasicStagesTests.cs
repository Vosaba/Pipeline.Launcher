using System.Collections.Generic;
using System.Threading;
using PipelineLauncher.Demo.Tests.Items;
using PipelineLauncher.Demo.Tests.Stages.Single;
using PipelineLauncher.Extensions.PipelineRunner;
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
                .Stage<Stage, Item>()
                .Stage<Stage_1>()
                .Stage<Stage_Item_To_Item2, Item2>()
                .Stage<Stage_Item2_To_Item, Item>()
                .Stage<Stage_3>();

            // Make pipeline from stageSetup
            var pipelineRunner = pipelineSetup.Create();

            PostItemsAndPrintProcessedWithDefaultConditionToStop(pipelineRunner, items);
        }

        [Fact]
        public void Simple_Running_Stages_Extensions()
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

            var itemsProcessed = 0;
            pipelineRunner.WaitForItemReceived(
                () => pipelineRunner.Post(items),
                item =>
                {
                    PrintProcessed(item);
                    Interlocked.Increment(ref itemsProcessed);
                    if (itemsProcessed >= 6)
                    {
                        return true;
                    }

                    return false;
                });
        }
    }
}
