using System.Collections.Generic;

using UnityEngine;

namespace kumaS.Tracker.PoseNet
{
    /// <summary>
    /// 体の部位のデータ。
    /// </summary>
    public class BodyPoints
    {
        /// <summary>
        /// 位置。
        /// </summary>
        public Vector3[] Position { get; }

        /// <summary>
        /// 回転。ボーンの向く先がz軸・上がy軸とする。
        /// </summary>
        public Quaternion[] Rotation { get; }

        /// <param name="position">位置。</param>
        /// <param name="rotation">回転。</param>
        public BodyPoints(Vector3[] position, Quaternion[] rotation)
        {
            Position = position;
            Rotation = rotation;
        }

        /// <summary>
        /// 位置のデバッグデータを追加する。
        /// </summary>
        /// <param name="message">追加する先。</param>
        /// <param name="index">デバッグするインデックス。</param>
        /// <param name="labelX">xのラベル。</param>
        /// <param name="labelY">yのラベル。</param>
        /// <param name="labelZ">zのラベル。</param>
        public void ToDebugPosition(Dictionary<string, string> message, int index, string labelX, string labelY, string labelZ)
        {
            ToDebugVector3(message, Position[index], labelX, labelY, labelZ);
        }

        /// <summary>
        /// 回転のデバッグデータを追加する。
        /// </summary>
        /// <param name="message">追加する先。</param>
        /// <param name="index">デバッグするインデックス。</param>
        /// <param name="labelX">xのラベル。</param>
        /// <param name="labelY">yのラベル。</param>
        /// <param name="labelZ">zのラベル。</param>
        public void ToDebugRotation(Dictionary<string, string> message, int index, string labelX, string labelY, string labelZ)
        {
            ToDebugVector3(message, Rotation[index].eulerAngles, labelX, labelY, labelZ);
        }

        private void ToDebugVector3(Dictionary<string, string> message, Vector3 data, string labelX, string labelY, string labelZ)
        {
            message[labelX] = data.x.ToString();
            message[labelY] = data.y.ToString();
            message[labelZ] = data.z.ToString();
        }
    }
}
