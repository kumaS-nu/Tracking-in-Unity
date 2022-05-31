using OpenCvSharp;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace kumaS.Tracker.Dlib
{
    /// <summary>
    /// Dlib68�ɂ�����ڋʂ̈ʒu�������f�[�^�B
    /// </summary>
    public class Dlib68EyePoint
    {
        /// <summary>
        /// ��̓����_�B
        /// </summary>
        public DlibDotNet.Point[] Landmarks { get; }

        /// <summary>
        /// ���̍��ڂ̒��S�B
        /// </summary>
        public Vector2 LeftCenter { get; }

        /// <summary>
        /// �E�̍��ڂ̒��S�B
        /// </summary>
        public Vector2 RightCenter { get; }

        /// <summary>
        /// �摜�̑傫���B
        /// </summary>
        public Vector2Int ImageSize { get; }

        /// <summary>
        /// ���摜������Ƃ��̃R���X�g���N�^�B
        /// </summary>
        /// <param name="image">���摜�B</param>
        /// <param name="landmarks">�����_�B</param>
        public Dlib68EyePoint(Mat image, DlibDotNet.Point[] landmarks, Vector2 left, Vector2 right)
        {
            Landmarks = landmarks;
            ImageSize = new Vector2Int(image.Width, image.Height);
            LeftCenter = left;
            RightCenter = right;
        }

        /// <summary>
        /// ���摜���Ȃ��Ƃ��̃R���X�g���N�^�B
        /// </summary>
        /// <param name="imageSize">���摜�̑傫���B</param>
        /// <param name="landmarks">�����_�B</param>
        public Dlib68EyePoint(Vector2Int imageSize, DlibDotNet.Point[] landmarks, Vector2 left, Vector2 right)
        {
            Landmarks = landmarks;
            ImageSize = imageSize;
            LeftCenter = left;
            RightCenter = right;
        }
    }
}
