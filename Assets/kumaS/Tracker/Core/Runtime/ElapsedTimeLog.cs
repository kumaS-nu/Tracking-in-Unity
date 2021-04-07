using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace kumaS.Tracker.Core
{
    public class ElapsedTimeLog
    {
        public int SourceId { get; }
        public DateTime StartTime { get; }
        public TimeSpan ElapsedTime { get; }

        public ElapsedTimeLog(int sourceId, DateTime startTime, TimeSpan elapsedTime)
        {
            SourceId = sourceId;
            StartTime = startTime;
            ElapsedTime = elapsedTime;
        }
    }
}
