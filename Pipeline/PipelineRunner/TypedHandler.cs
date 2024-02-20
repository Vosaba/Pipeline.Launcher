using System.Linq;
using PipelineLauncher.Abstractions.PipelineEvents;
using PipelineLauncher.Abstractions.PipelineRunner;

namespace PipelineLauncher.PipelineRunner
{
    internal abstract class TypedHandler
    {
        public abstract bool TryToHandleExceptionItems(ExceptionItemsEventArgs exceptionItemsEventArgs);
        public abstract bool TryToHandleSkippedItem(SkippedItemEventArgs skippedItemEventArgs);
    }

    internal class TypedHandler<TItem> : TypedHandler, ITypedHandler<TItem>
    {
        public event ExceptionItemsReceivedEventHandler<TItem> ExceptionItemsReceivedEvent;
        public event SkippedItemReceivedEventHandler<TItem> SkippedItemReceivedEvent;

        public override bool TryToHandleExceptionItems(ExceptionItemsEventArgs exceptionItemsEventArgs)
        {
            if (exceptionItemsEventArgs.Items.Any() && exceptionItemsEventArgs.Items.All(x => x.GetType() == typeof(TItem)))
            {
                return TryToHandleExceptionItems(new ExceptionItemsEventArgs<TItem>(exceptionItemsEventArgs.Items, exceptionItemsEventArgs.StageType, exceptionItemsEventArgs.Exception, exceptionItemsEventArgs.Retry));
            }

            return false;
        }

        public override bool TryToHandleSkippedItem(SkippedItemEventArgs skippedItemEventArgs)
        {
            if (skippedItemEventArgs.Item.GetType() == typeof(TItem))
            {
                return TryToHandleSkippedItem(new SkippedItemEventArgs<TItem>(skippedItemEventArgs.Item, skippedItemEventArgs.StageType));
            }

            return false;
        }

        private bool TryToHandleExceptionItems(ExceptionItemsEventArgs<TItem> exceptionItemsEventArgs)
        {
            if (ExceptionItemsReceivedEvent != null)
            {
                ExceptionItemsReceivedEvent(exceptionItemsEventArgs);
                return true;
            }

            return false;
        }

        private bool TryToHandleSkippedItem(SkippedItemEventArgs<TItem> skippedItemEventArgs)
        {
            if (SkippedItemReceivedEvent != null)
            {
                SkippedItemReceivedEvent(skippedItemEventArgs);
                return true;
            }

            return false;
        }
    }
}