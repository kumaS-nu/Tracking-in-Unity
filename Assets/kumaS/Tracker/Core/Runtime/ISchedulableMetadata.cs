using System;
using System.Collections.Generic;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// スケジュール可能なデータのメタデータのインターフェース。
    /// </summary>
    public interface ISchedulableMetadata
    {
        /// <summary>
        /// このデータは成功しているか。
        /// </summary>
        bool IsSuccess { get; }

        /// <summary>
        /// エラー内容。
        /// </summary>
        string ErrorMessage { get; }

        /// <summary>
        /// このデータストリームの始点の時間。
        /// </summary>
        DateTime StartTime { get; }

        /// <summary>
        /// このデータストリームのソースのId。
        /// </summary>
        int SourceId { get; }

        /// <summary>
        /// 各フェーズの経過時間。
        /// </summary>
        List<TimeSpan> ElapsedTimes { get; }
    }
}
