using Cysharp.Threading.Tasks;

using kumaS.Tracker.Core;

using OpenCvSharp;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

using UniRx;

using Unity.Barracuda;
using Unity.Collections;
using Unity.Jobs;

using UnityEngine;

namespace kumaS.Tracker.BlazeFace
{
    /// <summary>
    /// BlazeFaceを使い、バウンダリーボックスを検出する。
    /// </summary>
    public sealed class BlazeFaceStream : ScheduleStreamBase<Mat, BlazeFaceLandmarks>
    {
        [SerializeField]
        internal string filePath;

        [SerializeField]
        internal int pathType = 1;

        [SerializeField]
        internal InterpolationFlags interpolation = InterpolationFlags.Linear;

        [SerializeField]
        internal float minScore = 0.5f;

        [SerializeField]
        internal bool isDebugLandmark = true;

        [SerializeField]
        internal int interval = 0;

        public override string ProcessName { get; set; } = "BlazeFace landmark predict";
        public override Type[] UseType { get; } = new Type[] { typeof(Mat) };
        public override string[] DebugKey { get => debugKey; }

        private string[] debugKey = default;
        public override IReadOnlyReactiveProperty<bool> IsAvailable { get => IsAvailable; }

        private readonly ReactiveProperty<bool> isAvailable = new ReactiveProperty<bool>(false);

        private readonly string Image_Width = nameof(Image_Width);
        private readonly string Image_Height = nameof(Image_Height);
        private string[] Point_X = default;
        private string[] Point_Y = default;
        private readonly string X = nameof(X);
        private readonly string Y = nameof(Y);
        private readonly string Width = nameof(Width);
        private readonly string Height = nameof(Height);
        private readonly string Angle = nameof(Angle);

        private const int strideBound = 128 * 128 / 8 / 8 * 2;
        private Normalizer[] normalizers;
        private IWorker[] workers;
        private Mat[] data;
        private float minScoreInternal = 0;
        private volatile int skipped = 0;

        protected override void InitInternal(int thread, CancellationToken token)
        {
            minScoreInternal = -Mathf.Log(1 / minScore - 1);
            normalizers = new Normalizer[thread];
            workers = new IWorker[thread];
            data = new Mat[thread];
            Model model = ModelLoader.Load(filePath);
            for (var i = 0; i < thread; i++)
            {
                normalizers[i] = new Normalizer
                {
                    Length = 128 * 128,
                    Output = new NativeArray<float>(128 * 128 * 3, Allocator.Persistent)
                };
                workers[i] = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, model);
                data[i] = new Mat();
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
                Image_Height,
                X,
                Y,
                Width,
                Height,
                Angle
            };
            var px = new List<string>();
            var py = new List<string>();
            for (var i = 0; i < 6; i++)
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

        protected override IDebugMessage DebugLogInternal(SchedulableData<BlazeFaceLandmarks> data)
        {
            var message = new Dictionary<string, string>();
            data.ToDebugElapsedTime(message);
            if (data.IsSuccess && !data.IsSignal)
            {
                message[Image_Width] = data.Data.ImageSize.x.ToString();
                message[Image_Height] = data.Data.ImageSize.y.ToString();
                if (isDebugLandmark && data.Data.Box != null)
                {
                    message[X] = data.Data.Box.x.ToString();
                    message[Y] = data.Data.Box.y.ToString();
                    message[Width] = data.Data.Box.width.ToString();
                    message[Height] = data.Data.Box.height.ToString();
                    message[Angle] = data.Data.Angle.ToString();
                    for (var i = 0; i < 6; i++)
                    {
                        message[Point_X[i]] = data.Data.Landmarks[i].x.ToString();
                        message[Point_Y[i]] = data.Data.Landmarks[i].y.ToString();
                    }
                }
            }
            return new DebugMessage(data, message);
        }

        protected override SchedulableData<BlazeFaceLandmarks> ProcessInternal(SchedulableData<Mat> input)
        {
            try
            {
                if (skipped < interval || !input.IsSuccess)
                {
                    Interlocked.Increment(ref skipped);
                    return new SchedulableData<BlazeFaceLandmarks>(input, default);
                }

                if (TryGetThread(out var thread))
                {
                    Interlocked.Exchange(ref skipped, 0);
                    try
                    {
                        if (data[thread].IsEnabledDispose)
                        {
                            data[thread].Dispose();
                        }
                        data[thread] = new Mat();
                        Cv2.Resize(input.Data, data[thread], new Size(128, 128), interpolation: interpolation);
                        return ProcessInternalAsync(input, thread).ToObservable().Wait();
                    }
                    finally
                    {
                        FreeThread(thread);
                    }
                }

                return new SchedulableData<BlazeFaceLandmarks>(input, default, false, true, errorMessage: "スレッドを確保できませんでした。");
            }
            finally
            {
                ResourceManager.DisposeIfRelease(input.Data, Id);
            }
        }

        private async UniTask<SchedulableData<BlazeFaceLandmarks>> ProcessInternalAsync(SchedulableData<Mat> input, int thread)
        {
            await UniTask.SwitchToMainThread();
            normalizers[thread].Input = data[thread].Data;
            await normalizers[thread].Schedule();
            var tensor = new Tensor(1, 128, 128, 3, normalizers[thread].Output.ToArray());
            workers[thread].Execute(tensor);
            Tensor output = workers[thread].PeekOutput();
            await UniTask.SwitchToThreadPool();
            var max = float.MinValue;
            var index = -1;
            for (var i = 0; i < 896; i++)
            {
                if (max < output[17 * i])
                {
                    index = i;
                }
            }

            if (max < minScoreInternal || index == -1)
            {
                return new SchedulableData<BlazeFaceLandmarks>(input, default, false, true, errorMessage: "顔が見つかりませんでした");
            }

            float anchorY;
            float anchorX;
            if (index < strideBound)
            {
                anchorX = 8 * (index % 32 / 2 + 0.5f);
                anchorY = 8 * (index / 32 + 0.5f);
            }
            else
            {
                var idx = index - strideBound;
                anchorX = 16 * (idx % 48 / 6 + 0.5f);
                anchorY = 16 * (idx / 48 + 0.5f);
            }

            var centerX = output[17 * index + 1] + anchorX;
            var centerY = output[17 * index + 2] + anchorY;
            var width = output[17 * index + 3];
            var height = output[17 * index + 4];
            var scaleX = input.Data.Width / 128.0f;
            var scaleY = input.Data.Height / 128.0f;
            var landmarks = new Vector2[6];
            for (var i = 0; i < 6; i++)
            {
                landmarks[i] = new Vector2(output[17 * index + i * 2 + 5] + anchorX, output[17 * index + i * 2 + 6]);
            }
            var angle = Mathf.Atan2(landmarks[3].x - landmarks[2].x, landmarks[3].y - landmarks[2].x);
            var ret = new BlazeFaceLandmarks(input.Data, landmarks, new UnityEngine.Rect((centerX - width / 2) * scaleX, (centerY - height / 2) * scaleY, width * scaleX, height * scaleY), angle);
            return new SchedulableData<BlazeFaceLandmarks>(input, ret);
        }

        public override void Dispose()
        {
            foreach (Normalizer normalizer in normalizers)
            {
                normalizer.Output.Dispose();
            }

            foreach (var d in data)
            {
                d.Dispose();
            }

            foreach (IWorker worker in workers)
            {
                worker.Dispose();
            }
        }
    }
}
