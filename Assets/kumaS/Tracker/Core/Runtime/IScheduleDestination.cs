using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// スケジュール可能な終点のインターフェース。
    /// </summary>
    internal interface IScheduleDestination : ISchedule
    {
        /// <summary>
        /// ストリームで管理が必要な型で、このクラスで使う型。
        /// </summary>
        Type[] UseType { get; }

        /// <summary>
        /// 利用するデータ型。
        /// </summary>
        Type DestinationType { get; }

        /// <summary>
        /// この段階での処理をここに記述。メインスレッドが保証されている。
        /// </summary>
        /// <param name="input">流れてきたデータ。</param>
        void Process(object input);
    }
}