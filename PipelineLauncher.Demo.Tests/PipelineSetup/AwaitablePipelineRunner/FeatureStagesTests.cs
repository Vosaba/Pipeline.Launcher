using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Demo.Tests.Items;
using PipelineLauncher.Demo.Tests.Stages.Bulk;
using PipelineLauncher.Demo.Tests.Stages.Single;
using Xunit;
using Xunit.Abstractions;

namespace PipelineLauncher.Demo.Tests.PipelineSetup.AwaitablePipelineRunner
{
    public class FeatureStagesTests : PipelineTestBase
    {
        public FeatureStagesTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void Stages_Condition_And_Predicates()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            // Test input 6 items
            List<Item> items = MakeItemsInput(6);

            // Configure stages
            var pipelineSetup = PipelineCreator
                .Stage<Stage, Item>()
                .Stage<Stage_1>()
                .Stage<Stage_Conditional>()
                .Stage<Stage_2>(item => item.Index == 4 ? PredicateResult.Skip : PredicateResult.Keep)
                .Stage<Stage_3>();

            cancellationTokenSource.CancelAfter(1995);

            // Make pipeline from stageSetup
            var pipelineRunner = pipelineSetup.CreateAwaitable();

            pipelineRunner.SkippedItemReceivedEvent += PipelineRunner_SkippedItemReceivedEvent;

            // Process items and print result
            (this, pipelineRunner).ProcessAndPrintResults(items);
        }

        private void PipelineRunner_SkippedItemReceivedEvent(Abstractions.PipelineEvents.SkippedItemEventArgs args)
        {
            //throw new System.NotImplementedException();
        }

        [Fact]
        public void Stages_CancellationToken_On_PipelineSetup()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            // Test input 6 items
            List<Item> items = MakeItemsInput(6);

            // Configure stages
            var pipelineSetup = PipelineCreator
                .Stage<Stage, Item>()
                .Stage<Stage_1>()
                .Stage<Stage_Async>()
                .Stage<Stage_Async_CancelationToken>()
                .Stage<Stage_2>();

            cancellationTokenSource.CancelAfter(1995);

            // Make pipeline from stageSetup
            var pipelineRunner = pipelineSetup
                .CreateAwaitable()
                .SetupCancellationToken(cancellationTokenSource.Token);

            // Process items and print result
            (this, pipelineRunner).ProcessAndPrintResults(items, true);
        }
    }
}
