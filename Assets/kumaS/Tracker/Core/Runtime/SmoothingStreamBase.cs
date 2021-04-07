using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;

using UniRx;

using UnityEngine;
using Cysharp.Threading.Tasks;

namespace kumaS.Tracker.Core
{
    public abstract class SmoothingStreamBase<T> : ScheduleStreamBase<T, T>
    {
        [SerializeField]
        internal int bufferSize = 8;

        [SerializeField]
        internal KeyCode resetKey = KeyCode.R;

        public override Type[] UseType { get; } = new Type[0];
        public override IReadOnlyReactiveProperty<bool> IsAvailable { get; } = new ReactiveProperty<bool>(true);

        protected ConcurrentQueue<T> buffer = new ConcurrentQueue<T>();
        protected T lastOutput = default;
        private volatile bool isRequiredReset = false;

        public override void InitInternal(int thread){
            Observable.EveryUpdate().Where(_ => Input.GetKey(resetKey)).Subscribe(_ => isRequiredReset = true);
        }
        protected override SchedulableData<T> ProcessInternal(SchedulableData<T> input)
        {
            if (!input.IsSuccess)
            {
                return new SchedulableData<T>(input, default);
            }
            if (isRequiredReset)
            {
                while(buffer.TryDequeue(out _)){ }
            }

            var start = DateTime.Now;
            var count = buffer.Count < 8 ? buffer.Count + 1 : 8;
            var data = new T[count];
            if(buffer.Count < bufferSize)
            {
                buffer.Enqueue(input.Data);
                buffer.CopyTo(data, 0);
            }
            else
            {
                if (ValidateData(input.Data))
                {
                    buffer.Enqueue(input.Data);
                    while (!buffer.TryDequeue(out _)){ }
                }
                buffer.CopyTo(data, 0);
            }
            var output = Average(data);
            lastOutput = output;
            return new SchedulableData<T>(input, output);
        }

        /// <summary>
        /// 入ってきたデータが外れ値か判断するものを書く。<c>lastOutput</c>と比べること。
        /// </summary>
        /// <param name="input">判別するデータ。</param>
        /// <returns>正常値なら<c>true</c>。</returns>
        protected abstract bool ValidateData(T input);

        /// <summary>
        /// バッファにあるデータを平均するものを書く。
        /// </summary>
        /// <param name="datas">バッファにあるデータ。</param>
        /// <returns>平均のデータ。</returns>
        protected abstract T Average(T[] datas);
    }
}
