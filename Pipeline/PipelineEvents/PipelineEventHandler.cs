namespace PipelineLauncher.PipelineEvents
{
    public delegate void ExceptionItemsReceivedEventHandler(ExceptionItemsEventArgs items);
    public delegate void SkippedItemReceivedEventHandler(SkippedItemEventArgs item);
    public delegate void ItemReceivedEventHandler<in TItem>(TItem item);
}