using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.PipelineStage.Configuration;
using PipelineLauncher.Abstractions.PipelineStage.Dto;
using PipelineLauncher.Stages;

namespace PipelineLauncher.Demo.Tests.Stages
{
    public class Stage1 : Stage<Item>
    {
        public override Item Execute(Item item)
        {
            if (item.Value == "Item#1->ed")
            {
                throw new Exception("fdfdf");
            }
            item.Value = item.Value + "AsyncStage1->";
            Thread.Sleep(1000);

            item.ProcessedBy.Add(Thread.CurrentThread.ManagedThreadId);

            return (item);
        }

        public override String ToString()
        {
            return "AsyncStage1";
        }
    }

    public interface IInterface
    {
        
    }

    public class F : IInterface
    {

    }
    public class Stage2 : Stage<Item>
    {
        public Stage2(F s )
        {
            
        }

        public override Item Execute(Item item)
        {
            //return Remove(item);

            item.Value = item.Value + "AsyncStage2->";
            Thread.Sleep(1000);

            item.ProcessedBy.Add(Thread.CurrentThread.ManagedThreadId);

            return item;

            //return "";
        }


        public bool Condition(Item input)
        {
            return input.Value.StartsWith("Item#0") || input.Value.StartsWith("Item#1");
        }

        public override String ToString()
        {
            return "AsyncStage2";
        }
    }

    public class Stage2Alternative : Stage<Item>
    {
        public override Item Execute(Item item)
        {
            item.Value = item.Value + "AsyncStage2Alternative->";
            Thread.Sleep(1000);

            item.ProcessedBy.Add(Thread.CurrentThread.ManagedThreadId);

            if (item.Value == "Item#0->AsyncStage1->AsyncStage2Alternative->")
            {
               //throw new Exception("dddddd");
            }

            return item;
        }

        public override StageConfiguration Configuration => new StageConfiguration { MaxDegreeOfParallelism = 2 };

        public bool Condition(Item input)
        {
            return !input.Value.StartsWith("Item#0") && !input.Value.StartsWith("Item#1");
        }

        public override String ToString()
        {
            return "AsyncStage2Alternative";
        }
    }

    public class Stage3 : Stage<Item>
    {
        public override Item Execute(Item item)
        {
            item.Value = item.Value + "AsyncStage3->";
            Thread.Sleep(1000);

            item.ProcessedBy.Add(Thread.CurrentThread.ManagedThreadId);

            return item;
        }

        public override string ToString()
        {
            return "AsyncStage3";
        }
    }

    public class Stage4 : Stage<Item>
    {

        public override async Task<Item> ExecuteAsync(Item item)
        {
            item.Value = item.Value + "AsyncStage4->";
            await Task.Delay(1000);

            item.ProcessedBy.Add(Thread.CurrentThread.ManagedThreadId);

            return item;
        }

        public override string ToString()
        {
            return "AsyncStage4";
        }
    }

    public class BulkcStage_Item_To_String : BulkStage<Item, string>
    {
        public override IEnumerable<string> Execute(IEnumerable<Item> items)
        {
            foreach (var item in items)
            {
                item.Value = item.Value + "Stage_Item_To_String->";
                Thread.Sleep(1000);

                item.ProcessedBy.Add(Thread.CurrentThread.ManagedThreadId);
            }

            return items.Select(e => e.Value);
        }

        public override string ToString()
        {
            return "Stage_Item_To_String";
        }
    }

    public class AsyncStage_String_To_Object : BulkStage<string, object>
    {
        public override IEnumerable<object> Execute(IEnumerable<string> items)
        {
            return items.Select(e => new object());
        }

        public override string ToString()
        {
            return "Stage_String_To_Object";
        }
    }
}
