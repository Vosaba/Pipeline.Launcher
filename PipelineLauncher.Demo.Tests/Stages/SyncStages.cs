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
    public class BulkStageStage1 : BulkStage<Item>
    {
        public override IEnumerable<Item> Execute(IEnumerable<Item> items)
        {
            foreach (var item in items)
            {
                item.Value = item.Value + "Stage1->";
                Thread.Sleep(1000);

                item.ProcessedBy.Add(Thread.CurrentThread.ManagedThreadId);
            }

            return items;
        }

        public override String ToString()
        {
            return "Stage1";
        }
    }

    public class BulkStageStage2 : BulkStage<Item>
    {
        public override IEnumerable<Item> Execute(IEnumerable<Item> items)
        {
            foreach (var item in items)
            {
                item.Value = item.Value + "Stage2->";
                Thread.Sleep(1000);

                item.ProcessedBy.Add(Thread.CurrentThread.ManagedThreadId);
            }

            return items;
        }

        public  bool Condition(Item input)
        {
            return input.Value.StartsWith("Item#0") || input.Value.StartsWith("Item#1");
        }

        public override String ToString()
        {
            return "Stage2";
        }
    }

    public class BulkStageStage2Alternative : BulkStage<Item>
    {
        public override IEnumerable<Item> Execute(IEnumerable<Item> items)
        {
            foreach (var item in items)
            {
                item.Value = item.Value + "Stage2Alternative->";
                Thread.Sleep(1000);

                item.ProcessedBy.Add(Thread.CurrentThread.ManagedThreadId);
            }

            return items;
        }

        public  bool Condition(Item input)
        {
            return !input.Value.StartsWith("Item#0") && !input.Value.StartsWith("Item#1");
        }

        public override String ToString()
        {
            return "Stage2Alternative";
        }
    }

    public class BulkStageStage3 : BulkStage<Item>
    {
        public override async Task<IEnumerable<Item>> ExecuteAsync(IEnumerable<Item> items, CancellationToken token)
        {
            foreach (var item in items)
            {
                if (!token.IsCancellationRequested)
                {
                    await Task.Delay(1000);
                    item.Value = item.Value + "Stage3->";

                    item.ProcessedBy.Add(Thread.CurrentThread.ManagedThreadId);
                }
            }

            return items;
        }
        public override string ToString()
        {
            return "Stage3";
        }
    }

    public class BulkStageStage4 : BulkStage<Item>
    {
        public override IEnumerable<Item> Execute(IEnumerable<Item> items)
        {
            foreach (var item in items)
            {
                item.Value = item.Value + "Stage4->";
                Thread.Sleep(1000);

                item.ProcessedBy.Add(Thread.CurrentThread.ManagedThreadId);
            }

            return items;
        }

        public override string ToString()
        {
            return "Stage4";
        }

        public override BulkStageConfiguration Configuration => new BulkStageConfiguration {
            BatchItemsCount = 10,
            BatchItemsTimeOut = 3000
        };
    }

    

    public class BulkStageIntStage : BulkStage<int>
    {
        public override IEnumerable<int> Execute(IEnumerable<int> items)
        {
            return items;
        }

        public override string ToString()
        {
            return "Stage4";
        }
    }

    public class BulkStageStageItemToString : BulkStage<Item, string>
    {
        public override IEnumerable<string> Execute(IEnumerable<Item> items)
        {
            foreach (var item in items)
            {
                item.Value = item.Value + "Stage_Item_To_String->";
                Thread.Sleep(1000);

                item.ProcessedBy.Add(Thread.CurrentThread.ManagedThreadId);
            }

            return items.Select(e=> e.Value);
        }

        public override string ToString()
        {
            return "Stage_Item_To_String";
        }
    }

    public class BulkStageStageStringToObject : BulkStage<string, object>
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
