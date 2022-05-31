using Cysharp.Threading.Tasks;

using OpenCvSharp;

using System.Collections;
using System.Collections.Generic;

using Unity.Collections;
using Unity.Jobs;

using UnityEngine;
using UnityEngine.Rendering;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// UnityのWebカメラを扱うクラスをラップしたクラス。
    /// </summary>
    public class WrapedWebCamTexture : IVideo
    {
        private bool sendSame = false;
        private WebCamTexture unityWebcam;
        private int width;
        private int height;
        private Mat cashed = default;

        public bool IsPrepared { get => unityWebcam.isPlaying; }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="cameraIndex">使うカメラのインデックス。</param>
        /// <param name="requestFps">使いたいFPS。（このFPSにならない場合も。）</param>
        /// <param name="requestHeight">使いたい画像の高さ。</param>
        /// <param name="requestWidth">使いたい画像の横幅。</param>
        /// <param name="sendSameFrame">同じフレームを画像として送るか。</param>
        public WrapedWebCamTexture(int cameraIndex, int requestFps, int requestHeight, int requestWidth, bool sendSameFrame)
        {
            sendSame = sendSameFrame;
            if (sendSame)
            {
                cashed = new Mat();
            }
            Texture.allowThreadedTextureCreation = true;
            unityWebcam = new WebCamTexture(WebCamTexture.devices[cameraIndex].name);
            if (requestFps > 0)
            {
                unityWebcam.requestedFPS = requestFps;
            }
            if (requestHeight > 0 && requestWidth > 0)
            {
                unityWebcam.requestedHeight = requestHeight;
                unityWebcam.requestedWidth = requestWidth;
            }
            unityWebcam.Play();
        }

        public void Dispose()
        {
            unityWebcam.Stop();
            if (cashed != null && cashed.IsEnabledDispose)
            {
                cashed.Dispose();
            }
        }

        public async UniTask<Mat> Read()
        {
            await UniTask.SwitchToMainThread();

            if (unityWebcam.didUpdateThisFrame)
            {
                var mat = await GetMat();
                await UniTask.SwitchToThreadPool();
                if (sendSame)
                {
                    if (cashed != null && cashed.IsEnabledDispose)
                    {
                        cashed.Dispose();
                    }
                    cashed = mat.Clone();
                }
                return mat;
            }

            if (sendSame)
            {
                return cashed.Clone();
            }

            return default;
        }

        /// <summary>
        /// 画像を取得。
        /// </summary>
        /// <returns>画像。</returns>
        private async UniTask<Mat> GetMat()
        {
            var request = AsyncGPUReadback.Request(unityWebcam);
            await request;
            var data = request.GetData<Color32>();

            await UniTask.SwitchToThreadPool();
            Mat ret = new Mat(height, width, MatType.CV_8UC3);
            var job = new Color32ToMat()
            {
                Input = data,
                mat = ret.Data,
                height_ = height,
                width_ = width
            };
            await job.Schedule();
            return ret;
        }
    }
}
