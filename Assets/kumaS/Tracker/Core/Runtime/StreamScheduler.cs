using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;
using System.IO;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;

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

        private Subject<DebugFPS> fpsSource = new Subject<DebugFPS>();

        private string[] fpsDebugKey;

        private List<IReadOnlyReactiveProperty<bool>> nodeIsDebug = new List<IReadOnlyReactiveProperty<bool>>();

        private List<IObservable<IDebugMessage>> debugStreams = new List<IObservable<IDebugMessage>>();

        private IDisposable debugStream = default;

        private Dictionary<int, StreamWriter> streamWriters = new Dictionary<int, StreamWriter>();

        private List<StreamSourceInfomation> sourceInfomations = new List<StreamSourceInfomation>();

        private Dictionary<int, int> sourceInfomationIndex = new Dictionary<int, int>();

        private void Reset()
        {
            if (FindObjectsOfType<StreamScheduler>().Length > 1)
            {
                Debug.LogError("Schedulerはシーン上に一つしか置けません。");
                DestroyImmediate(this);
            }
        }

        private void Awake()
        {
            var builder = new StreamBuilder(sources, streams, destinations, streamUnitStarts, streamInputs, streamOutputs);
            allNodes = builder.Build(gameObject);

            InitializeEachNode();

            var timeStreams = new List<IObservable<ElapsedTimeLog>>();
            var errorStreams = new List<IObservable<object>>();
            var startIds = new List<int>();
            foreach(var node in allNodes)
            {
                if(node.TryGetSourceStream(out var sStream))
                {
                    sourceInfomations.Add(new StreamSourceInfomation(allNodes.IndexOf(node), sStream, (IScheduleSource)node.Schedulable, fps[sourceInfomations.Count], DateTime.MinValue, thread));
                }

                if(node.TryGetMainStream(out var mStream))
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

                if(node.TryGetTimeStream(out var tStream))
                {
                    timeStreams.Add(tStream);
                    startIds.AddRange(node.StartId);
                }

                if(node.TryGetErrorStream(out var eStream))
                {
                    errorStreams.Add(eStream);
                }

                ((ISchedule)node.Schedulable).Id = allNodes.IndexOf(node);
            }

            foreach(var index in startIds)
            {
                ++sourceInfomations.First(info => info.id == index).destinationCount;
            }
            nodeIsDebug.Add(isDebugFPS);
            debugStreams.Add(fpsSource);
            
            nodeIsDebug.Merge().Merge(isDebug, isWriteFile).Subscribe(_ => SetDebugStream()).AddTo(this);

            errorStreams.Merge().Cast<object, ISchedulableMetadata>().Where(data => isDebug.Value && !data.IsSuccess)
                .ObserveOnMainThread().Subscribe(ShowError).AddTo(this);

            var fpsKey = new List<string>();
            foreach(var sourceInfomation in sourceInfomations)
            {
                sourceInfomationIndex.Add(sourceInfomation.id, sourceInfomations.IndexOf(sourceInfomation));
                fpsKey.Add(sourceInfomation.id + sourceInfomation.scheduleSource.ProcessName);
            }
            fpsDebugKey = fpsKey.ToArray();
            timeStreams.Merge().Subscribe(SetInterval).AddTo(this);

            ResourceManager.SetResource(allNodes);

            var dummyProperty = new ReactiveProperty<bool>(false);
            Observable.Merge(allNodes.Select(node => ((ISchedule)node.Schedulable).IsAvailable)).Merge(dummyProperty)
                .First(_ => allNodes.All(node => ((ISchedule)node.Schedulable).IsAvailable.Value)).Select((_) => this.GetCancellationTokenOnDestroy())
                .Subscribe(token => _ = UniTask.RunOnThreadPool(() => ScheduleLoop(token))).AddTo(this);
            dummyProperty.Value = true;
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
            foreach (var sw in streamWriters)
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

                    foreach (var node in allNodes)
                    {
                        if (node.TryGetMainStream(out _)) {
                            var debaggable = (IDebuggable)node.Schedulable;
                            if (debaggable.IsDebug.Value)
                            {
                                var sw = new StreamWriter(Path.Combine(Application.dataPath, "Debug data", time, allNodes.IndexOf(node) + debaggable.ProcessName + ".csv"), false, Encoding.UTF8);
                                streamWriters.Add(allNodes.IndexOf(node), sw);
                                StringBuilder sb = new StringBuilder();
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
                for(var i = 0; i < debugStreams.Count; i++)
                {
                    if (nodeIsDebug[i].Value)
                    {
                        currentDebugStreams.Add(debugStreams[i]);
                    }
                }
                debugStream = currentDebugStreams.Merge().ObserveOnMainThread().Subscribe(OutputDebug).AddTo(this);
            }
        }
        
        /// <summary>
        /// デバッグ出力する。
        /// </summary>
        /// <param name="message">内容。</param>
        private void OutputDebug(IDebugMessage message)
        {
            StringBuilder sb = new StringBuilder();
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
                if (message.Data.TryGetValue(key, out string value))
                {
                    sb.Append(value);
                }
                sb.Append(",");
            }
            sb.Remove(sb.Length - 1, 1);

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
                StringBuilder sb2 = new StringBuilder();
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
                
                foreach(var key in debugKey)
                {
                    sb2.Append(key);
                    sb2.Append(",");
                }
                sb2.Remove(sb2.Length - 1, 1);
                sb2.Append("\n");
                sb2.Append(sb);
                Debug.Log(sb2.ToString());
            }
        }

        private void ShowError(ISchedulableMetadata data)
        {
            Debug.LogError(data.ErrorMessage);
        }

        /// <summary>
        /// ストリームの各ノードを初期化する。
        /// </summary>
        private void InitializeEachNode()
        {
            var failed = false;
            foreach(var node in allNodes)
            {
                try
                {
                    var stream = node.Schedulable as IScheduleStream;
                    if (stream != null)
                    {
                        stream.Init(thread);
                    }
                }
                catch(Exception e)
                {
                    failed = true;
                    Debug.LogError(((ISchedule)node.Schedulable).ProcessName + e.ToString());
                }
            }

            if (failed)
            {
                Application.Quit();
            }
        }

        /// <summary>
        /// ソースを流すインターバルを設定。
        /// </summary>
        /// <param name="time">計測された時間。</param>
        private void SetInterval(ElapsedTimeLog time)
        {
            var sourceInfomation = sourceInfomations[sourceInfomationIndex[time.SourceId]];
            sourceInfomation.elapsedTimes.TryAdd(time.StartTime, new ConcurrentBag<TimeSpan>());
            var elapsedTimes = sourceInfomation.elapsedTimes[time.StartTime];
            elapsedTimes.Add(time.ElapsedTime);
            if(elapsedTimes.Count == sourceInfomation.destinationCount)
            {
                var buffer = sourceInfomation.elapsedTimeBuffer;
                buffer.Enqueue(new TimeSpan(elapsedTimes.Sum(t => t.Ticks) / sourceInfomation.destinationCount));
                buffer.TryDequeue(out _);
                sourceInfomation.elapsedTimes.TryRemove(time.StartTime, out _);
                var interval = buffer.Sum(t => t.Ticks) / buffer.Count / thread;
                interval = interval < sourceInfomation.baseInterval ? sourceInfomation.baseInterval : interval;
                sourceInfomation.interval = new TimeSpan(interval);
            }
        }

        /// <summary>
        /// 実行ループをする。
        /// </summary>
        /// <param name="token">止める</param>
        /// <returns></returns>
        private async UniTask ScheduleLoop(CancellationToken token)
        {
            Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(_ => LogFPS()).AddTo(token);

            while (true)
            {
                token.ThrowIfCancellationRequested();
                var now = DateTime.Now;
                foreach(var sourceInfomation in sourceInfomations)
                {
                    var updates = sourceInfomation.updates;
                    if (now - updates.Last.Value >= sourceInfomation.interval)
                    {
                        lock (updates)
                        {
                            updates.AddLast(now);
                            if (updates.Count > 50)
                            {
                                updates.RemoveFirst();
                            }
                        }
                        UniTask.RunOnThreadPool(() => Distribute(now, sourceInfomation, token)).Forget();
                    }
                }

                await UniTask.WaitForEndOfFrame();
            }
        }

        /// <summary>
        /// ソースを流す。
        /// </summary>
        /// <param name="sourceInfomation">流すソース。</param>
        /// <param name="now">判断時間。</param>
        private async UniTask Distribute(DateTime startTime, StreamSourceInfomation sourceInfomation, CancellationToken token)
        {
            var source = await sourceInfomation.scheduleSource.GetSource(startTime, token);
            token.ThrowIfCancellationRequested();
            sourceInfomation.source.OnNext(source);
        }

        /// <summary>
        /// FPSのログをつくる。
        /// </summary>
        private void LogFPS()
        {
            var data = new Dictionary<string, string>();
            foreach(var infomation in sourceInfomations)
            {
                data.Add(infomation.id + infomation.scheduleSource.ProcessName, (1 / (infomation.updates.Last.Value - infomation.updates.First.Value).TotalSeconds * infomation.updates.Count).ToString());
            }
            fpsSource.OnNext(new DebugFPS(data));
        }
    }
}
