using System;


namespace kumaS.Tracker.Core
{
    /// <summary>
    /// スケジュール可能なストリームの基底クラス。
    /// </summary>
    internal interface IScheduleStream : ISchedule, IDebuggable
    {
        /// <summary>
        /// ストリームで管理が必要な型で、このクラスで使う型。
        /// </summary>
        Type[] UseType { get; }

        /// <summary>
        /// 入力のデータ型。
        /// </summary>
        Type InputType { get; }

        /// <summary>
        /// 出力するデータ型。
        /// </summary>
        Type OutputType { get; }

        /// <summary>
        /// ストリームをつなげる。（Hot変換はスケジューラー側。）
        /// </summary>
        /// <param name="input">このストリームにつなげる。</param>
        /// <returns>つなげたストリームを返すようにする。</returns>
        IObservable<object> Process(IObservable<object> input);
    }
}
