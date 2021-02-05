using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using TSE_Core;
using System.Threading;
using System.Data;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;

namespace TSE
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Int64 _StopwatchTimeStamp = 0;

        public static DBReader reader;
        public static WordsManager wordsManager;

        public static bool RankLoaded = false; 
        public static bool WordsManagerLoaded = false;
        public static bool DBReaderLoaded = false;

        public static bool DataLoaded {
            get { return RankLoaded && WordsManagerLoaded && DBReaderLoaded;}
        }
        public MainWindow()
        {
            InitializeComponent();
            this.outlog.Content = "";
            Thread thread = new Thread(new ThreadStart(LoadData));
            thread.Start();
        }

        private void Search(object sender, RoutedEventArgs e)
        {
            if(!DataLoaded)
            {
                outlog.Content = "加载数据中，请稍后";
                return ;
            }


            outlog.Content = "搜索中，请稍后";
            ListView1.Items.Clear();
            GetTimeElapsed();

            List<string> words = Splitter.GetWords(inputBox.Text);

            List<int> results = GetRawSearchResult(wordsManager, words);
            List<int> wordidxs = wordsManager.GetWordsIndex(words);

            results = Rank.SortResult(wordsManager, results, wordidxs, 0.005);

            foreach (int id in results)
            {
                var item = DisplayItem.GetDisplayItem(reader, id);
                ListView1.Items.Add(item);
            }


            this.outlog.Content = String.Format("找到 {0} 条搜索结果，耗时 {1:F3} s", results.Count, GetTimeElapsed());
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

        /// <summary>
        /// 返回两次调用之间的时间
        /// </summary>
        /// <returns></returns>
        public static double GetTimeElapsed()
        {
            Int64 t = Stopwatch.GetTimestamp();
            double time = (t - _StopwatchTimeStamp) / (double)Stopwatch.Frequency;
            _StopwatchTimeStamp = t;
            return time;
        }


        private void LoadData()
        {
            GetTimeElapsed();

            Thread thread0 = new Thread(new ThreadStart(LoadDBReader));

            Thread thread1 = new Thread(new ThreadStart(LoadWordManager));

            Thread thread2 = new Thread(new ThreadStart(LoadRank));

            thread0.Start();
            thread1.Start();
            thread2.Start();
        }

        void LoadWordManager()
        {
            wordsManager = new WordsManager();
            wordsManager.LoadFromLocal();
            WordsManagerLoaded = true;
            
            
            if (DataLoaded)
                MessageBox.Show(String.Format("初始化完成，用时 {0:F3} s", GetTimeElapsed()));
        }

        void LoadDBReader()
        {
            reader = new DBReader();
            reader.ReadDBIndex();
            reader.ReadDBContent();
            DBReaderLoaded = true;

            if(DataLoaded)
                MessageBox.Show(String.Format("初始化完成，用时 {0:F3} s", GetTimeElapsed()));
        }

        void LoadRank()
        {
            Rank.LoadFromLocal();
            RankLoaded = true;
            
            if (DataLoaded)
                MessageBox.Show(String.Format("初始化完成，用时 {0:F3} s", GetTimeElapsed()));
        }

        private void HandleDoubleClick(object sender, MouseButtonEventArgs e)//双击ListView1事件
        {
            try {
                DisplayItem item = (sender as System.Windows.Controls.ListViewItem).Content as DisplayItem;
                ExecuteCommand("start " + item.url);
            } catch  {
                // do nothing
            }
        }

        public static void ExecuteCommand(string cmd)
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            p.StartInfo.CreateNoWindow = true;//不显示程序窗口
            p.Start();

            p.StandardInput.WriteLine(cmd + " & exit");
            p.WaitForExit();//等待程序执行完退出进程
            p.Close();
        }

        private void ListView1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
