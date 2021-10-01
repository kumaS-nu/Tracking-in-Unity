using kumaS.Tracker.Core;

using OpenCvSharp;

using System;
using System.Collections.Generic;
using System.Threading;

using UniRx;

using UnityEngine;

namespace kumaS.Tracker.Dlib
{
    public sealed class Dlib5ToBBStream : ScheduleStreamBase<Dlib5Landmarks, BoundaryBox>
    {
        [SerializeField]
        internal bool isDebugBox = true;

        public override string ProcessName { get; set; } = "Dlib 5 landmarks to BB";
        public override Type[] UseType { get; } = new Type[] { typeof(Mat) };
        public override string[] DebugKey { get; } = new string[] { SchedulableData<object>.Elapsed_Time, nameof(Image_Width), nameof(Image_Height), nameof(X), nameof(Y), nameof(Width), nameof(Height) };
        public override IReadOnlyReactiveProperty<bool> IsAvailable { get; } = new ReactiveProperty<bool>(true);

        private readonly string Image_Width = nameof(Image_Width);
        private readonly string Image_Height = nameof(Image_Height);
        private readonly string X = nameof(X);
        private readonly string Y = nameof(Y);
        private readonly string Width = nameof(Width);
        private readonly string Height = nameof(Height);

        protected override IDebugMessage DebugLogInternal(SchedulableData<BoundaryBox> data)
        {
            var message = new Dictionary<string, string>();
            data.ToDebugElapsedTime(message);
            if (data.IsSuccess && data.Data != null && !data.IsSignal)
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

        protected override void InitInternal(int thread, CancellationToken token) { }

        protected override SchedulableData<BoundaryBox> ProcessInternal(SchedulableData<Dlib5Landmarks> input)
        {

            if (!input.IsSuccess || input.IsSignal)
            {
                return new SchedulableData<BoundaryBox>(input, default);
            }

            try
            {
                if (input.Data.Landmarks == default)
                {
                    return new SchedulableData<BoundaryBox>(input, new BoundaryBox(input.Data.OriginalImage, default));
                }

                var angle = Mathf.Atan2(input.Data.Landmarks[2].Y - input.Data.Landmarks[0].Y, input.Data.Landmarks[0].X - input.Data.Landmarks[2].X) * Mathf.Rad2Deg;
                UnityEngine.Rect rect = DlibExtentions.GetRect(input.Data.Landmarks[4], input.Data.Landmarks[0], input.Data.Landmarks[2]);
                var ret = new BoundaryBox(input.Data.OriginalImage, rect, angle);
                return new SchedulableData<BoundaryBox>(input, ret);
            }
            finally
            {
                ResourceManager.DisposeIfRelease(input.Data.OriginalImage, Id);
            }
        }

        public override void Dispose(){ }
    }
}
