using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PipelineLauncher.Demo.Tests.Extensions;
using PipelineLauncher.Demo.Tests.Items;
using PipelineLauncher.Demo.Tests.Stages.Bulk;
using PipelineLauncher.Demo.Tests.Stages.Single;
using Xunit;
using Xunit.Abstractions;

namespace PipelineLauncher.Demo.Tests.PipelineTest.PipelineRunner
{
    public class BasicStagesTests : PipelineTestBase
    {
        public BasicStagesTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void Single_Stage()
        {
            // Test input 6 items
            List<Item> items = MakeItemsInput(6);

            // Configure stages
            var pipelineSetup = PipelineCreator
                    .Stage(new StageS());

            // Make pipeline from stageSetup
            var pipelineRunner = pipelineSetup.CreateAwaitable();

            // Start timer
            var stopWatch = StartTimer();

            // Process items
            var result = pipelineRunner.Process(items).ToArray();

            // Print elapsed time and result
            StopTimerAndPrintResult(result, stopWatch);
        }

        [Fact]
        public async Task Single_Stage_Different_Approaches()
        {
            // Test input 6 items
            List<Item> items = MakeItemsInput(6);

            // Configure stages
            var pipelineSetup = PipelineCreator
                .Stage(new StageS());

            // Make pipeline from stageSetup
            var pipelineRunner = pipelineSetup.CreateAwaitable();

            // 1) Process TODO

            // Process items
            var result = pipelineRunner.Process(items).ToArray();
            // Print elapsed time and result
            PrintResult(result);

            // 2) ProcessAsync TODO

            // Process items
            result = (await pipelineRunner.ProcessAsync(items)).ToArray();
            // Print elapsed time and result
            PrintResult(result);

            // 3) ItemReceivedEvent TODO

            pipelineRunner.ItemReceivedEvent += PrintProcessed;
            // Process items
            await pipelineRunner.GetCompletionTaskFor(items);
            pipelineRunner.ItemReceivedEvent -= PrintProcessed;
            WriteSeparator();

            // 4) ProcessAsyncEnumerable TODO

            await foreach (var item in pipelineRunner.ProcessAsyncEnumerable(items))
            {
                PrintProcessed(item);
            }
            WriteSeparator();
        }

        [Fact]
        public void Multiple_Stages()
        {
            // Test input 6 items
            List<Item> items = MakeItemsInput(6);

            // Configure stages
            var pipelineSetup = PipelineCreator
                    .Stage(new StageS())
                    .Stage(new StageS1())
                    .Stage(new StageS2())
                    .Stage(new StageS3());

            // Make pipeline from stageSetup
            var pipelineRunner = pipelineSetup.CreateAwaitable();

            // Process items and print result
            (this, pipelineRunner).ProcessAndPrintResults(items);
        }

        [Fact]
        public void Multiple_BulkStages()
        {
            // Test input 6 items
            List<Item> items = MakeItemsInput(6);

            // Configure stages
            var pipelineSetup = PipelineCreator
                .BulkStage(new BulkStage())
                .BulkStage(new BulkStage_1())
                .BulkStage(new BulkStage_2())
                .BulkStage(new BulkStage_3());

            // Make pipeline from stageSetup
            var pipelineRunner = pipelineSetup.CreateAwaitable();

            // Process items and print result
            (this, pipelineRunner).ProcessAndPrintResults(items);
        }

        [Fact]
        public void Multiple_Mixed_Stages()
        {
            // Test input 6 items
            List<Item> items = MakeItemsInput(6);

            // Configure stages
            var pipelineSetup = PipelineCreator
                .Stage(new StageS())
                .BulkStage(new BulkStage_1())
                .Stage(new StageS2())
                .BulkStage(new BulkStage_3());

            // Make pipeline from stageSetup
            var pipelineRunner = pipelineSetup.CreateAwaitable();

            // Process items and print result
            (this, pipelineRunner).ProcessAndPrintResults(items);
        }

        [Fact]
        public void Multiple_Mixed_Stages_Change_Types()
        {
            // Test input 6 items
            List<Item> items = MakeItemsInput(6);

            // Configure stages
            var pipelineSetup = PipelineCreator
                .Stage(new StageS())
                .BulkStage(new BulkStage_1())
                .Stage(new StageSItemToItem2())
                .Stage(new StageSItem2ToItem())
                .BulkStage(new BulkStage_Item_To_Object());

            // Make pipeline from stageSetup
            var pipelineRunner = pipelineSetup.CreateAwaitable();

            // Process items and print result
            (this, pipelineRunner).ProcessAndPrintResults(items);
        }
    }
}
