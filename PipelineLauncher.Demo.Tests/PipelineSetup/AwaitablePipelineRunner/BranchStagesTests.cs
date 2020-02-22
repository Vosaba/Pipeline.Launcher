using System.Collections.Generic;
using PipelineLauncher.Demo.Tests.Items;
using PipelineLauncher.Demo.Tests.Stages.Bulk;
using PipelineLauncher.Demo.Tests.Stages.Single;
using Xunit;
using Xunit.Abstractions;

namespace PipelineLauncher.Demo.Tests.PipelineSetup.AwaitablePipelineRunner
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
                .Stage<Stage, Item>()
                .Branch(
                    (x => x.Index % 2 == 0,
                        branchSetup => branchSetup
                            .Stage<Stage_1>()
                            .Branch(
                                (x => x.Index > 1,
                                    subBranchSetup => subBranchSetup
                                        .Stage<Stage_3>()),
                                (x => true,
                                    subBranchSetup => subBranchSetup
                                        .Stage<Stage_2>()))),
                    (x => true, 
                        branchSetup => branchSetup
                            .BulkStage<BulkStage>()))
                //.Stage<Stage_Conditional>()
                //.Stage<Stage_2>()
                .Stage<Stage_5>();

            // Make pipeline from stageSetup
            var pipelineRunner = pipelineSetup.CreateAwaitable();

            // Process items and print result
            (this, pipelineRunner).ProcessAndPrintResults(items);

            // Process items and print result
            (this, pipelineRunner).ProcessAndPrintResults(items);
        }
    }
}
