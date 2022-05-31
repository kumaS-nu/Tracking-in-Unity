using Cysharp.Threading.Tasks;

using OpenCvSharp;

using System;
using System.Collections.Generic;
using System.Threading;

using UniRx;

using UnityEngine;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// OpenCvを使い、バウンダリーボックスを検出する。
    /// </summary>
    public sealed class OpenCvBBStream : ScheduleStreamBase<Mat, BoundaryBox>
    {
        [SerializeField]
        internal string filePath = "";

        [SerializeField]
        internal int pathType = 1;

        [SerializeField]
        internal bool isDebugBox = true;

        [SerializeField]
        internal int interval = 0;

        [SerializeField]
        internal bool isDebugImage = false;

        [SerializeField]
        internal int debugInterval = 2;

        [SerializeField]
        internal DebugImageStream debugImage;

        [SerializeField]
        internal Color markColor = new Color(0, 1, 0, 0.5f);

        [SerializeField]
        internal int markSize = 3;

        private CascadeClassifier[] predictor;
        private volatile int skipped = 0;
        private Scalar color;
        private int skipCount = 0;

        public override string ProcessName { get; set; } = "OpenCvSharp BB predict";
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

        /// <inheritdoc/>
        protected override async void InitInternal(int thread, CancellationToken token)
        {
            predictor = new CascadeClassifier[thread];
            List<UniTask> tasks = new List<UniTask>();
            try
            {
                for (var i = 0; i < thread; i++)
                {
                    tasks.Add(LoadAsync(i));
                }
                await UniTask.WhenAll(tasks);
            }
            catch (Exception)
            {
                throw;
            }

            if (isDebug.Value && isDebugImage && debugImage != null)
            {
                debugImage.SetAlphaData(Id, markColor.a);
                color = new Scalar(markColor.b * 255, markColor.g * 255, markColor.r * 255);
            }

            isAvailable.Value = true;
        }

        private async UniTask LoadAsync(int thread)
        {
            await UniTask.SwitchToThreadPool();
            predictor[thread] = new CascadeClassifier();
            if (!predictor[thread].Load(filePath))
            {
                new ArgumentException("カスケード分類のファイルを読み込めませんでした。");
            }
        }

        /// <inheritdoc/>
        protected override SchedulableData<BoundaryBox> ProcessInternal(SchedulableData<Mat> input)
        {
            try
            {
                if (!input.IsSuccess || input.IsSignal)
                {
                    return new SchedulableData<BoundaryBox>(input, default);
                }

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
                        OpenCvSharp.Rect[] box = predictor[thread].DetectMultiScale(input.Data);
                        if (box.Length == 0)
                        {
                            return new SchedulableData<BoundaryBox>(input, default, false, true, errorMessage: "顔が検出されませんでした。");
                        }
                        var bb = new BoundaryBox(input.Data, new UnityEngine.Rect(box[0].Left, box[0].Top, box[0].Width, box[0].Height));
                        return new SchedulableData<BoundaryBox>(input, bb);
                    }
                    finally
                    {
                        FreeThread(thread);
                    }
                }
                else
                {
                    return new SchedulableData<BoundaryBox>(input, default, false, true, errorMessage: "スレッドを確保できませんでした。");
                }
            }
            finally
            {
                ResourceManager.DisposeIfRelease(input.Data, Id);
            }
        }

        /// <inheritdoc/>
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

                if (isDebugImage && debugImage != null)
                {
                    if (skipCount < debugInterval)
                    {
                        skipCount++;
                    }
                    else
                    {
                        var mat = new Mat(data.Data.ImageSize.y, data.Data.ImageSize.x, MatType.CV_8UC3);
                        var rect = new OpenCvSharp.Rect((int)data.Data.Box.x, (int)data.Data.Box.y, (int)data.Data.Box.width, (int)data.Data.Box.height);
                        mat.Rectangle(rect, color, markSize);
                        debugImage.SetImage(Id, mat);
                        skipCount = 0;
                    }
                }
            }
            return new DebugMessage(data, message);
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            foreach (CascadeClassifier p in predictor)
            {
                if (p.IsEnabledDispose)
                {
                    p.Dispose();
                }
            }
        }
    }
}
