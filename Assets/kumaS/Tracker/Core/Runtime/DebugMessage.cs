using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// デバッグメッセージを表すクラス。
    /// </summary>
    public class DebugMessage : IDebugMessage
    {
        /// <summary>
        /// このデータストリームの始まった時間。画像を取得した時間とか。
        /// </summary>
        public DateTime StartTime { get; }

        /// <summary>
        /// デバッグメッセージの内容。
        /// </summary>
        public Dictionary<string, string> Data { get; }

        /// <summary>
        /// この処理をしたやつの名前。
        /// </summary>
        public string ProcessName { get; set; }

        /// <summary>
        /// この処理をしたノードのID。
        /// </summary>
        public int Id { get; set; }

        public DebugMessage(ISchedulableMetadata input, Dictionary<string, string> message)
        {
            StartTime = input.StartTime;
            Data = message;
        }

    }
}
