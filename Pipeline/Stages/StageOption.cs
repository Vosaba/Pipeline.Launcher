using PipelineLauncher.Abstractions.PipelineStage;
using PipelineLauncher.Abstractions.Stages;
using PipelineLauncher.Exceptions;
using PipelineLauncher.PipelineStage;

namespace PipelineLauncher.Stages
{
    public struct StageOption<TInput, TOutput>
    {
        public TOutput Remove(TInput input)
        {
            throw new NoneParamException<TOutput>(new RemoveStageItem<TOutput>(input, GetType()));
        }

        public TOutput Skip(TInput input)
        {
            throw new NoneParamException<TOutput>(new SkipStageItem<TOutput>(input, GetType()));
        }

        public TOutput SkipTo<TSkipToStage>(TInput input) where TSkipToStage : IStageIn<TInput>
        {
            throw new NoneParamException<TOutput>(new SkipStageItemTill<TOutput>(typeof(TSkipToStage), input, GetType()));
        }
    }
}