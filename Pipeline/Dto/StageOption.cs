using PipelineLauncher.Abstractions.Pipeline;
using PipelineLauncher.Exceptions;

namespace PipelineLauncher.Dto
{
    public struct StageOption<TInput, TOutput>
    {
        public TOutput Remove(TInput input)
        {
            throw new NoneParamException<TOutput>(new RemoveItem<TOutput>(input, GetType()));
        }

        public TOutput Skip(TInput input)
        {
            throw new NoneParamException<TOutput>(new SkipItem<TOutput>(input, GetType()));
        }

        public TOutput SkipTo<TSkipToStage>(TInput input) where TSkipToStage : IPipelineIn<TInput>
        {
            throw new NoneParamException<TOutput>(new SkipItemTill<TOutput>(typeof(TSkipToStage), input, GetType()));
        }
    }
}