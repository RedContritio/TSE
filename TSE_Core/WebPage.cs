using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSE_Core
{
    public class WEBPAGE
    {
        public string URL {
            get { return url; }
        }
        public List<string> LINKS {
            get { return point_urls; }
        }

        public string TITLE {
            get { return title; }
        }

        public int id;
        string url;
        string title;
        public string raw_source;
        List<string> point_urls = new List<string>();
        public HashSet<int> point_id = new HashSet<int>();

        public WEBPAGE(string url, string content) {
            this.url = url;
            this.raw_source = content;
            this.title = htmlParser.GetTitleFromContent(content);
            this.point_urls = htmlParser.getUrlsFromContent(content);
        }

    }
}
