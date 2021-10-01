using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using Cysharp.Threading.Tasks;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// ����𓝈�I�Ɉ������߂̃C���^�[�t�F�[�X�B
    /// </summary>
    public interface IVideo : IDisposable
    {
        /// <summary>
        /// �������ł������B
        /// </summary>
        public bool IsPrepared { get; }

        /// <summary>
        /// �摜���擾�B
        /// </summary>
        /// <returns>�摜�B</returns>
        public UniTask<Mat> Read();
    }
}
