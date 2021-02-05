using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TSE_Core
{
    public class WebSpider
    {
        private string DataFilePath {
            get { return ConfigurationManager.AppSettings["DataFilePath"] ?? @"..\Data";}
        }
        private string RootPath
        {
            get { return ConfigurationManager.AppSettings["ProjectRootDir"] ?? @".."; }
        }

        public int Count {
            get { return WebData.GetTableSize(connection);}
        }

        private int globalUniqueIndex = 0;

        Dictionary<string, WEBPAGE> pages = new Dictionary<string, WEBPAGE>();

        Queue<string> to_visit_links = new Queue<string>();

        HashSet<string> url_visited = new HashSet<string>();

        HashSet<string> black_list = new HashSet<string>();

        SQLiteConnection connection = null;
        public WebSpider() {
            LoadBlackList(DataFilePath + @"\BlackList.txt");
            WebData.CreateDBFile();
            connection = WebData.ConnectDB();
            connection.Open();
            WebData.CreateWebTable(connection, "webs");
            if(Count > 0) {
                globalUniqueIndex = WebData.GetIntByExecution(connection, "select max(idx) from webs") + 1;
            }
        }

        ~WebSpider() {
            if(connection != null) {
                //connection.Close();
            }
        }

        public void SetSourcePage(string source) {
            to_visit_links.Clear();
            url_visited.Clear();
            to_visit_links.Enqueue(source);
            url_visited.Add(to_visit_links.Peek());
        }

        public int CountToVisit {
            get { return to_visit_links.Count(); }
        }
        public WEBPAGE SingleStep() {
            if(to_visit_links.Count <= 0) return null;

            string link = to_visit_links.Dequeue();
            Console.WriteLine(String.Format("{0} visited.", link));
            string content = "";
            if (!isLegal(link)) {
                Console.WriteLine("Failed in visit {0}, because it is illegal", link);
                return null;
            }
            else if(!DownloadHtml(link, out content)) {
                Console.WriteLine("Failed in visit {0}, error: {1}", link, content);
                return null;
            }

            WEBPAGE page = new WEBPAGE(link, content);

            if(!WebData.ContainsRecord(connection, link))
            {
                page.id = globalUniqueIndex++;
                WebData.SaveToLocal(page.id, content);
                WebData.LoadFromLocal(page.id, out content);
                WebData.SaveToDB(connection, link, content.GetMD5(), content.Length, page.id);
            }

            pages.Add(link, page);
            foreach(string next_link in page.LINKS) {
                next_link.Trim('/');
                if(!url_visited.Contains(next_link)) {
                    to_visit_links.Enqueue(next_link);
                    url_visited.Add(next_link);
                }
            }
            return page;
        }

        private void LoadBlackList(string path = "Data/BlackList.txt") {
            string[] lines = System.IO.File.ReadAllLines(path);
            foreach (string line in lines)
            {
                this.black_list.Add(line.Trim());
            }
        }

        public static bool DownloadHtml(string url, out string content)
        {
            HttpWebRequest req;
            try {
                req = (HttpWebRequest)WebRequest.Create(url);
            } catch (UriFormatException e) {
                req = (HttpWebRequest)WebRequest.Create("http://" + url);
            } catch (Exception e) {
                content = e.Message + " " + e.ToString();
                return false;
            }

            req.Method = "GET";
            req.AllowAutoRedirect = false;
            req.ContentType = "application/x-www-form-urlencoded";

            try {
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                content = new StreamReader(resp.GetResponseStream()).ReadToEnd();
                return true;
            } catch(Exception e) {
                content = e.Message;
                return false;
            }
        }

        public bool isLegal(string link) {
            return !black_list.Contains(link)
                && link.Contains("nwpu") && // 与西工大有关
                !(link.EndsWith(".doc") || link.EndsWith(".docx") || link.EndsWith(".xls") 
                || link.EndsWith(".pdf") || link.EndsWith(".rar") || link.EndsWith(".jpg")); // 不是各种文件
        }
    }
}
