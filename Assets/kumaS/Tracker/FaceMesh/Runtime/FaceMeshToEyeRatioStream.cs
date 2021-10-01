using kumaS.Tracker.Core;

using System;
using System.Collections.Generic;
using System.Threading;

using UniRx;

using UnityEngine;

namespace kumaS.Tracker.FaceMesh
{
    /// <summary>
    /// FaceMeshの特徴点から目の比を抽出するストリーム。
    /// </summary>
    public sealed class FaceMeshToEyeRatioStream : ScheduleStreamBase<FaceMeshLandmarks, EyeRatio>
    {
        [SerializeField]
        internal bool isDebugRatio = true;

        [SerializeField]
        internal bool sourceIsMirror = false;

        [SerializeField]
        internal bool wantMirror = true;

        private bool mirror = true;

        public override string ProcessName { get; set; } = "FaceMesh to eye ratio";
        public override Type[] UseType { get; } = new Type[0];
        public override string[] DebugKey { get; } = new string[] { SchedulableData<object>.Elapsed_Time, nameof(L_Eye_Ratio), nameof(R_Eye_Ratio) };
        public override IReadOnlyReactiveProperty<bool> IsAvailable { get => isAvailable; }

        private readonly ReactiveProperty<bool> isAvailable = new ReactiveProperty<bool>(false);

        private readonly string L_Eye_Ratio = nameof(L_Eye_Ratio);
        private readonly string R_Eye_Ratio = nameof(R_Eye_Ratio);

        protected override IDebugMessage DebugLogInternal(SchedulableData<EyeRatio> data)
        {
            var message = new Dictionary<string, string>();
            data.ToDebugElapsedTime(message);
            if (data.IsSuccess && isDebugRatio && !data.IsSignal)
            {
                message[L_Eye_Ratio] = data.Data.Left.ToString();
                message[R_Eye_Ratio] = data.Data.Right.ToString();
            }
            return new DebugMessage(data, message);
        }

        protected override void InitInternal(int thread, CancellationToken token)
        {
            mirror = sourceIsMirror != wantMirror;
            isAvailable.Value = true;
        }

        protected override SchedulableData<EyeRatio> ProcessInternal(SchedulableData<FaceMeshLandmarks> input)
        {
            if (!input.IsSuccess || input.IsSignal)
            {
                return new SchedulableData<EyeRatio>(input, default);
            }
            var left = (input.Data.Landmarks[384] + input.Data.Landmarks[385] - input.Data.Landmarks[379] - input.Data.Landmarks[373]).magnitude / (input.Data.Landmarks[262] - input.Data.Landmarks[361]).magnitude * 0.5f;
            var right = (input.Data.Landmarks[157] + input.Data.Landmarks[158] - input.Data.Landmarks[152] - input.Data.Landmarks[144]).magnitude / (input.Data.Landmarks[32] - input.Data.Landmarks[132]).magnitude * 0.5f;
            if (mirror)
            {
                var tmp = left;
                left = right;
                right = tmp;
            }
            var ret = new EyeRatio((float)left, (float)right);
            return new SchedulableData<EyeRatio>(input, ret);
        }

        public override void Dispose(){ }
    }
}
