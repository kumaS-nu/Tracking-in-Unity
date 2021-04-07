using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// デバッグ可能にするためのインターフェース。内部用。
    /// </summary>
    internal interface IDebuggable: ISchedule
    {
        /// <summary>
        /// デバッグ出力をするか。
        /// </summary>
        IReadOnlyReactiveProperty<bool> IsDebug { get; }

        /// <summary>
        /// デバッグで表示するデータ。
        /// </summary>
        string[] DebugKey { get; }

        /// <summary>
        /// デバッグで表示するデータを作成。
        /// </summary>
        /// <param name="data">データ。</param>
        /// <returns>デバッグする内容。</returns>
        IDebugMessage DebugLog(object data);
    }
}
