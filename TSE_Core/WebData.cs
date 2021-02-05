using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSE_Core
{
    public static class WebData
    {
        private static string SaveFilePath
        {
            get { return ConfigurationManager.AppSettings["SaveFilePath"] ?? @"..\Save"; }
        }
        public static void CreateDBFile() {
            string path = SaveFilePath + @"\webpageLinks.sqlite";
            // 如果不存在，就重新创建
            if (!File.Exists(path))
            {
                SQLiteConnection.CreateFile(path);
            }
        }

        public static SQLiteConnection ConnectDB() {
            return new SQLiteConnection("Data Source=" + SaveFilePath + @"\webpageLinks.sqlite;Version=3;");
        }

        public static bool CreateWebTable(SQLiteConnection connection, string tableName = "webs") {
            string cmdstr = String.Format("create table if not exists {0} (" +
                "url VARCHAR(256) PRIMARY KEY NOT NULL," +
                "md5 VARCHAR(32) NOT NULL," +
                "length integer," +
                "idx integer," +
                "date DATETIME)", tableName);
            SQLiteCommand command = new SQLiteCommand(cmdstr, connection);
            command.ExecuteNonQuery();
            return true;
        }

        public static void SaveToDB(SQLiteConnection connection, string url, string md5, int length, int idx, string tableName = "webs") {
            string cmdstr = String.Format("insert into {0} (url, md5, length, idx, date) values ('{1}', '{2}', {3}, {4},  datetime('now'))", tableName, url, md5, length, idx);
            SQLiteCommand command = new SQLiteCommand(cmdstr, connection);
            command.ExecuteNonQuery(); // 不知道为啥，插入语句返回 Scalar 为 null
        }

        public static SQLiteDataReader GetReaderByExecution(SQLiteConnection connection, string cmd)
        {
            SQLiteCommand command = new SQLiteCommand(cmd, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            return reader;
        }

        public static int GetTableSize(SQLiteConnection connection, string tableName = "webs")
        {
            string cmdstr = String.Format("select count(*) from {0}", tableName);
            return GetIntByExecution(connection, cmdstr);
        }

        public static bool ContainsRecord(SQLiteConnection connection, string url, string tableName = "webs")
        {
            string cmdstr = String.Format("select count(*) from {0} where url=='{1}'", tableName, url);
            SQLiteCommand command = new SQLiteCommand(cmdstr, connection);
            return int.Parse(command.ExecuteScalar().ToString()) != 0;
        }

        public static void GetTable(SQLiteConnection connection, string tableName = "webs")
        {
            string cmdstr = String.Format("select * from {0}", tableName);
            SQLiteCommand command = new SQLiteCommand(cmdstr, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            return ;
        }

        public static void SaveToLocal(int idx, string content, string FolderName = "pages")
        {
            if(!Directory.Exists(SaveFilePath + @"\" + FolderName))
                Directory.CreateDirectory(SaveFilePath + @"\" + FolderName);

            StreamWriter sw = new StreamWriter(SaveFilePath + @"\" + FolderName + @"\" + idx);
            sw.Write(content);
            sw.Close();
        }

        public static bool LoadFromLocal(int idx, out string content, string FolderName = "pages")
        {
            StreamReader sw = new StreamReader(SaveFilePath + @"\" + FolderName + @"\" + idx);
            content = sw.ReadToEnd();
            sw.Close();
            return true;
        }


        public static int GetIntByExecution(SQLiteConnection connection, string cmd)
        {
            SQLiteCommand command = new SQLiteCommand(cmd, connection);
            return int.Parse(command.ExecuteScalar().ToString());
        }

        /// <summary>
        /// 执行一条 SQL 指令
        /// ExecuteNonQuery
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static int Execute(SQLiteConnection connection, string cmd)
        {
            SQLiteCommand command = new SQLiteCommand(cmd, connection);
            return command.ExecuteNonQuery();
        }
    }
}
