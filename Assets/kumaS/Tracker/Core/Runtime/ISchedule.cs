using System;
using System.Threading;

using UniRx;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// スケジュールで管理するためのインターフェース。内部用。
    /// </summary>
    public interface ISchedule : IDisposable
    {
        /// <summary>
        /// このプロセスの名前。
        /// </summary>
        string ProcessName { get; set; }

        /// <summary>
        /// このノードのId。スケジューラーに設定されるので設定はそちらに任せる。
        /// </summary>
        int Id { get; set; }

        /// <summary>
        /// 現在利用可能か。初期化が終わったら<c>true</c>を返すようにするのを忘れずに。
        /// </summary>
        IReadOnlyReactiveProperty<bool> IsAvailable { get; }

        /// <summary>
        /// 初期化。
        /// </summary>
        /// <param name="thread">用意するスレッド数。</param>
        /// <param name="token">このシステムを止める通知。</param>
        void Init(int thread, CancellationToken token);
    }
}
