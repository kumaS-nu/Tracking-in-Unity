using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// インスペクターでUnityのパスを使うためのutil。
    /// </summary>
    public static class PathUtil
    {
        public static string[] PathHeadLabel { get { Init(); return pathHeadLabel.ToArray(); }}
        private static List<string> pathHeadLabel = new List<string>();
        public static string[] PathHead { get { Init(); return pathHead.ToArray(); } }
        private static List<string> pathHead = new List<string>();

        private static void Init()
        {
            if(pathHead.Count == 0)
            {
                pathHead = new List<string>();
                pathHeadLabel = new List<string>();
                pathHead.Add("");
                pathHeadLabel.Add("URL");
                pathHead.Add("file://");
                pathHeadLabel.Add("Absolute path");
                pathHead.Add(Path.Combine("file://", Application.dataPath));
                pathHeadLabel.Add(nameof(Application.dataPath));
                pathHead.Add(Path.Combine("file://", Application.dataPath));
                pathHeadLabel.Add(nameof(Application.streamingAssetsPath));
                pathHead.Add(Path.Combine("file://", Application.persistentDataPath));
                pathHeadLabel.Add(nameof(Application.persistentDataPath));
            }
        }

        /// <summary>
        /// パス名をシリアライズ。
        /// </summary>
        /// <param name="fileName">入力されたパス名。</param>
        /// <param name="pathType">パスの種類。</param>
        /// <param name="useUnity">Unityを使うか。</param>
        /// <returns>シリアライズされたパス名。</returns>
        public static string Serialize(string fileName, int pathType, bool useUnity)
        {
            Init();
            if (pathType != 1 && fileName != "" && (fileName[0] == '\\' || fileName[0] == '/'))
            {
                fileName = fileName.Remove(0, 1);
            }
            if (pathType != 0)
            {
                try
                {
                    fileName = Path.Combine(pathHead[pathType], fileName);
                }
                catch (Exception) { }
            }
            if (!useUnity)
            {
                fileName = fileName.Replace("file://", "");
            }

            return fileName;
        }

        /// <summary>
        /// パス名をデシリアライズ。
        /// </summary>
        /// <param name="fileName">シリアライズされたパス名。</param>
        /// <param name="pathType">パスの種類。</param>
        /// <returns>デシリアライズされたパス名。</returns>
        public static string Deserialize(string fileName, int pathType)
        {
            Init();
            if (fileName != "")
            {
                fileName = fileName.Replace("file://", "");
                if (pathType > 1)
                {
                    fileName = fileName.Replace(pathHead[pathType].Replace("file://", ""), "");
                }
            }

            return fileName;
        }
    }
}
