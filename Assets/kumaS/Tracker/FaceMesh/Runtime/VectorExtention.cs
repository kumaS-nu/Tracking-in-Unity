using OpenCvSharp;

using UnityEngine;

namespace kumaS.Tracker.FaceMesh
{
    public static class VectorExtention
    {
        public static Point2f ToPoint2f(this Vector3 vec)
        {
            return new Point2f(vec.x, vec.y);
        }
    }
}
