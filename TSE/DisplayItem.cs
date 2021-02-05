using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TSE_Core;

namespace TSE
{
    public class DisplayItem
    {
        public string title { get; set;}
        public string url { get; set; }
        public int page_id;
        public string desc { get; set; }

        public static DisplayItem GetDisplayItem(DBReader reader, int pageid)
        {
            WEBPAGE page = reader.GetPageByIndex(pageid);

            DisplayItem item = new DisplayItem();
            item.title = page.TITLE;
            item.url = page.URL;
            item.page_id = pageid;
            string text = htmlParser.Html2PlainText(page.raw_source);

            item.desc = Regex.Replace(Regex.Replace(text, @"(\s)", " "), " {1,}", " ").Substring(0, 100);
            return item;
        }
    }
}
