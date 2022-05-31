using UnityEngine;
using System;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// クォータニオンの拡張メゾッドを提供するクラス。
    /// </summary>
    public static class QuaternionExtention { 
        
        /// <summary>
        /// コンパクトなクォータニオンからクォータニオンに変換する。
        /// </summary>
        /// <param name="log">コンパクトなクォータニオン。</param>
        /// <returns>クォータニオン。</returns>
        public static Quaternion FromLogQuaternion(this Vector3 log)
        {
            var theta = log.magnitude;
            while(theta > Mathf.PI)
            {
                theta -= 2 * Mathf.PI;
            }
            var theta_h = theta * 0.5f;
            var sin = theta < 0.000001f ? 0 : Mathf.Sin(theta_h) / theta;
            return new Quaternion(log.x * sin, log.y * sin, log.z * sin, Mathf.Cos(theta_h));
        }
    }


    /// <summary>
    /// クォータニオンの補間を提供するクラス。
    /// </summary>
    public class QuaternionInterpolation
    {
        private static (Quaternion rotate, Quaternion qPos) GetPosRotate(Vector3 a, Vector3 b, float t)
        {
            var axis = Vector3.Cross(a, b);
            var posCos = Vector3.Dot(a, b);
            var posTheta = Mathf.Acos(posCos);
            if (float.IsNaN(posTheta))
            {
                posTheta = 0;
            }
            var posCos_h = Mathf.Cos(posTheta / 2);
            var posSin_h = Mathf.Sqrt(1 - posCos_h * posCos_h);
            var posSin_h_r = posSin_h / Mathf.Sqrt(1 - posCos * posCos);
            var thetaT = posTheta / 2 * t;
            var posCosT = Mathf.Cos(thetaT);
            var posSinT = Mathf.Sin(thetaT) / posSin_h;
            var rotate = new Quaternion(axis.x * posSin_h_r, axis.y * posSin_h_r, axis.z * posSin_h_r, posCos_h);
            return (rotate, new Quaternion(rotate.x * posSinT, rotate.y * posSinT, rotate.z * posSinT, posCosT));
        }

        private static bool Approximately(Quaternion a, Quaternion b)
        {
            if(Mathf.Abs(a.x - b.x) > 0.01f)
            {
                return false;
            }

            if (Mathf.Abs(a.y - b.y) > 0.01f)
            {
                return false;
            }

            if (Mathf.Abs(a.z - b.z) > 0.01f)
            {
                return false;
            }

            if (Mathf.Abs(a.w - b.w) > 0.01f)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// x軸方向のベクトルの軌跡を最小とする補間をする。
        /// </summary>
        /// <param name="a">始点。</param>
        /// <param name="b">終点。</param>
        /// <param name="t">欲しい補間。</param>
        /// <returns>補間点。</returns>
        public static Quaternion Xslerp(Quaternion a, Quaternion b, float t)
        {
            if(Approximately(a, b))
            {
                return a;
            }
            var posRotate = GetPosRotate(a * Vector3.right, b * Vector3.right, t);
            if (float.IsNaN(posRotate.rotate.x)) {
                return a;
            }
            var xRotate = Quaternion.Inverse(posRotate.rotate * a) * b;
            if (xRotate.w < 0)
            {
                xRotate.x *= -1;
                xRotate.w *= -1;
            }
            var theta = Mathf.Acos(xRotate.w) * t;
            theta = float.IsNaN(theta) ? 0 : theta;
            var qX = new Quaternion(Mathf.Sign(xRotate.x) * Mathf.Sin(theta), 0, 0, Mathf.Cos(theta));
            return posRotate.qPos * a * qX;
        }

        /// <summary>
        /// y軸方向のベクトルの軌跡を最小とする補間をする。
        /// </summary>
        /// <param name="a">始点。</param>
        /// <param name="b">終点。</param>
        /// <param name="t">欲しい補間。</param>
        /// <returns>補間点。</returns>
        public static Quaternion Yslerp(Quaternion a, Quaternion b, float t)
        {
            if (Approximately(a, b))
            {
                return a;
            }
            var posRotate = GetPosRotate(a * Vector3.up, b * Vector3.up, t);
            if (float.IsNaN(posRotate.rotate.x))
            {
                return a;
            }
            var yRotate = Quaternion.Inverse(posRotate.rotate * a) * b;
            if (yRotate.w < 0)
            {
                yRotate.y *= -1;
                yRotate.w *= -1;
            }
            var theta = Mathf.Acos(yRotate.w) * t;
            theta = float.IsNaN(theta) ? 0 : theta;
            var qY = new Quaternion(0, Mathf.Sign(yRotate.y) * Mathf.Sin(theta), 0, Mathf.Cos(theta));
            return posRotate.qPos * a * qY;
        }

        /// <summary>
        /// z軸方向のベクトルの軌跡を最小とする補間をする。
        /// </summary>
        /// <param name="a">始点。</param>
        /// <param name="b">終点。</param>
        /// <param name="t">欲しい補間。</param>
        /// <returns>補間点。</returns>
        public static Quaternion Zslerp(Quaternion a, Quaternion b, float t)
        {
            if (Approximately(a, b))
            {
                return a;
            }
            var posRotate = GetPosRotate(a * Vector3.forward, b * Vector3.forward, t);
            if (float.IsNaN(posRotate.rotate.x))
            {
                return a;
            }
            var zRotate = Quaternion.Inverse(posRotate.rotate * a) * b;
            if (zRotate.w < 0)
            {
                zRotate.z *= -1;
                zRotate.w *= -1;
            }
            var theta = Mathf.Acos(zRotate.w) * t;
            theta = float.IsNaN(theta) ? 0 : theta;
            var qZ = new Quaternion(0, 0, Mathf.Sign(zRotate.z) * Mathf.Sin(theta), Mathf.Cos(theta));
            return posRotate.qPos * a * qZ;
        }

        /// <summary>
        /// 指定方向のベクトルの軌跡を最小とする補間をする。
        /// </summary>
        /// <param name="a">始点。</param>
        /// <param name="b">終点。</param>
        /// <param name="t">欲しい補間。</param>
        /// <param name="axis">指定する方向ベクトル。</param>
        /// <returns>補間点。</returns>
        public static Quaternion AxisSlerp(Quaternion a, Quaternion b, float t, Vector3 axis)
        {
            if(Approximately(a, b))
            {
                return a;
            }
            axis.Normalize();
            var posRotate = GetPosRotate(a * axis, b * axis, t);
            if (float.IsNaN(posRotate.rotate.x))
            {
                return a;
            }
            var axisRotate = Quaternion.Inverse(posRotate.rotate * a) * b;
            if(axisRotate.w < 0)
            {
                axisRotate.x *= -1;
                axisRotate.y *= -1;
                axisRotate.z *= -1;
                axisRotate.w *= -1;
            }
            var theta = Mathf.Acos(axisRotate.w) * t;
            theta = float.IsNaN(theta) ? 0 : theta;
            var sin = Mathf.Sin(theta);
            var qAxis = new Quaternion(axis.x * Mathf.Sign(axisRotate.x) * sin, axis.y * Mathf.Sign(axisRotate.y) * sin, axis.z * Mathf.Sign(axisRotate.z) * sin, Mathf.Cos(theta));
            return posRotate.qPos * a * qAxis;
        }
    }
}
