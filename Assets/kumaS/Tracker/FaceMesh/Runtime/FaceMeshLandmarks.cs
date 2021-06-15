using kumaS.Tracker.Core;

using OpenCvSharp;

using UnityEngine;

namespace kumaS.Tracker.FaceMesh
{
    /// <summary>
    /// フェイスメッシュの特徴点。
    /// </summary>
    public class FaceMeshLandmarks : BoundaryBox
    {
        /// <summary>
        /// 特徴点。
        /// </summary>
        public Vector3[] Landmarks { get; private set; }

        /// <summary>
        /// 元画像のあるときのコンストラクタ。
        /// </summary>
        /// <param name="originalImage">元画像。</param>
        /// <param name="landmarks">特徴点。</param>
        /// <param name="box">バウンダリ―ボックス。</param>
        /// <param name="angle">角度。</param>
        public FaceMeshLandmarks(Mat originalImage, Vector3[] landmarks, UnityEngine.Rect box, float angle) : base(originalImage, box, angle)
        {
            Landmarks = landmarks;
        }

        /// <summary>
        /// 元画像がないときのコンストラクタ。
        /// </summary>
        /// <param name="imageSize">元画像のサイズ。</param>
        /// <param name="landmarks">特徴点。</param>
        /// <param name="box">バウンダリーボックス。</param>
        /// <param name="angle">角度。</param>
        public FaceMeshLandmarks(Vector2Int imageSize, Vector3[] landmarks, UnityEngine.Rect box, float angle) : base(imageSize, box, angle)
        {
            Landmarks = landmarks;
        }
    }
}
