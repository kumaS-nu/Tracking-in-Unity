using System;
using System.Collections.Generic;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// デバッグメッセージを表すインターフェース。
    /// </summary>
    public interface IDebugMessage
    {
        /// <summary>
        /// この処理をしたやつの名前。スケジューラーが操作するのでいじらない。
        /// </summary>
        string ProcessName { get; set; }

        /// <summary>
        /// この処理をしたノードのID。スケジューラーが操作するのでいじらない。
        /// </summary>
        int Id { get; set; }

        /// <summary>
        /// このデータストリームの始まった時間。画像を取得した時間とか。
        /// </summary>
        DateTime StartTime { get; }

        /// <summary>
        /// デバッグメッセージの内容。
        /// </summary>
        Dictionary<string, string> Data { get; }
    }
}
