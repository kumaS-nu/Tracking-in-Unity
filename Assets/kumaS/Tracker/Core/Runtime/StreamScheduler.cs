using Cysharp.Threading.Tasks;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using UniRx;

using UnityEngine;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// ストリームを管理し、パフォーマンスを制御する。
    /// </summary>
    public class StreamScheduler : MonoBehaviour
    {

        #region インスペクターで使うものたち

        /// <value>
        /// ソースの折り畳み。
        /// </value>
        [SerializeField]
        internal bool sourceFold = false;

        /// <value>
        /// ストリームの折り畳み。
        /// </value>
        [SerializeField]
        internal bool streamFold = false;

        /// <value>
        /// ストリームそれぞれの折り畳み。
        /// </value>
        [SerializeField]
        internal List<bool> streamFolds;

        /// <value>
        /// FPSの折り畳み。
        /// </value>
        [SerializeField]
        internal bool fpsFold = true;

        /// <value>
        /// 終点の折り畳み。
        /// </value>
        [SerializeField]
        internal bool destinationFold = false;

        /// <value>
        /// ストリームの始点たち。
        /// </value>
        [SerializeField]
        internal List<MonoBehaviour> sources = new List<MonoBehaviour>();

        /// <value>
        /// ストリームのパイプたち。
        /// </value>
        [SerializeField]
        internal List<MonoBehaviour> streams = new List<MonoBehaviour>();

        /// <value>
        /// ストリームの終点たち。
        /// </value>
        [SerializeField]
        internal List<MonoBehaviour> destinations = new List<MonoBehaviour>();

        /// <value>
        /// ストリームのパイプの始点のインデックス。
        /// </value>
        [SerializeField]
        internal List<int> streamUnitStarts = new List<int>();

        /// <value>
        /// ストリームの入力。始点はマイナスで、パイプはプラス。なしは0。
        /// </value>
        [SerializeField]
        internal List<int> streamInputs = new List<int>();

        /// <value>
        /// ストリームの出力。終点はマイナスで、パイプはプラス。なしは0。
        /// </value>
        [SerializeField]
        internal List<int> streamOutputs = new List<int>();

        /// <value>
        /// 目標のfps。フレームレートを超えないように。
        /// </value>
        [SerializeField]
        internal List<int> fps = new List<int>();

        /// <value>
        /// 最大スレッド数。
        /// </value>
        [SerializeField]
        internal int thread = 8;

        /// <value>
        /// デバッグ出力をするか。
        /// </value>
        [SerializeField]
        internal BoolReactiveProperty isDebug = new BoolReactiveProperty(false);

        /// <value>
        /// エラーを表示するか。
        /// </value>
        [SerializeField]
        internal BoolReactiveProperty isShowError = new BoolReactiveProperty(true);

        /// <value>
        /// デバッグ出力をファイルに書き込むか。
        /// </value>
        [SerializeField]
        internal BoolReactiveProperty isWriteFile = new BoolReactiveProperty(false);

        /// <value>
        /// FPSのデバッグをするか。
        /// </value>
        [SerializeField]
        internal BoolReactiveProperty isDebugFPS = new BoolReactiveProperty(false);

        #endregion

        private List<StreamNode> allNodes = default;

        private readonly Subject<DebugFPS> fpsSource = new Subject<DebugFPS>();

        private string[] fpsDebugKey;

        private readonly List<IReadOnlyReactiveProperty<bool>> nodeIsDebug = new List<IReadOnlyReactiveProperty<bool>>();

        private readonly List<IObservable<IDebugMessage>> debugStreams = new List<IObservable<IDebugMessage>>();

        private IDisposable debugStream = default;

        private readonly Dictionary<int, StreamWriter> streamWriters = new Dictionary<int, StreamWriter>();

        private readonly Dictionary<int, StreamSourceRunner> sourceRunners = new Dictionary<int, StreamSourceRunner>();

        private CompositeDisposable disposable = new CompositeDisposable();

        private CancellationTokenSource source = new CancellationTokenSource();

        private bool broken = false;

        public bool Broken { get => broken; }

        private void Reset()
        {
            if (FindObjectsOfType<StreamScheduler>().Length > 1)
            {
                Debug.LogError("Schedulerはシーン上に一つしか置けません。");
                DestroyImmediate(this);
            }
        }

        private void Start()
        {
            Application.wantsToQuit += ShouldQuit;
            Debug.Log("開始しています...");
            _ = Initialize(source.Token);
        }

        private void Detouch()
        {
            Application.wantsToQuit -= ShouldQuit;
        }

        private bool ShouldQuit()
        {
            return disposable.IsDisposed;
        }

        async UniTaskVoid Initialize(CancellationToken token)
        {
            await UniTask.DelayFrame(5);
            var builder = new StreamBuilder(sources, streams, destinations, streamUnitStarts, streamInputs, streamOutputs);
            allNodes = builder.Build(disposable, OnError);

            _ = InitializeEachNode(token);

            var finishStreams = new List<IObservable<object>>();
            var startIds = new List<int>();
            foreach (StreamNode node in allNodes)
            {
                if (node.TryGetSourceStream(out Subject<object> sStream))
                {
                    sourceRunners[allNodes.IndexOf(node)] = new StreamSourceRunner(allNodes.IndexOf(node), sStream, (IScheduleSource)node.Schedulable, fps[sourceRunners.Count], thread);
                }

                if (node.TryGetMainStream(out IObservable<object> mStream))
                {
                    var schedulable = (IDebuggable)node.Schedulable;

                    var id = allNodes.IndexOf(node);
                    var processName = schedulable.ProcessName;
                    IDebugMessage AddNodeInfo(IDebugMessage data)
                    {
                        data.Id = id;
                        data.ProcessName = processName;
                        return data;
                    };

                    nodeIsDebug.Add(schedulable.IsDebug);
                    debugStreams.Add(mStream.Select(schedulable.DebugLog).Select(AddNodeInfo));
                }

                if (node.TryGetFinishStream(out IObservable<object> eStream))
                {
                    finishStreams.Add(eStream);
                    startIds.AddRange(node.StartId);
                }

                node.Schedulable.Id = allNodes.IndexOf(node);
            }

            var fpsKey = new List<string>();
            foreach (var runner in sourceRunners)
            {
                foreach (var startId in startIds)
                {
                    runner.Value.SetDestinationCount(startId);
                }
                fpsKey.Add(runner.Value.Label);
            }
            fpsDebugKey = fpsKey.ToArray();

            nodeIsDebug.Add(isDebugFPS);
            debugStreams.Add(fpsSource);

            nodeIsDebug.Merge().Merge(isDebug, isWriteFile).Subscribe(_ => SetDebugStream()).AddTo(disposable);

            var finishStream = finishStreams.Merge().Cast<object, ISchedulableMetadata>().Share();

            var missStream = finishStream.Where(data => data.ErrorMessage != "").Share();
            missStream.Subscribe(UpdateMissCount).AddTo(disposable);
            if (isShowError.Value && isDebug.Value)
            {
                missStream.ObserveOnMainThread().Subscribe(ShowError).AddTo(disposable);
            }

            finishStream.Where(data => data.ErrorMessage == "").Subscribe(UpdateElapsedTime).AddTo(disposable);

            ResourceManager.SetResource(allNodes);

            var dummyProperty = new ReactiveProperty<bool>(false);
            Observable.Merge(allNodes.Select(node => node.Schedulable.IsAvailable)).Merge(dummyProperty)
                .First(_ => allNodes.All(node => node.Schedulable.IsAvailable.Value)).Do(_ => Debug.Log("開始しました．"))
                .Subscribe(__ => _ = UniTask.RunOnThreadPool(() => ScheduleLoop(source.Token)), _ => broken = true).AddTo(disposable);
            dummyProperty.Value = true;
        }

        private void OnError(Exception e)
        {
            broken = true;
            Debug.LogException(e);
            source.Cancel();
            var isRunnings = sourceRunners.Select(runner => runner.Value.IsRunning);
            isRunnings.Merge().Select(_ => source.Token).ObserveOn(Scheduler.ThreadPool).Subscribe(token =>
            {
                if (isRunnings.All(value => !value.Value) && token.IsCancellationRequested)
                {
                    foreach (var node in allNodes)
                    {
                        node.Schedulable.Dispose();
                    }
                    source.Dispose();
                    disposable.Dispose();
                    Application.Quit();
                }
            }).AddTo(disposable);
        }

        /// <summary>
        /// デバッグ出力の設定をする。
        /// </summary>
        private void SetDebugStream()
        {
            if (debugStream != null)
            {
                debugStream.Dispose();
            }
            foreach (KeyValuePair<int, StreamWriter> sw in streamWriters)
            {
                sw.Value.Flush();
                sw.Value.Close();
                sw.Value.Dispose();
            }
            streamWriters.Clear();

            if (isDebug.Value)
            {
                if (isWriteFile.Value)
                {
                    if (!Directory.Exists(Path.Combine(Application.dataPath, "Debug data")))
                    {
                        Directory.CreateDirectory(Path.Combine(Application.dataPath, "Debug data"));
                    }
                    var time = DateTime.Now.ToString("yy-MM-dd-hh-mm-ss");
                    if (!Directory.Exists(Path.Combine(Application.dataPath, "Debug data", time)))
                    {
                        Directory.CreateDirectory(Path.Combine(Application.dataPath, "Debug data", time));
                    }

                    foreach (StreamNode node in allNodes)
                    {
                        if (node.TryGetMainStream(out _))
                        {
                            var debaggable = (IDebuggable)node.Schedulable;
                            if (debaggable.IsDebug.Value)
                            {
                                var sw = new StreamWriter(Path.Combine(Application.dataPath, "Debug data", time, allNodes.IndexOf(node) + debaggable.ProcessName + ".csv"), false, Encoding.UTF8);
                                streamWriters.Add(allNodes.IndexOf(node), sw);
                                var sb = new StringBuilder();
                                sb.Append("startTime,");
                                foreach (var header in debaggable.DebugKey)
                                {
                                    sb.Append(header);
                                    sb.Append(",");
                                }
                                sb.Remove(sb.Length - 1, 1);
                                sw.WriteLine(sb.ToString());
                            }
                        }

                    }

                    if (isDebugFPS.Value)
                    {
                        var sw = new StreamWriter(Path.Combine(Application.dataPath, "Debug data", time, "FPS.csv"), false, Encoding.UTF8);
                        streamWriters.Add(-1, sw);
                    }
                }
                var currentDebugStreams = new List<IObservable<IDebugMessage>>();
                for (var i = 0; i < debugStreams.Count; i++)
                {
                    if (nodeIsDebug[i].Value)
                    {
                        currentDebugStreams.Add(debugStreams[i]);
                    }
                }
                debugStream = currentDebugStreams.Merge().ObserveOnMainThread().Subscribe(OutputDebug).AddTo(disposable);
            }
        }

        /// <summary>
        /// デバッグ出力する。
        /// </summary>
        /// <param name="message">内容。</param>
        private void OutputDebug(IDebugMessage message)
        {
            var sb = new StringBuilder();
            string[] dKey;
            if (message.Id == -1)
            {
                dKey = fpsDebugKey;
            }
            else
            {
                dKey = ((IDebuggable)allNodes[message.Id].Schedulable).DebugKey;
            }
            foreach (var key in dKey)
            {
                if (message.Data.TryGetValue(key, out var value))
                {
                    sb.Append(value);
                }
                sb.Append(",");
            }
            if (dKey.Length > 0) {
                sb.Remove(sb.Length - 1, 1); 
            }

            if (isWriteFile.Value)
            {
                sb.Insert(0, ",");
                sb.Insert(0, message.StartTime);

                lock (streamWriters[message.Id])
                {
                    streamWriters[message.Id].WriteLine(sb.ToString());
                }
            }
            else
            {
                var sb2 = new StringBuilder();
                string[] debugKey;
                if (message.Id == -1)
                {
                    sb2.Append("FPS\n");
                    debugKey = fpsDebugKey;
                }
                else
                {
                    sb2.AppendFormat("{0},{1},{2}\n", message.ProcessName, message.Id, message.StartTime);
                    debugKey = ((IDebuggable)allNodes[message.Id].Schedulable).DebugKey;
                }

                foreach (var key in debugKey)
                {
                    sb2.Append(key);
                    sb2.Append(",");
                }
                if (debugKey.Length > 0)
                {
                    sb2.Remove(sb2.Length - 1, 1);
                }
                sb2.Append("\n");
                sb2.Append(sb);
                Debug.Log(sb2.ToString());
            }
        }

        private void UpdateMissCount(ISchedulableMetadata data)
        {
            sourceRunners[data.SourceId].UpdateMissCount(data.StartTime);
        }

        /// <summary>
        /// エラーを見せる。
        /// </summary>
        /// <param name="data">エラー内容。</param>
        private void ShowError(ISchedulableMetadata data)
        {
            Debug.LogError(data.ErrorMessage);
        }

        /// <summary>
        /// ストリームの各ノードを初期化する。
        /// </summary>
        private async UniTask InitializeEachNode(CancellationToken token)
        {
            List<UniTask> tasks = new List<UniTask>();
            foreach (StreamNode node in allNodes)
            {
                try
                {
                    tasks.Add(UniTask.Run(async () =>
                    {
                        await UniTask.SwitchToMainThread();
                        node.Schedulable.Init(thread, token);
                    }));
                }
                catch (Exception e)
                {
                    broken = true;
                    Debug.LogError(node.Schedulable.ProcessName + e.ToString());
                    source.Dispose();
                    disposable.Dispose();
                    Application.Quit();
                }
            }
            await tasks;
        }

        /// <summary>
        /// 経過時間をアップデートする。
        /// </summary>
        /// <param name="time">データ。</param>
        private void UpdateElapsedTime(ISchedulableMetadata time)
        {
            var elapsedTime = new ElapsedTimeLog(time.SourceId, time.StartTime, new TimeSpan(time.ElapsedTimes.Sum(t => t.Ticks)));
            sourceRunners[time.SourceId].UpdateElapsedTime(elapsedTime);
        }

        /// <summary>
        /// 実行ループをする。
        /// </summary>
        /// <param name="token">止める</param>
        /// <returns></returns>
        private async UniTask ScheduleLoop(CancellationToken token)
        {
            Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(_ => LogFPS()).AddTo(token);
            while (!token.IsCancellationRequested)
            {
                foreach (var runner in sourceRunners)
                {
                    runner.Value.Run();
                }

                await UniTask.WaitForEndOfFrame();
            }
        }

        /// <summary>
        /// FPSのログをつくる。
        /// </summary>
        private void LogFPS()
        {
            var data = new Dictionary<string, string>();
            foreach (var runner in sourceRunners)
            {
                runner.Value.LogFPS(data);
            }
            fpsSource.OnNext(new DebugFPS(data));
        }

        public async UniTask Dispose()
        {
            var isRunnings = sourceRunners.Select(runner => runner.Value.IsRunning);
            var stoped = isRunnings.Merge().Skip(1).Select(_ => source.Token).First(token => isRunnings.All(value => !value.Value) && token.IsCancellationRequested).ToUniTask();
            source.Cancel();
            await stoped;
            await UniTask.SwitchToMainThread();
            foreach (var node in allNodes)
            {
                node.Schedulable.Dispose();
            }
            source.Dispose();
            disposable.Dispose();
        }
    }
}
