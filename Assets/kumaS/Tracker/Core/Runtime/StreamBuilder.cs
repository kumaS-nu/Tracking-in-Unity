using System;
using System.Collections.Generic;
using System.Linq;

using UniRx;

using UnityEngine;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// ストリームを構築する。
    /// </summary>
    internal class StreamBuilder
    {
        private readonly List<IScheduleSource> sources;
        private readonly List<IScheduleStream> streams;
        private readonly List<IScheduleDestination> destinations;
        private readonly List<int> streamUnitStarts;
        private readonly List<int> streamInputs;
        private readonly List<int> streamOutputs;

        internal StreamBuilder(List<MonoBehaviour> source, List<MonoBehaviour> stream, List<MonoBehaviour> destination, List<int> streamUnitStart, List<int> streamInput, List<int> streamOutput)
        {
            sources = source.Select(value => (IScheduleSource)value).ToList();
            streams = stream.Select(value => (IScheduleStream)value).ToList();
            destinations = destination.Select(value => (IScheduleDestination)value).ToList();
            streamUnitStarts = streamUnitStart;
            streamInputs = streamInput;
            streamOutputs = streamOutput;
        }

        /// <summary>
        /// ストリームを構築。
        /// </summary>
        /// <returns>全てのノード。</returns>
        internal List<StreamNode> Build(GameObject scheduler)
        {
            List<StreamNode> starts = BuildNetwork();
            var allNodes = new List<StreamNode>();

            foreach (StreamNode start in starts)
            {
                var schedulable = start.Schedulable;
                start.SourceStream = ((IScheduleSource)schedulable).GetStream();
                start.TryGetSourceStream(out Subject<object> stream);
                start.MainStream = stream.Share();
                allNodes.Add(start);
                start.StartId.Add(allNodes.IndexOf(start));
                foreach (StreamNode next in start.Next)
                {
                    BuildStream(next, allNodes, scheduler);
                }
            }

            return allNodes;
        }

        /// <summary>
        /// ストリームを解釈し、ネットワークを構成する。
        /// </summary>
        /// <returns>始点たち。</returns>
        internal List<StreamNode> BuildNetwork()
        {
            var start = new List<StreamNode>();
            var streamNodes = new List<List<StreamNode>>();
            var destinationNodes = new List<StreamNode>();

            foreach (IScheduleStream stream in streams)
            {
                if (streamUnitStarts.Contains(streams.IndexOf(stream)))
                {
                    streamNodes.Add(new List<StreamNode>());
                }

                var node = new StreamNode(stream);
                if (streamNodes.Last().Any())
                {
                    StreamNode previous = streamNodes.Last().Last();
                    previous.Next.Add(node);
                    node.Previous.Add(previous);
                }
                streamNodes.Last().Add(node);
            }

            foreach (object destination in destinations)
            {
                destinationNodes.Add(new StreamNode(destination));
            }

            for (var i = 0; i < streamUnitStarts.Count; i++)
            {
                if (streamInputs[i] > 0)
                {
                    StreamNode previousNode = streamNodes[streamInputs[i] - 1].Last();
                    StreamNode nextNode = streamNodes[i].First();
                    if (!nextNode.Previous.Any(node => node == previousNode))
                    {
                        previousNode.Next.Add(nextNode);
                        nextNode.Previous.Add(previousNode);
                    }
                }
                else if (streamInputs[i] < 0)
                {
                    var node = new StreamNode(sources[-1 - streamInputs[i]]);
                    streamNodes[i].First().Previous.Add(node);
                    node.Next.Add(streamNodes[i].First());
                    start.Add(node);
                }

                if (streamOutputs[i] > 0)
                {
                    StreamNode previousNode = streamNodes[i].Last();
                    StreamNode nextNode = streamNodes[streamOutputs[i] - 1].First();
                    if (!previousNode.Next.Any(node => node != nextNode))
                    {
                        previousNode.Next.Add(nextNode);
                        nextNode.Previous.Add(previousNode);
                    }
                }
                else if (streamOutputs[i] < 0)
                {
                    StreamNode node = destinationNodes[-1 - streamOutputs[i]];
                    streamNodes[i].Last().Next.Add(node);
                    node.Previous.Add(streamNodes[i].Last());
                }
            }

            return start;
        }

        /// <summary>
        /// ストリームを構築する。
        /// </summary>
        /// <param name="node">対象のノード。</param>
        internal void BuildStream(StreamNode node, List<StreamNode> allNodes, GameObject scheduler)
        {
            allNodes.Add(node);
            if (!node.Previous.All(previous => previous.TryGetMainStream(out IObservable<object> _)))
            {
                return;
            }

            IObservable<object> stream = default;
            var streams = new List<IObservable<object>>();
            if (node.Previous.Count > 1)
            {
                foreach (StreamNode previous in node.Previous)
                {
                    previous.TryGetMainStream(out IObservable<object> s);
                    streams.Add(s);
                    node.StartId.AddRange(previous.StartId);
                }
                stream = streams.Merge();
            }
            else
            {
                node.Previous[0].TryGetMainStream(out stream);
                node.StartId.AddRange(node.Previous[0].StartId);
            }

            if (node.Next.Count == 0)
            {
                stream = stream.Share();
                stream.ObserveOnMainThread().Subscribe(((IScheduleDestination)node.Schedulable).Process).AddTo(scheduler);
                node.ErrorStream = stream;
                node.TimeStream = stream.Where(data => ((ISchedulableMetadata)data).IsSuccess).Select(ElapsedTimeLog);
                return;
            }

            var schedulable = (IScheduleStream)node.Schedulable;
            stream = schedulable.Process(stream).Share();
            node.MainStream = stream;

            foreach (StreamNode next in node.Next)
            {
                BuildStream(next, allNodes, scheduler);
            }
        }

        /// <summary>
        /// 経過時間に変換する。
        /// </summary>
        /// <param name="d">データ。</param>
        /// <returns>経過時間。</returns>
        private ElapsedTimeLog ElapsedTimeLog(object d)
        {
            var data = (ISchedulableMetadata)d;
            return new ElapsedTimeLog(data.SourceId, data.StartTime, new TimeSpan(data.ElapsedTimes.Sum(time => time.Ticks)));
        }
    }
}
