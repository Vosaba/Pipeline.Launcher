using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.Demo.Tests.Items;
using PipelineLauncher.Demo.Tests.Stages.Bulk;
using PipelineLauncher.Demo.Tests.Stages.Single;
using PipelineLauncher.Stages;
using Xunit;
using Xunit.Abstractions;

namespace PipelineLauncher.Demo.Tests.PipelineSetup.AwaitablePipelineRunner
{
    public class LambdaStagesTests : PipelineTestBase
    {
        public LambdaStagesTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void Multiple_Mixed_Lambda_Stages()
        {
            // Test input 6 items
            List<Item> items = MakeItemsInput(6);

            // Configure stages
            var pipelineSetup = PipelineCreator
                .Stage((Item item) =>
                {
                    item.Process(GetType());

                    return item;
                })
                .BulkStage(items =>
                {
                    foreach (var item in items)
                    {
                        item.Process(GetType());
                    }

                    return items;
                })
                .Stage(async item =>
                {
                    await Task.Delay(1000);
                    item.Process(GetType());

                    return item;
                })
                .BulkStage(async (items) =>
                {
                    await Task.Delay(1000);
                    foreach (var item in items)
                    {
                        item.Process(GetType());
                    }

                    return items.AsEnumerable();
                })
                .Stage<Stage>();

            // Make pipeline from stageSetup
            var pipelineRunner = pipelineSetup.CreateAwaitable();

            // Process items and print result
            (this, pipelineRunner).ProcessAndPrintResults(items);
        }

        [Fact]
        public void Multiple_Mixed_Lambda_Stages_Options()
        {
            // Test input 6 items
            List<Item> items = MakeItemsInput(6);

            // Configure stages
            var pipelineSetup = PipelineCreator
                .Stage((Item item) =>
                {
                    item.Process(GetType());

                    return item;
                })
                .Stage((Item item, StageOption<Item, Item> options) =>
                {
                    if (item.Index == 0)
                    {
                        return options.Remove(item);
                    }

                    if (item.Index == 1)
                    {
                        return options.Skip(item);
                    }

                    if (item.Index == 2)
                    {
                        return options.SkipTo<Stage_1>(item);
                    }

                    item.Process(GetType());

                    return item;
                })
                .Stage<Stage>()
                .Stage<Stage_1>();

            // Make pipeline from stageSetup
            var pipelineRunner = pipelineSetup.CreateAwaitable();

            // Process items and print result
            (this, pipelineRunner).ProcessAndPrintResults(items);
        }

        [Fact]
        public void Multiple_Mixed_Lambda_Stages_Configuration()
        {
            // Test input 6 items
            List<Item> items = MakeItemsInput(6);

            // Configure stages
            var pipelineSetup = PipelineCreator
                .Stage((Item item) =>
                {
                    item.Process(GetType());

                    return item;
                })
                .Stage<Stage>()
                .BulkStage(items =>
                {
                    foreach (var item in items)
                    {
                        item.Process(GetType());
                    }

                    return items;
                },
                new BulkStageConfiguration
                {
                    BatchItemsCount = 3
                })
                .Stage<Stage_1>();

            // Make pipeline from stageSetup
            var pipelineRunner = pipelineSetup.CreateAwaitable();

            // Process items and print result
            (this, pipelineRunner).ProcessAndPrintResults(items);
        }
    }
}
