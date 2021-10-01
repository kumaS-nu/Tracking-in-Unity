using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using Cysharp.Threading.Tasks;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// 動画を統一的に扱うためのインターフェース。
    /// </summary>
    public interface IVideo : IDisposable
    {
        /// <summary>
        /// 準備ができたか。
        /// </summary>
        public bool IsPrepared { get; }

        /// <summary>
        /// 画像を取得。
        /// </summary>
        /// <returns>画像。</returns>
        public UniTask<Mat> Read();
    }
}
