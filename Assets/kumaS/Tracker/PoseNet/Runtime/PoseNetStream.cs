using Cysharp.Threading.Tasks;

using kumaS.Tracker.Core;

using OpenCvSharp;

using System;
using System.Collections.Generic;
using System.Threading;

using UniRx;

using Unity.Barracuda;

using UnityEngine;

namespace kumaS.Tracker.PoseNet
{
    public sealed class PoseNetStream : ScheduleStreamBase<Mat, PoseNetLandmarks>
    {
        [SerializeField]
        internal string filePath;

        [SerializeField]
        internal int pathType = 1;

        [SerializeField]
        internal int modelType = 0;

        [SerializeField]
        internal int stride = 8;

        [SerializeField]
        internal InterpolationFlags interpolation = InterpolationFlags.Linear;

        [SerializeField]
        internal bool isDefaultInputSize = true;

        [SerializeField]
        internal Vector2Int inputResolution = new Vector2Int(257, 257);

        [SerializeField]
        internal float minScore = 0.5f;

        [SerializeField]
        internal bool isDebugLandmark = true;

        public override string ProcessName { get; set; } = "PoseNet landmark";
        public override Type[] UseType { get; } = new Type[] { typeof(Mat) };
        public override string[] DebugKey { get => debugKey; }

        private string[] debugKey;

        public override IReadOnlyReactiveProperty<bool> IsAvailable { get => isAvailable; }

        private readonly ReactiveProperty<bool> isAvailable = new ReactiveProperty<bool>(false);

        private readonly string Image_Width = nameof(Image_Width);
        private readonly string Image_Height = nameof(Image_Height);
        private string[] Point_X = default;
        private string[] Point_Y = default;

        public static readonly string[] MODEL_TYPE = new string[] { "Mobile Net", "ResNet" };
        public static readonly int[] MODEL_TYPE_INDEX = new int[] { 0, 1 };
        public static readonly int[] MOBILE_NET_STRIDE = new int[] { 8, 16 };
        public static readonly int[] RES_NET_STRIDE = new int[] { 16, 32 };

        private PoseNetBase[] models;


        protected override IDebugMessage DebugLogInternal(SchedulableData<PoseNetLandmarks> data)
        {
            var message = new Dictionary<string, string>();
            data.ToDebugElapsedTime(message);
            if (data.IsSuccess && !data.IsSignal)
            {
                message[Image_Width] = data.Data.ImageSize.x.ToString();
                message[Image_Height] = data.Data.ImageSize.y.ToString();
                if (isDebugLandmark)
                {
                    for (var i = 0; i < 17; i++)
                    {
                        message[Point_X[i]] = data.Data.Landmarks[i].x.ToString();
                        message[Point_Y[i]] = data.Data.Landmarks[i].y.ToString();
                    }
                }
            }
            return new DebugMessage(data, message);
        }

        protected override void InitInternal(int thread, CancellationToken token)
        {
            if (modelType == 0)
            {
                models = new MobileNet[thread];
            }
            else
            {
                models = new ResNet[thread];
            }
            Model model = ModelLoader.Load(filePath);
            Size input = isDefaultInputSize ? default : new Size(inputResolution.x, inputResolution.y);
            for (var i = 0; i < thread; i++)
            {
                if (modelType == 0)
                {
                    models[i] = new MobileNet(model, stride, interpolation, minScore, input);
                }
                else
                {
                    models[i] = new ResNet(model, stride, interpolation, minScore, input);
                }
            }
            MakeDebugKey();
            isAvailable.Value = true;
        }

        private void MakeDebugKey()
        {
            var key = new List<string>
            {
                SchedulableData<object>.Elapsed_Time,
                Image_Width,
                Image_Height
            };
            var px = new List<string>();
            var py = new List<string>();
            for (var i = 0; i < 17; i++)
            {
                px.Add("Point" + i + "_X");
                key.Add("Point" + i + "_X");
                py.Add("Point" + i + "_Y");
                key.Add("Point" + i + "_Y");
            }
            Point_X = px.ToArray();
            Point_Y = py.ToArray();
            debugKey = key.ToArray();
        }


        protected override SchedulableData<PoseNetLandmarks> ProcessInternal(SchedulableData<Mat> input)
        {
            try
            {
                if (!input.IsSuccess || input.IsSignal)  
                {
                    return new SchedulableData<PoseNetLandmarks>(input, default);
                }

                if (TryGetThread(out var thread))
                {
                    try
                    {
                        PoseNetLandmarks pose = models[thread].Execute(input.Data).ToObservable().Wait();
                        if (pose == default)
                        {
                            return new SchedulableData<PoseNetLandmarks>(input, default, false, true, errorMessage: "顔が見つかりませんでした");
                        }

                        return new SchedulableData<PoseNetLandmarks>(input, pose);
                    }
                    finally
                    {
                        FreeThread(thread);
                    }
                }
                else
                {
                    return new SchedulableData<PoseNetLandmarks>(input, default, false, true, errorMessage: "スレッドを確保できませんでした。");
                }
            }
            finally
            {
                ResourceManager.DisposeIfRelease(input.Data, Id);
            }
        }

        public override void Dispose()
        {
            foreach (PoseNetBase model in models)
            {
                model.Dispose();
            }
        }
    }
}
