using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using kumaS.Tracker.Core;
using System;
using UniRx;
using OpenCvSharp;
using DlibDotNet;

namespace kumaS.Tracker.Dlib
{
    /// <summary>
    /// Dlibの5つの顔の特徴点を検出するストリーム。
    /// </summary>
    public class Dlib5Stream : ScheduleStreamBase<BoundaryBox, Dlib5Landmarks>
    {
        [SerializeField]
        internal string filePath;

        [SerializeField]
        internal int pathType = 1;

        [SerializeField]
        internal bool isDebugPoint = true;

        public override string ProcessName { get; set; } = "Dlib 5 landmark detector";
        public override Type[] UseType { get; } = new Type[] { typeof(Mat) };
        public override string[] DebugKey { get => debugKey; }

        private string[] debugKey = default;
        public override IReadOnlyReactiveProperty<bool> IsAvailable { get => isAvailable; }

        private ReactiveProperty<bool> isAvailable = new ReactiveProperty<bool>(false);

        private ShapePredictor[] predictor;

        private readonly string Image_Width = nameof(Image_Width);
        private readonly string Image_Height = nameof(Image_Height);
        private string[] Point_X = default;
        private string[] Point_Y = default;

        public override void InitInternal(int thread)
        {
            predictor = new ShapePredictor[thread];
            for(var i = 0; i < thread; i++)
            {
                predictor[i] = ShapePredictor.Deserialize(filePath);
            }
            var key = new List<string>();
            key.Add(SchedulableData<object>.Elapsed_Time);
            key.Add(Image_Width);
            key.Add(Image_Height);
            var px = new List<string>();
            var py = new List<string>();
            for(var i = 0; i < 5; i++)
            {
                px.Add("Point" + i + "_X");
                key.Add("Point" + i + "_X");
                py.Add("Point" + i + "_Y");
                key.Add("Point" + i + "_Y");
            }
            Point_X = px.ToArray();
            Point_Y = py.ToArray();
            debugKey = key.ToArray();

            isAvailable.Value = true;
        }

        protected override IDebugMessage DebugLogInternal(SchedulableData<Dlib5Landmarks> data)
        {
            var message = new Dictionary<string, string>();
            data.ToDebugElapsedTime(message);
            if (data.IsSuccess)
            {
                message[Image_Width] = data.Data.ImageSize.x.ToString();
                message[Image_Height] = data.Data.ImageSize.y.ToString();
                if (isDebugPoint)
                {
                    for(var i = 0; i < 5; i++)
                    {
                        message[Point_X[i]] = data.Data.Landmarks[i].X.ToString();
                        message[Point_Y[i]] = data.Data.Landmarks[i].Y.ToString();
                    }
                }
            }
            return new DebugMessage(data, message);
        }

        protected override SchedulableData<Dlib5Landmarks> ProcessInternal(SchedulableData<BoundaryBox> input)
        {
            try
            {
                if (!input.IsSuccess)
                {
                    return new SchedulableData<Dlib5Landmarks>(input, default);
                }

                if (TryGetThread(out var thread))
                {
                    try
                    {
                        var origin = input.Data.OriginalImage;
                        using (var image = DlibDotNet.Dlib.LoadImageData<BgrPixel>(origin.Data, (uint)origin.Height, (uint)origin.Width, (uint)(origin.Width * origin.ElemSize())))
                        {
                            DlibDotNet.Point[] points = new DlibDotNet.Point[5];
                            using (FullObjectDetection shapes = predictor[thread].Detect(image, new Rectangle((int)input.Data.Box.x, (int)input.Data.Box.y, (int)input.Data.Box.xMax, (int)input.Data.Box.yMax)))
                            {
                                for (uint i = 0; i < 5; i++)
                                {
                                    points[i] = shapes.GetPart(i);
                                }
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
                    return new SchedulableData<Dlib5Landmarks>(input, default, false, "スレッドを確保できませんでした。");
                }
            }
            finally
            {
                if(ResourceManager.isRelease(typeof(Mat), Id))
                {
                    input.Data.OriginalImage.Dispose();
                }
            }
        }
    }
}
