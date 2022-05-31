using Cysharp.Threading.Tasks;

using OpenCvSharp;

using System;
using System.Runtime.InteropServices;

using Unity.Barracuda;

using UnityEngine;

namespace kumaS.Tracker.PoseNet
{
    internal abstract class PoseNetBase : IDisposable
    {
        protected IWorker worker;
        protected Mat data = new Mat();
        protected Size inputSize = new Size(257, 257);
        protected int outputStride;
        protected InterpolationFlags flag;
        protected float minScore;

        private Size beforeSize = default;
        private bool isSameSize = true;
        private bool isNoPad = true;
        private Size resizedSize = default;
        private float invScale = 1;
        private int topPad = 0;
        private int leftPad = 0;
        private int bottomPad = 0;
        private int rightPad = 0;

        internal async UniTask<PoseNetLandmarks> Execute(Mat input)
        {
            var size = input.Size();
            if (beforeSize != size)
            {
                SetScaleSetting(size);
            }
            GetInputImage(input);
            await UniTask.SwitchToMainThread();
            var (heatmap, offsets) = ExecuteInternal();

            var landmarks = new Vector2[17];
            float totalScore = 0;
            for (var c = 0; c < 17; c++)
            {
                var max = float.MinValue;
                var index = Vector2Int.zero;

                for (var h = 0; h < heatmap.height; h++)
                {
                    for (var w = 0; w < heatmap.width; w++)
                    {
                        if (heatmap[0, h, w, c] > max)
                        {
                            index = new Vector2Int(w, h);
                            max = heatmap[0, h, w, c];
                        }
                    }
                }

                Vector2 landmark = index * outputStride;
                landmark.y += offsets[0, index.y, index.x, c];
                landmark.x += offsets[0, index.y, index.x, c + 17];
                landmark.y -= topPad;
                landmark.x -= leftPad;
                landmark *= invScale;
                landmarks[c] = landmark;
                totalScore += (Mathf.Atan(max / 2) + 1) / 2;
            }

            offsets.Dispose();
            heatmap.Dispose();

            if (totalScore < minScore)
            {
                return default;
            }

            return new PoseNetLandmarks(input, landmarks);
        }

        private void SetScaleSetting(Size input)
        {
            beforeSize = input;
            if (input == inputSize)
            {
                isSameSize = true;
                isNoPad = true;
                resizedSize = input;
                invScale = 1;
                topPad = 0;
                leftPad = 0;
                bottomPad = 0;
                rightPad = 0;
                return;
            }

            isSameSize = false;

            int width, height;
            float scale;
            if ((float)inputSize.Height / input.Height > (float)inputSize.Width / input.Width)
            {
                scale = (float)inputSize.Width / input.Width;
                width = inputSize.Width;
                height = (int)(input.Height * scale);
            }
            else
            {
                scale = (float)inputSize.Height / input.Height;
                width = (int)(input.Width * scale);
                height = inputSize.Height;
            }

            invScale = 1 / scale;
            resizedSize = new Size(width, height);

            if (resizedSize == inputSize)
            {
                isNoPad = true;
                topPad = 0;
                leftPad = 0;
                bottomPad = 0;
                rightPad = 0;
                return;
            }

            isNoPad = false;
            var heightMargin = inputSize.Height - height;
            var widthMargin = inputSize.Width - width;
            topPad = heightMargin / 2;
            leftPad = widthMargin / 2;
            bottomPad = heightMargin - topPad;
            rightPad = widthMargin - leftPad;
        }

        private void GetInputImage(Mat input)
        {
            if (data.IsEnabledDispose)
            {
                data.Dispose();
            }

            if (isSameSize)
            {
                data = input.Clone();
                return;
            }

            if (isNoPad)
            {
                data = new Mat();
                Cv2.Resize(input, data, resizedSize, 0, 0, flag);
                return;
            }

            if (invScale == 1)
            {
                data = new Mat();
                Cv2.CopyMakeBorder(input, data, topPad, bottomPad, leftPad, rightPad, BorderTypes.Constant, Scalar.Black);
                return;
            }

            using (var resized = new Mat())
            {
                data = new Mat();
                Cv2.Resize(input, resized, resizedSize, 0, 0, flag);
                Cv2.CopyMakeBorder(resized, data, topPad, bottomPad, leftPad, rightPad, BorderTypes.Constant, Scalar.Black);
            }
        }

        protected abstract (Tensor heatmap, Tensor offsets) ExecuteInternal();

        public void Dispose()
        {
            if (data.IsEnabledDispose)
            {
                data.Dispose();
            }
            DisposeInternal();
        }

        public abstract void DisposeInternal();
    }
}
