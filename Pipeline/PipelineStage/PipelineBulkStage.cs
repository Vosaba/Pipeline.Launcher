using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.Abstractions.PipelineStage.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Stages;

namespace PipelineLauncher.PipelineStage
{
    internal class PipelineBulkStage<TInput, TOutput> : PipelineBaseStage<IEnumerable<PipelineStageItem<TInput>>, IEnumerable<PipelineStageItem<TOutput>>>
    {
        private readonly IBulkStage<TInput, TOutput> _bulkStage;
        public PipelineBulkStage(IBulkStage<TInput, TOutput> bulkStage)
        {
            _bulkStage = bulkStage;
        }

        public BulkStageConfiguration Configuration => _bulkStage.Configuration;

        protected Task<IEnumerable<TOutput>> ExecuteAsync(TInput[] input, CancellationToken cancellationToken)
            => _bulkStage.ExecuteAsync(input, cancellationToken);

        protected override StageBaseConfiguration BaseConfiguration => Configuration;
        protected override Type StageType => _bulkStage.GetType();

        protected override async Task<IEnumerable<PipelineStageItem<TOutput>>> InternalExecute(IEnumerable<PipelineStageItem<TInput>> input, StageExecutionContext executionContext)
        {
            var inputArray = input as PipelineStageItem<TInput>[] ?? input.ToArray();

            var itemsToProcess = inputArray
                .Where(x => x.GetType() == typeof(PipelineStageItem<TInput>));

            var nonItems = inputArray
                .Where(x => x.GetType() != typeof(PipelineStageItem<TInput>))
                .Cast<NonResultStageItem<TInput>>()
                .ToArray();

            var nonItemsToProcess = nonItems
                .Where(x => x.ReadyToProcess<TInput>(StageType));

            var nonItemsToReturn = nonItems
                .Where(x => !x.ReadyToProcess<TInput>(StageType))
                .Select(x => x.Return<TOutput>())
                .ToArray();

            if (nonItemsToReturn.Any(x => x.GetType() != typeof(ExceptionStageItem<TOutput>)))
            {
                executionContext.ActionsSet?.DiagnosticHandler?.Invoke(
                    new DiagnosticItem(nonItemsToReturn.Where(x => x.GetType() != typeof(ExceptionStageItem<TOutput>)).Select(e=>e.OriginalItem).ToArray(), StageType, DiagnosticState.Skip));
            }

            var totalItemsToProcess =
                itemsToProcess.Select(x => x.Item)
                .Concat(nonItemsToProcess.Select(x => (TInput)x.OriginalItem));

            return (await ExecuteAsync(totalItemsToProcess.ToArray(), executionContext.CancellationToken)).Select(x => new PipelineStageItem<TOutput>(x))
                .Concat(nonItemsToReturn).ToArray();
        }

        protected override IEnumerable<PipelineStageItem<TOutput>> GetExceptionItem(IEnumerable<PipelineStageItem<TInput>> input, Exception ex, StageExecutionContext executionContext)
        {
            return new[] { new ExceptionStageItem<TOutput>(ex, executionContext.ActionsSet?.Retry, GetType(), GetItemsToBeProcessed(input)) };
        }

        protected override object[] GetItemsToBeProcessed(IEnumerable<PipelineStageItem<TInput>> input)
        {
            var pipelineStageItems = input as PipelineStageItem<TInput>[] ?? input.ToArray();
            
            var itemsToProcess = pipelineStageItems
                .Where(x => x.GetType() == typeof(PipelineStageItem<TInput>));

            var nonItems = pipelineStageItems
                .Where(x => x.GetType() != typeof(PipelineStageItem<TInput>))
                .Cast<NonResultStageItem<TInput>>();

            var nonItemsToProcess = nonItems
                .Where(x => x.ReadyToProcess<TInput>(StageType));

            
            return itemsToProcess.Select(x => (object)x.Item)
                    .Concat(nonItemsToProcess.Select(x => x.OriginalItem))
                    .ToArray();
        }

        protected override object[] GetProcessedItems(IEnumerable<PipelineStageItem<TOutput>> output)
        {
            var pipelineStageItems = output as PipelineStageItem<TOutput>[] ?? output.ToArray();

            var resultItems = pipelineStageItems
                .Where(x => x.GetType() == typeof(PipelineStageItem<TOutput>))
                .Select(x => x.Item)
                .Cast<object>();

            return resultItems.ToArray();
        }
    }
}