using kumaS.Tracker.Core;

using System;
using System.Collections.Generic;
using System.Threading;

using UniRx;

using UnityEngine;

namespace kumaS.Tracker.VRM
{
    /// <summary>
    /// 予測されたモデルのデータからVRMで使えるデータに変換するストリーム。
    /// </summary>
    public sealed class PMDToVRMStream : ScheduleStreamBase<PredictedModelData, PredictedVRMData>
    {
        [SerializeField]
        internal bool fold0 = false;

        [SerializeField]
        internal List<string> PMDPosition = new List<string>(PredictedModelData.DefaultPositionList);

        [SerializeField]
        internal List<string> VRMPosition = new List<string>(PredictedModelData.DefaultPositionList);

        [SerializeField]
        internal bool fold1 = false;

        [SerializeField]
        internal List<string> PMDRotation = new List<string>(PredictedModelData.DefaultRotationList);

        [SerializeField]
        internal List<string> VRMRotation = new List<string>(PredictedModelData.DefaultRotationList);

        [SerializeField]
        internal List<Vector3> RotationOffset = new List<Vector3>(new Vector3[] {
        Vector3.zero, Vector3.zero,
        new Vector3(0, -90, -90), new Vector3(0, 90, 90),
        new Vector3(0, -90, -90), new Vector3(0, 90, 90),
        new Vector3(0, -90, -90), new Vector3(0, 90, 90),
        new Vector3(90, 0, 0), new Vector3(90, 0, 0),
        new Vector3(90, 0, 180), new Vector3(90, 0, 180),
        Vector3.zero, Vector3.zero,
        new Vector3(-90, 0, 180),
        Vector3.zero, Vector3.zero
        });

        [SerializeField]
        internal bool fold2 = false;

        [SerializeField]
        internal List<string> PMDParameter = new List<string>(PredictedModelData.DefaultParameterList);

        [SerializeField]
        internal List<string> VRMParameter = new List<string>(PredictedModelData.DefaultParameterList);

        [SerializeField]
        internal bool fold3 = false;

        [SerializeField]
        internal List<string> PMDOption = new List<string>();

        [SerializeField]
        internal List<string> VRMOption = new List<string>();

        [SerializeField]
        internal bool isDebugPosition = true;

        [SerializeField]
        internal bool isDebugRotation = true;

        [SerializeField]
        internal bool isDebugParameter = true;

        [SerializeField]
        internal bool isDebugOption = true;

        public override string ProcessName { get; set; } = "Convert PMD to VRM";
        public override Type[] UseType { get; } = new Type[0];
        public override string[] DebugKey { get => debugKey; }

        private string[] debugKey;
        public override IReadOnlyReactiveProperty<bool> IsAvailable { get => isAvailable; }

        private readonly ReactiveProperty<bool> isAvailable = new ReactiveProperty<bool>(false);

        private string[] positionX;
        private string[] positionY;
        private string[] positionZ;
        private string[] rotationX;
        private string[] rotationY;
        private string[] rotationZ;

        private Quaternion[] rotationOffsetInverse;


        protected override IDebugMessage DebugLogInternal(SchedulableData<PredictedVRMData> data)
        {
            var message = new Dictionary<string, string>();
            data.ToDebugElapsedTime(message);
            if (data.IsSuccess && !data.IsSignal)
            {
                if (isDebugPosition)
                {
                    for (var i = 0; i < VRMPosition.Count; i++)
                    {
                        data.Data.ToDebugPosition(message, VRMPosition[i], positionX[i], positionY[i], positionZ[i]);
                    }
                }

                if (isDebugRotation)
                {
                    for (var i = 0; i < VRMRotation.Count; i++)
                    {
                        data.Data.ToDebugRotation(message, VRMRotation[i], rotationX[i], rotationY[i], rotationZ[i]);
                    }
                }

                if (isDebugParameter)
                {
                    foreach (var p in VRMParameter)
                    {
                        data.Data.ToDebugParameter(message, p, p);
                    }
                }

                if (isDebugOption)
                {
                    foreach (var o in VRMOption)
                    {
                        data.Data.ToDebugOption(message, o, o);
                    }
                }
            }
            return new DebugMessage(data, message);
        }

        protected override void InitInternal(int thread, CancellationToken token)
        {
            for (var i = PMDPosition.Count - 1; i >= 0; i--)
            {
                if (PMDPosition[i] == "" || VRMPosition[i] == "")
                {
                    PMDPosition.RemoveAt(i);
                    VRMPosition.RemoveAt(i);
                }
            }

            for (var i = PMDRotation.Count - 1; i >= 0; i--)
            {
                if (PMDRotation[i] == "" || VRMRotation[i] == "")
                {
                    PMDRotation.RemoveAt(i);
                    VRMRotation.RemoveAt(i);
                    RotationOffset.RemoveAt(i);
                }
            }

            for (var i = PMDParameter.Count - 1; i >= 0; i--)
            {
                if (PMDParameter[i] == "" || VRMParameter[i] == "")
                {
                    PMDParameter.RemoveAt(i);
                    VRMParameter.RemoveAt(i);
                }
            }

            for (var i = PMDOption.Count - 1; i >= 0; i--)
            {
                if (PMDOption[i] == "" || VRMOption[i] == "")
                {
                    PMDOption.RemoveAt(i);
                    VRMOption.RemoveAt(i);
                }
            }

            var ri = new List<Quaternion>();
            for (var i = 0; i < RotationOffset.Count; i++)
            {
                ri.Add(Quaternion.Inverse(Quaternion.Euler(RotationOffset[i])));
            }
            rotationOffsetInverse = ri.ToArray();

            var dk = new List<string>();
            var px = new List<string>();
            var py = new List<string>();
            var pz = new List<string>();

            dk.Add(SchedulableData<object>.Elapsed_Time);
            foreach (var p in VRMPosition)
            {
                var x = p + "_X";
                dk.Add(x);
                px.Add(x);
                var y = p + "_Y";
                dk.Add(y);
                py.Add(y);
                var z = p + "_Z";
                dk.Add(z);
                pz.Add(z);
            }
            positionX = px.ToArray();
            positionY = py.ToArray();
            positionZ = pz.ToArray();

            var rx = new List<string>();
            var ry = new List<string>();
            var rz = new List<string>();
            foreach (var r in VRMRotation)
            {
                var x = r + "_X";
                dk.Add(x);
                rx.Add(x);
                var y = r + "_Y";
                dk.Add(y);
                ry.Add(y);
                var z = r + "_Z";
                dk.Add(z);
                rz.Add(z);
            }
            rotationX = rx.ToArray();
            rotationY = ry.ToArray();
            rotationZ = rz.ToArray();

            dk.AddRange(VRMParameter);
            dk.AddRange(VRMOption);

            debugKey = dk.ToArray();
            isAvailable.Value = true;
        }

        protected override SchedulableData<PredictedVRMData> ProcessInternal(SchedulableData<PredictedModelData> input)
        {
            if (!input.IsSuccess || input.IsSignal)
            {
                return new SchedulableData<PredictedVRMData>(input, default);
            }

            Dictionary<string, Vector3> pos = default;
            if (input.Data.Position != default)
            {
                pos = new Dictionary<string, Vector3>();
                foreach (KeyValuePair<string, Vector3> p in input.Data.Position)
                {
                    var index = PMDPosition.IndexOf(p.Key);
                    if (index >= 0)
                    {
                        pos[VRMPosition[index]] = p.Value;
                    }
                }
            }

            Dictionary<string, Quaternion> rot = default;
            if (input.Data.Rotation != default)
            {
                rot = new Dictionary<string, Quaternion>();
                foreach (KeyValuePair<string, Quaternion> r in input.Data.Rotation)
                {
                    var index = PMDRotation.IndexOf(r.Key);
                    if (index >= 0)
                    {
                        rot[VRMRotation[index]] = r.Value * rotationOffsetInverse[index];
                    }
                }
            }

            Dictionary<string, float> parameter = default;
            if (input.Data.Position != default)
            {
                parameter = new Dictionary<string, float>();
                foreach (KeyValuePair<string, float> p in input.Data.Parameter)
                {
                    var index = PMDParameter.IndexOf(p.Key);
                    if (index >= 0)
                    {
                        parameter[VRMParameter[index]] = p.Value;
                    }
                }
            }

            Dictionary<string, object> opt = default;
            if (input.Data.Option != default)
            {
                opt = new Dictionary<string, object>();
                foreach (KeyValuePair<string, object> o in input.Data.Option)
                {
                    var index = PMDOption.IndexOf(o.Key);
                    if (index >= 0)
                    {
                        opt[VRMOption[index]] = o.Value;
                    }
                }
            }

            var ret = new PredictedVRMData(pos, rot, parameter, opt);
            return new SchedulableData<PredictedVRMData>(input, ret);
        }

        public override void Dispose(){ }
    }
}
