namespace PipelineLauncher.Abstractions.Dto
{
    public class PipelineItem
    {
        public object Item { get; }

        private PipelineItem() { }

        public PipelineItem(object item)
        {
            Item = item;
        }
    }

    public class PipelineItem<TItem>: PipelineItem
    {
        public new TItem Item { get; }

        public PipelineItem(TItem item): base (item)
        {
            Item = item;
        }
    }
}
