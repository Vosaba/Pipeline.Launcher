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

    public interface IPipelineItem<in TItem>
    {
        //TItem Item { get; }

    }
    public class PipelineItem<TItem>: PipelineItem//, IPipelineItem<TItem>
    {
        public new TItem Item { get; }

        public PipelineItem(TItem item): base (item)
        {
            Item = item;
        }
    }
}
