using kumaS.Tracker.Core;

using OpenCvSharp;

using System;
using System.Collections.Generic;
using System.Threading;

using UniRx;

using UnityEngine;

namespace kumaS.Tracker.FaceMesh
{
    /// <summary>
    /// FaceMeshの特徴点から目線を取得するストリーム。
    /// </summary>
    public sealed class FaceMeshToEyeRotationStream : ScheduleStreamBase<FaceMeshLandmarks, EyeRotation>
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

        public override string ProcessName { get; set; } = "FaceMesh to eye rotation";
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
        protected override void InitInternal(int thread, CancellationToken token)
        {
            mirror = sourceIsMirror != wantMirror;
            isAvailable.Value = true;
        }

        /// <inheritdoc/>
        protected override SchedulableData<EyeRotation> ProcessInternal(SchedulableData<FaceMeshLandmarks> input)
        {
            if (!input.IsSuccess || input.IsSignal)
            {
                return new SchedulableData<EyeRotation>(input, default);
            }

            try
            {
                Vector2 predictedLeftCenter = PredictCenter(input.Data.OriginalImage, GetRect(GetLeft(input.Data.Landmarks)));
                Vector2 predictedRightCenter = PredictCenter(input.Data.OriginalImage, GetRect(GetRight(input.Data.Landmarks)));
                Vector2 leftPoint = GetNormalizedPoint(input.Data.Landmarks[462], input.Data.Landmarks[358], predictedLeftCenter);
                Vector2 rightPoint = GetNormalizedPoint(input.Data.Landmarks[242], input.Data.Landmarks[129], predictedRightCenter);
                leftPoint -= leftCenter;
                rightPoint -= rightCenter;
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
        /// 左目の特徴点を抽出。
        /// </summary>
        /// <param name="landmarks">特徴点。</param>
        /// <returns>左目の特徴点。</returns>
        private Vector3[] GetLeft(Vector3[] landmarks)
        {
            var left = new Vector3[16];
            left[0] = landmarks[246];
            left[1] = landmarks[29];
            left[2] = landmarks[28];
            left[3] = landmarks[26];
            left[4] = landmarks[27];
            left[5] = landmarks[55];
            left[6] = landmarks[189];
            left[7] = landmarks[129];
            left[8] = landmarks[24];
            left[9] = landmarks[109];
            left[10] = landmarks[23];
            left[11] = landmarks[22];
            left[12] = landmarks[21];
            left[13] = landmarks[25];
            left[14] = landmarks[111];
            left[15] = landmarks[242];
            return left;
        }

        /// <summary>
        /// 右目の特徴点を抽出。
        /// </summary>
        /// <param name="landmarks">特徴点。</param>
        /// <returns>右目の特徴点。</returns>
        private Vector3[] GetRight(Vector3[] landmarks)
        {
            var right = new Vector3[16];
            right[0] = landmarks[466];
            right[1] = landmarks[259];
            right[2] = landmarks[258];
            right[3] = landmarks[256];
            right[4] = landmarks[257];
            right[5] = landmarks[285];
            right[6] = landmarks[413];
            right[7] = landmarks[358];
            right[8] = landmarks[254];
            right[9] = landmarks[338];
            right[10] = landmarks[253];
            right[11] = landmarks[252];
            right[12] = landmarks[251];
            right[13] = landmarks[255];
            right[14] = landmarks[340];
            right[15] = landmarks[462];
            return right;
        }

        /// <summary>
        /// 目の範囲を作成。
        /// </summary>
        /// <param name="input">特徴点。</param>
        /// <param name="start">開始のインデックス（含む）。</param>
        /// <param name="end">終了のインデックス（含まない）。</param>
        /// <returns>範囲。</returns>
        private OpenCvSharp.Rect GetRect(Vector3[] input)
        {
            var xMin = int.MaxValue;
            var yMin = int.MaxValue;
            var xMax = int.MinValue;
            var yMax = int.MinValue;
            foreach (Vector3 i in input)
            {
                if (i.x < xMin)
                {
                    xMin = (int)i.x;
                }
                if (i.x > xMax)
                {
                    xMax = (int)i.x;
                }
                if (i.y < yMin)
                {
                    yMin = (int)i.y;
                }
                if (i.y > yMax)
                {
                    yMax = (int)i.y;
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
        private Vector2 GetNormalizedPoint(Vector3 root, Vector3 xAxiesEnd, Vector3 center)
        {
            Vector3 xAxies = xAxiesEnd - root;
            Vector3 c = center - root;
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
        public override void Dispose(){ }
    }
}
