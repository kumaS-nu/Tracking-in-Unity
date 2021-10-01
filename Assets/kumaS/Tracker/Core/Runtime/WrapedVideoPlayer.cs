using Cysharp.Threading.Tasks;

using OpenCvSharp;
using System;
using System.Collections;
using System.Collections.Generic;

using Unity.Collections;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Video;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// Unityの動画ファイルを扱うクラスをラップしたクラス。
    /// </summary>
    public class WrapedVideoPlayer : IVideo
    {
        private bool sendSame = false;
        private VideoPlayer unityVideo;
        private int width;
        private int height;

        private long frame = -1;
        private Mat cashed = default;

        public bool IsPrepared { get => unityVideo.isPrepared; }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="obj">VideoPlayerがついている（つける）ゲームオブジェクト。</param>
        /// <param name="filePath">読み込む動画ファイル。</param>
        /// <param name="sendSameFrame">同じフレームの画像を送るか。</param>
        public WrapedVideoPlayer(GameObject obj, string filePath, bool sendSameFrame)
        {
            sendSame = sendSameFrame;
            if (sendSame)
            {
                cashed = new Mat();
            }
            unityVideo = obj.GetComponent<VideoPlayer>();
            if(unityVideo == null)
            {
                unityVideo = obj.AddComponent<VideoPlayer>();
            }
            unityVideo.url = filePath;
            unityVideo.renderMode = VideoRenderMode.APIOnly;
        }

        public void Dispose()
        {
            unityVideo.Stop();
            if (cashed != null && cashed.IsEnabledDispose)
            {
                cashed.Dispose();
            }
        }

        public async UniTask<Mat> Read()
        {
            await UniTask.SwitchToMainThread();

            if (frame < 0)
            {
                unityVideo.Play();
            }

            if(unityVideo.frameCount <= (ulong)unityVideo.frame)
            {
                throw new IndexOutOfRangeException("動画は終了しました。");
            }

            if(frame != unityVideo.frame)
            {
                frame = unityVideo.frame;
                var mat = await GetMat();
                await UniTask.SwitchToThreadPool();
                if (sendSame)
                {
                    if(cashed != null && cashed.IsEnabledDispose)
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
            var request = AsyncGPUReadback.Request(unityVideo.texture);
            await request;
            NativeArray<Color32> data = request.GetData<Color32>();

            await UniTask.SwitchToThreadPool();
            Mat ret = MatConverter.Color32ToMat(data.ToArray(), height, width);
            return ret;
        }
    }
}
