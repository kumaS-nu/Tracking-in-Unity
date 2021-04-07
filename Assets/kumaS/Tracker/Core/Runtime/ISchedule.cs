using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// スケジュールで管理するためのインターフェース。内部用。
    /// </summary>
    internal interface ISchedule
    {
        /// <summary>
        /// このプロセスの名前。
        /// </summary>
        string ProcessName { get; }

        /// <summary>
        /// このノードのId。スケジューラーに設定されるので設定はそちらに任せる。
        /// </summary>
        int Id { get; set; }

        /// <summary>
        /// 現在利用可能か。初期化が終わったら<c>true</c>を返すようにするのを忘れずに。
        /// </summary>
        IReadOnlyReactiveProperty<bool> IsAvailable { get; }
    }
}
