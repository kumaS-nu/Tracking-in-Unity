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
        internal bool sendSameImage = false;

        [SerializeField]
        internal int cameraIndex = 0;

        [SerializeField]
        internal int requestFps = 30;

        [SerializeField]
        internal Vector2Int requestResolution = new Vector2Int(1280, 720);

        [SerializeField]
        internal string filePath = default;

        [SerializeField]
        internal int pathType = 1;

        [SerializeField]
        internal bool isDebugSize = true;

        public override string[] DebugKey { get; } = new string[] { SchedulableData<object>.Elapsed_Time, nameof(Data_Pointer), nameof(Width), nameof(Height) };
        public override IReadOnlyReactiveProperty<bool> IsAvailable { get => isAvailable; }
        public override string ProcessName { get; set; } = "Mat source";

        private readonly ReactiveProperty<bool> isAvailable = new ReactiveProperty<bool>(false);
        private IVideo video;

        private readonly string Data_Pointer = nameof(Data_Pointer);
        private readonly string Width = nameof(Width);
        private readonly string Height = nameof(Height);

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

        protected override async UniTask<SchedulableData<Mat>> SourceInternal(DateTime startTime)
        {
            try
            {
                var mat = await video.Read();
                if (mat == null)
                {
                    return new SchedulableData<Mat>(default, Id, startTime, isSignal: true);
                }
                return new SchedulableData<Mat>(mat, Id, startTime);
            }
            catch (IndexOutOfRangeException e)
            {
                return new SchedulableData<Mat>(default, Id, startTime, false, true, e.Message);
            }
        }

        public override void Init(int thread, CancellationToken token)
        {
            if (useUnity)
            {
                if (isFile)
                {
                    video = new WrapedVideoPlayer(gameObject, filePath, sendSameImage);         
                }
                else
                {
                    video = new WrapedWebCamTexture(cameraIndex, requestFps, requestResolution.y, requestResolution.x, sendSameImage);
                }
            }
            else
            {
                if (isFile)
                {
                    video = new WrapedVideoCapture(filePath, sendSameImage, token);
                }
                else
                {
                    video = new WrapedVideoCapture(cameraIndex, requestFps, requestResolution.y, requestResolution.x, sendSameImage, token);
                }
            }
            Observable.EveryUpdate().First(_ => video.IsPrepared).Subscribe(_ => { isAvailable.Value = true; });
        }

        public override void Dispose()
        {
            if(video != null)
            {
                video.Dispose();
            }
        }
    }
}
