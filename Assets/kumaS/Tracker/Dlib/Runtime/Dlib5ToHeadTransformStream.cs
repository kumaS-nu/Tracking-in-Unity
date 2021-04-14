using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using kumaS.Tracker.Core;
using System;
using UniRx;

namespace kumaS.Tracker.Dlib
{
    /// <summary>
    /// Dlibの5つの顔の特徴点から頭の位置・回転に変換するストリーム。
    /// </summary>
    public class Dlib5ToHeadTransformStream : ScheduleStreamBase<Dlib5Landmarks, HeadTransform>
    {
        [SerializeField]
        internal float moveScale = 0.1f;

        [SerializeField]
        internal float depthCenter = 0.3f;

        [SerializeField]
        internal bool sourceIsMirror = false;

        [SerializeField]
        internal bool wantMirror = true;

        [SerializeField]
        internal bool isDebugHead = true;

        private bool mirror = true;

        public override string ProcessName { get; set; } = "Dlib 5 landmarks to head transform";
        public override Type[] UseType { get; } = new Type[0];
        public override string[] DebugKey { get; } = new string[] {
            SchedulableData<object>.Elapsed_Time,
            nameof(Head_Pos_X), nameof(Head_Pos_Y), nameof(Head_Pos_Z),
            nameof(Head_Rot_X), nameof(Head_Rot_Y), nameof(Head_Rot_Z)
        };
        public override IReadOnlyReactiveProperty<bool> IsAvailable { get => isAvailable; }

        private ReactiveProperty<bool> isAvailable = new ReactiveProperty<bool>(false);

        private readonly string Head = nameof(Head);
        private readonly string Head_Pos_X = nameof(Head_Pos_X);
        private readonly string Head_Pos_Y = nameof(Head_Pos_Y);
        private readonly string Head_Pos_Z = nameof(Head_Pos_Z);
        private readonly string Head_Rot_X = nameof(Head_Rot_X);
        private readonly string Head_Rot_Y = nameof(Head_Rot_Y);
        private readonly string Head_Rot_Z = nameof(Head_Rot_Z);

        public override void InitInternal(int thread){
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

        protected override SchedulableData<HeadTransform> ProcessInternal(SchedulableData<Dlib5Landmarks> input)
        {
            if (!input.IsSuccess)
            {
                return new SchedulableData<HeadTransform>(input, default);
            }
            var nomarizer = input.Data.ImageSize.x > input.Data.ImageSize.y ? input.Data.ImageSize.x : input.Data.ImageSize.y;
            var x = (input.Data.Landmarks[4].X - input.Data.ImageSize.x / 2) / nomarizer;
            var y = (input.Data.Landmarks[4].Y - input.Data.ImageSize.y / 2) / nomarizer;
            var z = (float)(input.Data.Landmarks[0] - input.Data.Landmarks[2]).Length / nomarizer - depthCenter;
            var vec = input.Data.Landmarks[0] - input.Data.Landmarks[2];
            var rot = Quaternion.Euler(0, 0, Mathf.Atan2(vec.Y, vec.X) * Mathf.Rad2Deg);
            if (sourceIsMirror == wantMirror)
            {
                x *= -1;
                rot.y *= -1;
                rot.z *= -1;
            }
            var ret = new HeadTransform(new Vector3(x, y, z) * moveScale, rot );
            return new SchedulableData<HeadTransform>(input, ret);
        }
    }
}
