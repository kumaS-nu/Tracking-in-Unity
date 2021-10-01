using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using OpenCvSharp;
using System.Runtime.InteropServices;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// Matを変換するクラス。
    /// </summary>
    public static class MatConverter
    {
        /// <summary>
        /// Color32からMatへ変換。
        /// </summary>
        /// <param name="data">変換するデータ。</param>
        /// <param name="height">画像の高さ。</param>
        /// <param name="width">画像の横幅。</param>
        /// <returns>Mat画像。</returns>
        public static Mat Color32ToMat(Color32[] data, int height, int width)
        {
            var mat = new Mat(height, width, MatType.CV_8UC3);
            var byteData = new byte[width * height * 3];
            for (var h = 0; h < height; h++)
            {
                for (var w = 0; w < width; w++)
                {
                    var colorIndex = data.Length - (h * width + width - w);
                    var byteIndex = 3 * (h * width + w);
                    byteData[byteIndex] = data[colorIndex].b;
                    byteData[byteIndex + 1] = data[colorIndex].g;
                    byteData[byteIndex + 2] = data[colorIndex].r;
                }
            }
            Marshal.Copy(byteData, 0, mat.Data, width * height * 3);
            return mat;
        }
    }
}
