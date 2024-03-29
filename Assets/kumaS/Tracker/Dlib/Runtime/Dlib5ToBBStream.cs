﻿using kumaS.Tracker.Core;

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

        [SerializeField]
        internal bool isDebugImage = false;

        [SerializeField]
        internal int debugInterval = 2;

        [SerializeField]
        internal DebugImageStream debugImage;

        [SerializeField]
        internal Color markColor = new Color(0, 1, 0, 0.5f);

        [SerializeField]
        internal int markSize = 3;

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
        private Scalar color;
        private int skipCount = 0;

        /// <inheritdoc/>
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

                if (isDebugImage && debugImage != null)
                {
                    if (skipCount < debugInterval)
                    {
                        skipCount++;
                    }
                    else
                    {
                        var mat = new Mat(data.Data.ImageSize.y, data.Data.ImageSize.x, MatType.CV_8UC3);
                        var rect = new OpenCvSharp.Rect((int)data.Data.Box.x, (int)data.Data.Box.y, (int)data.Data.Box.width, (int)data.Data.Box.height);
                        mat.Rectangle(rect, color, markSize);
                        debugImage.SetImage(Id, mat);
                        skipCount = 0;
                    }
                }
            }
            return new DebugMessage(data, message);
        }

        /// <inheritdoc/>
        protected override void InitInternal(int thread, CancellationToken token) { }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override void Dispose(){ }
    }
}
