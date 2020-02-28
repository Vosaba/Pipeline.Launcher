using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PipelineLauncher.Demo.Tests.Extensions;
using PipelineLauncher.Demo.Tests.Items;
using PipelineLauncher.Demo.Tests.Stages.Bulk;
using PipelineLauncher.Demo.Tests.Stages.Single;
using PipelineLauncher.Extensions;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace PipelineLauncher.Demo.Tests.PipelineTest.PipelineRunner.Extensions
{
    public class ExtensionsStagesTests : PipelineTestBase
    {
        public ExtensionsStagesTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void PipelineExtensions_Stages()
        {
            // Test input 6 items
            List<Item> items = MakeItemsInput(6);

            // Configure stages
            var pipelineSetup = PipelineCreator
                .Stage<StageS1, Item>()
                .BulkStage<BulkStage_1>()
                .DoNothing()
                .Stage(new StageS3())
                .ExtensionContext(extension => extension.GetHashCode(1995))
                .GetString("19_", "_95");


            // Make pipeline from stageSetup
            var pipelineRunner = pipelineSetup.CreateAwaitable();

            // Process items and print result
            (this, pipelineRunner).ProcessAndPrintResults(items);
        }

        [Fact]
        public void PipelineExtensions_Stages_More()
        {
            // Some common context
            var context = new { Value = 1995 }; 

            // Test input 6 items
            List<Item> items = MakeItemsInput(6);

            // Configure stages
            var pipelineSetup = PipelineCreator
                .Stage<StageS1, Item>()
                .MakeContext(context)
                .Stage(contextual =>
                {
                    //contextual.Context.Value++;
                    return contextual.Item;
                })
                .BulkStage<BulkStage_1>()
                .MakeContext(context)
                .Stage(contextual =>
                {
                    return contextual.Item;
                })
                .Stage(new StageS3());


            // Make pipeline from stageSetup
            var pipelineRunner = pipelineSetup.CreateAwaitable();

            // Process items and print result
            (this, pipelineRunner).ProcessAndPrintResults(items);
        }
    }
}
