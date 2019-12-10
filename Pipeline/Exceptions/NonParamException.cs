using PipelineLauncher.Abstractions.Dto;
using System;

namespace PipelineLauncher.Exceptions
{
    internal class NonParamException<TItem> : Exception
    {
        public PipelineItem<TItem> Item { get; }
        public NonParamException(PipelineItem<TItem> item)
        {
            Item = item;
        }
    }
}
