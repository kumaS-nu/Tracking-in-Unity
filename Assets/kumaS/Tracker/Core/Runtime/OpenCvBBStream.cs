using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using System;
using UniRx;
using Cysharp.Threading.Tasks;

namespace kumaS.Tracker.Core
{
    public class OpenCvBBStream : ScheduleStreamBase<Mat, BoundaryBox>
    {
        [SerializeField]
        internal string filePath = "";

        [SerializeField]
        internal int pathType = 1;

        [SerializeField]
        internal bool isDebugBox = true;

        private CascadeClassifier[] predictor;

        public override string ProcessName { get; set; } = "OpenCvSharp BB predict";
        public override Type[] UseType { get; } = new Type[] { typeof(Mat) };
        public override string[] DebugKey { get; } = new string[] { nameof(Elapsed_Time), nameof(Image_Width), nameof(Image_Height), nameof(X), nameof(Y), nameof(Width), nameof(Height) };
        public override IReadOnlyReactiveProperty<bool> IsAvailable { get => isAvailable; }

        private ReactiveProperty<bool> isAvailable = new ReactiveProperty<bool>(false);

        private readonly string Elapsed_Time = nameof(Elapsed_Time);
        private readonly string Image_Width = nameof(Image_Width);
        private readonly string Image_Height = nameof(Image_Height);
        private readonly string X = nameof(X);
        private readonly string Y = nameof(Y);
        private readonly string Width = nameof(Width);
        private readonly string Height = nameof(Height);

        public override void InitInternal(int thread)
        {
            predictor = new CascadeClassifier[thread];
            for(var i = 0; i < thread; i++)
            {
                predictor[i] = new CascadeClassifier();
                if (!predictor[i].Load(filePath)){
                    throw new ArgumentException("OpenCVのカスケード分類機のファイルを読み込めませんでした。");
                }
            }
            isAvailable.Value = true;
        }

        protected override SchedulableData<BoundaryBox> ProcessInternal(SchedulableData<Mat> input)
        {
            try
            {
                if (!input.IsSuccess)
                {
                    return new SchedulableData<BoundaryBox>(input, null);
                }

                if (TryGetThread(out var thread))
                {
                    try
                    {
                        var box = predictor[thread].DetectMultiScale(input.Data);
                        if (box.Length == 0)
                        {
                            return new SchedulableData<BoundaryBox>(input, null, false, "顔が検出されませんでした。");
                        }
                        var bb = new BoundaryBox(input.Data, new UnityEngine.Rect(box[0].Left, box[0].Top, box[0].Width, box[0].Height));
                        return new SchedulableData<BoundaryBox>(input, bb);
                    }
                    finally
                    {
                        FreeThread(thread);
                    }
                }
                else
                {
                    return new SchedulableData<BoundaryBox>(input, null, false, "スレッドを確保できませんでした。");
                }
            }
            finally
            {
                if (ResourceManager.isRelease(typeof(Mat), Id))
                {
                    input.Data.Dispose();
                }
            }
        }

        protected override IDebugMessage DebugLogInternal(SchedulableData<BoundaryBox> data)
        {
            var message = new Dictionary<string, string>();
            message[Elapsed_Time] = data.ElapsedTimes[data.ElapsedTimes.Count - 1].TotalMilliseconds.ToString("F") + "ms";
            if (data.IsSuccess)
            {
                message[Image_Width] = data.Data.ImageSize.x.ToString();
                message[Image_Height] = data.Data.ImageSize.y.ToString();
                if (isDebugBox)
                {
                    message[X] = data.Data.Box.x.ToString();
                    message[Y] = data.Data.Box.y.ToString();
                    message[Width] = data.Data.Box.width.ToString();
                    message[Height] = data.Data.Box.height.ToString();
                }
            }
            return new DebugMessage(data, message);
        }
    }
}
