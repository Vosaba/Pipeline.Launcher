﻿using System.Threading;
using PipelineLauncher.Demo.Tests.Items;
using PipelineLauncher.Stages;

namespace PipelineLauncher.Demo.Tests.Stages.Single
{
    public class Stage : Stage<Item>
    {
        public override Item Execute(Item item)
        {
            item.Process(GetType());

            if (item.Index == 1)
            {
                throw new AbandonedMutexException();
            }

            return item;
        }
    }

    public class Stage_1 : Stage<Item>
    {
        public override Item Execute(Item item)
        {
            item.Process(GetType());

            return item;
        }
    }

    public class Stage_2 : Stage<Item>
    {
        public override Item Execute(Item item)
        {
            item.Process(GetType());

            return item;
        }
    }

    public class Stage_3 : Stage<Item>
    {
        public override Item Execute(Item item)
        {
            item.Process(GetType());

            return item;
        }
    }
}
