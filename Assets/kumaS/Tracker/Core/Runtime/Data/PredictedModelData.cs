using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// 推定されたモデルのデータ。
    /// </summary>
    public class PredictedModelData
    {
        /// <summary>
        /// 位置。
        /// </summary>
        public Dictionary<string, Vector3> Position { get; }

        /// <summary>
        /// 回転。ボーンの向く先がz軸・上がy軸とする。
        /// </summary>
        public Dictionary<string, Quaternion> Rotation { get; }

        /// <summary>
        /// ブレンドシェイプのパラメーター。
        /// </summary>
        public Dictionary<string, float> Parameter { get; }

        /// <summary>
        /// その他。
        /// </summary>
        public Dictionary<string, object> Option { get; }

        public static string[] DefaultPositionList { get; } = { 
            "Root", "Head",
            "L_Shoulder", "R_Shoulder",
            "L_Elbow", "R_Elbow",
            "L_Wrist", "R_Wrist",
            "L_Hip", "R_Hip",
            "L_Knee", "R_Knee",
            "L_Ankle", "R_Ankle"
        };
        public static string[] DefaultRotationList { get; } = {
            "Root", "Head",
            "L_Shoulder", "R_Shoulder",
            "L_Elbow", "R_Elbow",
            "L_Wrist", "R_Wrist",
            "L_Hip", "R_Hip",
            "L_Knee", "R_Knee",
            "L_Ankle", "R_Ankle",
            "L_Eye", "R_Eye"
        };
        public static string[] DefaultParameterList { get; } = {
            "Blink_L", "Blink_R",
            "A", "I", "U", "E", "O"
        };

        /// <param name="position">位置。</param>
        /// <param name="rotation">回転。</param>
        /// <param name="parameter">ブレンドシェイプの値。</param>
        /// <param name="option">その他。</param>
        public PredictedModelData(Dictionary<string, Vector3> position, Dictionary<string, Quaternion> rotation, Dictionary<string, float> parameter, Dictionary<string, object> option = null)
        {
            Position = position;
            Rotation = rotation;
            Parameter = parameter;
            Option = option;
        }

        /// <summary>
        /// 位置のデバッグデータを追加する。
        /// </summary>
        /// <param name="message">追加する先。</param>
        /// <param name="key">デバッグするキー。</param>
        /// <param name="labelX">xのラベル。</param>
        /// <param name="labelY">yのラベル。</param>
        /// <param name="labelZ">zのラベル。</param>
        public void ToDebugPosition(Dictionary<string, string> message, string key, string labelX, string labelY, string labelZ)
        {
            if (Position != null && Position.ContainsKey(key))
            {
                ToDebugVector3(message, Position[key], labelX, labelY, labelZ);
            }
        }

        /// <summary>
        /// 回転のデバッグデータを追加する。
        /// </summary>
        /// <param name="message">追加する先。</param>
        /// <param name="key">デバッグするキー。</param>
        /// <param name="labelX">xのラベル。</param>
        /// <param name="labelY">yのラベル。</param>
        /// <param name="labelZ">zのラベル。</param>
        public void ToDebugRotation(Dictionary<string, string> message, string key, string labelX, string labelY, string labelZ)
        {
            if (Rotation != null && Rotation.ContainsKey(key))
            {
                ToDebugVector3(message, Rotation[key].eulerAngles, labelX, labelY, labelZ);
            }
        }

        /// <summary>
        /// ブレンドシェイプのパラメーターのデバッグデータを追加する。
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

        private void ToDebugVector3(Dictionary<string, string> message, Vector3 data, string labelX, string labelY, string labelZ)
        {
            message[labelX] = data.x.ToString();
            message[labelY] = data.y.ToString();
            message[labelZ] = data.z.ToString();
        }
    }
}
