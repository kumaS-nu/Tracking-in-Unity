using Cysharp.Threading.Tasks;

using OpenCvSharp;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

using UniRx;

using Unity.Collections;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Video;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// 画像を取得する。
    /// </summary>
    public sealed class MatSource : ScheduleSourceBase<Mat>
    {
        [SerializeField]
        internal bool useUnity = false;

        [SerializeField]
        internal bool isFile = false;

        [SerializeField]
        internal int cameraIndex = 0;

        [SerializeField]
        internal string filePath = default;

        [SerializeField]
        internal int pathType = 1;

        [SerializeField]
        internal bool isDebugSize = true;

        public override string[] DebugKey { get; } = new string[] { SchedulableData<object>.Elapsed_Time, nameof(Data_Pointer), nameof(Width), nameof(height) };
        public override IReadOnlyReactiveProperty<bool> IsAvailable { get => isAvailable; }
        public override string ProcessName { get; set; } = "Mat source";

        private readonly ReactiveProperty<bool> isAvailable = new ReactiveProperty<bool>(false);
        private VideoCapture CvVideo = default;
        private WebCamTexture unityWebcam;
        private VideoPlayer unityVideo;
        private int width;
        private int height;

        private readonly string Data_Pointer = nameof(Data_Pointer);
        private readonly string Width = nameof(Width);
        private readonly string Height = nameof(Height);

        private void Awake()
        {
            if (useUnity)
            {
                if (isFile)
                {
                    unityVideo = GetComponent<VideoPlayer>();
                    if (unityVideo == null)
                    {
                        unityVideo = gameObject.AddComponent<VideoPlayer>();
                    }
                    unityVideo.url = filePath;
                    unityVideo.renderMode = VideoRenderMode.APIOnly;
                    Observable.EveryUpdate().First(_ => unityVideo.isPrepared).Subscribe((_ => { unityVideo.Play(); width = (int)unityVideo.width; height = (int)unityVideo.height; isAvailable.Value = true; }));
                }
                else
                {
                    unityWebcam = new WebCamTexture(WebCamTexture.devices[cameraIndex].name);
                    unityWebcam.Play();
                    Observable.EveryUpdate().First(_ => unityWebcam.GetPixel(0, 0).r > 0.0001f).Subscribe((_ => { width = unityWebcam.width; height = unityWebcam.height; isAvailable.Value = true; }));
                }
            }
            else
            {
                if (isFile)
                {
                    CvVideo = new VideoCapture(filePath);
                }
                else
                {
                    CvVideo = new VideoCapture(cameraIndex);
                }
                Observable.EveryUpdate().First(_ => CvVideo.IsOpened()).Subscribe(_ => isAvailable.Value = true);
            }
        }

        protected override IDebugMessage DebugLogInternal(SchedulableData<Mat> data)
        {
            var msg = new Dictionary<string, string>();
            data.ToDebugElapsedTime(msg);
            if (data.IsSuccess)
            {
                if (!data.Data.IsDisposed)
                {
                    msg[Data_Pointer] = data.Data.Data.ToInt64().ToString();
                    if (isDebugSize)
                    {
                        msg[Width] = data.Data.Width.ToString();
                        msg[Height] = data.Data.Height.ToString();
                    }
                }
            }
            return new DebugMessage(data, msg);
        }

        protected override async UniTask<SchedulableData<Mat>> SourceInternal(DateTime startTime, CancellationToken token)
        {
            if (!useUnity)
            {
                token.ThrowIfCancellationRequested();
                var ret = new Mat();
                var isSuccess = CvVideo.Read(ret);
                var message = isSuccess ? "" : "フレームを取得できませんでした。";
                return new SchedulableData<Mat>(ret, Id, startTime, isSuccess, message);
            }
            else
            {
                await UniTask.SwitchToMainThread();

                token.ThrowIfCancellationRequested();
                AsyncGPUReadbackRequest request = default;
                if (isFile)
                {
                    request = AsyncGPUReadback.Request(unityVideo.texture);
                }
                else
                {
                    request = AsyncGPUReadback.Request(unityWebcam);
                }
                await request;
                NativeArray<Color32> data = request.GetData<Color32>();

                await UniTask.SwitchToThreadPool();

                Mat ret = Color32ToMat(data.ToArray());
                var isSuccess = ret.Width * ret.Height == data.Length;
                var message = isSuccess ? "" : "フレームを取得できませんでした。";
                return new SchedulableData<Mat>(ret, Id, startTime, isSuccess, message);
            }
        }

        private Mat Color32ToMat(Color32[] data)
        {
            var mat = new Mat(height, width, MatType.CV_8UC3);
            var byteData = new byte[width * height * 3];
            for (var h = 0; h < height; h++)
            {
                for (var w = 0; w < width; w++)
                {
                    var colorIndex = data.Length - (h * width + width - w);
                    var byteIndex = 3 * (h * width + w);
                    byteData[byteIndex] = data[colorIndex].b;
                    byteData[byteIndex + 1] = data[colorIndex].g;
                    byteData[byteIndex + 2] = data[colorIndex].r;
                }
            }
            Marshal.Copy(byteData, 0, mat.Data, width * height * 3);
            return mat;
        }

        private void OnDestroy()
        {
            if (unityVideo != null)
            {
                unityVideo.Stop();
            }

            if (unityWebcam != null)
            {
                unityWebcam.Stop();
            }

            if (CvVideo.IsEnabledDispose)
            {
                CvVideo.Dispose();
            }
        }
    }
}
