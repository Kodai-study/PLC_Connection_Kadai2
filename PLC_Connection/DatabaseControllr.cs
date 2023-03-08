//#define debug 

using System;
using System.Data.SqlClient;
using System.Threading;

namespace PLC_Connection
{
    /// <summary>
    ///  データベースへの書込みを管理するクラス
    /// </summary>
    public class DatabaseController
    {
        /// <summary>
        ///  現在データベースに接続しているかどうか
        /// </summary>
        private static bool Isconnection = false;

        /// <summary>
        ///  接続文字列。RBPC12のデータベースに接続するようにしている
        /// </summary>
        private const string DEFAULT_CONNECTION_STR =
            "Data Source=RBPC12;Initial Catalog=Robot22_2DB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        private static SqlConnection dbConnection;

        public static SqlConnection Connecter
        {
            get { return dbConnection; }
        }

        /// <summary>
        ///  データベースへの接続を行う。接続済みの場合は何もしない
        /// </summary>
        /// <returns> 接続が成功、もしくはすでに接続済みならTrue </returns>
        public static bool DBConnection()
        {

            return DBConnection(DEFAULT_CONNECTION_STR);
        }

        /// <summary>
        ///  接続文字列を指定してのデータベースへの接続。
        ///  デフォルトのデータベースサーバ以外へ接続したいときに使う
        /// </summary>
        /// <param name="connectionString"> 接続文字列プロパティからコピーしてくる </param>
        /// <returns> 接続が成功、もしくはすでに接続済みならTrue </returns>
        public static bool DBConnection(string connectionString)
        {
            if (!Isconnection)
            {
                try
                {
                    dbConnection = new SqlConnection(connectionString);
                    dbConnection.Open();
                    return Isconnection = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return Isconnection = false;
                }
            }
            return true;
        }

        /// <summary>
        ///  SQL文を実行する。結果を得られないため、挿入や
        ///  更新を行う。
        /// </summary>
        /// <param name="sql"> SQL文。内容については特に干渉しない </param>
        public static void ExecSQL(string sql)
        {
            if (sql == null || sql.Equals(""))
            {
                Console.WriteLine("何もないSQL文が実行されようとしました");
                return;
            }
            var command = new SqlCommand(sql, dbConnection);
            try
            {
                command.ExecuteNonQuery();
            }
            //TODO 書き込みが失敗したときの処理を考える
            catch (Exception)
            {
                Thread.Sleep(10);
                try
                {
                    command.ExecuteNonQuery();
                }
                //TODO Exceiptionの種類によってエラーログを使い分ける
                catch (Exception exception)
                {
                    //TODO 書き込み失敗したときの処理を決めておく エラーログ?
                    Console.WriteLine("書き込み失敗" + exception.ToString());
                }
            }
        }

        /// <summary>
        ///  SQL文を実行して、1つだけ要素を取得する。
        ///  1つのレコードのみが該当し、1つのコラムだけが取得できる
        ///  SQL文を実行する。
        /// </summary>
        /// <typeparam name="T"> 
        ///  取ってくるデータの型を指定する。
        ///  データベースからとってきたデータから変換できないとエラー
        /// </typeparam>
        /// <param name="sql"> SQLのSELECT文。選択するコラムは1つ、WHERE文で1レコードを特定できる </param>
        /// <param name="param"> 取得が成功したら、引数の値を書き換える </param>
        /// <returns> 取得が成功したらTrue </returns>
        public static bool GetOneParameter<T>(string sql,ref T param)
        {
            SqlDataReader sqlData = null;
            try
            {
                var command = new SqlCommand(sql, dbConnection);
                sqlData = command.ExecuteReader();
                if (sqlData.Read())
                {
                    param = (T)sqlData[0];  //
                    return !sqlData.Read();//2つ以上のレコード該当する場合は失敗
                    //TODO レコードが2つ以上該当する、もしくはコラムが2つ以上ある場合は失敗
                    //return !sqlData.Read() && sqlData.FieldCount != 1; 
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("変換時にエラーが発生しました\n" + e.ToString());
                return false;
            }
            finally
            {
                sqlData.Close();
            }
        }
    }
}