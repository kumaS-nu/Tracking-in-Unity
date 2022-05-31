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

using UnityEngine;

namespace kumaS.Tracker.FaceMesh
{
    /// <summary>
    /// フェイスメッシュの特徴点を取得するストリーム。
    /// </summary>
    public sealed class FaceMeshStream : ScheduleStreamBase<BoundaryBox, FaceMeshLandmarks>
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

        public override string ProcessName { get; set; } = "FaceMesh landmark";
        public override Type[] UseType { get; } = new Type[] { typeof(Mat) };
        public override string[] DebugKey { get => debugKey; }

        private string[] debugKey;

        public override IReadOnlyReactiveProperty<bool> IsAvailable { get => isAvailable; }

        private readonly ReactiveProperty<bool> isAvailable = new ReactiveProperty<bool>(false);

        private readonly string Image_Width = nameof(Image_Width);
        private readonly string Image_Height = nameof(Image_Height);
        private string[] Point_X = default;
        private string[] Point_Y = default;
        private string[] Point_Z = default;
        private readonly string X = nameof(X);
        private readonly string Y = nameof(Y);
        private readonly string Width = nameof(Width);
        private readonly string Height = nameof(Height);
        private readonly string Angle = nameof(Angle);
        private const int UNIT = 192;
        private readonly float ROOT2 = Mathf.Sqrt(2);
        private readonly Size SIZE = new Size(192, 192);
        private readonly Point2f[] POINTS = new Point2f[] { new Point2f(0, 0), new Point2f(192, 0), new Point2f(0, 192) };

        private Normalizer[] normalizers;
        private AffineTransform[] affineTransform;
        private IWorker[] workers;
        private Mat[] data;
        private Mat[] affine;
        private BoundaryBox box = default;
        private readonly object boxLock = new object();

        /// <inheritdoc/>
        protected override IDebugMessage DebugLogInternal(SchedulableData<FaceMeshLandmarks> data)
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
                    for (var i = 0; i < 478; i++)
                    {
                        message[Point_X[i]] = data.Data.Landmarks[i].x.ToString();
                        message[Point_Y[i]] = data.Data.Landmarks[i].y.ToString();
                        message[Point_Z[i]] = data.Data.Landmarks[i].z.ToString();
                    }
                }
            }
            return new DebugMessage(data, message);
        }

        /// <inheritdoc/>
        protected override void InitInternal(int thread, CancellationToken token)
        {
            normalizers = new Normalizer[thread];
            affineTransform = new AffineTransform[thread];
            workers = new IWorker[thread];
            data = new Mat[thread];
            affine = new Mat[thread];
            Model model = ModelLoader.Load(filePath);
            for (var i = 0; i < thread; i++)
            {
                normalizers[i] = new Normalizer
                {
                    Length = UNIT * UNIT,
                    Output = new NativeArray<float>(UNIT * UNIT * 3, Allocator.Persistent)
                };
                affineTransform[i].Input = new NativeArray<float>(1404, Allocator.Persistent);
                affineTransform[i].Output = new NativeArray<float>(1404, Allocator.Persistent);
                workers[i] = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, model);
                affine[i] = new Mat();
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
            var pz = new List<string>();
            for (var i = 0; i < 478; i++)
            {
                px.Add("Point" + i + "_X");
                key.Add("Point" + i + "_X");
                py.Add("Point" + i + "_Y");
                key.Add("Point" + i + "_Y");
                pz.Add("Point" + i + "_Z");
                key.Add("Point" + i + "_Z");
            }
            Point_X = px.ToArray();
            Point_Y = py.ToArray();
            Point_Z = pz.ToArray();
            debugKey = key.ToArray();
        }

        /// <inheritdoc/>
        protected override SchedulableData<FaceMeshLandmarks> ProcessInternal(SchedulableData<BoundaryBox> input)
        {
            try
            {
                if (!input.IsSuccess || input.IsSignal)
                {
                    return new SchedulableData<FaceMeshLandmarks>(input, default);
                }

                BoundaryBox bb = default;
                if (input.Data.Box == default)
                {
                    lock (boxLock)
                    {
                        bb = box;
                    }
                }
                else
                {
                    bb = input.Data;
                    lock (boxLock)
                    {
                        if (box != null && box.OriginalImage != null && box.OriginalImage.IsEnabledDispose)
                        {
                            box.OriginalImage.Dispose();
                        }
                        box = bb;
                    }
                }

                if (TryGetThread(out var thread))
                {
                    try
                    {
                        var points = new Point2f[3];
                        var len = bb.Box.width > bb.Box.height ? bb.Box.width / ROOT2 : bb.Box.height / ROOT2;
                        var centerX = bb.Box.center.x;
                        var centerY = bb.Box.center.y;
                        var cos = Mathf.Cos(bb.Angle * Mathf.Deg2Rad + Mathf.PI / 4);
                        var sin = Mathf.Sin(bb.Angle * Mathf.Deg2Rad + Mathf.PI / 4);
                        points[0] = new Point2f(centerX - len * sin, centerY + len * cos);
                        points[1] = new Point2f(centerX + len * cos, centerY + len * sin);
                        points[2] = new Point2f(centerX - len * cos, centerY - len * sin);
                        using (Mat rotateMatrix = Cv2.GetAffineTransform(points, POINTS))
                        {
                            if (data[thread].IsEnabledDispose)
                            {
                                data[thread].Dispose();
                            }
                            if (affine[thread].IsEnabledDispose)
                            {
                                affine[thread].Dispose();
                            }
                            data[thread] = new Mat();
                            Cv2.WarpAffine(bb.OriginalImage, data[thread], rotateMatrix, SIZE, interpolation);
                            affine[thread] = Cv2.GetAffineTransform(POINTS, points);
                            return ProcessInternalAsync(input, thread).ToObservable().Wait();
                        }
                    }
                    finally
                    {
                        FreeThread(thread);
                    }
                }
                else
                {
                    return new SchedulableData<FaceMeshLandmarks>(input, default, false, true, errorMessage: "スレッドを確保できませんでした。");
                }
            }
            finally
            {
                ResourceManager.DisposeIfRelease(input.Data.OriginalImage, Id);
            }
        }

        private async UniTask<SchedulableData<FaceMeshLandmarks>> ProcessInternalAsync(SchedulableData<BoundaryBox> input, int thread)
        {
            await UniTask.SwitchToMainThread();
            normalizers[thread].Input = data[thread].Data;
            normalizers[thread].Execute();
            var tensor = new Tensor(1, UNIT, UNIT, 3, normalizers[thread].Output.ToArray());
            workers[thread].Execute(tensor);
            Tensor coords = workers[thread].PeekOutput("Identity");
            Tensor flags = workers[thread].PeekOutput("Identity_2");

            if (flags[0] < minScore)
            {
                return new SchedulableData<FaceMeshLandmarks>(input, default, false, true, errorMessage: "顔が見つかりませんでした");
            }

            affineTransform[thread].Input.CopyFrom(coords.ToReadOnlyArray());
            affineTransform[thread].Affine = affine[thread].Data;
            affineTransform[thread].Execute();

            var transformedCoords = affineTransform[thread].Output.ToArray();
            var landmarks = new Vector3[478];
            var minX = float.MaxValue;
            var maxX = float.MinValue;
            var minY = float.MaxValue;
            var maxY = float.MinValue;
            for (var i = 0; i < 478; i++)
            {
                var x = transformedCoords[3 * i];
                var y = transformedCoords[3 * i + 1];
                landmarks[i] = new Vector3(x, y, transformedCoords[3 * i + 2]);

                if (minX > x)
                {
                    minX = x;
                }
                if (maxX < x)
                {
                    maxX = x;
                }
                if (minY > y)
                {
                    minY = y;
                }
                if (maxY < y)
                {
                    maxY = y;
                }
            }

            var width = maxX - minX;
            var height = maxY - minY;
            var len = width > height ? width : height;
            var halflen = len / 2;
            var angle = Mathf.Atan2(landmarks[12].x - landmarks[167].x, landmarks[12].y - landmarks[167].x);
            var ret = new FaceMeshLandmarks(input.Data.OriginalImage, landmarks, new UnityEngine.Rect(landmarks[0].x - halflen, landmarks[0].y - halflen, width, height), angle);
            return new SchedulableData<FaceMeshLandmarks>(input, ret);
        }

        /// <inheritdoc/>
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

            foreach (AffineTransform transformer in affineTransform)
            {
                transformer.Input.Dispose();
                transformer.Output.Dispose();
            }

            foreach (var a in affine)
            {
                a.Dispose();
            }

            foreach (IWorker worker in workers)
            {
                worker.Dispose();
            }

            if (box != null && box.OriginalImage != null && box.OriginalImage.IsEnabledDispose)
            {
                box.OriginalImage.Dispose();
            }
        }
    }
}
