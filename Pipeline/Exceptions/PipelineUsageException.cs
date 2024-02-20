using System;

namespace PipelineLauncher.Exceptions
{
    public class PipelineUsageException : Exception
    {
        public PipelineUsageException()
        {
        }

        public PipelineUsageException(string message) : base(message)
        {
        }

        public PipelineUsageException(string message, Exception ex) : base(message, ex)
        {
        }
    }
}