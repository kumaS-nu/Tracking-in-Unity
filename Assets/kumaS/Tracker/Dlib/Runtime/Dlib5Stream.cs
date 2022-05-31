using Cysharp.Threading.Tasks;

using DlibDotNet;

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
    /// Dlibの5つの顔の特徴点を検出するストリーム。
    /// </summary>
    public sealed class Dlib5Stream : ScheduleStreamBase<BoundaryBox, Dlib5Landmarks>
    {
        [SerializeField]
        internal string filePath;

        [SerializeField]
        internal int pathType = 1;

        [SerializeField]
        internal bool isDebugPoint = true;

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

        [SerializeField]
        internal bool isDebugIndex = true;

        [SerializeField]
        internal double fontScale = 1;

        public override string ProcessName { get; set; } = "Dlib 5 landmark detector";
        public override Type[] UseType { get; } = new Type[] { typeof(Mat) };
        public override string[] DebugKey { get => debugKey; }

        private string[] debugKey = default;
        public override IReadOnlyReactiveProperty<bool> IsAvailable { get => isAvailable; }

        private readonly ReactiveProperty<bool> isAvailable = new ReactiveProperty<bool>(false);

        private ShapePredictor[] predictor;
        private volatile int skipped = 0;
        private Rectangle box = default;
        private readonly object boxLock = new object();

        private readonly string Image_Width = nameof(Image_Width);
        private readonly string Image_Height = nameof(Image_Height);
        private string[] Point_X = default;
        private string[] Point_Y = default;
        private Scalar color;
        private int skipCount = 0;

        /// <inheritdoc/>
        protected override async void InitInternal(int thread, CancellationToken token)
        {
            predictor = new ShapePredictor[thread];
            List<UniTask> tasks = new List<UniTask>();
            for (var i = 0; i < thread; i++)
            {
                tasks.Add(LoadAsync(i));
            }
            var key = new List<string>
            {
                SchedulableData<object>.Elapsed_Time,
                Image_Width,
                Image_Height
            };
            var px = new List<string>();
            var py = new List<string>();
            for (var i = 0; i < 5; i++)
            {
                px.Add("Point" + i + "_X");
                key.Add("Point" + i + "_X");
                py.Add("Point" + i + "_Y");
                key.Add("Point" + i + "_Y");
            }
            Point_X = px.ToArray();
            Point_Y = py.ToArray();
            debugKey = key.ToArray();

            if (isDebug.Value && isDebugImage && debugImage != null)
            {
                debugImage.SetAlphaData(Id, markColor.a);
                color = new Scalar(markColor.b * 255, markColor.g * 255, markColor.r * 255);
            }

            await UniTask.WhenAll(tasks);
            isAvailable.Value = true;
        }

        private async UniTask LoadAsync(int thread)
        {
            await UniTask.SwitchToThreadPool();
            predictor[thread] = ShapePredictor.Deserialize(filePath);
        }

        /// <inheritdoc/>
        protected override IDebugMessage DebugLogInternal(SchedulableData<Dlib5Landmarks> data)
        {
            var message = new Dictionary<string, string>();
            data.ToDebugElapsedTime(message);
            if (data.IsSuccess && !data.IsSignal)
            {
                message[Image_Width] = data.Data.ImageSize.x.ToString();
                message[Image_Height] = data.Data.ImageSize.y.ToString();
                if (isDebugPoint)
                {
                    for (var i = 0; i < 5; i++)
                    {
                        message[Point_X[i]] = data.Data.Landmarks[i].X.ToString();
                        message[Point_Y[i]] = data.Data.Landmarks[i].Y.ToString();
                    }
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
                        for (var i = 0; i < 5; i++)
                        {
                            var point = new OpenCvSharp.Point(data.Data.Landmarks[i].X, data.Data.Landmarks[i].Y);
                            mat.Circle(point, markSize, color, -1);
                            if (isDebugIndex)
                            {
                                mat.PutText((i + 1).ToString(), point, HersheyFonts.HersheyComplex, fontScale, color);
                            }
                        }
                        debugImage.SetImage(Id, mat);
                        skipCount = 0;
                    }
                }
            }
            return new DebugMessage(data, message);
        }

        /// <inheritdoc/>
        protected override SchedulableData<Dlib5Landmarks> ProcessInternal(SchedulableData<BoundaryBox> input)
        {
            try
            {
                if (!input.IsSuccess || input.IsSignal)
                {
                    return new SchedulableData<Dlib5Landmarks>(input, default);
                }

                Rectangle rect = default;
                if (input.Data.Box == default)
                {
                    if (box == default)
                    {
                        if (input.Data.OriginalImage.IsEnabledDispose)
                        {
                            input.Data.OriginalImage.Dispose();
                        }
                        return new SchedulableData<Dlib5Landmarks>(input, default, false, true, errorMessage: "BoundaryBoxがまだ来ていません。");
                    }
                    lock (boxLock)
                    {
                        rect = box;
                    }
                }
                else
                {
                    rect = input.Data.Box.ToRectangle();
                    lock (boxLock)
                    {
                        box = rect;
                    }
                }

                if (skipped < interval)
                {
                    Interlocked.Increment(ref skipped);
                    return new SchedulableData<Dlib5Landmarks>(input, new Dlib5Landmarks(input.Data.OriginalImage, default));
                }

                if (TryGetThread(out var thread))
                {
                    Interlocked.Exchange(ref skipped, 0);
                    try
                    {
                        using (Array2D<BgrPixel> image = DlibDotNet.Dlib.LoadImageData<BgrPixel>(input.Data.OriginalImage.Data, (uint)input.Data.ImageSize.y, (uint)input.Data.ImageSize.x, (uint)input.Data.ImageSize.x))
                        {
                            var points = new DlibDotNet.Point[5];

                            using (FullObjectDetection shapes = predictor[thread].Detect(image, rect))
                            {
                                for (uint i = 0; i < 5; i++)
                                {
                                    points[i] = shapes.GetPart(i);
                                }
                            }
                            var b = DlibExtentions.GetRect(points[4], points[0], points[2]).ToRectangle();
                            lock (boxLock)
                            {
                                box = b;
                            }

                            var ret = new Dlib5Landmarks(input.Data.OriginalImage, points);
                            return new SchedulableData<Dlib5Landmarks>(input, ret);
                        }
                    }
                    finally
                    {
                        FreeThread(thread);
                    }
                }
                else
                {
                    if (input.Data.OriginalImage.IsEnabledDispose)
                    {
                        input.Data.OriginalImage.Dispose();
                    }
                    return new SchedulableData<Dlib5Landmarks>(input, default, false, true, errorMessage: "スレッドを確保できませんでした。");
                }
            }
            finally
            {
                ResourceManager.DisposeIfRelease(input.Data.OriginalImage, Id);
            }
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            foreach (ShapePredictor p in predictor)
            {
                if (p.IsEnableDispose)
                {
                    p.Dispose();
                }
            }
        }
    }
}
