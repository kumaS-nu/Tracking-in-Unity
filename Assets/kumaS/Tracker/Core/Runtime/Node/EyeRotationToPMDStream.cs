﻿using Cysharp.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Threading;

using UniRx;

using UnityEngine;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// 目線をPMDに変換するストリーム。
    /// </summary>
    public sealed class EyeRotationToPMDStream : ScheduleStreamBase<EyeRotation, PredictedModelData>
    {
        [SerializeField]
        internal bool isDebugRotation = true;

        [SerializeField]
        internal Transform forward;

        public override string ProcessName { get; set; } = "Eye rotation to PMD";
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

        protected override void InitInternal(int thread, CancellationToken token) { }

        protected override IDebugMessage DebugLogInternal(SchedulableData<PredictedModelData> data)
        {
            var message = new Dictionary<string, string>();
            data.ToDebugElapsedTime(message);
            if (data.IsSuccess && isDebugRotation && !data.IsSignal)
            {
                data.Data.ToDebugRotation(message, L_Eye, L_Eye_X, L_Eye_Y, L_Eye_Z);
                data.Data.ToDebugRotation(message, R_Eye, R_Eye_X, R_Eye_Y, R_Eye_Z);
            }
            return new DebugMessage(data, message);
        }

        protected override SchedulableData<PredictedModelData> ProcessInternal(SchedulableData<EyeRotation> input)
        {
            if (!input.IsSuccess || input.IsSignal)
            {
                return new SchedulableData<PredictedModelData>(input, default);
            }

            return ProcessInternalAsync(input).ToObservable().Wait();
        }

        private async UniTask<SchedulableData<PredictedModelData>> ProcessInternalAsync(SchedulableData<EyeRotation> input)
        {
            await UniTask.SwitchToMainThread();
            var rot = new Dictionary<string, Quaternion>
            {
                [L_Eye] = forward.rotation * input.Data.Left,
                [R_Eye] = forward.rotation * input.Data.Right
            };
            var ret = new PredictedModelData(rotation: rot);
            return new SchedulableData<PredictedModelData>(input, ret);
        }

        public override void Dispose(){ }
    }
}
