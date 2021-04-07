using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// ストリームをまたぐ、解放が必要な型がどの時点で解放すればいいか監理する。
    /// </summary>
    public static class ResourceManager
    {
        private static Dictionary<Type, List<int>> releaseId = new Dictionary<Type, List<int>>();

        /// <summary>
        /// 解放するノードを設定。
        /// </summary>
        /// <param name="allNodes">全てのノード。</param>
        internal static void SetResource(List<StreamNode> allNodes)
        {
            releaseId.Clear();

            foreach(var node in allNodes)
            {
                foreach (Type type in GetUseType(node))
                {
                    if (!node.Next.SelectMany(n => GetUseType(n)).Any(t => t == type))
                    {
                        if (!releaseId.ContainsKey(type))
                        {
                            releaseId.Add(type, new List<int>());
                        }
                        releaseId[type].Add(allNodes.IndexOf(node));
                    }
                }
            }
        }

        /// <summary>
        /// 使う型を取得する。
        /// </summary>
        /// <param name="node">ノード。</param>
        /// <returns>使う型。</returns>
        private static Type[] GetUseType(StreamNode node)
        {
            if(node.Schedulable is IScheduleStream)
            {
                return ((IScheduleStream)node.Schedulable).UseType;
            }

            if(node.Schedulable is IScheduleDestination)
            {
                return ((IScheduleDestination)node.Schedulable).UseType;
            }

            return new Type[0];
        }

        /// <summary>
        /// 型とノードのIdから解放するものか返す。
        /// </summary>
        /// <param name="type">型。</param>
        /// <param name="id">Id。</param>
        /// <returns>解放するか。</returns>
        public static bool isRelease(Type type, int id)
        {
            return releaseId[type].Contains(id);
        }
    }
}
