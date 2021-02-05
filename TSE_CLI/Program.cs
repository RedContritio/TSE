using JiebaNet.Segmenter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TSE_Core;

namespace TSE_CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            //LaunchSpider("http://www.nwpu.edu.cn");

            DBReader reader = new DBReader();

            reader.ReadDBIndex();

            reader.ReadDBContent();

            //CalculateArgument(reader);

            WordsManager wordsManager = new WordsManager();
            wordsManager.LoadFromLocal();

            Rank.LoadFromLocal();

            Console.WriteLine("初始化完成");

            while (true) 
            {
                string text = Console.ReadLine();
                if(text.ToLower() == "exit")
                    break;
                
                List<string> words = Splitter.GetWords(text);

                List<int> results = GetRawSearchResult(wordsManager, words);
                List<int> wordidxs = wordsManager.GetWordsIndex(words);

                results = Rank.SortResult(wordsManager, results, wordidxs, 0.001);

                Console.WriteLine(String.Join("\n", reader.PageIdx2Description(results)));
            }
        }

        static List<int> GetRawSearchResult(WordsManager wordsManager, List<string> words)
        {
            HashSet<int> pages = new HashSet<int>();

            foreach (string word in words)
            {
                pages.UnionWith(wordsManager.QueryAllPages(word).Keys);
            }

            return pages.ToList();
        }

        static void LaunchSpider(string source, int max_count = 5000)
        {
            WebSpider spider = new WebSpider();
            spider.SetSourcePage(source);

            while (spider.CountToVisit > 0 && spider.Count < max_count)
            {
                WEBPAGE page = spider.SingleStep();
            }
        }

        static WordsManager CalculateWords(DBReader reader, List<int> idxs = null)
        {
            if(idxs == null)
                idxs = reader.GetAllIndex();

            WordsManager wordsManager = new WordsManager();

            foreach (int idx in idxs)
            {
                string content = reader.GetPageByIndex(idx).raw_source;

                string text = htmlParser.Html2PlainText(content);

                wordsManager.Add(idx, Splitter.GetWords(text));
            }

            return wordsManager;
        }

        static void CalculatePageRank(DBReader reader, List<int> idxs = null, double PR0 = 1, double d = 0.85, double epsilon = 0.001)
        {
            if (idxs == null)
                idxs = reader.GetAllIndex();

            Rank.pagelinks = new Dictionary<int, HashSet<int>>();

            foreach (int idx in idxs)
            {
                HashSet<int> ts = reader.GetPageByIndex(idx).point_id;
                ts.IntersectWith(idxs);
                Rank.pagelinks.Add(idx, ts);
            }

            Rank.CalculateRP(PR0, d, epsilon);
        }

        /// <summary>
        /// 计算出相关参数并且保存到本地
        ///// 参数包括 词的倒排表、页面RP
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="idx">要求的页面idx列表</param>
        static void CalculateArgument(DBReader reader, List<int> idx = null)
        {
            Console.WriteLine("start arguments calculating.");

            CalculatePageRank(reader, idx, epsilon: 0.000001);

            Console.WriteLine("page rank calculated.");

            Rank.SaveToLocal();

            WordsManager wordsManager = CalculateWords(reader, idx);

            Console.WriteLine("words calculated.");

            wordsManager.SaveToLocal();
        }
    }
}
