using kumaS.Tracker.Core;

using OpenCvSharp;

using System;
using System.Collections.Generic;
using System.Threading;

using UniRx;

using UnityEngine;

namespace kumaS.Tracker.Dlib
{
    /// <summary>
    /// Dlibの68の特徴点から目線を取得するストリーム。
    /// </summary>
    public sealed class Dlib68ToEyeRotationStream : ScheduleStreamBase<Dlib68Landmarks, EyeRotation>
    {
        [SerializeField]
        internal Vector2 leftCenter = new Vector2(0.5f, 0.5f);

        [SerializeField]
        internal Vector2 rightCenter = new Vector2(0.5f, 0.5f);

        [SerializeField]
        internal bool isDebugRotation = true;

        [SerializeField]
        internal float rotateScale = 20;

        [SerializeField]
        internal bool sourceIsMirror = false;

        [SerializeField]
        internal bool wantMirror = true;

        private bool mirror = true;

        public override string ProcessName { get; set; } = "Dlib 68 landmarks to eye rotation";
        public override Type[] UseType { get; } = new Type[] { typeof(Mat) };
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

        protected override void InitInternal(int thread, CancellationToken token)
        {
            mirror = sourceIsMirror != wantMirror;
            isAvailable.Value = true;
        }

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

        protected override SchedulableData<EyeRotation> ProcessInternal(SchedulableData<Dlib68Landmarks> input)
        {
            if (!input.IsSuccess || input.IsSignal)
            {
                return new SchedulableData<EyeRotation>(input, default);
            }

            try
            {
                Vector2 predictedLeftCenter = PredictCenter(input.Data.OriginalImage, GetRect(input.Data.Landmarks, 42, 48));
                Vector2 predictedRightCenter = PredictCenter(input.Data.OriginalImage, GetRect(input.Data.Landmarks, 36, 42));
                Vector2 leftPoint = GetNormalizedPoint(input.Data.Landmarks[42].ToVector2(), input.Data.Landmarks[45].ToVector2(), predictedLeftCenter);
                Vector2 rightPoint = GetNormalizedPoint(input.Data.Landmarks[36].ToVector2(), input.Data.Landmarks[39].ToVector2(), predictedRightCenter);
                leftPoint -= leftCenter;
                rightPoint -= rightCenter;
                if (float.IsNaN(leftPoint.x) || float.IsInfinity(leftPoint.x) || float.IsNaN(leftPoint.y) || float.IsInfinity(leftPoint.y) 
                    || float.IsNaN(rightPoint.x) || float.IsInfinity(rightPoint.x) || float.IsNaN(rightPoint.y) || float.IsInfinity(rightPoint.y))
                {
                    return new SchedulableData<EyeRotation>(input, default, false, errorMessage: "目線の取得に失敗しました。");
                }
                var left = Quaternion.Euler(-leftPoint.y * rotateScale, -leftPoint.x * rotateScale, 0);
                var right = Quaternion.Euler(-rightPoint.y * rotateScale, -rightPoint.x * rotateScale, 0);

                if (mirror)
                {
                    Quaternion tmp = left;
                    left = right;
                    right = tmp;
                    left.y *= -1;
                    left.z *= -1;
                    right.y *= -1;
                    right.y *= -1;
                }
                var ret = new EyeRotation(left, right);
                return new SchedulableData<EyeRotation>(input, ret);
            }
            finally
            {
                ResourceManager.DisposeIfRelease(input.Data.OriginalImage, Id);
            }
        }

        /// <summary>
        /// 目の範囲を作成。
        /// </summary>
        /// <param name="input">特徴点。</param>
        /// <param name="start">開始のインデックス（含む）。</param>
        /// <param name="end">終了のインデックス（含まない）。</param>
        /// <returns>範囲。</returns>
        private OpenCvSharp.Rect GetRect(DlibDotNet.Point[] input, int start, int end)
        {
            var xMin = int.MaxValue;
            var yMin = int.MaxValue;
            var xMax = int.MinValue;
            var yMax = int.MinValue;
            for (var i = start; i < end; i++)
            {
                if (input[i].X < xMin)
                {
                    xMin = input[i].X;
                }
                if (input[i].X > xMax)
                {
                    xMax = input[i].X;
                }
                if (input[i].Y < yMin)
                {
                    yMin = input[i].Y;
                }
                if (input[i].Y > yMax)
                {
                    yMax = input[i].Y;
                }
            }

            return new OpenCvSharp.Rect(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        /// <summary>
        /// 目の中心を推測する。
        /// </summary>
        /// <param name="originalImage">元画像。</param>
        /// <param name="rect">範囲。</param>
        /// <returns>座標。</returns>
        private Vector2 PredictCenter(Mat originalImage, OpenCvSharp.Rect rect)
        {
            Vector2 ret = Vector2.zero;
            var roi = new Mat(originalImage, rect);
            Mat monoRoi = roi.Split()[2];
            using (var binary = new Mat())
            using (var inverseBinary = new Mat())
            {
                Cv2.Threshold(monoRoi, binary, 0, 255, ThresholdTypes.Otsu);
                Cv2.BitwiseNot(binary, inverseBinary);
                Moments moment = inverseBinary.Moments(true);
                ret.x = (float)(moment.M10 / moment.M00) + rect.X;
                ret.y = (float)(moment.M01 / moment.M00) + rect.Y;
            }
            return ret;
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

        public override void Dispose() { }
    }
}
