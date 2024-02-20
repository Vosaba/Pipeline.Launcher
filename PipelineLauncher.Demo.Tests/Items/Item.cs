using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

namespace PipelineLauncher.Demo.Tests.Items
{
    public class Item
    {
        public List<(int ProcessId, Type StageType)> ProcessedBy { get; }
        public int Index { get; }
        public string Name => $"{nameof(Item)}#{Index}";

        public Item(int index)
        {
            Index = index;
            ProcessedBy = new List<(int ProcessId, Type StageType)>();
        }

        public Item(int index, List<(int ProcessId, Type StageType)> processedBy)
        {
            Index = index;
            ProcessedBy = processedBy;
        }

        public void Process(Type stageType)
        {
            ProcessedBy.Add((Thread.CurrentThread.ManagedThreadId, stageType));
        }

        public override string ToString()
        {
            var values = ProcessedBy.Select(x => $"{x.StageType.Name} : {x.ProcessId}").ToArray();
            return $"{Name}: {{ {string.Join(" } -> { ", values)} }};";
        }

        [DllImport("Kernel32.dll"), SuppressUnmanagedCodeSecurity]
        public static extern int GetCurrentProcessorNumber();
    }
}
