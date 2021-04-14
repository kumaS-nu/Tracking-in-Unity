using System;
using System.Collections;
using System.Collections.Generic;

using UniRx;

using UnityEngine;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// 頭の位置・回転を滑らかにするストリーム。
    /// </summary>
    public class SmoothingHeadTransformStream : SmoothingStreamBase<HeadTransform>
    {
        [SerializeField]
        internal float moveSpeedLimit = 0.05f;

        [SerializeField]
        internal float moveRange = 0.5f;

        [SerializeField]
        internal float rotateSpeedLimit = 15;

        [SerializeField]
        internal float rotateRange = 90;

        [SerializeField]
        internal bool isDebugHead = true;

        public override string ProcessName { get; set; } = "Smoothing head transform";
        public override string[] DebugKey { get; } = new string[] { 
            SchedulableData<object>.Elapsed_Time, 
            nameof(Head_Pos_X), nameof(Head_Pos_Y), nameof(Head_Pos_Z),
            nameof(Head_Rot_X), nameof(Head_Rot_Y), nameof(Head_Rot_Z) 
        };

        private readonly string Head_Pos_X = nameof(Head_Pos_X);
        private readonly string Head_Pos_Y = nameof(Head_Pos_Y);
        private readonly string Head_Pos_Z = nameof(Head_Pos_Z);
        private readonly string Head_Rot_X = nameof(Head_Rot_X);
        private readonly string Head_Rot_Y = nameof(Head_Rot_Y);
        private readonly string Head_Rot_Z = nameof(Head_Rot_Z);

        private float moveSpeedLimitInternal = 1;
        private float moveRangeInternal = 1;
        private float rotateSpeedLimitInternal = 180;

        private void Awake()
        {
            moveSpeedLimitInternal = moveSpeedLimit * bufferSize * moveSpeedLimit * bufferSize;
            moveRangeInternal = moveRange * moveRange;
            rotateSpeedLimitInternal = rotateSpeedLimit * bufferSize;
        }

        protected override HeadTransform Average(HeadTransform[] datas)
        {
            Vector3 position = Vector3.zero;
            Vector3 logLotation = Vector3.zero;
            Quaternion average = Quaternion.identity;
            if(lastOutput != default)
            {
                average = lastOutput.Rotation;
            }
            var inverse = Quaternion.Inverse(average);

            foreach (var data in datas)
            {
                position += data.Position;
                logLotation += (data.Rotation * inverse).ToLogQuaternion();
            }
            position /= datas.Length;
            logLotation /= datas.Length;
            return new HeadTransform(position, logLotation.ToQuaternion() * average);
        }

        protected override IDebugMessage DebugLogInternal(SchedulableData<HeadTransform> data)
        {
            var message = new Dictionary<string, string>();
            data.ToDebugElapsedTime(message);
            if (data.IsSuccess)
            {
                if (isDebugHead)
                {
                    data.Data.ToDebugPosition(message, Head_Pos_X, Head_Pos_Y, Head_Pos_Z);
                    data.Data.ToDebugRotation(message, Head_Rot_X, Head_Rot_Y, Head_Rot_Z);
                }
            }
            return new DebugMessage(data, message);
        }

        protected override bool ValidateData(HeadTransform input)
        {
            if(input.Position.sqrMagnitude > moveRangeInternal)
            {
                return false;
            }
            if(input.Position.sqrMagnitude > moveSpeedLimitInternal)
            {
                return false;
            }
            if(Quaternion.Angle(input.Rotation, Quaternion.identity) > rotateRange)
            {
                return false;
            }
            if(Quaternion.Angle(input.Rotation, lastOutput.Rotation) > rotateSpeedLimitInternal)
            {
                return false;
            }

            return true;
        }
    }
}
