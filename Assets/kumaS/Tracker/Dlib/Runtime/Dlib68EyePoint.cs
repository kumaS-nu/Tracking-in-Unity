using OpenCvSharp;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace kumaS.Tracker.Dlib
{
    /// <summary>
    /// Dlib68における目玉の位置情報を持つデータ。
    /// </summary>
    public class Dlib68EyePoint
    {
        /// <summary>
        /// 顔の特徴点。
        /// </summary>
        public DlibDotNet.Point[] Landmarks { get; }

        /// <summary>
        /// 左の黒目の中心。
        /// </summary>
        public Vector2 LeftCenter { get; }

        /// <summary>
        /// 右の黒目の中心。
        /// </summary>
        public Vector2 RightCenter { get; }

        /// <summary>
        /// 画像の大きさ。
        /// </summary>
        public Vector2Int ImageSize { get; }

        /// <summary>
        /// 元画像があるときのコンストラクタ。
        /// </summary>
        /// <param name="image">元画像。</param>
        /// <param name="landmarks">特徴点。</param>
        public Dlib68EyePoint(Mat image, DlibDotNet.Point[] landmarks, Vector2 left, Vector2 right)
        {
            Landmarks = landmarks;
            ImageSize = new Vector2Int(image.Width, image.Height);
            LeftCenter = left;
            RightCenter = right;
        }

        /// <summary>
        /// 元画像がないときのコンストラクタ。
        /// </summary>
        /// <param name="imageSize">元画像の大きさ。</param>
        /// <param name="landmarks">特徴点。</param>
        public Dlib68EyePoint(Vector2Int imageSize, DlibDotNet.Point[] landmarks, Vector2 left, Vector2 right)
        {
            Landmarks = landmarks;
            ImageSize = imageSize;
            LeftCenter = left;
            RightCenter = right;
        }
    }
}
