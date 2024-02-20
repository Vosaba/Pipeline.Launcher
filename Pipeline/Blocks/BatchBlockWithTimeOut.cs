using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace PipelineLauncher.Blocks
{
    public sealed class BatchBlockWithTimeOut<T> : IDataflowBlock, IPropagatorBlock<T, T[]>, ISourceBlock<T[]>, ITargetBlock<T>, IReceivableSourceBlock<T[]>
    {
        private readonly AsyncAutoResetEvent _asyncAutoResetEvent = new AsyncAutoResetEvent();

        private readonly BatchBlock<T> _base;

        private readonly CancellationToken _cancellationToken;

        private readonly int _triggerTimeMs;

        public BatchBlockWithTimeOut(int batchSize, int triggerTimeMs)
        {
            _triggerTimeMs = triggerTimeMs;
            _base = new BatchBlock<T>(batchSize);
            PollReTrigger();
        }

        public BatchBlockWithTimeOut(int batchSize, int triggerTimeMs, GroupingDataflowBlockOptions dataflowBlockOptions)
        {
            _triggerTimeMs = triggerTimeMs;
            _cancellationToken = dataflowBlockOptions.CancellationToken;
            _base = new BatchBlock<T>(batchSize, dataflowBlockOptions);
            PollReTrigger();
        }

        public int BatchSize => _base.BatchSize;

        public int OutputCount => _base.OutputCount;

        public Task Completion => _base.Completion;

        public void Complete() => _base.Complete();

        void IDataflowBlock.Fault(Exception exception) => ((IDataflowBlock)_base).Fault(exception);

        public IDisposable LinkTo(ITargetBlock<T[]> target, DataflowLinkOptions linkOptions) => _base.LinkTo(target, linkOptions);

        T[] ISourceBlock<T[]>.ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<T[]> target, out bool messageConsumed) => ((ISourceBlock<T[]>)_base).ConsumeMessage(messageHeader, target, out messageConsumed);

        void ISourceBlock<T[]>.ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<T[]> target) => ((ISourceBlock<T[]>)_base).ReleaseReservation(messageHeader, target);

        bool ISourceBlock<T[]>.ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<T[]> target) => ((ISourceBlock<T[]>)_base).ReserveMessage(messageHeader, target);

        DataflowMessageStatus ITargetBlock<T>.OfferMessage(DataflowMessageHeader messageHeader, T messageValue, ISourceBlock<T> source, bool consumeToAccept)
        {
            _asyncAutoResetEvent.Set();
            return ((ITargetBlock<T>)_base).OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }

        public bool TryReceive(Predicate<T[]> filter, out T[] item) => _base.TryReceive(filter, out item);

        public bool TryReceiveAll(out IList<T[]> items) => _base.TryReceiveAll(out items);

        public override string ToString() => _base.ToString();

        public void TriggerBatch() => _base.TriggerBatch();

        private void PollReTrigger()
        {
            async Task Poll()
            {
                try
                {
                    while (!_cancellationToken.IsCancellationRequested)
                    {
                        await _asyncAutoResetEvent.WaitAsync()
                                    .ConfigureAwait(false);

                        await Task.Delay(_triggerTimeMs, _cancellationToken)
                                    .ConfigureAwait(false);
                        TriggerBatch();
                    }
                }
                catch (TaskCanceledException)
                {
                    // nope
                }
            }

            Task.Run(Poll, _cancellationToken);
        }
    }
}
