using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineStage;
using PipelineLauncher.Demo.Tests.Items;
using PipelineLauncher.Stages;

namespace PipelineLauncher.Demo.Tests.Stages.Single
{
    public class StageSConditional : StageS<Item>, IConditionalStage<Item>
    {
        public override Item Execute(Item item)
        {
            item.Process(GetType());

            return item;
        }

        public PredicateResult Predicate(Item input)
        {
            return PredicateResult.Keep;
        }
    }

    public class StageSConditional1 : ConditionalStageS<Item>
    {
        public override Item Execute(Item item)
        {
            item.Process(GetType());

            return item;
        }

        public override PredicateResult Predicate(Item input)
        {
            return PredicateResult.Keep;
        }
    }
}
