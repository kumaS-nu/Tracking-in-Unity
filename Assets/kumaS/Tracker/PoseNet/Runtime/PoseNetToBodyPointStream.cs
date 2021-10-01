using Cysharp.Threading.Tasks;

using kumaS.Tracker.Core;

using System;
using System.Collections.Generic;
using System.Threading;

using UniRx;

using UnityEngine;

namespace kumaS.Tracker.PoseNet
{
    /// <summary>
    /// PoseNetを3dの座標に変換。
    /// </summary>
    public sealed class PoseNetToBodyPointStream : ScheduleStreamBase<PoseNetLandmarks, BodyPoints>
    {
        [SerializeField]
        internal bool fold0 = false;

        [SerializeField]
        internal float[] realDistance = new float[7];

        [SerializeField]
        internal bool fold1 = false;

        [SerializeField]
        internal float[] avatarDistance = new float[7];

        [SerializeField]
        internal float zOffset;

        [SerializeField]
        internal int type = 0;

        [SerializeField]
        internal float fov = 78;

        [SerializeField]
        internal int width = 1920;

        [SerializeField]
        internal float focalLength = 2371;

        [SerializeField]
        internal bool sourceIsMirror = false;

        [SerializeField]
        internal bool wantMirror = true;

        [SerializeField]
        internal bool isDebugPosition = true;

        [SerializeField]
        internal bool isDebugRotation = true;

        public override string ProcessName { get; set; } = "PoseNet to BodyPoint";
        public override Type[] UseType { get; } = new Type[0];
        public override string[] DebugKey { get => debugKey; }

        private string[] debugKey;
        public override IReadOnlyReactiveProperty<bool> IsAvailable { get => isAvailable; }

        private readonly ReactiveProperty<bool> isAvailable = new ReactiveProperty<bool>(false);

        internal static readonly string[] distanceName = new string[] { "Head to shoulder center", "Shoulder width", "Shoulder to elbow", "Elbow to wrist", "Shoulder to hip", "Hip to knee", "Knee to ankle" };
        private string[] positionX;
        private string[] positionY;
        private string[] positionZ;
        private string[] rotationX;
        private string[] rotationY;
        private string[] rotationZ;

        private TransformTo3d[] workers;

        /// <inheritdoc/>
        protected override IDebugMessage DebugLogInternal(SchedulableData<BodyPoints> data)
        {
            var message = new Dictionary<string, string>();
            data.ToDebugElapsedTime(message);
            if (data.IsSuccess && !data.IsSignal)
            {
                if (isDebugPosition)
                {
                    for (var i = 0; i < 14; i++)
                    {
                        data.Data.ToDebugPosition(message, i, positionX[i], positionY[i], positionZ[i]);
                    }
                }
                if (isDebugRotation)
                {
                    for (var i = 0; i < 15; i++)
                    {
                        data.Data.ToDebugRotation(message, i, rotationX[i], rotationY[i], rotationZ[i]);
                    }
                }
            }
            return new DebugMessage(data, message);
        }

        /// <inheritdoc/>
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

            workers = new TransformTo3d[thread];
            for (var i = 0; i < thread; i++)
            {
                workers[i] = new TransformTo3d(zOffset, focalLength, wantMirror != sourceIsMirror);
                workers[i].Real.CopyFrom(realDistance);
                workers[i].Avatar.CopyFrom(avatarDistance);
            }

            isAvailable.Value = true;
        }

        /// <inheritdoc/>
        protected override SchedulableData<BodyPoints> ProcessInternal(SchedulableData<PoseNetLandmarks> input)
        {
            if (!input.IsSuccess || input.IsSignal)
            {
                return new SchedulableData<BodyPoints>(input, default);
            }

            if (TryGetThread(out var thread))
            {
                try
                {
                    BodyPoints ret = ProcessInternalAsync(input.Data, thread).ToObservable().Wait();
                    return new SchedulableData<BodyPoints>(input, ret);
                }
                finally
                {
                    FreeThread(thread);
                }
            }

            return new SchedulableData<BodyPoints>(input, default, false, true, errorMessage: "スレッドを確保できませんでした");
        }

        /// <inheritdoc/>
        private async UniTask<BodyPoints> ProcessInternalAsync(PoseNetLandmarks input, int thread)
        {
            await UniTask.SwitchToMainThread();
            workers[thread].pixcelCenter = input.ImageSize / 2;
            workers[thread].Input.CopyFrom(input.Landmarks);
            workers[thread].Execute();
            return new BodyPoints(workers[thread].Position.ToArray(), workers[thread].Rotation.ToArray());
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            foreach (TransformTo3d worker in workers)
            {
                worker.Dispose();
            }
        }
    }
}
