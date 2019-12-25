namespace PipelineLauncher.Abstractions.PipelineEvents
{
    public delegate void ExceptionItemsReceivedEventHandler(ExceptionItemsEventArgs args);
    public delegate void SkippedItemReceivedEventHandler(SkippedItemEventArgs args);
    public delegate void ItemReceivedEventHandler<in TItem>(TItem item);

    public delegate void DiagnosticEventHandler(DiagnosticEventArgs args);
}