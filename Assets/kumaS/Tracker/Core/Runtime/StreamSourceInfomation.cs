using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using UniRx;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// ストリームのソースの情報。
    /// </summary>
    internal class StreamSourceInfomation
    {
        internal int id;
        internal Subject<object> source;
        internal IScheduleSource scheduleSource;
        internal long baseInterval;
        internal TimeSpan interval;
        internal ConcurrentDictionary<DateTime, ConcurrentBag<TimeSpan>> elapsedTimes = new ConcurrentDictionary<DateTime, ConcurrentBag<TimeSpan>>();
        internal ConcurrentQueue<TimeSpan> elapsedTimeBuffer = new ConcurrentQueue<TimeSpan>();
        internal LinkedList<DateTime> updates = new LinkedList<DateTime>();
        internal int destinationCount;

        public StreamSourceInfomation(int _id, Subject<object> _source, IScheduleSource _scheduleSource, int _fps, DateTime _lastUpdate, int _thread)
        {
            id = _id;
            source = _source;
            scheduleSource = _scheduleSource;
            baseInterval = TimeSpan.FromSeconds(1.0 / _fps).Ticks;
            interval = TimeSpan.FromSeconds(2.0 / _fps);
            updates.AddLast(_lastUpdate);
            destinationCount = 0;
            for (var i = 0; i < _fps; i++)
            {
                elapsedTimeBuffer.Enqueue(new TimeSpan(interval.Ticks * _thread));
            }
        }
    }
}
