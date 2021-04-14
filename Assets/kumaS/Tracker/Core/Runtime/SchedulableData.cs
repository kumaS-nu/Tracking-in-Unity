using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// スケジュール可能なデータのクラス。
    /// </summary>
    /// <typeparam name="T">このデータ型。</typeparam>
    public class SchedulableData<T> : ISchedulableMetadata
    {
        /// <summary>
        /// このデータは成功しているか。
        /// </summary>
        public  bool IsSuccess { get; private set; }

        /// <summary>
        /// エラー内容。
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// このデータストリームの始まった時間。画像を取得した時間とか。
        /// </summary>
        public DateTime StartTime { get; private set; }

        /// <summary>
        /// このデータストリームの始点のId。
        /// </summary>
        public int SourceId { get; private set; }

        /// <summary>
        /// 各フェーズの経過時間。
        /// </summary>
        public List<TimeSpan> ElapsedTimes { get; private set; }

        /// <summary>
        /// データ。
        /// </summary>
        public T Data { get; private set; }

        /// <summary>
        /// ストリーム中のコンストラクタ。
        /// </summary>
        /// <param name="input">入力のスケジュール可能なデータ。</param>
        /// <param name="data">出力データ。</param>
        /// <param name="isSuccess">この段階で失敗したとき<c>false</c>にする。</param>
        /// <param name="errorMessage">この段階でのエラー内容。</param>
        public SchedulableData(ISchedulableMetadata input, T data, bool isSuccess = true, string errorMessage = "")
        {
            IsSuccess = input.IsSuccess && isSuccess;
            if (isSuccess)
            {
                ErrorMessage = input.ErrorMessage;
            }
            else
            {
                ErrorMessage = errorMessage;
            }
            StartTime = input.StartTime;
            SourceId = input.SourceId;
            ElapsedTimes = new List<TimeSpan>(input.ElapsedTimes);
            var elapsedtick = 0L;
            foreach(var elapsed in ElapsedTimes)
            {
                elapsedtick += elapsed.Ticks;
            }
            ElapsedTimes.Add(DateTime.Now - StartTime - new TimeSpan(elapsedtick));
            Data = data;
        }

        /// <summary>
        /// ストリームの始点のコンストラクタ。
        /// </summary>
        /// <param name="data">データ</param>
        /// <param name="id">ソースのId。</param>
        /// <param name="startTime">パイプライン処理の開始時間。</param>
        /// <param name="isSuccess">このデータは成功いるか。</param>
        /// <param name="errorMessage">エラー内容。</param>
        public SchedulableData(T data, int id, DateTime startTime, bool isSuccess = true, string errorMessage = "")
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            StartTime = startTime;
            SourceId = id;
            ElapsedTimes = new List<TimeSpan>();
            ElapsedTimes.Add(DateTime.Now - startTime);
            Data = data;
        }

        public static readonly string Elapsed_Time = nameof(Elapsed_Time);

        public void ToDebugElapsedTime(Dictionary<string, string> message)
        {
            message[Elapsed_Time] = ElapsedTimes[ElapsedTimes.Count - 1].TotalMilliseconds.ToString("F") + "ms";
        }
    }
}