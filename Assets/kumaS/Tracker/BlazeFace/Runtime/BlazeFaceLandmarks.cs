
using kumaS.Tracker.Core;

using OpenCvSharp;

using UnityEngine;

namespace kumaS.Tracker.BlazeFace
{
    /// <summary>
    /// BlazeFaceの顔の特徴点。
    /// </summary>
    public class BlazeFaceLandmarks : BoundaryBox
    {

        /// <summary>
        /// 顔の特徴点。
        /// </summary>
        public Vector2[] Landmarks { get; }


        /// <summary>
        /// 元画像があるときのコンストラクタ。
        /// </summary>
        /// <param name="originalImage">元画像。</param>
        /// <param name="landmarks">特徴点。</param>
        /// <param name="box">バウンダリーボックス。</param>
        /// <param name="angle">頭の傾き。</param>
        public BlazeFaceLandmarks(Mat originalImage, Vector2[] landmarks, UnityEngine.Rect box, float angle) : base(originalImage, box, angle)
        {
            Landmarks = landmarks;
        }

        /// <summary>
        /// 元画像がないときのコンストラクタ。
        /// </summary>
        /// <param name="imageSize">元画像の大きさ。</param>
        /// <param name="landmarks">特徴点。</param>
        /// <param name="box">バウンダリーボックス。</param>
        /// <param name="angle">頭の傾き。</param>
        public BlazeFaceLandmarks(Vector2Int imageSize, Vector2[] landmarks, UnityEngine.Rect box, float angle) : base(imageSize, box, angle)
        {
            Landmarks = landmarks;
        }
    }
}
