using System;
using System.Collections.Generic;
using System.Text;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Attributes;

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
