using JiebaNet.Segmenter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TSE_Core
{
    public static class Splitter
    {
        public static string RemoveBadSymbols(string content)
        {
            // 中文空格就离谱
            // 第一行中文标点，第二行英文标点，第三行 &lt;
            return Regex.Replace(content, @"(·|！|@|￥|…|（|）|—|｛|｝|【|】|、|‘|’|“|”|；|：|，|《|。|》|？|　|"
                                        + @"|`|~|!|@|#|\$|\%|\^|&|\*|\(|\)|\-|_|=|\+|\[|\]|\{|\}|<|>|\,|\.|\\|\||/|\?|\r|\n"
                                        +  "|;|:|'|\""
                                        + @"|(&.{2,4};))", " "); // 特别考虑如 &lt; 之类的内容
        }

        public static List<string> GetWords(string content)
        {
            JiebaSegmenter segmenter = new JiebaSegmenter();
            string goodstr = RemoveBadSymbols(content);
            List<string> words = segmenter.CutForSearch(goodstr).ToList();
            words.RemoveAll(str => str.Equals(""));
            words.RemoveAll(str => str.Equals(" "));

            return words;
        }
    }
}
