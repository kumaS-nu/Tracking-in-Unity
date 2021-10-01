using System;
using System.Collections.Generic;
using System.Linq;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// ストリームをまたぐ、解放が必要な型がどの時点で解放すればいいか監理する。
    /// </summary>
    public static class ResourceManager
    {
        private static readonly Dictionary<Type, List<int>> releaseId = new Dictionary<Type, List<int>>();

        /// <summary>
        /// 解放するノードを設定。
        /// </summary>
        /// <param name="allNodes">全てのノード。</param>
        internal static void SetResource(List<StreamNode> allNodes)
        {
            releaseId.Clear();

            foreach (StreamNode node in allNodes)
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
            if (node.Schedulable is IScheduleStream)
            {
                return ((IScheduleStream)node.Schedulable).UseType;
            }

            if (node.Schedulable is IScheduleDestination)
            {
                return ((IScheduleDestination)node.Schedulable).UseType;
            }

            return new Type[0];
        }

        /// <summary>
        /// もし開放すべきリソースであれば解放する。
        /// </summary>
        /// <typeparam name="T">解放する型。</typeparam>
        /// <param name="obj">解放するオブジェクト。</param>
        /// <param name="id">ノードのId。</param>
        public static void DisposeIfRelease<T>(T obj, int id) where T: IDisposable
        {
            if(obj != null && IsRelease(obj.GetType(), id)){
                obj.Dispose();
            }
        }

        /// <summary>
        /// 型とノードのIdから解放するものか返す。
        /// </summary>
        /// <param name="type">解放する型。</param>
        /// <param name="id">ノードのId。</param>
        /// <returns>解放するか。</returns>
        private static bool IsRelease(Type type, int id)
        {
            return releaseId[type].Contains(id);
        }
    }
}
