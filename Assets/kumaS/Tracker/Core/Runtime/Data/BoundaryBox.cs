using OpenCvSharp;

using UnityEngine;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// 正規化しないバウンダリーボックス。
    /// </summary>
    public class BoundaryBox
    {
        /// <summary>
        /// 元画像。
        /// </summary>
        public Mat OriginalImage { get; }

        /// <summary>
        /// 検出されたバウンダリーボックス。
        /// </summary>
        public UnityEngine.Rect Box { get; }

        /// <summary>
        /// 元画像の大きさ。
        /// </summary>
        public Vector2Int ImageSize { get; }

        /// <summary>
        /// 顔の回転。単位はdeg。（°）
        /// </summary>
        public float Angle { get; }

        /// <summary>
        /// 元画像がある際のコンストラクタ。
        /// </summary>
        /// <param name="originalImage">元画像。</param>
        /// <param name="box">バウンダリーボックス。</param>
        /// <param name="angle">顔の回転。単位はdeg。</param>
        public BoundaryBox(Mat originalImage, UnityEngine.Rect box, float angle = 0)
        {
            OriginalImage = originalImage;
            Box = box;
            ImageSize = new Vector2Int(originalImage.Width, originalImage.Height);
            Angle = angle;
        }

        /// <summary>
        /// 元画像がない際のコンストラクタ。
        /// </summary>
        /// <param name="imageSize">元画像の大きさ。</param>
        /// <param name="box">バウンダリーボックス。</param>
        /// <param name="angle">顔の回転。単位はdeg。</param>
        public BoundaryBox(Vector2Int imageSize, UnityEngine.Rect box, float angle = 0)
        {
            OriginalImage = null;
            Box = box;
            ImageSize = imageSize;
            Angle = angle;
        }
    }
}
