using System;
using System.Collections.Generic;

using UniRx;

using UnityEngine;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// バウンダリーボックスから頭の位置・回転に変換。
    /// </summary>
    public sealed class BBToHeadTransformStream : ScheduleStreamBase<BoundaryBox, HeadTransform>
    {
        [SerializeField]
        internal float moveScale = 0.1f;

        [SerializeField]
        internal bool isEnableDepth = true;

        [SerializeField]
        internal float depthCenter = 0.3f;

        [SerializeField]
        internal bool sourceIsMirror = false;

        [SerializeField]
        internal bool wantMirror = true;

        [SerializeField]
        internal bool isDebugHead = true;

        private bool mirror = true;

        public override string ProcessName { get; set; } = "Convert BB to head transform";
        public override Type[] UseType { get; } = new Type[0];
        public override string[] DebugKey { get; } = new string[] {
            SchedulableData<object>.Elapsed_Time,
            nameof(Head_Pos_X), nameof(Head_Pos_Y), nameof(Head_Pos_Z),
            nameof(Head_Rot_X), nameof(Head_Rot_Y), nameof(Head_Rot_Z)
        };
        public override IReadOnlyReactiveProperty<bool> IsAvailable { get => isAvailable; }

        private readonly ReactiveProperty<bool> isAvailable = new ReactiveProperty<bool>(false);

        private readonly string Head_Pos_X = nameof(Head_Pos_X);
        private readonly string Head_Pos_Y = nameof(Head_Pos_Y);
        private readonly string Head_Pos_Z = nameof(Head_Pos_Z);
        private readonly string Head_Rot_X = nameof(Head_Rot_X);
        private readonly string Head_Rot_Y = nameof(Head_Rot_Y);
        private readonly string Head_Rot_Z = nameof(Head_Rot_Z);

        private readonly string Head = nameof(Head);

        protected override void InitInternal(int thread)
        {
            mirror = sourceIsMirror != wantMirror;
            isAvailable.Value = true;
        }

        protected override IDebugMessage DebugLogInternal(SchedulableData<HeadTransform> data)
        {
            var message = new Dictionary<string, string>();
            data.ToDebugElapsedTime(message);
            if (data.IsSuccess && isDebugHead)
            {
                data.Data.ToDebugPosition(message, Head_Pos_X, Head_Pos_Y, Head_Pos_Z);
                data.Data.ToDebugRotation(message, Head_Rot_X, Head_Rot_Y, Head_Rot_Z);
            }
            return new DebugMessage(data, message);
        }

        protected override SchedulableData<HeadTransform> ProcessInternal(SchedulableData<BoundaryBox> input)
        {
            if (!input.IsSuccess)
            {
                return new SchedulableData<HeadTransform>(input, default);
            }
            var nomarizer = input.Data.ImageSize.x > input.Data.ImageSize.y ? input.Data.ImageSize.x : input.Data.ImageSize.y;
            Vector2 bb2D = (input.Data.Box.center - input.Data.ImageSize / 2) / nomarizer;
            var z = 0.0f;
            var angle = input.Data.Angle;
            if (isEnableDepth)
            {
                z = (input.Data.Box.width + input.Data.Box.height) / 2 / nomarizer - depthCenter;
            }
            if (!mirror)
            {
                bb2D.x *= -1;
                angle *= -1;
            }
            var ret = new HeadTransform(new Vector3(bb2D.x, bb2D.y, z) * moveScale, Quaternion.Euler(0, 0, angle));
            return new SchedulableData<HeadTransform>(input, ret);
        }
    }
}
