using System;
using PipelineLauncher.Abstractions.Pipeline;

namespace PipelineLauncher.Dto
{
    internal class StageSkipObject
    {
        private readonly Type _skipToJobType;
        private bool _isJobUnderFilterSkipped;
        public object Item { get; }

        public StageSkipObject(object item, Type skipToJobType = null)
        {
            Item = item;
            _isJobUnderFilterSkipped = false;
            _skipToJobType = skipToJobType;
        }

        public bool CanProcess(IPipelineJob job)
        {
            if (!_isJobUnderFilterSkipped)
            {
                _isJobUnderFilterSkipped = true;
                return false;
            }

            if (_skipToJobType != null && _skipToJobType != job.GetType())
            {
                return false;
            }

            return job.AcceptedType == Item.GetType();
        }
    }
}