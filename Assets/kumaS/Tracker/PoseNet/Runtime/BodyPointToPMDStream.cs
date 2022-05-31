using kumaS.Tracker.Core;

using System;
using System.Collections.Generic;
using System.Threading;

using UniRx;

using UnityEngine;

namespace kumaS.Tracker.PoseNet
{

    /// <summary>
    /// 体の点をPMDに変換。
    /// </summary>
    public sealed class BodyPointToPMDStream : ScheduleStreamBase<BodyPoints, PredictedModelData>
    {
        [SerializeField]
        internal bool isDebugPosition = true;

        [SerializeField]
        internal bool isDebugRotation = true;

        public override string ProcessName { get; set; } = "BodyPoint to PMD";
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

        protected override IDebugMessage DebugLogInternal(SchedulableData<PredictedModelData> data)
        {
            var message = new Dictionary<string, string>();
            data.ToDebugElapsedTime(message);
            if (data.IsSuccess && !data.IsSignal)
            {
                if (isDebugPosition)
                {
                    for (var i = 0; i < 14; i++)
                    {
                        data.Data.ToDebugPosition(message, PredictedModelData.DefaultPositionList[i], positionX[i], positionY[i], positionZ[i]);
                    }
                }

                if (isDebugRotation)
                {
                    for (var i = 0; i < 15; i++)
                    {
                        data.Data.ToDebugRotation(message, PredictedModelData.DefaultRotationList[i], rotationX[i], rotationY[i], rotationZ[i]);
                    }
                }
            }
            return new DebugMessage(data, message);
        }

        protected override void InitInternal(int thread, CancellationToken token)
        {
            var dkey = new List<string>();
            var px = new List<string>();
            var py = new List<string>();
            var pz = new List<string>();
            dkey.Add(SchedulableData<object>.Elapsed_Time);
            for (var i = 0; i < 14; i++)
            {
                var part = PredictedModelData.DefaultPositionList[i];
                dkey.Add(part + "_Pos_X");
                px.Add(part + "_Pos_X");
                dkey.Add(part + "_Pos_Y");
                py.Add(part + "_Pos_Y");
                dkey.Add(part + "_Pos_Z");
                pz.Add(part + "Pos_Z");
            }
            positionX = px.ToArray();
            positionY = py.ToArray();
            positionZ = pz.ToArray();

            var rx = new List<string>();
            var ry = new List<string>();
            var rz = new List<string>();
            for (var i = 0; i < 15; i++)
            {
                var part = PredictedModelData.DefaultRotationList[i];
                dkey.Add(part + "_Rot_X");
                rx.Add(part + "_Rot_X");
                dkey.Add(part + "_Rot_Y");
                ry.Add(part + "_Rot_Y");
                dkey.Add(part + "_Rot_Z");
                rz.Add(part + "_Rot_Z");
            }

            rotationX = rx.ToArray();
            rotationY = ry.ToArray();
            rotationZ = rz.ToArray();

            debugKey = dkey.ToArray();

            isAvailable.Value = true;
        }

        protected override SchedulableData<PredictedModelData> ProcessInternal(SchedulableData<BodyPoints> input)
        {
            if (!input.IsSuccess || input.IsSignal)
            {
                return new SchedulableData<PredictedModelData>(input, default);
            }
            var pos = new Dictionary<string, Vector3>();
            for (var i = 0; i < 14; i++)
            {
                pos[PredictedModelData.DefaultPositionList[i]] = input.Data.Position[i];
            }

            var rot = new Dictionary<string, Quaternion>();
            for (var i = 0; i < 15; i++)
            {
                rot[PredictedModelData.DefaultRotationList[i]] = input.Data.Rotation[i];
            }

            var ret = new PredictedModelData(pos, rot);
            return new SchedulableData<PredictedModelData>(input, ret);
        }

        public override void Dispose() { }
    }
}
