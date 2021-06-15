using Cysharp.Threading.Tasks;

using System;
using System.Threading;

using UniRx;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// スケジュール可能なソースのインターフェース。
    /// </summary>
    internal interface IScheduleSource : ISchedule, IDebuggable
    {
        /// <summary>
        /// 取得可能なデータ。
        /// </summary>
        UniTask<object> GetSource(DateTime startTime, CancellationToken token);

        /// <summary>
        /// このクラスから取得可能なデータ型。
        /// </summary>
        Type SourceType { get; }

        /// <summary>
        /// このデータ型のストリームを作成・取得する。
        /// </summary>
        /// <returns>ストリーム</returns>
        Subject<object> GetStream();
    }
}