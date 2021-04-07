using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// 正規化しないバウンディングボックス。
    /// </summary>
    public class BoundaryBox
    {
        public Mat OriginalImage { get; }
        public UnityEngine.Rect Box { get; }

        public Vector2Int ImageSize { get; }

        public BoundaryBox(Mat originalImage, UnityEngine.Rect box)
        {
            OriginalImage = originalImage;
            Box = box;
            ImageSize = new Vector2Int(originalImage.Width, originalImage.Height);
        }

        public BoundaryBox(Vector2Int imageSize, UnityEngine.Rect box)
        {
            OriginalImage = null;
            Box = box;
            ImageSize = imageSize;
        }
    }
}
