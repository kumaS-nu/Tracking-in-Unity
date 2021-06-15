using System.Collections.Generic;

namespace kumaS.Tracker.Live2D
{
    public sealed class PredictedLive2DData
    {
        /// <summary>
        /// Live2Dのパラメーター。
        /// </summary>
        public Dictionary<string, float> Parameter { get; }

        public static string[] DefaultParameterList { get; } = {
            "ParamAngleX", "ParamAngleY", "ParamAngleZ",
            "ParamEyeLOpen", "ParamEyeROpen",
            "ParamEyeBallX", "ParamEyeBallY",
            "ParamBodyAngleX", "ParamBodyAngleY", "ParamBodyAngleZ",
            "ParamArmLA", "ParamArmRA",
            "ParamArmLB", "ParamArmRB"
        };

        /// <param name="parameter">Live2Dの値。</param>
        /// <param name="option">その他。</param>
        public PredictedLive2DData(Dictionary<string, float> parameter)
        {
            Parameter = parameter;
        }

        /// <summary>
        /// パラメーターのデバッグデータを追加する。
        /// </summary>
        /// <param name="message">追加する先。</param>
        /// <param name="key">デバッグするキー。</param>
        /// <param name="label">ラベル。</param>
        public void ToDebugParameter(Dictionary<string, string> message, string key, string label)
        {
            if (Parameter != null && Parameter.ContainsKey(key))
            {
                message[label] = Parameter[key].ToString();
            }
        }
    }
}
