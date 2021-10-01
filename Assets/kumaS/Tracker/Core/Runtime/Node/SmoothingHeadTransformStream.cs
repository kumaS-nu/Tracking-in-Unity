using System.Collections.Generic;
using System.Threading;

using UnityEngine;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// 頭の位置・回転を滑らかにするストリーム。
    /// </summary>
    public sealed class SmoothingHeadTransformStream : SmoothingStreamBase<HeadTransform>
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

        /// <inheritdoc/>
        protected override HeadTransform Average(HeadTransform[] datas, HeadTransform removed, bool isRemoved)
        {
            Vector3 position = Vector3.zero;
            foreach (HeadTransform data in datas)
            {
                position += data.Position;
            }
            position /= datas.Length;

            Quaternion rotation = datas[0].Rotation;
            for(var i = 1; i < datas.Length; i++)
            {
                rotation = QuaternionInterpolation.Yslerp(rotation, datas[i].Rotation, 1.0f / (i + 1));
            }

            return new HeadTransform(position, rotation);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        protected override bool ValidateData(HeadTransform input)
        {
            if (input.Position.sqrMagnitude > moveRangeInternal)
            {
                return false;
            }
            if ((input.Position - lastOutput.Position).sqrMagnitude > moveSpeedLimitInternal)
            {
                return false;
            }
            if (Quaternion.Angle(input.Rotation, Quaternion.identity) > rotateRange)
            {
                return false;
            }
            if (Quaternion.Angle(input.Rotation, lastOutput.Rotation) > rotateSpeedLimitInternal)
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public override void Dispose(){ }

        /// <inheritdoc/>
        protected override void InitInternal2(int thread, CancellationToken token)
        {
            moveSpeedLimitInternal = moveSpeedLimit * bufferSize * moveSpeedLimit * bufferSize;
            moveRangeInternal = moveRange * moveRange;
            rotateSpeedLimitInternal = rotateSpeedLimit * bufferSize;
        }
    }
}
