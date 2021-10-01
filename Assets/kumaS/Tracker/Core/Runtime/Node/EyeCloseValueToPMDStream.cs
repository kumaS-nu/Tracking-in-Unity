using System;
using System.Collections.Generic;
using System.Threading;

using UniRx;

using UnityEngine;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// 目の閉じ具合をPMDに変換するストリーム。
    /// </summary>
    public sealed class EyeCloseValueToPMDStream : ScheduleStreamBase<EyeCloseValue, PredictedModelData>
    {
        [SerializeField]
        internal bool isDebugValue = true;

        public override string ProcessName { get; set; } = "Eye close value to PMD";
        public override Type[] UseType { get; } = new Type[0];
        public override string[] DebugKey { get; } = new string[] { SchedulableData<object>.Elapsed_Time, nameof(Blink_L), nameof(Blink_R) };
        public override IReadOnlyReactiveProperty<bool> IsAvailable { get; } = new ReactiveProperty<bool>(true);

        private readonly string Blink_L = nameof(Blink_L);
        private readonly string Blink_R = nameof(Blink_R);

        protected override void InitInternal(int thread, CancellationToken token) { }

        protected override IDebugMessage DebugLogInternal(SchedulableData<PredictedModelData> data)
        {
            var message = new Dictionary<string, string>();
            data.ToDebugElapsedTime(message);
            if (data.IsSuccess && isDebugValue && !data.IsSignal)
            {
                data.Data.ToDebugParameter(message, Blink_L, Blink_L);
                data.Data.ToDebugParameter(message, Blink_R, Blink_R);
            }
            return new DebugMessage(data, message);
        }

        protected override SchedulableData<PredictedModelData> ProcessInternal(SchedulableData<EyeCloseValue> input)
        {
            if (!input.IsSuccess || input.IsSignal)
            {
                return new SchedulableData<PredictedModelData>(input, default);
            }

            var paramater = new Dictionary<string, float>
            {
                [Blink_L] = input.Data.Left,
                [Blink_R] = input.Data.Right
            };
            var ret = new PredictedModelData(parameter: paramater);
            return new SchedulableData<PredictedModelData>(input, ret);
        }

        public override void Dispose(){ }
    }
}
