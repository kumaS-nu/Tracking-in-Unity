using System;
using System.Collections.Generic;

using UniRx;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// ストリームのノード。ストリーム構築のために使う。
    /// </summary>
    internal class StreamNode
    {
        /// <summary>
        /// スケジュール可能なストリーム。
        /// </summary>
        internal ISchedule Schedulable { get; }

        /// <summary>
        /// この前のノード。
        /// </summary>
        internal List<StreamNode> Previous { get; } = new List<StreamNode>();

        /// <summary>
        /// この後のノード。
        /// </summary>
        internal List<StreamNode> Next { get; } = new List<StreamNode>();

        /// <summary>
        /// このストリームの始点のId。
        /// </summary>
        internal List<int> StartId { get; } = new List<int>();

        /// <summary>
        /// ストリームのソースを設定。(Share()済み。)
        /// </summary>
        internal Subject<object> SourceStream { set => sourceStream = value; }

        /// <summary>
        /// ストリームのソースを取得。(Share()済み。)
        /// </summary>
        /// <param name="stream">ストリーム。</param>
        /// <returns>成功したか。</returns>
        internal bool TryGetSourceStream(out Subject<object> stream)
        {
            if (sourceStream != null && Schedulable is IScheduleSource)
            {
                stream = sourceStream;
                return true;
            }
            stream = default;
            return false;
        }
        private Subject<object> sourceStream;

        /// <summary>
        /// ストリームを設定。(Share()済み。)
        /// </summary>
        internal IObservable<object> MainStream { set => mainStream = value; }

        /// <summary>
        /// ストリームを取得。(Share()済み。)
        /// </summary>
        /// <param name="stream">ストリーム。</param>
        /// <returns>成功したか。</returns>
        internal bool TryGetMainStream(out IObservable<object> stream)
        {
            if (mainStream != null && (Schedulable is IScheduleSource || Schedulable is IScheduleStream))
            {
                stream = mainStream;
                return true;
            }
            stream = default;
            return false;
        }
        private IObservable<object> mainStream;

        /// <summary>
        /// エラーのストリームを設定。
        /// </summary>
        internal IObservable<object> FinishStream { set => finishStream = value; }

        /// <summary>
        /// 終点のストリームを取得。
        /// </summary>
        /// <param name="stream">ストリーム。</param>
        /// <returns>成功したか。</returns>
        internal bool TryGetFinishStream(out IObservable<object> stream)
        {
            if (finishStream != null && Schedulable is IScheduleDestination)
            {
                stream = finishStream;
                return true;
            }
            stream = default;
            return false;
        }
        private IObservable<object> finishStream;

        internal StreamNode(ISchedule schedulable)
        {
            Schedulable = schedulable;
        }
    }
}
