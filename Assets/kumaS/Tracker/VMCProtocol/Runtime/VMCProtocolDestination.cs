using kumaS.Tracker.Core;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using UniRx;

using UnityEngine;

using uOSC;

namespace kumaS.Tracker.VMCProtocol
{
    /// <summary>
    /// VMCプロトコルで送る。
    /// </summary>
    public sealed class VMCProtocolDestination : ScheduleDestinationBase<PredictedModelData>
    {
        [SerializeField]
        internal string adress = "127.0.0.1";

        [SerializeField]
        internal int sendRate = 60;

        [SerializeField]
        internal bool fold0 = false;

        [SerializeField]
        internal List<string> hmdLabel = new List<string>();

        [SerializeField]
        internal List<string> hmdSerial = new List<string>();

        [SerializeField]
        internal bool fold1 = false;

        [SerializeField]
        internal List<string> trackerLabel = new List<string>();

        [SerializeField]
        internal List<string> trackerSerial = new List<string>();

        [SerializeField]
        internal bool fold2 = false;

        [SerializeField]
        internal List<string> blendShapeLabel = new List<string>();

        [SerializeField]
        internal List<string> blendShapeSerial = new List<string>();

        [SerializeField]
        internal bool fold3 = false;

        [SerializeField]
        internal List<string> eyeLabel = new List<string>();


        public override string ProcessName { get; set; } = "Send to VMC protocol";
        public override Type[] UseType { get; } = new Type[0];
        public override IReadOnlyReactiveProperty<bool> IsAvailable { get => isAvailable; }

        private readonly ReactiveProperty<bool> isAvailable = new ReactiveProperty<bool>(false);

        private readonly Udp udp = new uOSC.DotNet.Udp();

        private bool isArrived = false;

        private readonly List<Vector3> hmdPos = new List<Vector3>();
        private readonly List<Quaternion> hmdRot = new List<Quaternion>();
        private readonly List<Vector3> trackerPos = new List<Vector3>();
        private readonly List<Quaternion> trackerRot = new List<Quaternion>();
        private readonly List<float> blendShapeValue = new List<float>();
        private Vector3 eyePos = new Vector3();


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
                    idx = hmdLabel.IndexOf(pos.Key);
                    if (idx != -1)
                    {
                        hmdPos[idx] = pos.Value;
                    }
                }

                Quaternion eyeBuf = Quaternion.identity;
                var eyeCount = 0;
                foreach (KeyValuePair<string, Quaternion> rot in input.Data.Rotation)
                {
                    var idx = trackerLabel.IndexOf(rot.Key);
                    if (idx != -1)
                    {
                        trackerRot[idx] = rot.Value;
                        continue;
                    }
                    idx = hmdLabel.IndexOf(rot.Key);
                    if (idx != -1)
                    {
                        hmdRot[idx] = rot.Value;
                        continue;
                    }
                    idx = eyeLabel.IndexOf(rot.Key);
                    if (idx != -1)
                    {
                        if (eyeCount == eyeLabel.Count - 1)
                        {
                            Quaternion q = eyeCount == 0 ? rot.Value : Quaternion.Lerp(rot.Value, eyeBuf, 0.5f);
                            Vector3 dir = q * Vector3.forward;
                            eyePos = dir;
                            eyeCount++;
                        }
                        else
                        {
                            eyeBuf = rot.Value;
                            eyeCount++;
                        }
                    }
                }

                foreach (KeyValuePair<string, float> p in input.Data.Parameter)
                {
                    var idx = blendShapeLabel.IndexOf(p.Key);
                    if (idx != -1)
                    {
                        blendShapeValue[idx] = p.Value;
                    }
                }
            }
        }

        private void SendAllData(long tick)
        {
            var messages = new List<Message>();
            for (var i = 0; i < hmdSerial.Count; i++)
            {
                messages.Add(new Message("/VMC/Ext/Hmd/Pos", hmdSerial[i], hmdPos[i].x, hmdPos[i].y, hmdPos[i].z, hmdRot[i].x, hmdRot[i].y, hmdRot[i].z, hmdRot[i].w));
            }

            for (var i = 0; i < trackerSerial.Count; i++)
            {
                messages.Add(new Message("/VMC/Ext/Tra/Pos", trackerSerial[i], trackerPos[i].x, trackerPos[i].y, trackerPos[i].z, trackerRot[i].x, trackerRot[i].y, trackerRot[i].z, trackerRot[i].w));
            }

            for (var i = 0; i < blendShapeSerial.Count; i++)
            {
                messages.Add(new Message("/VMC/Ext/Blend/Val", blendShapeSerial[i], blendShapeValue[i]));
            }

            messages.Add(new Message("/VMC/Ext/Blend/Apply"));

            if (eyeLabel.Count > 0)
            {
                messages.Add(new Message("/VMC/Ext/Set/Eye", 1, eyePos.x, eyePos.y, eyePos.z));
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

        public override void Dispose()
        {
            udp.Stop();
        }

        public override void Init(int thread, CancellationToken token)
        {
            while (hmdPos.Count < hmdLabel.Count)
            {
                hmdPos.Add(Vector3.zero);
                hmdRot.Add(Quaternion.identity);
            }
            while (trackerPos.Count < trackerLabel.Count)
            {
                trackerPos.Add(Vector3.zero);
                trackerRot.Add(Quaternion.identity);
            }
            while (blendShapeValue.Count < blendShapeLabel.Count)
            {
                blendShapeValue.Add(0);
            }
            udp.StartClient(adress, 39540);
            isAvailable.Value = true;
        }
    }
}
