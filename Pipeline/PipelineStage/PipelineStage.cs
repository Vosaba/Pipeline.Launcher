using System;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineStage;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.Abstractions.PipelineStage.Dto;
using PipelineLauncher.Exceptions;

namespace PipelineLauncher.PipelineStage
{
    public abstract class PipelineStage<TInput, TOutput> : PipelineBaseStage<PipelineStageItem<TInput>, PipelineStageItem<TOutput>>, IPipelineStage<TInput, TOutput>
    {
        public abstract StageConfiguration Configuration { get; }
        public override StageBaseConfiguration BaseConfiguration => Configuration;

        public abstract Task<TOutput> ExecuteAsync(TInput input, CancellationToken cancellationToken);

        protected override async Task<PipelineStageItem<TOutput>> InternalExecute(PipelineStageItem<TInput> input, PipelineStageContext context)
        {
            try
            {
                switch (input)
                {
                    case RemoveStageItem<TInput> removeItem:
                        return removeItem.Return<TOutput>();

                    case ExceptionStageItem<TInput> exceptionItem:
                        return exceptionItem.Return<TOutput>();

                    case SkipStageItem<TInput> skipItem when typeof(TInput) == skipItem.OriginalItem.GetType():
                        return new PipelineStageItem<TOutput>(await ExecuteAsync((TInput)skipItem.OriginalItem, context.CancellationToken));
                    case SkipStageItem<TInput> skipItem when typeof(TInput) != skipItem.OriginalItem.GetType():
                        return skipItem.Return<TOutput>();

                    case SkipStageItemTill<TInput> skipItemTill
                        when GetType() == skipItemTill.SkipTillType:
                        return new PipelineStageItem<TOutput>(await ExecuteAsync((TInput)skipItemTill.OriginalItem, context.CancellationToken));
                    case SkipStageItemTill<TInput> skipItemTill
                        when GetType() != skipItemTill.SkipTillType:
                        return skipItemTill.Return<TOutput>();

                    default:
                        return new PipelineStageItem<TOutput>(await ExecuteAsync(input.Item, context.CancellationToken));
                }
            }
            catch (NoneParamException<TOutput> e)
            {
                context.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(input, GetType(), DiagnosticState.Skip));
                return e.StageItem;
            }
        }

        protected override PipelineStageItem<TOutput> GetExceptionItem(PipelineStageItem<TInput> input, Exception ex, PipelineStageContext context)
        {
            return new ExceptionStageItem<TOutput>(ex, context.ActionsSet?.Retry, GetType(), GetOriginalItems(input));
        }

        protected override object[] GetOriginalItems(PipelineStageItem<TInput> input)
        {
            return new object[] { input.Item };
        }
    }
}
