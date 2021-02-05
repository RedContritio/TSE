using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace TSE_Core
{
    public class WordsManager
    {
        /// <summary>
        /// 页面词频
        /// key : 页面 idx
        /// value : 词频字典，键值对为 {word idx, count} 
        /// </summary>
        Dictionary<int, Dictionary<int, int>> FrequencyPerPage =  new Dictionary<int, Dictionary<int, int>>();

        Dictionary<int, int> WordsCountPerPage = new Dictionary<int, int>();

        Dictionary<int, string> idx2word = new Dictionary<int, string>();

        Dictionary<string, int> word2idx = new Dictionary<string, int>();

        /// <summary>
        /// key : 词 id
        /// value : 词密度字典，键值对为 {page idx, frequency / total_count}
        /// </summary>
        Dictionary<int, Dictionary<int, double>> wordQpage = new Dictionary<int, Dictionary<int, double>>();


        /// <summary>
        /// 添加一个分词后的页面表，该页面必须尚未添加
        /// 若添加成功，则返回 true
        /// 否则返回 false
        /// </summary>
        /// <param name="idx">页面 id</param>
        /// <param name="words">该页面的分词结果</param>
        /// <returns></returns>
        public bool Add(int idx, List<string> words) {
            if(FrequencyPerPage.ContainsKey(idx)) return false;
            Dictionary<int, int> frequency = new Dictionary<int, int>();
            foreach(string word in words)
            {
                // 添加新词
                if(!word2idx.ContainsKey(word))
                    RegisterNewWord(idx, word);

                int key = word2idx[word];

                if (!frequency.ContainsKey(key))
                {
                    frequency.Add(key, 0);
                }

                frequency[key] += 1;
            }

            FrequencyPerPage.Add(idx, frequency);
            foreach (string word in words)
            {
                int key = word2idx[word];
                if(!wordQpage[key].ContainsKey(idx))
                    wordQpage[key].Add(idx, 1.0 * frequency[key] / words.Count );
                else
                    wordQpage[key][idx] = 1.0 * frequency[key] / words.Count;
            }
            return true;
        }

        private int RegisterNewWord(int pageid, string word) {
            if (!word2idx.ContainsKey(word))
            {
                idx2word.Add(word2idx.Count, word);
                word2idx.Add(word, word2idx.Count);
                wordQpage.Add(word2idx[word], new Dictionary<int, double>());
            }
            return word2idx[word];
        }

        public void ShowFrequencyAtPage(int pageid) {
            foreach(int wordid in FrequencyPerPage[pageid].Keys)
            {
                Console.WriteLine("word: {0}, frequency: {1}", idx2word[wordid], FrequencyPerPage[pageid][wordid]);
            }
        }

        public Dictionary<int, double> QueryAllPages(string word)
        {
            if(!word2idx.ContainsKey(word))
                return new Dictionary<int, double>();

            return wordQpage[word2idx[word]];
        }
        public Dictionary<int, double> QueryAllPages(int wordid)
        {
            if (!idx2word.ContainsKey(wordid))
                return new Dictionary<int, double>();

            return QueryAllPages(idx2word[wordid]);
        }

        public List<int> GetWordsIndex(List<string> words)
        {
            List<int> idxs = new List<int>();
            foreach (string word in words)
            {
                if(!word2idx.ContainsKey(word))
                    continue;
                idxs.Add(word2idx[word]);
            }
            return idxs;
        }

        public void LoadFromLocal()
        {
            FrequencyPerPage = DBReader.LoadVariable<Dictionary<int, Dictionary<int, int>>>("FrequencyPerPage");
            idx2word = DBReader.LoadVariable<Dictionary<int, string>>("idx2word");
            wordQpage = DBReader.LoadVariable<Dictionary<int, Dictionary<int, double>>>("wordQpage");
            foreach(int idx in idx2word.Keys)
            {
                if(!word2idx.ContainsKey(idx2word[idx]))
                    word2idx.Add(idx2word[idx], idx);
                else
                    word2idx[idx2word[idx]] = idx;
            }
        }

        public void SaveToLocal()
        {
            DBReader.SaveVariable(FrequencyPerPage, "FrequencyPerPage");
            DBReader.SaveVariable(idx2word, "idx2word");
            DBReader.SaveVariable(wordQpage, "wordQpage");
        }
    }
}
