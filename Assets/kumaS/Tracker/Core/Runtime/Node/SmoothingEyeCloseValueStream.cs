﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// 目の閉じ具合を滑らかにするストリーム。
    /// </summary>
    public class SmoothingEyeCloseValueStream : SmoothingStreamBase<EyeCloseValue>
    {

        [SerializeField]
        internal bool isDebugValue = true;

        public override string ProcessName { get; set; } = "Smoothing eye close value";
        public override string[] DebugKey { get; } = new string[] { SchedulableData<object>.Elapsed_Time, nameof(Left_Close_Value), nameof(Right_Close_Value) };

        private readonly string Left_Close_Value = nameof(Left_Close_Value);
        private readonly string Right_Close_Value = nameof(Right_Close_Value);

        protected override EyeCloseValue Average(EyeCloseValue[] datas)
        {
            var leftSum = 0f;
            var rightSum = 0f;
            foreach(var d in datas)
            {
                leftSum += d.Left;
                rightSum += d.Right;
            }
            return new EyeCloseValue(leftSum / datas.Length, rightSum / datas.Length);
        }

        protected override IDebugMessage DebugLogInternal(SchedulableData<EyeCloseValue> data)
        {
            var message = new Dictionary<string, string>();
            data.ToDebugElapsedTime(message);
            if(data.IsSuccess && isDebugValue)
            {
                message[Left_Close_Value] = data.Data.Left.ToString();
                message[Right_Close_Value] = data.Data.Right.ToString();
            }
            return new DebugMessage(data, message);
        }

        protected override bool ValidateData(EyeCloseValue input)
        {
            if(lastOutput == default)
            {
                return true;
            }

            if(input.Left < 0 || input.Left > 1 || input.Right < 0 || input.Right > 1)
            {
                return false;
            }

            return true;
        }
    }
}