using OpenCvSharp;

using UnityEngine;

namespace kumaS.Tracker.PoseNet
{
    /// <summary>
    /// PoseNetの特徴点のデータ。
    /// </summary>
    public class PoseNetLandmarks
    {
        /// <summary>
        /// 元画像。
        /// </summary>
        public Mat OriginalImage { get; }

        /// <summary>
        /// 特徴点。
        /// </summary>
        public Vector2[] Landmarks { get; }

        /// <summary>
        /// 画像の大きさ。
        /// </summary>
        public Vector2Int ImageSize { get; }

        /// <summary>
        /// 元画像があるときのコンストラクタ。
        /// </summary>
        /// <param name="originalImage">元画像。</param>
        /// <param name="landmarks">特徴点。</param>
        public PoseNetLandmarks(Mat originalImage, Vector2[] landmarks)
        {
            OriginalImage = originalImage;
            Landmarks = landmarks;
            ImageSize = new Vector2Int(originalImage.Width, originalImage.Height);
        }

        /// <summary>
        /// 元画像がないときのコンストラクタ。
        /// </summary>
        /// <param name="imageSize">画像の大きさ。</param>
        /// <param name="landmarks">特徴点。</param>
        public PoseNetLandmarks(Vector2Int imageSize, Vector2[] landmarks)
        {
            ImageSize = imageSize;
            Landmarks = landmarks;
        }
    }
}
