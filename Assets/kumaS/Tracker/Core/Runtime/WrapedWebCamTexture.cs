using Cysharp.Threading.Tasks;

using OpenCvSharp;

using System.Collections;
using System.Collections.Generic;

using Unity.Collections;

using UnityEngine;
using UnityEngine.Rendering;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// Unity��Web�J�����������N���X�����b�v�����N���X�B
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
        /// �R���X�g���N�^�B
        /// </summary>
        /// <param name="cameraIndex">�g���J�����̃C���f�b�N�X�B</param>
        /// <param name="requestFps">�g������FPS�B�i����FPS�ɂȂ�Ȃ��ꍇ���B�j</param>
        /// <param name="requestHeight">�g�������摜�̍����B</param>
        /// <param name="requestWidth">�g�������摜�̉����B</param>
        /// <param name="sendSameFrame">�����t���[�����摜�Ƃ��đ��邩�B</param>
        public WrapedWebCamTexture(int cameraIndex, int requestFps, int requestHeight, int requestWidth, bool sendSameFrame)
        {
            sendSame = sendSameFrame;
            if (sendSame)
            {
                cashed = new Mat();
            }
            Texture.allowThreadedTextureCreation = true;
            unityWebcam = new WebCamTexture(WebCamTexture.devices[cameraIndex].name);
            unityWebcam.requestedFPS = requestFps;
            unityWebcam.requestedHeight = requestHeight;
            unityWebcam.requestedWidth = requestWidth;
            unityWebcam.Play();
        }

        public void Dispose()
        {
            unityWebcam.Stop();
            if(cashed != null && cashed.IsEnabledDispose)
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
        /// �摜���擾�B
        /// </summary>
        /// <returns>�摜�B</returns>
        private async UniTask<Mat> GetMat()
        {
            var request = AsyncGPUReadback.Request(unityWebcam);
            await request;
            NativeArray<Color32> data = request.GetData<Color32>();

            await UniTask.SwitchToThreadPool();
            Mat ret = MatConverter.Color32ToMat(data.ToArray(), height, width);
            return ret;
        }
    }
}
