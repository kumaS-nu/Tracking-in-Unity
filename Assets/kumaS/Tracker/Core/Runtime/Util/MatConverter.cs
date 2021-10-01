using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using OpenCvSharp;
using System.Runtime.InteropServices;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// Mat��ϊ�����N���X�B
    /// </summary>
    public static class MatConverter
    {
        /// <summary>
        /// Color32����Mat�֕ϊ��B
        /// </summary>
        /// <param name="data">�ϊ�����f�[�^�B</param>
        /// <param name="height">�摜�̍����B</param>
        /// <param name="width">�摜�̉����B</param>
        /// <returns>Mat�摜�B</returns>
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
