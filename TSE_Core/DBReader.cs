using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace TSE_Core
{
    public class DBReader
    {
        private static string SaveFilePath
        {
            get { return ConfigurationManager.AppSettings["SaveFilePath"] ?? @"..\Save"; }
        }
        public Dictionary<string, int> URL2IDX
        {
            get { return url2idx; }
        }

        public Dictionary<int, string> IDX2URL
        {
            get { return idx2url; }
        }

        private SQLiteConnection connection = null;

        Dictionary<int, string> idx2url = new Dictionary<int, string>();

        Dictionary<string, int> url2idx = new Dictionary<string, int>();

        Dictionary<string, WEBPAGE> url2page = new Dictionary<string, WEBPAGE>();
        public DBReader()
        {
            connection = WebData.ConnectDB();
            connection.Open();
        }

        public WEBPAGE GetPageByIndex(int idx)
        {
            if(idx2url.ContainsKey(idx))
            {
                return url2page[idx2url[idx]];
            }
            return null;
        }

        public void ReadDBIndex()
        {
            SQLiteDataReader reader = WebData.GetReaderByExecution(connection, "select url, idx from webs");
            string url;
            int idx;
            url2idx.Clear();
            idx2url.Clear();
            while (reader.Read())
            {
                url = reader.GetString(0);
                idx = reader.GetInt32(1);
                url2idx.Add(url, idx);
                idx2url.Add(idx, url);
            }
        }

        public int ReadDBContent()
        {
            SQLiteDataReader reader = WebData.GetReaderByExecution(connection, "select url, md5 from webs");
            string url, md5;
            url2page.Clear();
            while (reader.Read())
            {
                url = reader.GetString(0);
                md5 = reader.GetString(1);
                int idx = url2idx[url];
                if (WebData.LoadFromLocal(idx, out string content))
                {
                    if(content.GetMD5() == md5)
                        url2page.Add(url, new WEBPAGE(url, content));
                    else
                        Console.WriteLine("File at index {0} has different md5, md5 of content is {1}, but in db is {2}", idx, content.GetMD5(), md5);
                }
                else
                {
                    Console.WriteLine("Failed to load file at index {0}", idx);
                }
            }

            foreach (string src in url2idx.Keys)
            {
                foreach (string key in url2page[src].LINKS)
                {
                    if(url2idx.ContainsKey(key))
                        url2page[src].point_id.Add(url2idx[key]);
                }
            }
            return url2page.Count;
        }

        public List<int> GetAllIndex()
        {
            return idx2url.Keys.ToList();
        }

        public List<string> PageIdx2Description(List<int> idx, int count = 10)
        {
            List<string> texts = new List<string>();
            foreach (int id in idx)
            {
                WEBPAGE page = GetPageByIndex(id);
                string text = String.Format("PAGE_ID: {0}\n TITLE: {1}\nURL: {2}\n", id, page.TITLE, page.URL);
                texts.Add(text);
            }
            return texts.Take(count).ToList();
        }

        public static void SaveVariable<T>(T variable, string name)
        {
            // .wmc means .WordsManagerCache
            FileStream fileStream = new FileStream(SaveFilePath + string.Format("\\{0}.wmc", name), FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(fileStream, variable);
            fileStream.Close();
        }

        public static T LoadVariable<T>(string name) where T : class
        {
            // .wmc means .WordsManagerCache
            FileStream fileStream = new FileStream(SaveFilePath + string.Format("\\{0}.wmc", name),
                FileMode.Open, FileAccess.Read, FileShare.Read);
            BinaryFormatter formatter = new BinaryFormatter();
            T t = formatter.Deserialize(fileStream) as T;
            fileStream.Close();
            return t;
        }
    }
}
