using kumaS.Tracker.Core;

using OpenCvSharp;

using System;
using System.Collections.Generic;
using System.Threading;

using UniRx;

using UnityEngine;

namespace kumaS.Tracker.BlazeFace
{
    /// <summary>
    /// BlazeFaceの特徴点をバウンダリーボックスに変換するストリーム。
    /// </summary>
    public class BlazeFaceToBBStream : ScheduleStreamBase<BlazeFaceLandmarks, BoundaryBox>
    {
        [SerializeField]
        internal bool isDebugBox = true;

        public override string ProcessName { get; set; } = "BlazeFace to BB";
        public override Type[] UseType { get; } = new Type[] { typeof(Mat) };
        public override string[] DebugKey { get; } = new string[] { SchedulableData<object>.Elapsed_Time, nameof(Image_Width), nameof(Image_Height), nameof(X), nameof(Y), nameof(Width), nameof(Height), nameof(Angle) };
        public override IReadOnlyReactiveProperty<bool> IsAvailable { get; } = new ReactiveProperty<bool>(true);

        private readonly string Image_Width = nameof(Image_Width);
        private readonly string Image_Height = nameof(Image_Height);
        private readonly string X = nameof(X);
        private readonly string Y = nameof(Y);
        private readonly string Width = nameof(Width);
        private readonly string Height = nameof(Height);
        private readonly string Angle = nameof(Angle);

        protected override IDebugMessage DebugLogInternal(SchedulableData<BoundaryBox> data)
        {
            var message = new Dictionary<string, string>();
            data.ToDebugElapsedTime(message);
            if (data.IsSuccess && !data.IsSignal)
            {
                message[Image_Width] = data.Data.ImageSize.x.ToString();
                message[Image_Height] = data.Data.ImageSize.y.ToString();
                if (isDebugBox && data.Data.Box != default)
                {
                    message[X] = data.Data.Box.x.ToString();
                    message[Y] = data.Data.Box.y.ToString();
                    message[Width] = data.Data.Box.width.ToString();
                    message[Height] = data.Data.Box.height.ToString();
                    message[Angle] = data.Data.Angle.ToString();
                }
            }
            return new DebugMessage(data, message);
        }

        protected override void InitInternal(int thread, CancellationToken token) { }

        protected override SchedulableData<BoundaryBox> ProcessInternal(SchedulableData<BlazeFaceLandmarks> input)
        {
            if (!input.IsSuccess || input.IsSignal)
            {
                return new SchedulableData<BoundaryBox>(input, default);
            }
            try
            {
                return new SchedulableData<BoundaryBox>(input, input.Data);
            }
            finally
            {
                ResourceManager.DisposeIfRelease(input.Data.OriginalImage, Id);
            }
        }

        public override void Dispose(){ }
    }
}
