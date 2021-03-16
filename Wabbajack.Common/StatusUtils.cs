using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using DynamicData;
using DynamicData.Binding;

namespace Wabbajack.Common
{
    public enum StatusCategory
    {
        Disk,
        Compute,
        Network,
        Finished
    }
    public static class StatusUtils
    {

        public static IObservable<StatusMessage> Updates => _updates;
        private static Subject<StatusMessage> _updates = new();
        public static ObservableCollectionExtended<StatusMessage> StatusMessages { get; } = new();

        static  StatusUtils()
        {
            Updates.ToObservableChangeSet(x => x.Id)
                .Sort(SortExpressionComparer<StatusMessage>.Ascending(x => x.Id))
                .Bind(StatusMessages)
                .Subscribe();
        }



        private class Comparitor : IComparer<StatusMessage>
        {
            public int Compare(StatusMessage x, StatusMessage y)
            {
                return x.Id.CompareTo(y.Id);
            }
        }

        internal static void SendStatus(StatusMessage msg)
        {
            _updates.OnNext(msg);
        }
        
        
    }

    public struct StatusMessage
    {
        public long Id;
        public StatusCategory Category;
        public string Message;
        public Percent Percent;
        public long BytesPerSecond;
    }

    public class StatusTracker : IDisposable
    {
        private StatusCategory _category;
        private static long _nextId = 0;
        private long _id;
        private long _maxBytes;
        private string _message = "";
        private long _processed = 0;
        private DateTime _lastUpdate;
        private long _oldProcessed;
        private static TimeSpan _oneSecond = TimeSpan.FromSeconds(1);
        private long _bytesPerSecond;

        public StatusTracker(StatusCategory category, long maxBytes, string message)
        {
            _category = category;
            _id = Interlocked.Increment(ref _nextId);
            _maxBytes = maxBytes;
            _message = message;
            _processed = 0;
            _lastUpdate = DateTime.Now;
            _oldProcessed = 0;
        }

        public void Update(long processed)
        {
            lock (this)
            {
                _processed += processed;

                var now = DateTime.Now;
                if (DateTime.Now - now >= _oneSecond)
                {
                    _bytesPerSecond = _processed - _oldProcessed;
                    _lastUpdate = now;
                }
                
                StatusUtils.SendStatus(new StatusMessage
                {
                    Id = _id, 
                    Category = _category, 
                    Message = _message, 
                    Percent = Percent.FactoryPutInRange(_processed, _maxBytes), 
                    BytesPerSecond = _bytesPerSecond
                });
            }
        }

        public void Dispose()
        {
            StatusUtils.SendStatus(new StatusMessage
            {
                Id = _id,
                Category = StatusCategory.Finished
            });
        }
    }
}
