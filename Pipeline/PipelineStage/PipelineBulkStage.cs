﻿using PipelineLauncher.Abstractions.PipelineEvents;
using PipelineLauncher.Abstractions.PipelineStage;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.Abstractions.PipelineStage.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Dto;

namespace PipelineLauncher.PipelineStage
{
    internal class PipelineBulkStage<TInput, TOutput> : PipelineBaseStage<IEnumerable<PipelineStageItem<TInput>>, IEnumerable<PipelineStageItem<TOutput>>>
    {
        private readonly IPipelineBulkStage<TInput, TOutput> _pipelineBulkStage;
        public PipelineBulkStage(IPipelineBulkStage<TInput, TOutput> pipelineBulkStage)
        {
            _pipelineBulkStage = pipelineBulkStage;
        }

        public BulkStageConfiguration Configuration => _pipelineBulkStage.Configuration;

        protected Task<IEnumerable<TOutput>> ExecuteAsync(TInput[] input, CancellationToken cancellationToken)
            => _pipelineBulkStage.ExecuteAsync(input, cancellationToken);

        protected override StageBaseConfiguration BaseConfiguration => Configuration;
        protected override Type StageType => _pipelineBulkStage.GetType();

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
                    new DiagnosticItem(nonItemsToReturn.Where(x => x.GetType() != typeof(ExceptionStageItem<TOutput>)).Select(e=>e.OriginalItem).ToArray(), GetType(), DiagnosticState.Skip));
            }

            var totalItemsToProcess =
                itemsToProcess.Select(x => x.Item)
                .Concat(nonItemsToProcess.Select(x => (TInput)x.OriginalItem));

            return (await ExecuteAsync(totalItemsToProcess.ToArray(), executionContext.CancellationToken)).Select(x => new PipelineStageItem<TOutput>(x))
                .Concat(nonItemsToReturn).ToArray();
        }

        protected override IEnumerable<PipelineStageItem<TOutput>> GetExceptionItem(IEnumerable<PipelineStageItem<TInput>> input, Exception ex, StageExecutionContext executionContext)
        {
            return new[] { new ExceptionStageItem<TOutput>(ex, executionContext.ActionsSet?.Retry, GetType(), GetOriginalItems(input)) };
        }

        protected override object[] GetOriginalItems(IEnumerable<PipelineStageItem<TInput>> input)
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

        protected override object[] GetOriginalItems(IEnumerable<PipelineStageItem<TOutput>> output)
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