using System;
using System.Collections.Generic;
using System.Threading;

using UniRx;

using UnityEngine;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// 目の比から閉じ具合へ変換するストリーム。
    /// </summary>
    public sealed class RatioToCloseValueStream : ScheduleStreamBase<EyeRatio, EyeCloseValue>
    {
        [SerializeField]
        internal float ratioMax = 0.3f;

        [SerializeField]
        internal float ratioMin = 0.2f;

        [SerializeField]
        internal bool isDebugValue = true;

        private float coeff = 10;

        public override string ProcessName { get; set; } = "Eye ratio to close value";
        public override Type[] UseType { get; } = new Type[0];
        public override string[] DebugKey { get; } = new string[] { SchedulableData<object>.Elapsed_Time, nameof(Left_Close_Value), nameof(Right_Close_Value) };
        public override IReadOnlyReactiveProperty<bool> IsAvailable { get => isAvailable; }

        private readonly ReactiveProperty<bool> isAvailable = new ReactiveProperty<bool>(false);

        private readonly string Left_Close_Value = nameof(Left_Close_Value);
        private readonly string Right_Close_Value = nameof(Right_Close_Value);

        protected override void InitInternal(int thread, CancellationToken token)
        {
            coeff = 1 / (ratioMax - ratioMin);
            isAvailable.Value = true;
        }

        protected override IDebugMessage DebugLogInternal(SchedulableData<EyeCloseValue> data)
        {
            var message = new Dictionary<string, string>();
            data.ToDebugElapsedTime(message);
            if (data.IsSuccess && isDebugValue && !data.IsSignal)
            {
                message[Left_Close_Value] = data.Data.Left.ToString();
                message[Right_Close_Value] = data.Data.Right.ToString();
            }
            return new DebugMessage(data, message);
        }

        protected override SchedulableData<EyeCloseValue> ProcessInternal(SchedulableData<EyeRatio> input)
        {
            if (!input.IsSuccess || input.IsSignal)
            {
                return new SchedulableData<EyeCloseValue>(input, default);
            }
            var ret = new EyeCloseValue(Convert(input.Data.Left), Convert(input.Data.Right));
            return new SchedulableData<EyeCloseValue>(input, ret);
        }

        /// <summary>
        /// 目の比から閉じ具合へ変換する。
        /// </summary>
        /// <param name="ratio">目の比。</param>
        /// <returns>目の閉じ具合。</returns>
        private float Convert(float ratio)
        {
            var value = (ratio - ratioMin) * coeff;
            if (value > 1)
            {
                return 1;
            }
            else if (value < 0)
            {
                return 0;
            }
            else
            {
                return value;
            }
        }

        public override void Dispose(){ }
    }
}
