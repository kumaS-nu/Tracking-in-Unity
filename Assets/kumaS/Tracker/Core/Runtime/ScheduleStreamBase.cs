using Cysharp.Threading.Tasks;

using System;
using System.Collections.Concurrent;
using System.Threading;

using UniRx;

using UnityEngine;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// スケジュール可能なストリームの基底クラス。
    /// </summary>
    /// <typeparam name="TInput">入力のデータ型。</typeparam>
    /// <typeparam name="TOutput">出力のデータ型。</typeparam>
    public abstract class ScheduleStreamBase<TInput, TOutput> : MonoBehaviour, IScheduleStream
    {
        /// <summary>
        /// このプロセスの名前。初期化をさせるため抽象化。
        /// </summary>
        public abstract string ProcessName { get; set; }

        /// <summary>
        /// このノードのId。スケジューラーに設定されるので設定はそちらに任せる。
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ストリームで管理が必要な型で、このクラスで使う型。
        /// </summary>
        public abstract Type[] UseType { get; }

        /// <value>
        /// スレッドが利用可能か。
        /// </value>
        private bool[] isThreadAvailable;

        /// <value>
        /// スレッドが開くのを待つキュー。
        /// </value>
        private readonly ConcurrentQueue<CancellationTokenSource> waitingQueue = new ConcurrentQueue<CancellationTokenSource>();

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
        /// 現在利用可能か。初期化が終わったら<c>true</c>を返すようにするのを忘れずに。
        /// </summary>
        public abstract IReadOnlyReactiveProperty<bool> IsAvailable { get; }

        /// <summary>
        /// 入力のデータ型。
        /// </summary>
        public Type InputType { get => typeof(TInput); }

        /// <summary>
        /// 出力するデータ型。（ストリーム）
        /// </summary>
        public Type OutputType { get => typeof(TOutput); }

        /// <summary>
        /// 初期化。
        /// </summary>
        /// <param name="thread">用意するスレッド数。</param>
        public void Init(int thread)
        {
            isThreadAvailable = new bool[thread];
            for (var i = 0; i < thread; i++)
            {
                isThreadAvailable[i] = true;
            }

            InitInternal(thread);
        }

        /// <summary>
        /// 派生クラスではここに初期化処理を書く。初期化が終わったら利用可能にするのを忘れずに。
        /// </summary>
        /// <param name="thread">用意するスレッド数。</param>
        protected abstract void InitInternal(int thread);

        /// <summary>
        /// 処理をするプロセス。
        /// </summary>
        /// <param name="input">入力のストリーム。</param>
        /// <returns>出力されるストリーム。</returns>
        public IObservable<object> Process(IObservable<object> input)
        {
            return input.Cast<object, SchedulableData<TInput>>().Select(ProcessInternal).Cast<SchedulableData<TOutput>, object>();
        }

        /// <summary>
        /// 処理をここに書く。ストリームをつなげる。（Hot変換はスケジューラー側でするのでこちらではしない。）
        /// </summary>
        /// <param name="input">このストリームにつなげる。</param>
        /// <returns>つなげたストリームを返すようにする。</returns>
        protected abstract SchedulableData<TOutput> ProcessInternal(SchedulableData<TInput> input);

        /// <summary>
        /// 使えるスレッドを取得。これを使ったらこちらで開放すること。<see cref="FreeThread"/>
        /// </summary>
        /// <param name="thread">取得したスレッド番号。</param>
        /// <returns>スレッドを取得できたか。</returns>
        /// <code>
        /// if(TryGetThread(out var thread))
        /// {
        ///     try
        ///     {
        ///         // 処理
        ///     }
        ///     finary
        ///     {
        ///         FreeThread(thread);
        ///     }
        /// }
        /// </code>
        protected bool TryGetThread(out int thread)
        {
            lock (isThreadAvailable)
            {
                for (var i = 0; i < isThreadAvailable.Length; i++)
                {
                    if (isThreadAvailable[i])
                    {
                        isThreadAvailable[i] = false;
                        thread = i;
                        return true;
                    }
                }
            }
            thread = -1;
            return false;
        }

        /// <summary>
        /// スレッドを開放する。
        /// </summary>
        /// <param name="thread">解放するスレッド番号。</param>
        protected void FreeThread(int thread)
        {
            lock (isThreadAvailable)
            {
                isThreadAvailable[thread] = true;
            }
        }

        /// <summary>
        /// デバッグで表示できる内容にする。
        /// </summary>
        /// <param name="data">この処理の後のデータ。</param>
        /// <returns>デバッグで表示するデータ。</returns>
        public IDebugMessage DebugLog(object data)
        {
            return DebugLogInternal((SchedulableData<TOutput>)data);
        }

        /// <summary>
        /// デバッグで表示できる内容にする。
        /// </summary>
        /// <param name="data">この処理の後のデータ。</param>
        /// <returns>デバッグで表示するデータ。</returns>
        protected abstract IDebugMessage DebugLogInternal(SchedulableData<TOutput> data);
    }
}