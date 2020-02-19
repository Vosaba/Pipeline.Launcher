using System;
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

        [Fact]
        public void Stages_DiagnosticHandler_On_PipelineCreator()
        {
            // Test input 6 items
            List<Item> items = MakeItemsInput(4);

            // Configure stages
            var pipelineSetup = PipelineCreator
                //.WithDiagnostic((DiagnosticItem diagnosticItem) =>
                //{
                //    var itemsNames = diagnosticItem.Items.Cast<Item>().Select(x => x.Name).ToArray();
                //    var message = $"Stage: {diagnosticItem.StageType.Name} | Items: {{ {string.Join(" }; { ", itemsNames)} }} | State: {diagnosticItem.State}";

                //    if (!string.IsNullOrEmpty(diagnosticItem.Message))
                //    {
                //        message += $" | Message: {diagnosticItem.Message}";
                //    }

                //    WriteLine(message);
                //})
                .Stage<Stage, Item>()
                .BulkStage<BulkStage_Async>()
                .Stage(item =>
                {
                    if (item.Index == 0)
                    {
                        throw new Exception("Test exception");
                    }

                    return item;
                })
                .BulkStage<BulkStage_Async>(item => item.Index == 1 ? PredicateResult.Skip : PredicateResult.Keep)
                .Stage<Stage_2>();


            // Make pipeline from stageSetup
            var pipelineRunner = pipelineSetup
                .CreateAwaitable();

            //pipelineRunner.DiagnosticEvent += (DiagnosticItem diagnosticItem) =>
            //{
            //    var itemsNames = diagnosticItem.Items.Cast<Item>().Select(x => x.Name).ToArray();
            //    var message =
            //        $"Stage: {diagnosticItem.StageType.Name} | Items: {{ {string.Join(" }; { ", itemsNames)} }} | State: {diagnosticItem.State}";

            //    if (!string.IsNullOrEmpty(diagnosticItem.Message))
            //    {
            //        message += $" | Message: {diagnosticItem.Message}";
            //    }

            //    WriteLine(message);
            //};

            // Process items and print result
            (this, pipelineRunner).ProcessAndPrintResults(items, true);
        }
    }
}
