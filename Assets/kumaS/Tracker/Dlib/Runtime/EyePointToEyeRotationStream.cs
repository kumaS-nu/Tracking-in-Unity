using kumaS.Tracker.Core;

using System;
using System.Collections.Generic;
using System.Threading;

using UniRx;

using UnityEngine;

namespace kumaS.Tracker.Dlib
{
    /// <summary>
    /// 目玉の中心点から目線を取得するストリーム。
    /// </summary>
    public sealed class EyePointToEyeRotationStream : ScheduleStreamBase<Dlib68EyePoint, EyeRotation>
    {
        [SerializeField]
        internal Vector2 leftCenter = new Vector2(0.5f, 0);

        [SerializeField]
        internal Vector2 rightCenter = new Vector2(0.5f, 0);

        [SerializeField]
        internal bool isDebugRotation = true;

        [SerializeField]
        internal float rotateScale = 20;

        [SerializeField]
        internal bool sourceIsMirror = false;

        [SerializeField]
        internal bool wantMirror = true;

        private bool mirror = true;

        public override string ProcessName { get; set; } = "Eye point to eye rotation";
        public override Type[] UseType { get; } = new Type[] { };
        public override string[] DebugKey { get; } = new string[] {
            SchedulableData<object>.Elapsed_Time,
            nameof(L_Eye_X), nameof(L_Eye_Y), nameof(L_Eye_Z),
            nameof(R_Eye_X), nameof(R_Eye_Y), nameof(R_Eye_Z)
        };

        public override IReadOnlyReactiveProperty<bool> IsAvailable { get => isAvailable; }

        private readonly ReactiveProperty<bool> isAvailable = new ReactiveProperty<bool>(false);

        private readonly string L_Eye_X = nameof(L_Eye_X);
        private readonly string L_Eye_Y = nameof(L_Eye_Y);
        private readonly string L_Eye_Z = nameof(L_Eye_Z);
        private readonly string R_Eye_X = nameof(R_Eye_X);
        private readonly string R_Eye_Y = nameof(R_Eye_Y);
        private readonly string R_Eye_Z = nameof(R_Eye_Z);

        /// <inheritdoc/>
        protected override void InitInternal(int thread, CancellationToken token)
        {
            mirror = sourceIsMirror != wantMirror;
            isAvailable.Value = true;
        }

        /// <inheritdoc/>
        protected override IDebugMessage DebugLogInternal(SchedulableData<EyeRotation> data)
        {
            var message = new Dictionary<string, string>();
            data.ToDebugElapsedTime(message);
            if (data.IsSuccess && isDebugRotation && !data.IsSignal)
            {
                data.Data.ToDebugRoattion(message, L_Eye_X, L_Eye_Y, L_Eye_Z, R_Eye_X, R_Eye_Y, R_Eye_Z);
            }
            return new DebugMessage(data, message);
        }

        /// <inheritdoc/>
        protected override SchedulableData<EyeRotation> ProcessInternal(SchedulableData<Dlib68EyePoint> input)
        {
            if (!input.IsSuccess || input.IsSignal)
            {
                return new SchedulableData<EyeRotation>(input, default);
            }

            Vector2 leftPoint = GetNormalizedPoint(input.Data.Landmarks[42].ToVector2(), input.Data.Landmarks[45].ToVector2(), input.Data.LeftCenter);
            Vector2 rightPoint = GetNormalizedPoint(input.Data.Landmarks[36].ToVector2(), input.Data.Landmarks[39].ToVector2(), input.Data.RightCenter);
            leftPoint -= leftCenter;
            rightPoint -= rightCenter;
            if (float.IsNaN(leftPoint.x) || float.IsInfinity(leftPoint.x) || float.IsNaN(leftPoint.y) || float.IsInfinity(leftPoint.y)
                || float.IsNaN(rightPoint.x) || float.IsInfinity(rightPoint.x) || float.IsNaN(rightPoint.y) || float.IsInfinity(rightPoint.y))
            {
                return new SchedulableData<EyeRotation>(input, default, false, errorMessage: "目線の取得に失敗しました。");
            }
            var left = Quaternion.Euler(leftPoint.y * rotateScale, -leftPoint.x * rotateScale, 0);
            var right = Quaternion.Euler(rightPoint.y * rotateScale, -rightPoint.x * rotateScale, 0);

            if (mirror)
            {
                Quaternion tmp = left;
                left = right;
                right = tmp;
                left.y *= -1;
                left.z *= -1;
                right.y *= -1;
                right.z *= -1;
            }
            var ret = new EyeRotation(left, right);
            return new SchedulableData<EyeRotation>(input, ret);
        }

        /// <summary>
        /// 座標を正規化する。
        /// </summary>
        /// <param name="root">原点。</param>
        /// <param name="xAxiesEnd">x軸の終点。</param>
        /// <param name="center">目の中心。</param>
        /// <returns>正規化された座標。</returns>
        private Vector2 GetNormalizedPoint(Vector2 root, Vector2 xAxiesEnd, Vector2 center)
        {
            Vector2 xAxies = xAxiesEnd - root;
            Vector2 c = center - root;
            var dot = Vector2.Dot(xAxies.normalized, c.normalized);
            var ret = new Vector2(dot, Mathf.Sqrt(1 - dot * dot));
            if (xAxies.normalized.y > c.normalized.y)
            {
                ret.y *= -1;
            }
            ret = ret * c.magnitude / xAxies.magnitude;
            return ret;
        }

        /// <inheritdoc/>
        public override void Dispose() { }
    }
}
