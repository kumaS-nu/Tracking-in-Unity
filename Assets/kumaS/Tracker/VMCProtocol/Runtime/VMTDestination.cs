using kumaS.Tracker.Core;

using System;
using System.Collections.Generic;
using System.IO;

using UniRx;

using UnityEngine;

using uOSC;

namespace kumaS.Tracker.VMCProtocol
{
    /// <summary>
    /// VMTに送る。
    /// </summary>
    public sealed class VMTDestination : ScheduleDestinationBase<PredictedModelData>
    {
        [SerializeField]
        internal string adress = "127.0.0.1";

        [SerializeField]
        internal int sendRate = 60;

        [SerializeField]
        internal bool fold0 = false;

        [SerializeField]
        internal List<string> trackerLabel = new List<string>();

        [SerializeField]
        internal List<int> trackerIndex = new List<int>();

        public override string ProcessName { get; set; } = "Send to VMT";
        public override Type[] UseType { get; } = new Type[0];
        public override IReadOnlyReactiveProperty<bool> IsAvailable { get => isAvailable; }

        private readonly ReactiveProperty<bool> isAvailable = new ReactiveProperty<bool>(false);

        private readonly Udp udp = new uOSC.DotNet.Udp();

        private bool isArrived = false;

        private readonly List<Vector3> trackerPos = new List<Vector3>();
        private readonly List<Quaternion> trackerRot = new List<Quaternion>();

        private void Awake()
        {
            while (trackerPos.Count < trackerLabel.Count)
            {
                trackerPos.Add(Vector3.zero);
                trackerRot.Add(Quaternion.identity);
            }
            udp.StartClient(adress, 39540);
            isAvailable.Value = true;
        }

        private void OnDisable()
        {
            udp.Stop();
        }

        protected override void ProcessInternal(SchedulableData<PredictedModelData> input)
        {
            if (input.IsSuccess)
            {
                if (!isArrived)
                {
                    Observable.Interval(TimeSpan.FromSeconds(1.0 / sendRate)).Subscribe(SendAllData).AddTo(this);
                    isArrived = true;
                }

                foreach (KeyValuePair<string, Vector3> pos in input.Data.Position)
                {
                    var idx = trackerLabel.IndexOf(pos.Key);
                    if (idx != -1)
                    {
                        trackerPos[idx] = pos.Value;
                        continue;
                    }
                }

                foreach (KeyValuePair<string, Quaternion> rot in input.Data.Rotation)
                {
                    var idx = trackerLabel.IndexOf(rot.Key);
                    if (idx != -1)
                    {
                        trackerRot[idx] = rot.Value;
                        continue;
                    }
                }
            }
        }

        private void SendAllData(long tick)
        {
            var messages = new List<Message>();
            for (var i = 0; i < trackerIndex.Count; i++)
            {
                messages.Add(new Message("/VMT/Room/Unity", trackerIndex[i], trackerPos[i].x, trackerPos[i].y, trackerPos[i].z, trackerRot[i].x, trackerRot[i].y, trackerRot[i].z, trackerRot[i].w));
            }

            foreach (Message msg in messages)
            {
                using (var stream = new MemoryStream(512))
                {
                    msg.Write(stream);
                    udp.Send(Util.GetBuffer(stream), (int)stream.Position);
                }
            }
        }
    }
}
