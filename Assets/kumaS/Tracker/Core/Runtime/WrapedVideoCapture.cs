using Cysharp.Threading.Tasks;

using OpenCvSharp;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using UnityEngine;

namespace kumaS.Tracker.Core
{
    public class WrapedVideoCapture : IVideo
    {
        private VideoCapture cvVideo;
        private bool sendSame = false;
        private Mat cashed = default;
        private bool isLooping = false;
        private bool isCamera;
        private int sendCount = 0;
        private CancellationTokenSource source = new CancellationTokenSource();
        private CancellationToken outerToken;

        public bool IsPrepared { get => cvVideo.IsOpened(); }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="filePath">読み込む動画ファイル。</param>
        /// <param name="sendSameFrame">同じフレームを画像として送るか。</param>
        /// <param name="token">停止しようとしたかの通知。</param>
        public WrapedVideoCapture(string filePath, bool sendSameFrame, CancellationToken token)
        {
            sendSame = sendSameFrame;
            isCamera = false;
            outerToken = token;
            cashed = new Mat();
            cvVideo = new VideoCapture();
            cvVideo.SetExceptionMode(true);
            cvVideo.Open(filePath);
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="cameraIndex">使うカメラのインデックス。</param>
        /// <param name="requestFps">使いたいFPS。（このFPSにならない場合も。）</param>
        /// <param name="requestHeight">使いたい画像の高さ。</param>
        /// <param name="requestWidth">使いたい画像の横幅。</param>
        /// <param name="sendSameFrame">同じフレームを画像として送るか。</param>
        /// <param name="token">停止しようとしたかの通知。</param>
        public WrapedVideoCapture(int cameraIndex, int requestFps, int requestHeight, int requestWidth, bool sendSameFrame, CancellationToken token)
        {
            sendSame = sendSameFrame;
            isCamera = true;
            outerToken = token;
            cashed = new Mat();
            cvVideo = new VideoCapture();
            cvVideo.SetExceptionMode(true);
            cvVideo.Open(cameraIndex);
            cvVideo.Fps = requestFps;
            cvVideo.FrameWidth = requestWidth;
            cvVideo.FrameHeight = requestHeight;
        }

        public void Dispose()
        {
            source.Cancel();

            if (cvVideo.IsEnabledDispose)
            {
                cvVideo.Dispose();
            }

            if (cashed.IsEnabledDispose)
            {
                cashed.Dispose();
            }
        }

        public async UniTask<Mat> Read()
        {
            await UniTask.SwitchToThreadPool();
            if (!isLooping)
            {
                var waitTime = isCamera ? 0 : (int)(1000 / cvVideo.Fps);
                GetMatLoop(waitTime, outerToken, source.Token).Forget();
                isLooping = true;
            }

            lock (cashed)
            {
                if (cashed.Empty())
                {
                    throw new IndexOutOfRangeException("動画は範囲外です。");
                }
            }

            if (!sendSame && sendCount != 0)
            {
                return default;
            }

            lock (cashed)
            {
                ++sendCount;
                return cashed.Clone();
            }
        }

        private async UniTask GetMatLoop(int waitTime, CancellationToken token0, CancellationToken token1)
        {
            await UniTask.SwitchToThreadPool();
            while (!(token0.IsCancellationRequested || token1.IsCancellationRequested))
            {
                var next = new Mat();
                try
                {
                    cvVideo.Read(next);
                }
                catch (Exception)
                {
                    next = new Mat();
                }

                lock (cashed)
                {
                    if (cashed.IsEnabledDispose)
                    {
                        cashed.Dispose();
                    }
                    cashed = next;
                    sendCount = 0;
                }

                if (waitTime != 0)
                {
                    if (cvVideo.FrameCount <= cvVideo.PosFrames)
                    {
                        if (cashed.IsEnabledDispose)
                        {
                            cashed.Dispose();
                        }
                        cashed = new Mat();
                        break;
                    }
                    await UniTask.Delay(waitTime);
                }
            }
        }
    }
}
