using System;
using System.Collections;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using UniRx;

using UnityEngine;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// バウンディングボックスから座標に変換。
    /// </summary>
    public class BBToMPDStream : ScheduleStreamBase<BoundaryBox, ModelPredictedData>
    {
        [SerializeField]
        internal float moveScale = 0.1f;

        [SerializeField]
        internal Transform center = default;

        [SerializeField]
        internal bool isEnableDepth = true;

        [SerializeField]
        internal float depthCenter = 0.3f;

        [SerializeField]
        internal bool isDebugBB = true;

        public override string ProcessName { get; set; } = "Convert BB to MPD";
        public override Type[] UseType { get; } = new Type[0];
        public override string[] DebugKey { get; } = new string[] {
            nameof(Elapsed_Time),
            nameof(Head_Pos_X), nameof(Head_Pos_Y), nameof(Head_Pos_Z),
            nameof(Head_Rot_X), nameof(Head_Rot_Y), nameof(Head_Rot_Z)
        };
        public override IReadOnlyReactiveProperty<bool> IsAvailable { get; } = new ReactiveProperty<bool>(true);

        private string Elapsed_Time = nameof(Elapsed_Time);
        private string Head_Pos_X = nameof(Head_Pos_X);
        private string Head_Pos_Y = nameof(Head_Pos_Y);
        private string Head_Pos_Z = nameof(Head_Pos_Z);
        private string Head_Rot_X = nameof(Head_Rot_X);
        private string Head_Rot_Y = nameof(Head_Rot_Y);
        private string Head_Rot_Z = nameof(Head_Rot_Z);

        private string Head = nameof(Head);

        public override void InitInternal(int thread) { }

        protected override IDebugMessage DebugLogInternal(SchedulableData<ModelPredictedData> data)
        {
            var message = new Dictionary<string, string>();
            message[Elapsed_Time] = data.ElapsedTimes[data.ElapsedTimes.Count - 1].TotalMilliseconds.ToString("F") + "ms";
            if (data.IsSuccess && isDebugBB)
            {
                data.Data.ToDebugPosition(message, Head, Head_Pos_X, Head_Pos_Y, Head_Pos_Z);
                data.Data.ToDebugRotation(message, Head, Head_Rot_X, Head_Rot_Y, Head_Rot_Z);
            }
            return new DebugMessage(data, message);
        }

        protected override SchedulableData<ModelPredictedData> ProcessInternal(SchedulableData<BoundaryBox> input)
        {
            return ProcessInternalAsync(input).ToObservable().Wait();
        }

        private async UniTask<SchedulableData<ModelPredictedData>> ProcessInternalAsync(SchedulableData<BoundaryBox> input)
        {
            if (!input.IsSuccess)
            {
                return new SchedulableData<ModelPredictedData>(input, null);
            }
            var nomarizer = input.Data.ImageSize.x > input.Data.ImageSize.y ? input.Data.ImageSize.x : input.Data.ImageSize.y;
            var bb2D = (input.Data.Box.center - input.Data.ImageSize / 2) / nomarizer;
            float z = 0;
            if (isEnableDepth)
            {
                z = (input.Data.Box.width + input.Data.Box.height) / 2 / nomarizer - depthCenter;
            }
            var position = new Dictionary<string, Vector3>();
            await UniTask.SwitchToMainThread();
            position[Head] = center.TransformPoint(new Vector3(bb2D.x, bb2D.y, z));
            var rotation = new Dictionary<string, Quaternion>();
            rotation[Head] = Quaternion.Euler(center.forward);
            await UniTask.SwitchToThreadPool();
            var ret = new ModelPredictedData(position, rotation, new Dictionary<string, float>());
            return new SchedulableData<ModelPredictedData>(input, ret);
        }
    }
}
