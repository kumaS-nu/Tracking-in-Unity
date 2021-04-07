using System;
using System.Collections;
using System.Collections.Generic;

using UniRx;

using UnityEngine;

namespace kumaS.Tracker.Core
{
    public class SmoothingBBStream : SmoothingStreamBase<BoundaryBox>
    {
        [SerializeField]
        internal float speedLimit = 0.05f;

        [SerializeField]
        internal bool isDebugBox = true;

        public override string ProcessName { get; set; } = "Smoothing BB";
        public override string[] DebugKey { get; } = new string[] { nameof(Elapsed_Time), nameof(Image_Width), nameof(Image_Height), nameof(X), nameof(Y), nameof(Width), nameof(Height) };

        private readonly string Elapsed_Time = nameof(Elapsed_Time);
        private readonly string Image_Width = nameof(Image_Width);
        private readonly string Image_Height = nameof(Image_Height);
        private readonly string X = nameof(X);
        private readonly string Y = nameof(Y);
        private readonly string Width = nameof(Width);
        private readonly string Height = nameof(Height);

        private float speedLimitInternal = 1;

        private void Awake()
        {
            speedLimitInternal = speedLimit * bufferSize;
        }

        protected override BoundaryBox Average(BoundaryBox[] datas)
        {
            float x = 0;
            float y = 0;
            float width = 0;
            float height = 0;
            foreach(var data in datas)
            {
                x += data.Box.x;
                y += data.Box.y;
                width += data.Box.width;
                height += data.Box.height;
            }
            x /= datas.Length;
            y /= datas.Length;
            width /= datas.Length;
            height /= datas.Length;
            return new BoundaryBox(datas[0].ImageSize, new Rect(x, y, width, height));
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

        protected override bool ValidateData(BoundaryBox input)
        {
            var normalizer = input.ImageSize.x > input.ImageSize.y ? input.ImageSize.x : input.ImageSize.y;
            if((input.Box.center - lastOutput.Box.center).sqrMagnitude > speedLimitInternal * speedLimitInternal * normalizer * normalizer)
            {
                return false;
            }
            if(Math.Abs(input.Box.width + input.Box.height - lastOutput.Box.width - lastOutput.Box.height) > speedLimitInternal * 2 * normalizer)
            {
                return false;
            }

            return true;
        }
    }
}
