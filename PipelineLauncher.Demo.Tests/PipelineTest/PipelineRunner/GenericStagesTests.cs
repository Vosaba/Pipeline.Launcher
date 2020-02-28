using System.Collections.Generic;
using PipelineLauncher.Demo.Tests.Extensions;
using PipelineLauncher.Demo.Tests.Items;
using PipelineLauncher.Demo.Tests.Stages.Bulk;
using PipelineLauncher.Demo.Tests.Stages.Single;
using Xunit;
using Xunit.Abstractions;

namespace PipelineLauncher.Demo.Tests.PipelineTest.PipelineRunner
{
    public class GenericStagesTests : PipelineTestBase
    {
        public GenericStagesTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void Multiple_Generic_Stages()
        {
            // Test input 6 items
            List<Item> items = MakeItemsInput(6);

            // Configure stages
            var pipelineSetup = PipelineCreator
                .Stage(new StageS())
                .Stage<StageS1>()
                .BulkStage(new BulkStage())
                .Stage<StageS2>()
                .BulkStage<BulkStage_1>()
                .Stage(new StageS3());


            // Make pipeline from stageSetup
            var pipelineRunner = pipelineSetup.CreateAwaitable();

            // Process items and print result
            (this, pipelineRunner).ProcessAndPrintResults(items);
        }

        [Fact]
        public void Multiple_Generic_Stages_ChangeTypes()
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
            var pipelineRunner = pipelineSetup.CreateAwaitable();

            // Process items and print result
            (this, pipelineRunner).ProcessAndPrintResults(items);
        }
    }
}
