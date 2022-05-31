using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using System;
using UniRx;
using System.Threading;
using Cysharp.Threading.Tasks;
using System.Linq;
using Unity.Jobs;
using UnityEngine.UI;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// �f�o�b�O�Ƃ��ĉ摜�Ŋm�F���邽�߂̃X�g���[���B�����ŃJ�����̉摜��\�����A���̃m�[�h���瑗����摜���I�[�o�[���C�ŕ\���B�J�����摜�����T�C�Y���鎞�́A���T�C�Y��ɂ��̃m�[�h�����邱�ƁB
    /// </summary>
    [RequireComponent(typeof(Texture2D))]
    public sealed class DebugImageStream : ScheduleStreamBase<Mat, Mat>
    {
        [SerializeField]
        internal int interval = 2;

        [SerializeField]
        internal Material material;

        public override string ProcessName { get; set; } = "Debug image";
        public override Type[] UseType { get; } = new Type[] { typeof(Mat) };
        public override string[] DebugKey { get; } = new string[] { };
        public override IReadOnlyReactiveProperty<bool> IsAvailable { get; } = new ReactiveProperty<bool>(true);

        private Dictionary<int, Mat> images = new Dictionary<int, Mat>();
        private Dictionary<int, double> alphaData = new Dictionary<int, double>();
        private int skipCount = 0;
        private Texture2D texture;

        /// <summary>
        /// �`�ʂ���摜��ݒ肷��B
        /// </summary>
        /// <param name="id">�m�[�h��id�B</param>
        /// <param name="image">�`�ʂ���摜�B</param>
        public void SetImage(int id, Mat image)
        {
            lock (images)
            {
                if (images.ContainsKey(id))
                {
                    images[id].Dispose();
                }
                images[id] = image;
            }
        }

        /// <summary>
        /// �`�ʂ���摜�̃��l��ݒ肷��B
        /// </summary>
        /// <param name="id">�m�[�h��id�B</param>
        /// <param name="alpha">���l�B(0�`1�B0�ŕ`�ʂȂ��B)</param>
        public void SetAlphaData(int id, double alpha)
        {
            alphaData[id] = alpha;
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            foreach (var i in images)
            {
                if (i.Value.IsEnabledDispose)
                {
                    i.Value.Dispose();
                }
            }
        }

        /// <inheritdoc/>
        protected override IDebugMessage DebugLogInternal(SchedulableData<Mat> data)
        {
            return new DebugMessage(data, new Dictionary<string, string>());
        }

        /// <inheritdoc/>
        protected override void InitInternal(int thread, CancellationToken token)
        {
            if (material == null)
            {
                throw new ArgumentNullException("Output material is null. At " + ProcessName + ".");
            }
        }

        /// <inheritdoc/>
        protected override SchedulableData<Mat> ProcessInternal(SchedulableData<Mat> input)
        {
            if (!input.IsSuccess || input.IsSignal)
            {
                return new SchedulableData<Mat>(input, default);
            }

            if (skipCount < interval)
            {
                skipCount++;
                return new SchedulableData<Mat>(input, input.Data);
            }
            skipCount = 0;
            _ = UniTask.RunOnThreadPool(() => UpdateImage(input.Data));
            return new SchedulableData<Mat>(input, input.Data);
        }

        private async UniTask UpdateImage(Mat data)
        {
            var height = data.Height;
            var width = data.Width;
            var output = data.Clone();
            lock (images)
            {
                foreach (var i in images)
                {
                    output += i.Value * alphaData[i.Key];
                }
            }
            await UniTask.SwitchToMainThread();
            if (texture == null || texture.width != width || texture.height != height)
            {
                texture = new Texture2D(width, height);
                material.mainTexture = texture;
            }
            var job = new MatToColor32()
            {
                mat = output.Data,
                Output = texture.GetPixelData<Color32>(0),
                width_ = width,
                height_ = height
            };
            await job.Schedule();
            texture.Apply();
            output.Dispose();
        }
    }
}
