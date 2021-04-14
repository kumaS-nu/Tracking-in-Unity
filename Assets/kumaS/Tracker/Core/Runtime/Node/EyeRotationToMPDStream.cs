using System;
using System.Collections;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using UniRx;

using UnityEngine;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// 目線をMPDに変換するストリーム。
    /// </summary>
    public class EyeRotationToMPDStream : ScheduleStreamBase<EyeRotation, ModelPredictedData>
    {
        [SerializeField]
        internal bool isDebugRotation = true;

        [SerializeField]
        internal Transform forward;

        public override string ProcessName { get; set; } = "Eye rotation to MPD";
        public override Type[] UseType { get; } = new Type[0];
        public override string[] DebugKey { get; } = new string[]
        {
            SchedulableData<object>.Elapsed_Time,
            nameof(L_Eye_X), nameof(L_Eye_Y), nameof(L_Eye_Z),
            nameof(R_Eye_X), nameof(R_Eye_Y), nameof(R_Eye_Z)
        };
        public override IReadOnlyReactiveProperty<bool> IsAvailable { get; } = new ReactiveProperty<bool>(true);

        private readonly string L_Eye = nameof(L_Eye);
        private readonly string R_Eye = nameof(R_Eye);

        private readonly string L_Eye_X = nameof(L_Eye_X);
        private readonly string L_Eye_Y = nameof(L_Eye_Y);
        private readonly string L_Eye_Z = nameof(L_Eye_Z);
        private readonly string R_Eye_X = nameof(R_Eye_X);
        private readonly string R_Eye_Y = nameof(R_Eye_Y);
        private readonly string R_Eye_Z = nameof(R_Eye_Z);

        public override void InitInternal(int thread){ }

        protected override IDebugMessage DebugLogInternal(SchedulableData<ModelPredictedData> data)
        {
            var message = new Dictionary<string, string>();
            data.ToDebugElapsedTime(message);
            if(data.IsSuccess && isDebugRotation)
            {
                data.Data.ToDebugRotation(message, L_Eye, L_Eye_X, L_Eye_Y, L_Eye_Z);
                data.Data.ToDebugRotation(message, R_Eye, R_Eye_X, R_Eye_Y, R_Eye_Z);
            }
            return new DebugMessage(data, message);
        }

        protected override SchedulableData<ModelPredictedData> ProcessInternal(SchedulableData<EyeRotation> input)
        {
            if (!input.IsSuccess)
            {
                return new SchedulableData<ModelPredictedData>(input, default);
            }
            
            return ProcessInternalAsync(input).ToObservable().Wait();
        }

        private async UniTask<SchedulableData<ModelPredictedData>> ProcessInternalAsync(SchedulableData<EyeRotation> input)
        {
            await UniTask.SwitchToMainThread();
            var rot = new Dictionary<string, Quaternion>();
            rot[L_Eye] = input.Data.Left * forward.rotation;
            rot[R_Eye] = input.Data.Right * forward.rotation;
            var ret = new ModelPredictedData(new Dictionary<string, Vector3>(), rot, new Dictionary<string, float>());
            return new SchedulableData<ModelPredictedData>(input, ret);
        }
    }
}
