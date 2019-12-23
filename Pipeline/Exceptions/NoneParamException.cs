using PipelineLauncher.Abstractions.Dto;
using System;

namespace PipelineLauncher.Exceptions
{
    internal class NoneParamException<TItem> : Exception
    {
        public PipelineItem<TItem> Item { get; }
        public NoneParamException(PipelineItem<TItem> item)
        {
            Item = item;
        }
    }
}
