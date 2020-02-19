using System.Collections.Generic;
using System.Linq;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Demo.Tests.Items;
using PipelineLauncher.Demo.Tests.Stages.Single;
using Xunit;
using Xunit.Abstractions;

namespace PipelineLauncher.Demo.Tests.PipelineSetup.AwaitablePipelineRunner
{
    public class PredicatesStagesTests : PipelineTestBase
    {
        public PredicatesStagesTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void Stages_Condition_And_Predicates()
        {
            // Test input 6 items
            List<Item> items = MakeItemsInput(6);

            // Configure stages
            var pipelineSetup = PipelineCreator
                .Stage<Stage, Item>()
                .Stage<Stage_1>()
                .Stage<Stage_Conditional>()
                .Stage<Stage_2>(item => item.Index == 4 ? PredicateResult.Skip : PredicateResult.Keep)
                .Stage<Stage_3>();

            // Make pipeline from stageSetup
            var pipelineRunner = pipelineSetup.CreateAwaitable();

            // Process items and print result
            (this, pipelineRunner).ProcessAndPrintResults(items);
        }


        [Fact]
        public void Stages_Condition_And_Predicates_Overrides()
        {
            // Test input 6 items
            List<Item> items = MakeItemsInput(6);

            // Configure stages
            var pipelineSetup = PipelineCreator
                .Prepare<Item>() // TODO: explain
                .Stage<Stage_Conditional>()
                .Stage<Stage_Conditional_1>(item => PredicateResult.Remove)
                .Stage<Stage_2>(item => new []{ 1, 4 }.Contains(item.Index) ? PredicateResult.Remove : PredicateResult.Keep)
                .Stage<Stage_3>();

            // Make pipeline from stageSetup
            var pipelineRunner = pipelineSetup.CreateAwaitable();

            // Process items and print result
            (this, pipelineRunner).ProcessAndPrintResults(items);
        }
    }
}
