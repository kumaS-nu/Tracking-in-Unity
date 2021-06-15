using Cysharp.Threading.Tasks;

using kumaS.Tracker.Core;

using System;
using System.Collections.Generic;

using UniRx;

using UnityEngine;

namespace kumaS.Tracker.Live2D
{
    /// <summary>
    /// PMDからLive2Dのパラメーターに変換するストリーム。
    /// </summary>
    public sealed class PMDToLive2DStream : ScheduleStreamBase<PredictedModelData, PredictedLive2DData>
    {
        public bool isDebugParameter = true;
        public bool mouthToDefault = true;
        public Transform center;
        public float[] mouthCoeff = new float[] { 1, 0.1f, 0.3f, 0.5f, 1 };

        public override string ProcessName { get; set; } = "Convert PMD to Live2D";
        public override Type[] UseType { get; } = new Type[0];
        public override string[] DebugKey { get => debugKey; }

        private string[] debugKey = default;

        public override IReadOnlyReactiveProperty<bool> IsAvailable { get => isAvailable; }

        private readonly ReactiveProperty<bool> isAvailable = new ReactiveProperty<bool>(false);

        private readonly List<string> mouthKey = new List<string> { "A", "I", "U", "E", "O" };

        protected override void InitInternal(int thread)
        {
            debugKey = new string[PredictedLive2DData.DefaultParameterList.Length + 1];
            debugKey[0] = SchedulableData<object>.Elapsed_Time;
            for (var i = 0; i < PredictedLive2DData.DefaultParameterList.Length; i++)
            {
                debugKey[i + 1] = PredictedLive2DData.DefaultParameterList[i];
            }
            isAvailable.Value = true;
        }

        protected override IDebugMessage DebugLogInternal(SchedulableData<PredictedLive2DData> data)
        {
            var message = new Dictionary<string, string>();
            data.ToDebugElapsedTime(message);
            foreach (var key in PredictedLive2DData.DefaultParameterList)
            {
                data.Data.ToDebugParameter(message, key, key);
            }
            return new DebugMessage(data, message);
        }

        protected override SchedulableData<PredictedLive2DData> ProcessInternal(SchedulableData<PredictedModelData> input)
        {
            if (!input.IsSuccess)
            {
                return new SchedulableData<PredictedLive2DData>(input, default);
            }
            var output = new Dictionary<string, float>();
            AddParameter(output, input.Data);
            AddPosRot(output, input.Data);
            AddHead(output, input.Data).ToObservable().Wait();
            var ret = new PredictedLive2DData(output);
            return new SchedulableData<PredictedLive2DData>(input, ret);
        }

        /// <summary>
        /// パラメータの部分を追加。
        /// </summary>
        /// <param name="output">出力する先。</param>
        /// <param name="input">入力。</param>
        private void AddParameter(Dictionary<string, float> output, PredictedModelData input)
        {
            var mouth = -1f;
            foreach (KeyValuePair<string, float> parameter in input.Parameter)
            {
                if (parameter.Key == "Blink_L")
                {
                    output["ParamEyeLOpen"] = 1 - parameter.Value;
                }
                else if (parameter.Key == "Blink_R")
                {
                    output["Blink_R"] = 1 - parameter.Value;
                }
                else if (mouthToDefault && mouthKey.Contains(parameter.Key))
                {
                    if (mouth < 0)
                    {
                        mouth = 0;
                    }
                    switch (parameter.Key)
                    {
                        case "A":
                            mouth += mouthCoeff[0] * parameter.Value; break;
                        case "I":
                            mouth += mouthCoeff[1] * parameter.Value; break;
                        case "U":
                            mouth += mouthCoeff[2] * parameter.Value; break;
                        case "E":
                            mouth += mouthCoeff[3] * parameter.Value; break;
                        case "O":
                            mouth += mouthCoeff[4] * parameter.Value; break;
                    }

                }
                else
                {
                    output[parameter.Key] = parameter.Value;
                }
            }

            if (mouth >= 0)
            {
                output["ParamMouthOpenY"] = mouth;
            }
        }

        /// <summary>
        /// 位置・回転を追加。
        /// </summary>
        /// <param name="output">出力する先。</param>
        /// <param name="input">入力。</param>
        private void AddPosRot(Dictionary<string, float> output, PredictedModelData input)
        {
            Quaternion rootRot = input.Rotation[PredictedModelData.DefaultRotationList[0]];
            Quaternion leftInverse = Quaternion.Inverse(rootRot) * new Quaternion(0, 1 / Mathf.Sqrt(2), 0, 1 / Mathf.Sqrt(2)) * new Quaternion(0, 0, -1 / Mathf.Sqrt(2), 1 / Mathf.Sqrt(2));
            Quaternion rightInverse = Quaternion.Inverse(rootRot) * new Quaternion(0, -1 / Mathf.Sqrt(2), 0, 1 / Mathf.Sqrt(2)) * new Quaternion(0, 0, 1 / Mathf.Sqrt(2), 1 / Mathf.Sqrt(2));
            if (input.Position.ContainsKey(PredictedModelData.DefaultPositionList[0]) && input.Rotation.ContainsKey(PredictedModelData.DefaultRotationList[0]))
            {
                output["ParamBaseX"] = input.Position[PredictedModelData.DefaultPositionList[0]].x;
                output["ParamBaseY"] = input.Position[PredictedModelData.DefaultPositionList[0]].y;
                Vector3 rot = rootRot.eulerAngles;
                output["ParamBodyAngleX"] = rot.y;
                output["ParamBodyAngleY"] = -rot.x;
                output["ParamBodyAngleZ"] = -rot.z;
            }

            {
                if (input.Rotation.TryGetValue(PredictedModelData.DefaultRotationList[2], out Quaternion rot))
                {
                    var x = -(rot * leftInverse).eulerAngles.x;
                    output["ParamArmLA"] = x < 0 ? 0 : x;
                }
            }
            {
                if (input.Rotation.TryGetValue(PredictedModelData.DefaultRotationList[4], out Quaternion rot))
                {
                    var x = -(rot * leftInverse).eulerAngles.x;
                    output["ParamArmLB"] = x < 0 ? 0 : x;
                }
            }
            {
                if (input.Rotation.TryGetValue(PredictedModelData.DefaultRotationList[3], out Quaternion rot))
                {
                    var x = -(rot * rightInverse).eulerAngles.x;
                    output["ParamArmRA"] = x < 0 ? 0 : x;
                }
            }
            {
                if (input.Rotation.TryGetValue(PredictedModelData.DefaultRotationList[5], out Quaternion rot))
                {
                    var x = -(rot * rightInverse).eulerAngles.x;
                    output["ParamArmRB"] = x < 0 ? 0 : x;
                }
            }
        }

        private async UniTask AddHead(Dictionary<string, float> output, PredictedModelData input)
        {
            if (input.Position.ContainsKey(PredictedModelData.DefaultPositionList[1]) && input.Rotation.ContainsKey(PredictedModelData.DefaultRotationList[1]))
            {
                await UniTask.SwitchToMainThread();
                Vector3 rot = (input.Rotation[PredictedModelData.DefaultRotationList[0]] * Quaternion.Inverse(center.rotation)).eulerAngles;
                output["ParamAngleX"] = rot.y;
                output["ParamAngleY"] = -rot.x;
                output["ParamAngleZ"] = -rot.z;
            }
        }

    }
}
