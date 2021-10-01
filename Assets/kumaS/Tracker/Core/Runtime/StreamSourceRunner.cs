using Cysharp.Threading.Tasks;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using UniRx;

using UnityEngine;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// ストリームのソースを走らせる。
    /// </summary>
    internal class StreamSourceRunner
    {
        private int id;
        private Subject<object> source;
        private IScheduleSource scheduleSource;
        private int limitFps;
        private double realFps = 0;
        private double timeFps;
        private ConcurrentDictionary<DateTime, int> elapsedTimes = new ConcurrentDictionary<DateTime, int>();
        private ConcurrentDictionary<DateTime, int> missCount = new ConcurrentDictionary<DateTime, int>();
        private ConcurrentQueue<TimeSpan> elapsedTimeBuffer = new ConcurrentQueue<TimeSpan>();
        private LinkedList<DateTime> updates = new LinkedList<DateTime>();
        private int destinationCount;
        private int runningCount;
        private int cleanSkipped;
        private int thread;
        private ReactiveProperty<bool> isRunning = new ReactiveProperty<bool>(false);

        public string Label { get => id + scheduleSource.ProcessName; }

        public IReadOnlyReactiveProperty<bool> IsRunning { get => isRunning; }

        public StreamSourceRunner(int _id, Subject<object> _source, IScheduleSource _scheduleSource, int _fps, int _thread)
        {
            id = _id;
            source = _source;
            scheduleSource = _scheduleSource;
            limitFps = _fps;
            timeFps = _fps / 2.0;
            destinationCount = 0;
            runningCount = 0;
            cleanSkipped = 0;
            thread = _thread;
            for (var i = 0; i < _fps; i++)
            {
                elapsedTimeBuffer.Enqueue(new TimeSpan(TimeSpan.FromSeconds(1.0 / _fps).Ticks * _thread));
            }
        }

        /// <summary>
        /// ストリームを走らせる。
        /// </summary>
        public void Run()
        {
            var now = DateTime.Now;
            if(updates.Count == 0)
            {
                updates.AddLast(now - TimeSpan.FromSeconds(1.0 / limitFps) - TimeSpan.FromSeconds(1.0 / limitFps));
                updates.AddLast(now - TimeSpan.FromSeconds(1.0 / limitFps));
            }
            realFps = updates.Count / (updates.Last.Value - updates.First.Value).TotalSeconds;
            var predictFps = (updates.Count + 1) / (now - updates.First.Value).TotalSeconds;
            var limit = timeFps < limitFps ? timeFps : limitFps;
            if (runningCount < thread && predictFps <= limit)
            {
                isRunning.Value = true;
                Interlocked.Increment(ref runningCount);
                cleanSkipped++;
                updates.AddLast(now);
                if (updates.Count > limitFps)
                {
                    updates.RemoveFirst();
                }

                RunSource(now).Forget();

                if(cleanSkipped >= thread)
                {
                    CleanElapsedTime();
                }
            }
        }

        private async UniTask RunSource(DateTime startTime)
        {
            await UniTask.SwitchToThreadPool();
            var s = await scheduleSource.GetSource(startTime);
            source.OnNext(s);
        }

        /// <summary>
        /// デスティネーションの数を数え上げる。
        /// </summary>
        /// <param name="startId">ソースのId。</param>
        public void SetDestinationCount(int startId)
        {
            if(startId == id)
            {
                destinationCount++;
            }
        }

        /// <summary>
        /// FPSのログを書き足す。
        /// </summary>
        /// <param name="log">ログの辞書。</param>
        public void LogFPS(Dictionary<string, string> log)
        {
            log[Label] = realFps.ToString();
        }

        /// <summary>
        /// ストリームにかかった時間をアップデートする。
        /// </summary>
        /// <param name="time"></param>
        public void UpdateElapsedTime(ElapsedTimeLog time)
        {
            var elapsed = elapsedTimes.AddOrUpdate(time.StartTime, 1, (_, val) => ++val);
            missCount.TryGetValue(time.StartTime, out var miss);
            if(elapsed + miss == destinationCount)
            {
                Interlocked.Decrement(ref runningCount);
                isRunning.Value = runningCount != 0;
                if (miss == 0)
                {
                    elapsedTimeBuffer.Enqueue(time.ElapsedTime);
                    elapsedTimeBuffer.TryDequeue(out _);
                    elapsedTimes.TryRemove(time.StartTime, out _);
                    timeFps = thread / elapsedTimeBuffer.Average(t => t.TotalSeconds);
                }
            }       
        }

        /// <summary>
        /// ストリームがミスして終わったときに呼ぶ。
        /// </summary>
        public void UpdateMissCount(DateTime startTime)
        {
            var miss = missCount.AddOrUpdate(startTime, 1, (_, val) => ++val);
            elapsedTimes.TryGetValue(startTime, out var elapsed);
            if(miss + elapsed == destinationCount)
            {
                Interlocked.Decrement(ref runningCount);
                isRunning.Value = runningCount != 0;
            }
        }

        private void CleanElapsedTime()
        {
            var delete = new List<DateTime>();
            foreach(var miss in missCount)
            {   
                elapsedTimes.TryGetValue(miss.Key, out var elapsed);
                if(miss.Value + elapsed == destinationCount)
                {
                    delete.Add(miss.Key);
                }
            }

            foreach(var key in delete)
            {
                missCount.TryRemove(key, out _);
                elapsedTimes.TryRemove(key, out _);
            }
        }
    }
}
