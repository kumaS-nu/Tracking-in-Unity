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
        internal object Schedulable { get; }

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
        /// 時間のストリームを設定。
        /// </summary>
        internal IObservable<ElapsedTimeLog> TimeStream { set => timeStream = value; }

        /// <summary>
        /// 時間のストリームを取得。
        /// </summary>
        /// <param name="stream">ストリーム。</param>
        /// <returns>成功したか。</returns>
        internal bool TryGetTimeStream(out IObservable<ElapsedTimeLog> stream)
        {
            if (timeStream != null && Schedulable is IScheduleDestination)
            {
                stream = timeStream;
                return true;
            }
            stream = default;
            return false;
        }
        private IObservable<ElapsedTimeLog> timeStream;

        /// <summary>
        /// エラーのストリームを設定。
        /// </summary>
        internal IObservable<object> ErrorStream { set => errorStream = value; }

        /// <summary>
        /// エラーのストリームを取得。
        /// </summary>
        /// <param name="stream">ストリーム。</param>
        /// <returns>成功したか。</returns>
        internal bool TryGetErrorStream(out IObservable<object> stream)
        {
            if (errorStream != null && Schedulable is IScheduleDestination)
            {
                stream = errorStream;
                return true;
            }
            stream = default;
            return false;
        }
        private IObservable<object> errorStream;

        internal StreamNode(object schedulable)
        {
            if (!(schedulable is IScheduleSource || schedulable is IScheduleStream || schedulable is IScheduleDestination))
            {
                throw new ArgumentException("schedulableが不正な型です。");
            }
            Schedulable = schedulable;
        }
    }
}
