using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace kumaS.Tracker.Dlib
{
    public static class PointExtention
    {
        public static Vector2 ToVector2(this DlibDotNet.Point point)
        {
            return new Vector2(point.X, point.Y);
        }

        public static OpenCvSharp.Vec2f ToVec2f(this DlibDotNet.Point point)
        {
            return new OpenCvSharp.Vec2f(point.X, point.Y);
        }
    }
}
