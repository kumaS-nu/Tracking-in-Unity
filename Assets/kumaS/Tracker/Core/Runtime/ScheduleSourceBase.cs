using Cysharp.Threading.Tasks;

using System;
using System.Threading;

using UniRx;

using UnityEngine;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// スケジュール可能なストリームの開始点の基底クラス。
    /// </summary>
    /// <typeparam name="T">このクラスから取得可能なスケジュール可能なデータ型。</typeparam>
    /// <typeparam name="TData">このクラスから取得可能なデータ型。</typeparam>
    public abstract class ScheduleSourceBase<T> : MonoBehaviour, IScheduleSource
    {
        /// <summary>
        /// このプロセスの名前。初期化をさせるため抽象化。
        /// </summary>
        public abstract string ProcessName { get; set; }

        /// <summary>
        /// このノードのId。スケジューラーに設定されるので設定はそちらに任せる。
        /// </summary>
        public int Id { get; set; }

        /// <value>
        /// デバッグ出力をするか。
        /// </value>
        public BoolReactiveProperty isDebug = new BoolReactiveProperty(true);

        /// <summary>
        /// デバッグ出力をするか。
        /// </summary>
        public IReadOnlyReactiveProperty<bool> IsDebug { get => isDebug; }

        /// <summary>
        /// デバッグで出力するデータのキーを取得できるようにする。
        /// </summary>
        public abstract string[] DebugKey { get; }

        /// <summary>
        /// ソースをobjectに変換。
        /// </summary>
        public async UniTask<object> GetSource(DateTime startTime)
        {
            SchedulableData<T> source = await SourceInternal(startTime);
            return source;
        }

        /// <summary>
        /// データを取得するコードを書く。デフォルトでスレッドプール上で走る。
        /// </summary>
        /// <returns>データ</returns>
        protected abstract UniTask<SchedulableData<T>> SourceInternal(DateTime startTime);

        /// <summary>
        /// 現在利用可能か。初期化が終わったら<c>true</c>を返すように。
        /// </summary>
        public abstract IReadOnlyReactiveProperty<bool> IsAvailable { get; }

        /// <summary>
        /// このクラスから取得可能なデータ型。
        /// </summary>
        public Type SourceType { get => typeof(T); }

        /// <summary>
        /// このデータ型のストリームを作成・取得する。
        /// </summary>
        /// <returns>ストリーム</returns>
        public Subject<object> GetStream()
        {
            return new Subject<object>();
        }

        /// <summary>
        /// デバッグで表示できる内容にする。
        /// </summary>
        /// <param name="data">デバッグで表示するつもりのデータ。</param>
        /// <returns>デバッグで表示するデータ。</returns>
        public IDebugMessage DebugLog(object data)
        {
            return DebugLogInternal((SchedulableData<T>)data);
        }

        /// <summary>
        /// デバッグで表示できる内容にするのを書く。
        /// </summary>
        /// <param name="data">デバッグで表示するつもりのデータ。</param>
        /// <returns>デバッグで表示するデータ。</returns>
        protected abstract IDebugMessage DebugLogInternal(SchedulableData<T> data);

        /// <summary>
        /// 派生クラスではここに初期化処理を書く。初期化が終わったら利用可能にするのを忘れずに。
        /// </summary>
        /// <param name="thread">用意するスレッド数。</param>
        public abstract void Init(int thread, CancellationToken token);

        /// <summary>
        /// 派生クラスではこのソースを破棄する時の処理を書く。
        /// </summary>
        public abstract void Dispose();
    }
}
