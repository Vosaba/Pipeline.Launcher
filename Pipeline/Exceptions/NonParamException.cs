using System;
using System.Collections.Generic;
using System.Text;
using PipelineLauncher.Attributes;

namespace PipelineLauncher.Exceptions
{
    internal class NonParamException : Exception
    {
        public PipelineFilterResult Result { get; }
        public NonParamException(PipelineFilterResult result)
        {
            Result = result;
        }
    }
}
