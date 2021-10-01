using Cysharp.Threading.Tasks;

using System;
using System.Linq;

using UniRx;

using UnityEngine;
using System.Threading;
using System.Collections.Generic;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// データを平滑化するための基底クラス。
    /// </summary>
    /// <typeparam name="T">平滑化するデータの型。</typeparam>
    public abstract class SmoothingStreamBase<T> : ScheduleStreamBase<T, T>
    {
        public int bufferSize = 8;

        public KeyCode resetKey = KeyCode.R;

        public override Type[] UseType { get; } = new Type[0];
        public override IReadOnlyReactiveProperty<bool> IsAvailable { get; } = new ReactiveProperty<bool>(true);

        protected LinkedList<T> buffer = new LinkedList<T>();
        protected T lastOutput = default;
        private volatile bool isRequiredReset = false;

        /// <inheritdoc/>
        protected override void InitInternal(int thread, CancellationToken token)
        {
            InitInternal2(thread, token);
            var interfaces = typeof(T).GetInterfaces();
            Observable.EveryUpdate().Where(_ => Input.GetKey(resetKey)).Subscribe(_ => isRequiredReset = true).AddTo(this);
        }

        /// <summary>
        /// 初期化処理をここに書く。
        /// </summary>
        /// <param name="thread">スレッド数。</param>
        /// <param name="token">キャンセル要求。</param>
        protected abstract void InitInternal2(int thread, CancellationToken token);

        /// <inheritdoc/>
        protected override SchedulableData<T> ProcessInternal(SchedulableData<T> input)
        {
            if (isRequiredReset)
            {
                buffer = new LinkedList<T>();
                isRequiredReset = false;
            }
            T removed = default;
            T add;
            bool isRemoved = false;
            if (!input.IsSuccess || input.IsSignal)
            {
                lock (buffer)
                {
                    if (buffer.Count == 0)
                    {
                        if (input.IsSuccess)
                        {
                            return new SchedulableData<T>(input, default, false, errorMessage: "まだデータはありません。");
                        }
                        else
                        {
                            return new SchedulableData<T>(input, default);
                        }
                    }
                    add = buffer.Last.Value;
                }
            }
            else
            {
                add = input.Data;
            }

            T[] data;
            if (buffer.Count < bufferSize)
            {
                lock (buffer)
                {
                    buffer.AddLast(add);
                    data = buffer.ToArray();
                }
            }
            else
            {
                if (ValidateData(add))
                {
                    isRemoved = true;
                    lock (buffer)
                    {
                        removed = buffer.First.Value;
                        buffer.RemoveFirst();
                        buffer.AddLast(add);
                        data = buffer.ToArray();
                    }
                }
                else
                {
                    lock (buffer)
                    {
                        data = buffer.ToArray();
                    }
                }
            }

            if (buffer.Count == 1)
            {
                lastOutput = input.Data;
                return new SchedulableData<T>(input, input.Data, enfoce: true);
            }
            T output = Average(data, removed, isRemoved);
            lastOutput = output;
            return new SchedulableData<T>(input, output, enfoce: true);
        }

        /// <summary>
        /// 入ってきたデータが外れ値か判断するものを書く。<c>lastOutput</c>と比べること。
        /// </summary>
        /// <param name="input">判別するデータ。</param>
        /// <returns>正常値なら<c>true</c>。</returns>
        protected abstract bool ValidateData(T input);

        /// <summary>
        /// バッファにあるデータを平均するものを書く。データは必ず2つ以上ある。
        /// </summary>
        /// <param name="datas">バッファにあるデータ。新しいデータは後ろに、古いデータは前にある。</param>
        /// <param name="removed">取り除かれるデータ。</param>
        /// <param name="isRemoved">取り除かれるものはあるか。</param>
        /// <returns>平均のデータ。</returns>
        protected abstract T Average(T[] datas, T removed, bool isRemoved);
    }
}
