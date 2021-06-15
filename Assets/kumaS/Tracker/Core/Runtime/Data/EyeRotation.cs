using System.Collections.Generic;

using UnityEngine;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// 目の向きのデータ。
    /// </summary>
    public class EyeRotation
    {
        public Quaternion Left { get; }
        public Quaternion Right { get; }

        public EyeRotation(Quaternion left, Quaternion right)
        {
            Left = left;
            Right = right;
        }

        public void ToDebugRoattion(Dictionary<string, string> message, string L_labelX, string L_labelY, string L_labelZ, string R_labelX, string R_labelY, string R_labelZ)
        {
            message[L_labelX] = Left.eulerAngles.x.ToString();
            message[L_labelY] = Left.eulerAngles.y.ToString();
            message[L_labelZ] = Left.eulerAngles.z.ToString();
            message[R_labelX] = Right.eulerAngles.x.ToString();
            message[R_labelY] = Right.eulerAngles.y.ToString();
            message[R_labelZ] = Right.eulerAngles.z.ToString();
        }
    }
}
