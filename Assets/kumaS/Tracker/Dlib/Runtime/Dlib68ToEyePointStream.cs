using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using kumaS.Tracker.Core;
using System;
using UniRx;
using System.Threading;

namespace kumaS.Tracker.Dlib
{
    /// <summary>
    /// Dlib68の特徴点と画像から目玉の中心を推定するストリーム。
    /// </summary>
    public sealed class Dlib68ToEyePointStream : ScheduleStreamBase<Dlib68Landmarks, Dlib68EyePoint>
    {
        [SerializeField]
        internal bool isDebugPoint = true;

        [SerializeField]
        internal bool isDebugImage = false;

        [SerializeField]
        internal int interval = 2;

        [SerializeField]
        internal DebugImageStream debugImage;

        [SerializeField]
        internal Color markColor = new Color(0, 1, 0, 0.5f);

        [SerializeField]
        internal int markSize = 3;

        public override string ProcessName { get; set; } = "Dlib68 to eye point";
        public override Type[] UseType { get; } = new Type[] { typeof(Mat) };
        public override string[] DebugKey { get; } = new string[] {
            SchedulableData<object>.Elapsed_Time,
            nameof(Left_X), nameof(Left_Y), nameof(Right_X), nameof(Right_Y)};
        public override IReadOnlyReactiveProperty<bool> IsAvailable { get => isAvailable; }
        private ReactiveProperty<bool> isAvailable = new ReactiveProperty<bool>(false);

        private readonly string Left_X = nameof(Left_X);
        private readonly string Left_Y = nameof(Left_Y);
        private readonly string Right_X = nameof(Right_X);
        private readonly string Right_Y = nameof(Right_Y);

        private Scalar color;
        private int skipCount = 0;

        /// <inheritdoc/>
        public override void Dispose(){ }

        /// <inheritdoc/>
        protected override IDebugMessage DebugLogInternal(SchedulableData<Dlib68EyePoint> data)
        {
            var message = new Dictionary<string, string>();
            data.ToDebugElapsedTime(message);
            if (data.IsSuccess && !data.IsSignal)
            {
                if (isDebugPoint)
                {
                    message[Left_X] = data.Data.LeftCenter.x.ToString();
                    message[Left_Y] = data.Data.LeftCenter.y.ToString();
                    message[Right_X] = data.Data.RightCenter.x.ToString();
                    message[Right_Y] = data.Data.RightCenter.y.ToString();
                }

                if(isDebugImage && debugImage != null)
                {
                    if (skipCount < interval)
                    {
                        skipCount++;
                    }
                    else
                    {
                        var mat = new Mat(data.Data.ImageSize.y, data.Data.ImageSize.x, MatType.CV_8UC3);
                        mat.Circle((int)data.Data.LeftCenter.x, (int)data.Data.LeftCenter.y, markSize, color, -1);
                        mat.Circle((int)data.Data.RightCenter.x, (int)data.Data.RightCenter.y, markSize, color, -1);
                        debugImage.SetImage(Id, mat);
                        skipCount = 0;
                    }
                }
            }
            return new DebugMessage(data, message);
        }

        /// <inheritdoc/>
        protected override void InitInternal(int thread, CancellationToken token)
        {
            if(isDebug.Value && isDebugImage && debugImage == null)
            {
                throw new ArgumentNullException("Debug image is null. At " + ProcessName + ".");
            }

            if(isDebug.Value && isDebugImage && debugImage != null)
            {
                debugImage.SetAlphaData(Id, markColor.a);
                color = new Scalar(markColor.b * 255, markColor.g * 255, markColor.r * 255);
            }
            isAvailable.Value = true;
        }

        /// <inheritdoc/>
        protected override SchedulableData<Dlib68EyePoint> ProcessInternal(SchedulableData<Dlib68Landmarks> input)
        {
            if (!input.IsSuccess || input.IsSignal)
            {
                return new SchedulableData<Dlib68EyePoint>(input, default);
            }

            try
            {
                Vector2 predictedLeftCenter = PredictCenter(input.Data.OriginalImage, GetRect(input.Data.Landmarks, 42, 48));
                Vector2 predictedRightCenter = PredictCenter(input.Data.OriginalImage, GetRect(input.Data.Landmarks, 36, 42));
                var ret = input.Data.OriginalImage != null ?
                    new Dlib68EyePoint(input.Data.OriginalImage, input.Data.Landmarks, predictedLeftCenter, predictedRightCenter) :
                    new Dlib68EyePoint(input.Data.ImageSize, input.Data.Landmarks, predictedLeftCenter, predictedRightCenter);
                return new SchedulableData<Dlib68EyePoint>(input, ret);
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
    }
}
