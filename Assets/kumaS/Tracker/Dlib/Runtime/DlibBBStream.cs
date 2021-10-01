using Cysharp.Threading.Tasks;

using DlibDotNet;

using kumaS.Tracker.Core;

using OpenCvSharp;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

using UniRx;

using UnityEngine;

namespace kumaS.Tracker.Dlib
{
    /// <summary>
    /// Dlibを使い、バウンダリーボックスを検出する。
    /// </summary>
    public sealed class DlibBBStream : ScheduleStreamBase<Mat, BoundaryBox>
    {
        [SerializeField]
        internal bool isDebugBox = true;

        [SerializeField]
        internal int interval = 0;

        private FrontalFaceDetector[] detectors;
        private volatile int skipped = 0;

        public override string ProcessName { get; set; } = "Dlib BB predict";
        public override Type[] UseType { get; } = new Type[] { typeof(Mat) };
        public override string[] DebugKey { get; } = new string[] { SchedulableData<object>.Elapsed_Time, nameof(Image_Width), nameof(Image_Height), nameof(X), nameof(Y), nameof(Width), nameof(Height), nameof(Angle) };
        public override IReadOnlyReactiveProperty<bool> IsAvailable { get => isAvailable; }

        private readonly ReactiveProperty<bool> isAvailable = new ReactiveProperty<bool>(false);

        private readonly string Image_Width = nameof(Image_Width);
        private readonly string Image_Height = nameof(Image_Height);
        private readonly string X = nameof(X);
        private readonly string Y = nameof(Y);
        private readonly string Width = nameof(Width);
        private readonly string Height = nameof(Height);
        private readonly string Angle = nameof(Angle);

        protected override async void InitInternal(int thread, CancellationToken token)
        {
            detectors = new FrontalFaceDetector[thread];
            List<UniTask> tasks = new List<UniTask>();
            for (var i = 0; i < thread; i++)
            {
                tasks.Add(LoadAsync(i));
            }
            await UniTask.WhenAll(tasks);
            isAvailable.Value = true;
        }
        private async UniTask LoadAsync(int thread)
        {
            await UniTask.SwitchToThreadPool();
            detectors[thread] = DlibDotNet.Dlib.GetFrontalFaceDetector();
        }

        protected override IDebugMessage DebugLogInternal(SchedulableData<BoundaryBox> data)
        {
            var message = new Dictionary<string, string>();
            data.ToDebugElapsedTime(message);
            if (data.IsSuccess && !data.IsSignal)
            {
                message[Image_Width] = data.Data.ImageSize.x.ToString();
                message[Image_Height] = data.Data.ImageSize.y.ToString();
                if (isDebugBox && data.Data.Box != default)
                {
                    message[X] = data.Data.Box.x.ToString();
                    message[Y] = data.Data.Box.y.ToString();
                    message[Width] = data.Data.Box.width.ToString();
                    message[Height] = data.Data.Box.height.ToString();
                    message[Angle] = data.Data.Angle.ToString();
                }
            }
            return new DebugMessage(data, message);
        }

        protected override SchedulableData<BoundaryBox> ProcessInternal(SchedulableData<Mat> input)
        {
            if (!input.IsSuccess || input.IsSignal)
            {
                return new SchedulableData<BoundaryBox>(input, default);
            }

            try
            {
                if (skipped < interval)
                {
                    Interlocked.Increment(ref skipped);
                    return new SchedulableData<BoundaryBox>(input, new BoundaryBox(input.Data, default));
                }

                if (TryGetThread(out var thread))
                {
                    Interlocked.Exchange(ref skipped, 0);
                    try
                    {
                        using (Array2D<BgrPixel> image = DlibDotNet.Dlib.LoadImageData<BgrPixel>(input.Data.Data, (uint)input.Data.Height, (uint)input.Data.Width, (uint)input.Data.Width))
                        {
                            Rectangle[] bb = detectors[thread].Operator(image);
                            if (bb.Length == 0)
                            {
                                if (input.Data.IsEnabledDispose)
                                {
                                    input.Data.Dispose();
                                }
                                return new SchedulableData<BoundaryBox>(input, default, false, true, errorMessage: "顔が検出されませんでした。");
                            }
                            var ret = new BoundaryBox(input.Data, new UnityEngine.Rect(bb[0].Left, bb[0].Top, bb[0].Width, bb[0].Height));
                            return new SchedulableData<BoundaryBox>(input, ret);
                        }
                    }
                    finally
                    {
                        FreeThread(thread);
                    }
                }
                else
                {
                    if (input.Data.IsEnabledDispose)
                    {
                        input.Data.Dispose();
                    }
                    return new SchedulableData<BoundaryBox>(input, default, false, true, errorMessage: "スレッドを確保できませんでした。");
                }
            }
            finally
            {
                ResourceManager.DisposeIfRelease(input.Data, Id);
            }
        }

        public override void Dispose()
        {
            foreach (FrontalFaceDetector d in detectors)
            {
                if (d.IsEnableDispose)
                {
                    d.Dispose();
                }
            }
        }
    }
}
