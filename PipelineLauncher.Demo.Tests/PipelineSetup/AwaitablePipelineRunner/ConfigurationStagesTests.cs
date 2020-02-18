using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PipelineLauncher.Demo.Tests.Items;
using PipelineLauncher.Demo.Tests.Stages.Bulk;
using PipelineLauncher.Demo.Tests.Stages.Single;
using Xunit;
using Xunit.Abstractions;

namespace PipelineLauncher.Demo.Tests.PipelineSetup.AwaitablePipelineRunner
{
    public class ConfigurationStagesTests : PipelineTestBase
    {
        public ConfigurationStagesTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void Stages_CancellationToken_On_PipelineCreator()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            // Test input 6 items
            List<Item> items = MakeItemsInput(6);

            PipelineCreator.WithCancellationToken(cancellationTokenSource.Token);

            // Configure stages
            var pipelineSetup = PipelineCreator
                .Stage<Stage, Item>()
                .Stage<Stage_1>()
                .Stage<Stage_Async>()
                .Stage<Stage_Async_CancelationToken>()
                .Stage<Stage_2>();

            cancellationTokenSource.CancelAfter(1995);

            // Make pipeline from stageSetup
            var pipelineRunner = pipelineSetup.CreateAwaitable();

            // Process items and print result
            (this, pipelineRunner).ProcessAndPrintResults(items, true);
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
