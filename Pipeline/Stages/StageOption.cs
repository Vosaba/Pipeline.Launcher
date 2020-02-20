using PipelineLauncher.Abstractions.Stages;
using PipelineLauncher.Exceptions;
using PipelineLauncher.PipelineStage;

namespace PipelineLauncher.Stages
{
    public struct StageOption<TInput, TOutput>
    {
        public TOutput Remove(TInput input)
        {
            throw new NonResultStageItemException<TOutput>(new RemoveStageItem<TOutput>(input, GetType()));
        }

        public TOutput Skip(TInput input)
        {
            throw new NonResultStageItemException<TOutput>(new SkipStageItem<TOutput>(input, GetType(), true));
        }

        public TOutput SkipTo<TSkipToStage>(TInput input) where TSkipToStage : IPipelineStageIn<TInput>
        {
            throw new NonResultStageItemException<TOutput>(new SkipStageItemTill<TOutput>(typeof(TSkipToStage), input, GetType()));
        }
    }
}