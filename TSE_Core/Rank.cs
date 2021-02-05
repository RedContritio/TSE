using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winista.Text.HtmlParser.Lex;

namespace TSE_Core
{
    public static class Rank
    {
        public static Dictionary<int, HashSet<int>> pagelinks;

        public static Dictionary<int, double> pageRank = new Dictionary<int, double>();

        public static Dictionary<int, double> wordRank = new Dictionary<int, double>();

        static double _alpha = 0.5;

        /// <summary>
        /// 将得到的搜索结果排序
        /// </summary>
        /// <param name="wordsManager"> 单词管理器 </param>
        /// <param name="PagesIdx"> 得到的搜索结果 </param>
        /// <param name="wordsIdx"> 搜索词 </param>
        /// <param name="alpha"> PR 的权重 </param>
        /// <returns></returns>
        public static List<int> SortResult(WordsManager wordsManager, List<int> PagesIdx, List<int> wordsIdx, double alpha = 0.5)
        {
            _alpha = alpha;
            wordRank.Clear();
            foreach (int id in PagesIdx)
            {
                wordRank.Add(id, 0);
            }

            foreach (int id in wordsIdx)
            {
                var dic = wordsManager.QueryAllPages(id);
                foreach (int key in dic.Keys)
                {
                    wordRank[key] += dic[key];
                }
            }

            // 初始化了 wordRank，开始排序
            PagesIdx.Sort(new Rank.RankCompare());

            for(int i=0; i<5 && i < PagesIdx.Count; ++i)
            {
                Console.WriteLine("id: {0}, rank: {1} + {2} = {3}", PagesIdx[i],
                    pageRank[PagesIdx[i]], wordRank[PagesIdx[i]], GetRank(PagesIdx[i]));
            }

            return PagesIdx;
        }

        private static double GetRank(int pageid)
        {
            return _alpha * (Math.Pow(pageRank[pageid], 0.25)) + (1 - _alpha) * (wordRank[pageid]);
        }

        public static void CalculateRP(double PR0 = 1, double d = 0.85, double epsilon = 0.00001)
        {
            pageRank.Clear();
            foreach(int idx in pagelinks.Keys)
            {
                pageRank.Add(idx, PR0);
            }

            Dictionary<int, double> tmp = new Dictionary<int, double>();
            double diff;
            do
            {
                diff = 0;
                tmp = new Dictionary<int, double>();

                foreach (int idx in pageRank.Keys)
                {
                    //tmp.Add(idx, d);
                    tmp[idx] = d;
                }

                foreach (int idx in pageRank.Keys)
                {
                    foreach (int dstid in pagelinks[idx])
                        tmp[dstid] += d * pageRank[idx] / pagelinks[idx].Count;
                }

                foreach (int idx in pageRank.Keys)
                {
                    double absdiff = Math.Abs(pageRank[idx] - tmp[idx]);
                    if(diff < absdiff)
                        diff  = absdiff;
                }

                //Console.WriteLine("PR: {0}", String.Join(" ", tmp.Values));
                //Console.WriteLine("diff is {0}", diff);
                pageRank = tmp;
            } while(diff > epsilon);
        }

        public static void SaveToLocal()
        {
            DBReader.SaveVariable(pagelinks, "pagelinks");
            DBReader.SaveVariable(pageRank, "PageRank");
        }

        public static void LoadFromLocal()
        {
            pagelinks = DBReader.LoadVariable<Dictionary<int, HashSet<int>>>("pagelinks");
            pageRank = DBReader.LoadVariable<Dictionary<int, double>>("PageRank");
        }

        public class PRCompare : IComparer<int>
        {
            /// <summary>
            /// 按照 PR 降序
            /// </summary>
            /// <param name="idA"></param>
            /// <param name="idB"></param>
            /// <returns></returns>
            public int Compare(int idA, int idB)
            {
                return -pageRank[idA].CompareTo(pageRank[idB]);
            }
        }
        public class WFCompare : IComparer<int>
        {
            /// <summary>
            /// 按照 WF (word frequency) 降序
            /// </summary>
            /// <param name="idA"></param>
            /// <param name="idB"></param>
            /// <returns></returns>
            public int Compare(int idA, int idB)
            {
                return -wordRank[idA].CompareTo(wordRank[idB]);
            }
        }

        public class RankCompare : IComparer<int>
        {
            /// <summary>
            /// 综合 PR 和 WF 降序
            /// </summary>
            /// <param name="idA"></param>
            /// <param name="idB"></param>
            /// <returns></returns>
            public int Compare(int idA, int idB)
            {
                return -GetRank(idA).CompareTo(GetRank(idB));
            }
        }
    }
}
