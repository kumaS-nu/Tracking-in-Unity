﻿using System;
using System.Collections;
using System.Collections.Generic;

using UniRx;

using UnityEngine;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// 目の閉じ具合をMPDに変換するストリーム。
    /// </summary>
    public class EyeCloseValueToMPDStream : ScheduleStreamBase<EyeCloseValue, ModelPredictedData>
    {
        [SerializeField]
        internal bool isDebugValue = true;

        public override string ProcessName { get; set; } = "Eye close value to MPD";
        public override Type[] UseType { get; } = new Type[0];
        public override string[] DebugKey { get; } = new string[] { SchedulableData<object>.Elapsed_Time, nameof(Blink_L), nameof(Blink_R) };
        public override IReadOnlyReactiveProperty<bool> IsAvailable { get; } = new ReactiveProperty<bool>(true);

        private readonly string Blink_L = nameof(Blink_L);
        private readonly string Blink_R = nameof(Blink_R);

        public override void InitInternal(int thread){ }

        protected override IDebugMessage DebugLogInternal(SchedulableData<ModelPredictedData> data)
        {
            var message = new Dictionary<string, string>();
            data.ToDebugElapsedTime(message);
            if (data.IsSuccess && isDebugValue)
            {
                data.Data.ToDebugParameter(message, Blink_L, Blink_L);
                data.Data.ToDebugParameter(message, Blink_R, Blink_R);
            }
            return new DebugMessage(data, message);
        }

        protected override SchedulableData<ModelPredictedData> ProcessInternal(SchedulableData<EyeCloseValue> input)
        {
            if (!input.IsSuccess)
            {
                return new SchedulableData<ModelPredictedData>(input, default);
            }

            var paramater = new Dictionary<string, float>();
            paramater[Blink_L] = input.Data.Left;
            paramater[Blink_R] = input.Data.Right;
            var ret = new ModelPredictedData(new Dictionary<string, Vector3>(), new Dictionary<string, Quaternion>(), paramater);
            return new SchedulableData<ModelPredictedData>(input, ret);
        }
    }
}
