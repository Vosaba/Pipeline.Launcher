using PipelineLauncher.Demo.Tests.Extensions;
using PipelineLauncher.Demo.Tests.Items;
using PipelineLauncher.Demo.Tests.Stages.Bulk;
using PipelineLauncher.Demo.Tests.Stages.Single;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace PipelineLauncher.Demo.Tests.PipelineTest.PipelineRunner
{
    public class BranchStagesTests : PipelineTestBase
    {
        public BranchStagesTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void Stages_Branch()
        {
            // Test input 6 items
            List<Item> items = MakeItemsInput(6);

            // Configure stages
            var pipelineSetup = PipelineCreator
                .Stage<StageS, Item>()
                .Stage<StageS1>()
                .Branch(
                    (x => x.Index % 2 == 0,
                        branch => branch
                            .Stage<StageS2>()),
                    (x => false, 
                        branch => branch
                            .Stage<StageS2>()
                            .Stage<StageS3>()
                            .Stage<StageS4>()),
                    (x => true,
                        branch => branch
                            .BulkStage<BulkStage>()))
                .Stage<StageS5>()
                .Stage<StageS6>();

            // Make pipeline from stageSetup
            var pipelineRunner = pipelineSetup.CreateAwaitable();

            // Process items and print result
            (this, pipelineRunner).ProcessAndPrintResults(items);
        }


        [Fact]
        public void Stages_Branch_In_Branch()
        {
            // Test input 6 items
            List<Item> items = MakeItemsInput(6);

            // Configure stages
            var pipelineSetup = PipelineCreator
                .Stage<StageS, Item>()
                .Stage<StageS1>()
                .Branch(
                    (x => x.Index % 2 == 0,
                        branch => branch
                            .Stage<StageS2>()
                            .Branch(
                                (x => x.Index > 1,
                                    subBranch => subBranch
                                        .Stage<StageS3>()),
                                (x => true,
                                    subBranch => subBranch
                                        .Stage<StageS4>()))),
                    (x => true,
                        branch => branch
                            .BulkStage<BulkStage>()))
                .Stage<StageS5>()
                .Stage<StageS6>();

            // Make pipeline from stageSetup
            var pipelineRunner = pipelineSetup.CreateAwaitable();

            // Process items and print result
            (this, pipelineRunner).ProcessAndPrintResults(items);
        }

        [Fact]
        public void Stages_Broadcasting()
        {
            // Test input 6 items
            List<Item> items = MakeItemsInput(6);

            // Configure stages
            var pipelineSetup = PipelineCreator
                .Stage<StageS, Item>()
                .Stage<StageS1>()
                .Broadcast(
                    (x => x.Index % 2 == 0,
                        branch => branch
                            .Stage<StageS2>()),
                    (x => true,
                        branch => branch
                            .Stage<StageS3>()))
                .Stage<StageS5>()
                .Stage<StageS6>();

            // Make pipeline from stageSetup
            var pipelineRunner = pipelineSetup.CreateAwaitable();

            // Process items and print result
            (this, pipelineRunner).ProcessAndPrintResults(items);
        }
    }
}
