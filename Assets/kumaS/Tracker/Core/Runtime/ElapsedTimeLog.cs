using System;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// 経過時間のログデータ。
    /// </summary>
    public class ElapsedTimeLog
    {
        /// <summary>
        /// ソースのId。
        /// </summary>
        public int SourceId { get; }

        /// <summary>
        /// 開始時間。
        /// </summary>
        public DateTime StartTime { get; }

        /// <summary>
        /// 経過時間。
        /// </summary>
        public TimeSpan ElapsedTime { get; }

        public ElapsedTimeLog(int sourceId, DateTime startTime, TimeSpan elapsedTime)
        {
            SourceId = sourceId;
            StartTime = startTime;
            ElapsedTime = elapsedTime;
        }
    }
}
