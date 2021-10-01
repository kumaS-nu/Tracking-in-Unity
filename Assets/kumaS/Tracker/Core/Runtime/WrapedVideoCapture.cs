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
        /// �R���X�g���N�^�B
        /// </summary>
        /// <param name="filePath">�ǂݍ��ޓ���t�@�C���B</param>
        /// <param name="sendSameFrame">�����t���[�����摜�Ƃ��đ��邩�B</param>
        /// <param name="token">��~���悤�Ƃ������̒ʒm�B</param>
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
        /// �R���X�g���N�^�B
        /// </summary>
        /// <param name="cameraIndex">�g���J�����̃C���f�b�N�X�B</param>
        /// <param name="requestFps">�g������FPS�B�i����FPS�ɂȂ�Ȃ��ꍇ���B�j</param>
        /// <param name="requestHeight">�g�������摜�̍����B</param>
        /// <param name="requestWidth">�g�������摜�̉����B</param>
        /// <param name="sendSameFrame">�����t���[�����摜�Ƃ��đ��邩�B</param>
        /// <param name="token">��~���悤�Ƃ������̒ʒm�B</param>
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
                    throw new IndexOutOfRangeException("����͔͈͊O�ł��B");
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
