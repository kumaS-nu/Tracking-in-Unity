﻿using System.Collections.Generic;

using UnityEngine;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// 目線を滑らかにするストリーム。
    /// </summary>
    public sealed class SmoothingEyeRotationStream : SmoothingStreamBase<EyeRotation>
    {
        [SerializeField]
        internal bool isDebugRotation = true;

        [SerializeField]
        internal float rotateSpeedLimit = 5;

        [SerializeField]
        internal float rotateRangeLXMin = -10;

        [SerializeField]
        internal float rotateRangeLXMax = 10;

        [SerializeField]
        internal float rotateRangeLYMin = -20;

        [SerializeField]
        internal float rotateRangeLYMax = 20;

        [SerializeField]
        internal float rotateRangeRXMin = -10;

        [SerializeField]
        internal float rotateRangeRXMax = 10;

        [SerializeField]
        internal float rotateRangeRYMin = -20;

        [SerializeField]
        internal float rotateRangeRYMax = 20;

        private float rotateSpeedLimitInternal = 20;

        public override string ProcessName { get; set; } = "Smoothing eye rotation";
        public override string[] DebugKey { get; } = new string[]
        {
            SchedulableData<object>.Elapsed_Time,
            nameof(L_Eye_X), nameof(L_Eye_Y), nameof(L_Eye_Z),
            nameof(R_Eye_X), nameof(R_Eye_Y), nameof(R_Eye_Z)
        };

        private readonly string L_Eye_X = nameof(L_Eye_X);
        private readonly string L_Eye_Y = nameof(L_Eye_Y);
        private readonly string L_Eye_Z = nameof(L_Eye_Z);
        private readonly string R_Eye_X = nameof(R_Eye_X);
        private readonly string R_Eye_Y = nameof(R_Eye_Y);
        private readonly string R_Eye_Z = nameof(R_Eye_Z);

        private void Awake()
        {
            rotateSpeedLimitInternal = bufferSize * rotateSpeedLimit;
        }

        protected override EyeRotation Average(EyeRotation[] datas)
        {
            Vector3 logLotationL = Vector3.zero;
            Vector3 logLotationR = Vector3.zero;
            Quaternion averageL = datas[0].Left;
            Quaternion averageR = datas[0].Right;

            if (lastOutput != default)
            {
                averageL = lastOutput.Left;
                averageR = lastOutput.Right;
            }
            var inverseL = Quaternion.Inverse(averageL);
            var inverseR = Quaternion.Inverse(averageR);

            foreach (EyeRotation data in datas)
            {
                logLotationL += (inverseL * data.Left).ToLogQuaternion();
                logLotationR += (inverseR * data.Right).ToLogQuaternion();
            }
            logLotationL /= datas.Length;
            logLotationR /= datas.Length;
            return new EyeRotation(averageL * logLotationL.ToQuaternion(), averageR * logLotationR.ToQuaternion());
        }

        protected override IDebugMessage DebugLogInternal(SchedulableData<EyeRotation> data)
        {
            var message = new Dictionary<string, string>();
            data.ToDebugElapsedTime(message);
            if (data.IsSuccess && isDebugRotation)
            {
                data.Data.ToDebugRoattion(message, L_Eye_X, L_Eye_Y, L_Eye_Z, R_Eye_X, R_Eye_Y, R_Eye_Z);
            }
            return new DebugMessage(data, message);
        }

        protected override bool ValidateData(EyeRotation input)
        {
            Vector3 left = input.Left.eulerAngles;
            if (left.x > rotateRangeLXMax && left.x < 360 - rotateRangeLXMin)
            {
                return false;
            }
            if (left.y < rotateRangeLYMax && left.y < 360 - rotateRangeLYMin)
            {
                return false;
            }

            Vector3 right = input.Right.eulerAngles;
            if (right.x > rotateRangeRXMax && right.x < 360 - rotateRangeRXMin)
            {
                return false;
            }
            if (right.y > rotateRangeRYMax && right.y < 360 - rotateRangeRYMin)
            {
                return false;
            }

            if (Quaternion.Angle(lastOutput.Left, input.Left) > rotateSpeedLimitInternal)
            {
                return false;
            }
            if (Quaternion.Angle(lastOutput.Right, input.Right) > rotateSpeedLimitInternal)
            {
                return false;
            }

            return true;
        }
    }
}
