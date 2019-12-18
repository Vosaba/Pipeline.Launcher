namespace PipelineLauncher.Demo.Tests.Stages
{
    //public class RemoveFilter : FilterService<Item>
    //{
    //    public override PipelineItem<Item> Filter(Item t)
    //    {
    //        Thread.Sleep(300);

    //        t.Value = t.Value + "FILTER->";
    //        t.ProcessedBy.Add(Thread.CurrentThread.ManagedThreadId);

    //        if (t.Value == "Item#1->FILTER->")
    //        {
    //            return Remove(t);

    //        }

    //        if (t.Value == "Item#3->FILTER->")
    //        {
    //            //return SkipTo<AsyncStage4>();
    //        }

    //        return Remove(t);
    //    }
    //}

    //[PipelineFilter(typeof(RemoveFilter))]
    //public class AsyncStage1_Filter : AsyncJob<Item>
    //{
    //    public override Item Execute(Item item)
    //    {
    //        item.Value = item.Value + "AsyncStage1->";
    //        Thread.Sleep(1000);

    //        item.ProcessedBy.Add(Thread.CurrentThread.ManagedThreadId);

    //        return item;
    //    }

    //    public override String ToString()
    //    {
    //        return "AsyncStage1";
    //    }
    //}
}
