using System;

namespace PipelineLauncher.Exceptions
{
    public class PipelineRunTimeException : Exception
    {
        public PipelineRunTimeException()
        {
        }

        public PipelineRunTimeException(string message) : base(message)
        {
        }

        public PipelineRunTimeException(string message, Exception ex) : base(message, ex)
        {
        }
    }
}