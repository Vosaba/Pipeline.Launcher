using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Demo.Tests.Extensions;
using PipelineLauncher.Demo.Tests.Items;
using PipelineLauncher.Demo.Tests.Stages.Bulk;
using PipelineLauncher.Demo.Tests.Stages.Single;
using Xunit;
using Xunit.Abstractions;

namespace PipelineLauncher.Demo.Tests.PipelineTest.PipelineRunner
{
    public class ConfigurationStagesTests : PipelineTestBase
    {
        public ConfigurationStagesTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void Stages_CancellationToken_On_PipelineSetup()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            // Test input 6 items
            List<Item> items = MakeItemsInput(6);

            // Configure stages
            var pipelineSetup = PipelineCreator
                .Stage<StageS, Item>()
                .Stage<StageS1>()
                .Stage<StageSAsync>()
                .Stage<StageSAsyncCancelationToken>()
                .Stage<StageS2>();

            cancellationTokenSource.CancelAfter(1995);

            // Make pipeline from stageSetup
            var pipelineRunner = pipelineSetup
                .CreateAwaitable()
                .SetupCancellationToken(cancellationTokenSource.Token);

            // Process items and print result
            (this, pipelineRunner).ProcessAndPrintResults(items, true);
        }

        [Fact]
        public void Stages_DiagnosticEvent()
        {
            // Test input 3 items
            List<Item> items = MakeItemsInput(3);

            // Configure stages
            var pipelineSetup = PipelineCreator
                .Stage<StageS, Item>()
                .Stage<StageS1>(x => x.Index == 2 ? PredicateResult.Skip: PredicateResult.Keep)
                .BulkStage<BulkStage>(x => x.Index == 2 ? PredicateResult.Skip : PredicateResult.Keep)
                .Stage<StageS2>();


            // Make pipeline from stageSetup
            var pipelineRunner = pipelineSetup.CreateAwaitable();

            pipelineRunner.DiagnosticEvent += diagnosticItem =>
            {
                var itemsNames = diagnosticItem.Items.Cast<Item>().Select(x => x.Name).ToArray();
                var message = $"Stage: {diagnosticItem.StageType.Name} | Items: {{ {string.Join(" }; { ", itemsNames)} }} | State: {diagnosticItem.State}";

                if (!string.IsNullOrEmpty(diagnosticItem.Message))
                {
                    message += $" | Message: {diagnosticItem.Message}";
                }

                WriteLine(message);
            };

            // Process items and print result
            (this, pipelineRunner).ProcessAndPrintResults(items, true);
        }
    }
}
