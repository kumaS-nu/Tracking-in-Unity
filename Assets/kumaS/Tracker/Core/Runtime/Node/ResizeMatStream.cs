using OpenCvSharp;

using System;
using System.Collections.Generic;
using System.Threading;

using UniRx;

using UnityEngine;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// 画像をリサイズする。
    /// </summary>
    public sealed class ResizeMatStream : ScheduleStreamBase<Mat, Mat>
    {
        [SerializeField]
        internal double ratio = 1;

        [SerializeField]
        internal InterpolationFlags interpolation = InterpolationFlags.Linear;

        public override string ProcessName { get; set; } = "Resize mat";
        public override Type[] UseType { get; } = new Type[] { typeof(Mat) };
        public override string[] DebugKey { get; } = new string[] { SchedulableData<object>.Elapsed_Time, nameof(Resized_Pointer) };
        public override IReadOnlyReactiveProperty<bool> IsAvailable { get; } = new ReactiveProperty<bool>(true);

        private readonly string Resized_Pointer = nameof(Resized_Pointer);

        /// <inheritdoc/>
        protected override void InitInternal(int thread, CancellationToken token) { }

        /// <inheritdoc/>
        protected override SchedulableData<Mat> ProcessInternal(SchedulableData<Mat> input)
        {
            if (!input.IsSuccess || input.IsSignal)
            {
                return new SchedulableData<Mat>(input, default);
            }
            var resized = new Mat();
            Cv2.Resize(input.Data, resized, Size.Zero, ratio, ratio, interpolation);
            input.Data.Dispose();
            ResourceManager.DisposeIfRelease(resized, Id);
            
            return new SchedulableData<Mat>(input, resized);
        }

        /// <inheritdoc/>
        protected override IDebugMessage DebugLogInternal(SchedulableData<Mat> data)
        {
            var message = new Dictionary<string, string>();
            data.ToDebugElapsedTime(message);
            if (data.IsSuccess && !data.Data.IsDisposed && !data.IsSignal)
            {
                message[Resized_Pointer] = data.Data.Data.ToInt32().ToString();
            }
            return new DebugMessage(data, message);
        }

        /// <inheritdoc/>
        public override void Dispose(){ }
    }
}
