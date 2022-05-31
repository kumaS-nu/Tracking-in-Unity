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
    /// Dlibの68の顔の特徴点を抽出するストリーム。
    /// </summary>
    public sealed class Dlib68Stream : ScheduleStreamBase<BoundaryBox, Dlib68Landmarks>
    {
        [SerializeField]
        internal string filePath;

        [SerializeField]
        internal int pathType = 1;

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

        [SerializeField]
        internal bool isDebugIndex = true;

        [SerializeField]
        internal double fontScale = 1;

        public override string ProcessName { get; set; } = "Dlib 68 landmark detector";
        public override Type[] UseType { get; } = new Type[] { typeof(Mat) };
        public override string[] DebugKey { get => debugKey; }

        private string[] debugKey = default;
        public override IReadOnlyReactiveProperty<bool> IsAvailable { get => isAvailable; }

        private readonly ReactiveProperty<bool> isAvailable = new ReactiveProperty<bool>(false);

        private ShapePredictor[] predictor;
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
            if(isDebug.Value && isDebugImage && debugImage == null)
            {
                throw new ArgumentNullException("debugImage is null. At " + ProcessName + ".");
            }

            predictor = new ShapePredictor[thread];
            List<UniTask> tasks = new List<UniTask>();
            for (var i = 0; i < thread; i++)
            {
                var t = i;
                tasks.Add(UniTask.RunOnThreadPool(() => Load(t)));
            }
            var key = new List<string>
            {
                SchedulableData<object>.Elapsed_Time,
                Image_Width,
                Image_Height
            };
            var px = new List<string>();
            var py = new List<string>();
            for (var i = 0; i < 68; i++)
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

        private void Load(int thread)
        {
            predictor[thread] = ShapePredictor.Deserialize(filePath);
        }

        /// <inheritdoc/>
        protected override IDebugMessage DebugLogInternal(SchedulableData<Dlib68Landmarks> data)
        {
            var message = new Dictionary<string, string>();
            data.ToDebugElapsedTime(message);
            if (data.IsSuccess && !data.IsSignal)
            {
                message[Image_Width] = data.Data.ImageSize.x.ToString();
                message[Image_Height] = data.Data.ImageSize.y.ToString();
                if (isDebugPoint)
                {
                    for (var i = 0; i < 68; i++)
                    {
                        message[Point_X[i]] = data.Data.Landmarks[i].X.ToString();
                        message[Point_Y[i]] = data.Data.Landmarks[i].Y.ToString();
                    }
                }

                if (isDebugImage && debugImage != null)
                {
                    if (skipCount < interval)
                    {
                        skipCount++;
                    }
                    else
                    {
                        var mat = new Mat(data.Data.ImageSize.y, data.Data.ImageSize.x, MatType.CV_8UC3);
                        for (var i = 0; i < 68; i++)
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
        protected override SchedulableData<Dlib68Landmarks> ProcessInternal(SchedulableData<BoundaryBox> input)
        {
            if (!input.IsSuccess || input.IsSignal)
            {
                return new SchedulableData<Dlib68Landmarks>(input, default);
            }

            try
            {
                Rectangle rect = default;
                if (input.Data.Box == default)
                {
                    if (input.Data.Box == default)
                    {
                        if (input.Data.OriginalImage.IsEnabledDispose)
                        {
                            input.Data.OriginalImage.Dispose();
                        }
                        return new SchedulableData<Dlib68Landmarks>(input, default, false, true, errorMessage: "BoundaryBoxがまだ来ていません。");
                    }
                    lock (boxLock)
                    {
                        rect = box;
                    }
                }
                else
                {
                    rect = new Rectangle((int)input.Data.Box.x, (int)input.Data.Box.y, (int)input.Data.Box.xMax, (int)input.Data.Box.yMax);
                    lock (boxLock)
                    {
                        box = rect;
                    }
                }

                if (TryGetThread(out var thread))
                {
                    try
                    {
                        using (Array2D<BgrPixel> image = DlibDotNet.Dlib.LoadImageData<BgrPixel>(input.Data.OriginalImage.Data, (uint)input.Data.ImageSize.y, (uint)input.Data.ImageSize.x, (uint)input.Data.ImageSize.x))
                        {
                            var points = new DlibDotNet.Point[68];
                            using (FullObjectDetection shapes = predictor[thread].Detect(image, rect))
                            {
                                for (uint i = 0; i < 68; i++)
                                {
                                    points[i] = shapes.GetPart(i);
                                }
                            }
                            var b = DlibExtentions.GetRect(points[33], points[36], points[45]).ToRectangle();
                            lock (boxLock)
                            {
                                box = b;
                            }
                            var ret = new Dlib68Landmarks(input.Data.OriginalImage, points);
                            return new SchedulableData<Dlib68Landmarks>(input, ret);
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
                    return new SchedulableData<Dlib68Landmarks>(input, default, false, true, errorMessage: "スレッドを確保できませんでした。");
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
