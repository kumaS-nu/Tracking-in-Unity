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
        protected byte[] data;
        protected Size inputSize = new Size(257, 257);
        protected int outputStride;
        protected InterpolationFlags flag;
        protected float[] heatmap = default;
        protected float[] offsets = default;
        protected float minScore;
        protected int quantHeight;
        protected int quantWidth;
        protected int quantAmount;

        internal async UniTask<PoseNetLandmarks> Execute(Mat input)
        {
            GetInputImage(input);
            await UniTask.SwitchToMainThread();
            ExecuteInternal();

            var landmarks = new Vector2[17];
            float totalScore = 0;
            for (var i = 0; i < 17; i++)
            {
                var max = float.MinValue;
                var index = 0;
                for (var j = 0; j < quantAmount; j++)
                {
                    if (heatmap[17 * j + i] > max)
                    {
                        index = j;
                        max = heatmap[17 * j + i];
                    }
                }

                Vector2 landmark = new Vector2(index % quantWidth, index / quantWidth) * outputStride;
                landmark.y += offsets[34 * index + i];
                landmark.x += offsets[34 * index + i + 17];
                landmarks[i] = landmark;
                totalScore += heatmap[17 * index + i];
            }

            if (totalScore < minScore)
            {
                return default;
            }

            return new PoseNetLandmarks(input, landmarks);
        }

        private void GetInputImage(Mat input)
        {
            if (input.Size() == inputSize)
            {
                Marshal.Copy(input.Data, data, 0, data.Length);
            }
            else
            {
                int width;
                int height;
                if ((float)input.Width / input.Height > (float)inputSize.Width / inputSize.Height)
                {
                    var scale = (double)inputSize.Width / input.Width;
                    width = inputSize.Width;
                    height = (int)(input.Height * scale);
                }
                else
                {
                    var scale = (double)inputSize.Height / input.Height;
                    width = (int)(input.Width * scale);
                    height = inputSize.Height;
                }

                var resizedSize = new Size(width, height);

                if (resizedSize == inputSize)
                {
                    using (var resized = new Mat())
                    {
                        Cv2.Resize(input, resized, new Size(width, height), 0, 0, flag);
                        Marshal.Copy(resized.Data, data, 0, data.Length);
                    }
                }
                else if (resizedSize == input.Size())
                {
                    var heightMargin = inputSize.Height - height;
                    var widthMargin = inputSize.Width - width;
                    var topPad = heightMargin / 2;
                    var leftPad = widthMargin / 2;
                    using (var paded = new Mat())
                    {
                        Cv2.CopyMakeBorder(paded, input, topPad, heightMargin - topPad, leftPad, widthMargin - leftPad, BorderTypes.Constant, Scalar.Black);
                        Marshal.Copy(paded.Data, data, 0, data.Length);
                    }
                }
                else
                {
                    var heightMargin = inputSize.Height - height;
                    var widthMargin = inputSize.Width - width;
                    var topPad = heightMargin / 2;
                    var leftPad = widthMargin / 2;
                    using (var resized = new Mat())
                    using (var paded = new Mat())
                    {
                        Cv2.Resize(input, resized, new Size(width, height), 0, 0, flag);
                        Cv2.CopyMakeBorder(paded, resized, topPad, heightMargin - topPad, leftPad, widthMargin - leftPad, BorderTypes.Constant, Scalar.Black);
                        Marshal.Copy(paded.Data, data, 0, data.Length);
                    }
                }
            }
        }

        protected abstract void ExecuteInternal();

        public abstract void Dispose();
    }
}
