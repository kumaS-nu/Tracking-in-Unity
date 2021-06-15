
using OpenCvSharp;

using UnityEngine;

namespace kumaS.Tracker.Dlib
{
    /// <summary>
    /// Dlibの顔の68の特徴点のデータ。
    /// </summary>
    public class Dlib68Landmarks
    {
        /// <summary>
        /// 元画像。
        /// </summary>
        public Mat OriginalImage { get; }

        /// <summary>
        /// 顔の特徴点。
        /// </summary>
        public DlibDotNet.Point[] Landmarks { get; }

        /// <summary>
        /// 画像の大きさ。
        /// </summary>
        public Vector2Int ImageSize { get; }

        /// <summary>
        /// 元画像があるときのコンストラクタ。
        /// </summary>
        /// <param name="image">元画像。</param>
        /// <param name="landmarks">特徴点。</param>
        public Dlib68Landmarks(Mat image, DlibDotNet.Point[] landmarks)
        {
            OriginalImage = image;
            Landmarks = landmarks;
            ImageSize = new Vector2Int(image.Width, image.Height);
        }

        /// <summary>
        /// 元画像がないときのコンストラクタ。
        /// </summary>
        /// <param name="imageSize">元画像の大きさ。</param>
        /// <param name="landmarks">特徴点。</param>
        public Dlib68Landmarks(Vector2Int imageSize, DlibDotNet.Point[] landmarks)
        {
            Landmarks = landmarks;
            ImageSize = imageSize;
        }
    }
}
