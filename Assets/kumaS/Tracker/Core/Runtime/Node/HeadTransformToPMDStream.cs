using Cysharp.Threading.Tasks;

using System;
using System.Collections.Generic;

using UniRx;

using UnityEngine;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// 頭の位置・回転をPMDにするストリーム。
    /// </summary>
    public sealed class HeadTransformToPMDStream : ScheduleStreamBase<HeadTransform, PredictedModelData>
    {
        [SerializeField]
        internal bool isDebugHead = true;

        [SerializeField]
        internal Transform center;

        public override string ProcessName { get; set; } = "Head transform to PMD";
        public override Type[] UseType { get; } = new Type[0];
        public override string[] DebugKey { get; } = new string[] {
            SchedulableData<object>.Elapsed_Time,
            nameof(Head_Pos_X), nameof(Head_Pos_Y), nameof(Head_Pos_Z),
            nameof(Head_Rot_X), nameof(Head_Rot_Y), nameof(Head_Rot_Z)
        };
        public override IReadOnlyReactiveProperty<bool> IsAvailable { get; } = new ReactiveProperty<bool>(true);

        private readonly string Head = nameof(Head);
        private readonly string Head_Pos_X = nameof(Head_Pos_X);
        private readonly string Head_Pos_Y = nameof(Head_Pos_Y);
        private readonly string Head_Pos_Z = nameof(Head_Pos_Z);
        private readonly string Head_Rot_X = nameof(Head_Rot_X);
        private readonly string Head_Rot_Y = nameof(Head_Rot_Y);
        private readonly string Head_Rot_Z = nameof(Head_Rot_Z);

        protected override void InitInternal(int thread) { }

        protected override IDebugMessage DebugLogInternal(SchedulableData<PredictedModelData> data)
        {
            var message = new Dictionary<string, string>();
            data.ToDebugElapsedTime(message);
            if (data.IsSuccess && isDebugHead)
            {
                data.Data.ToDebugPosition(message, Head, Head_Pos_X, Head_Pos_Y, Head_Pos_Z);
                data.Data.ToDebugRotation(message, Head, Head_Rot_X, Head_Rot_Y, Head_Rot_Z);
            }
            return new DebugMessage(data, message);
        }

        protected override SchedulableData<PredictedModelData> ProcessInternal(SchedulableData<HeadTransform> input)
        {
            if (!input.IsSuccess)
            {
                return new SchedulableData<PredictedModelData>(input, default);
            }

            return ProcessInternalAsync(input).ToObservable().Wait();
        }

        private async UniTask<SchedulableData<PredictedModelData>> ProcessInternalAsync(SchedulableData<HeadTransform> input)
        {
            await UniTask.SwitchToMainThread();
            var pos = new Dictionary<string, Vector3>
            {
                [Head] = center.TransformPoint(input.Data.Position)
            };
            var rot = new Dictionary<string, Quaternion>
            {
                [Head] = input.Data.Rotation * center.rotation
            };
            var ret = new PredictedModelData(pos, rot);
            return new SchedulableData<PredictedModelData>(input, ret);
        }
    }
}
