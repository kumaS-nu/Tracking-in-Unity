using System.Collections.Generic;

using UnityEngine;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// 頭の位置・回転のデータ。
    /// </summary>
    public class HeadTransform
    {
        /// <summary>
        /// 頭の位置。
        /// </summary>
        public Vector3 Position { get; }

        /// <summary>
        /// 頭の回転。
        /// </summary>
        public Quaternion Rotation { get; }

        public HeadTransform(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }

        /// <summary>
        /// 位置のデバッグデータを追加する。
        /// </summary>
        /// <param name="message">追加する先。</param>
        /// <param name="labelX">xのラベル。</param>
        /// <param name="labelY">yのラベル。</param>
        /// <param name="labelZ">zのラベル。</param>
        public void ToDebugPosition(Dictionary<string, string> message, string labelX, string labelY, string labelZ)
        {
            if (Position != null)
            {
                ToDebugVector3(message, Position, labelX, labelY, labelZ);
            }
        }

        /// <summary>
        /// 回転のデバッグデータを追加する。
        /// </summary>
        /// <param name="message">追加する先。</param>
        /// <param name="labelX">xのラベル。</param>
        /// <param name="labelY">yのラベル。</param>
        /// <param name="labelZ">zのラベル。</param>
        public void ToDebugRotation(Dictionary<string, string> message, string labelX, string labelY, string labelZ)
        {
            if (Rotation != null)
            {
                ToDebugVector3(message, Rotation.eulerAngles, labelX, labelY, labelZ);
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
