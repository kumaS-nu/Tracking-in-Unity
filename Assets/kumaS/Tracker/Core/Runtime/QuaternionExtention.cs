using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace kumaS.Tracker.Core
{
    public static class QuaternionExtention
    {
        public static Vector3 ToLogQuaternion(this Quaternion rotation)
        {
            var v = new Vector3(rotation.x, rotation.y, rotation.z);
            var theta = Mathf.Acos(rotation.w);
            var sinc = theta <= 0 ? 1 : theta / Mathf.Sin(theta);
            return v * sinc;
        }

        public static Quaternion ToQuaternion(this Vector3 rotation)
        {
            var theta = rotation.magnitude;
            var sinc = theta <= 0 ? 1 : Mathf.Sin(theta) / theta;
            rotation *= sinc;
            return new Quaternion(rotation.x, rotation.y, rotation.z, Mathf.Cos(theta));
        }
    }
}
