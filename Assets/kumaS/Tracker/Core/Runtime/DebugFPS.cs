using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace kumaS.Tracker.Core {

    /// <summary>
    /// FPSをデバッグする際のデータ。
    /// </summary>
    public class DebugFPS : IDebugMessage
    {
        public string ProcessName { get; set; }
        public int Id { get; set; }
        public DateTime StartTime { get; }
        public Dictionary<string, string> Data { get; }

        public DebugFPS(Dictionary<string, string> data)
        {
            ProcessName = "FPS";
            Id = -1;
            StartTime = DateTime.Now;
            Data = data;
        }
    }
}
