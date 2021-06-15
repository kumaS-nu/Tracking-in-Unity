using UnityEngine;

namespace kumaS.Tracker.Dlib
{
    public static class DlibExtentions
    {
        public static Vector2 ToVector2(this DlibDotNet.Point point)
        {
            return new Vector2(point.X, point.Y);
        }

        public static OpenCvSharp.Vec2f ToVec2f(this DlibDotNet.Point point)
        {
            return new OpenCvSharp.Vec2f(point.X, point.Y);
        }

        public static DlibDotNet.Rectangle ToRectangle(this Rect rect)
        {
            return new DlibDotNet.Rectangle((int)rect.x, (int)rect.y, (int)rect.xMax, (int)rect.yMax);
        }

        public static Rect ToRect(this DlibDotNet.Rectangle rect)
        {
            return new Rect(rect.Left, rect.Top, rect.Width, rect.Height);
        }

        public static Rect GetRect(DlibDotNet.Point nose, DlibDotNet.Point leftEyeOuter, DlibDotNet.Point rightEyeOuter)
        {
            var halflen = (float)((leftEyeOuter - rightEyeOuter).Length * 0.75);
            var len = halflen * 2;
            return new Rect(nose.X - halflen, nose.Y - halflen, len, len);
        }
    }
}
