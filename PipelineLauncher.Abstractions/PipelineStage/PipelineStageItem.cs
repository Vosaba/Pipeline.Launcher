namespace PipelineLauncher.Abstractions.PipelineStage
{
    public class PipelineStageItem
    {
        public object Item { get; }

        private PipelineStageItem() { }

        public PipelineStageItem(object item)
        {
            Item = item;
        }
    }

    public class PipelineStageItem<TItem>: PipelineStageItem
    {
        public new TItem Item { get; }

        public PipelineStageItem(TItem item): base (item)
        {
            Item = item;
        }
    }
}
