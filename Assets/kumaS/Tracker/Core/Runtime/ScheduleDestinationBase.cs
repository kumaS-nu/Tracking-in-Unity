using System;
using System.Collections.Generic;

using UnityEngine;
using UniRx;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// スケジュール可能なストリームの末端の基底クラス。
    /// </summary>
    /// <typeparam name="T">スケジュール可能なデータ型。</typeparam>
    /// <typeparam name="TData">データ型。</typeparam>
    public abstract class ScheduleDestinationBase<T> : MonoBehaviour, IScheduleDestination
    {
        /// <summary>
        /// このプロセスの名前。初期化をさせるため抽象化。
        /// </summary>
        public abstract string ProcessName { get; set; }

        /// <summary>
        /// このノードのId。スケジューラーに設定されるので設定はそちらに任せる。
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ストリームで管理が必要な型で、このクラスで使う型。
        /// </summary>
        public abstract Type[] UseType { get; }

        /// <summary>
        /// 現在利用可能か。初期化が終わったら<c>true</c>を返すように。（初期化は<c>Awake()</c>等で。）
        /// </summary>
        public abstract IReadOnlyReactiveProperty<bool> IsAvailable { get; }

        /// <summary>
        /// 利用するデータ型。
        /// </summary>
        public Type DestinationType { get { return typeof(SchedulableData<T>); } }

        /// <summary>
        /// この段階での処理。メインスレッドが保証。
        /// </summary>
        /// <param name="input">流れてきたデータ。</param>
        public void Process(object input)
        {
            ProcessInternal((SchedulableData<T>)input);
        }

        /// <summary>
        /// この段階での処理をここに記述。メインスレッドが保証されている。
        /// </summary>
        /// <param name="input">流れてきたデータ。</param>
        protected abstract void ProcessInternal(SchedulableData<T> input);
    }
}

