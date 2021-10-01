using kumaS.Tracker.Core;

using System;
using System.Collections.Generic;
using System.Threading;

using UniRx;

using UnityEngine;

namespace kumaS.Tracker.Dlib
{
    /// <summary>
    /// Dlibの68の特徴点から目の比を抽出するストリーム。
    /// </summary>
    public sealed class Dlib68ToEyeRatioStream : ScheduleStreamBase<Dlib68Landmarks, EyeRatio>
    {
        [SerializeField]
        internal bool isDebugRatio = true;

        [SerializeField]
        internal bool sourceIsMirror = false;

        [SerializeField]
        internal bool wantMirror = true;

        private bool mirror = true;

        public override string ProcessName { get; set; } = "Dlib 68 landmarks to eye ratio";
        public override Type[] UseType { get; } = new Type[0];
        public override string[] DebugKey { get; } = new string[] { SchedulableData<object>.Elapsed_Time, nameof(L_Eye_Ratio), nameof(R_Eye_Ratio) };
        public override IReadOnlyReactiveProperty<bool> IsAvailable { get => isAvailable; }

        private readonly ReactiveProperty<bool> isAvailable = new ReactiveProperty<bool>(false);

        private readonly string L_Eye_Ratio = nameof(L_Eye_Ratio);
        private readonly string R_Eye_Ratio = nameof(R_Eye_Ratio);

        protected override void InitInternal(int thread, CancellationToken token)
        {
            mirror = sourceIsMirror != wantMirror;
            isAvailable.Value = true;
        }

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

        protected override SchedulableData<EyeRatio> ProcessInternal(SchedulableData<Dlib68Landmarks> input)
        {
            if (!input.IsSuccess || input.IsSignal)
            {
                return new SchedulableData<EyeRatio>(input, default);
            }
            var left = (input.Data.Landmarks[43] + input.Data.Landmarks[44] - input.Data.Landmarks[46] - input.Data.Landmarks[47]).Length / (input.Data.Landmarks[42] - input.Data.Landmarks[45]).Length * 0.5f;
            var right = (input.Data.Landmarks[37] + input.Data.Landmarks[38] - input.Data.Landmarks[40] - input.Data.Landmarks[41]).Length / (input.Data.Landmarks[36] - input.Data.Landmarks[39]).Length * 0.5f;
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
