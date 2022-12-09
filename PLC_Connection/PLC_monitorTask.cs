using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MITSUBISHI.Component;
using System.Data.SqlClient;
using System.Data;
using System.Reflection.Emit;
using System.Collections;

namespace PLC_Connection
{
    class PLC_MonitorTask
    {
        /// <summary>
        ///  チャタリング防止として、前のセンサ読み込みから
        ///  この時間がたつまでは読み込みを無視する。
        ///  1秒
        /// </summary>
        private readonly TimeSpan Chataring_time = new TimeSpan(0, 0, 1);

        /// <summary>
        ///  PLCのデータを読み込むに、コネクションを確立するオブジェクト
        /// </summary>
        public DotUtlType dotUtlType;

        /// <summary>
        /// 　PLCの読み取りタスクを終了させる命令を出すトークン
        /// </summary>
        private CancellationTokenSource cancellToken;

        /// <summary>
        /// データベースへの書込みを行うコネクションオブジェクト
        /// </summary>
        SqlConnection sqlConnection;

        /// <summary>
        ///  ステーション内のワークを管理するオブジェクト
        /// </summary>
        WorkController workController;

        /// <summary>
        ///  DEBUG 最後に工程が終了した時間の一覧を記録しておく
        /// </summary>
        DateTime[] processTimeStanps;

        /// <summary>
        ///  
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Start()
        {
            this.cancellToken = new CancellationTokenSource();
            workController = new WorkController();
            dotUtlType = new DotUtlType();
            dotUtlType.ActLogicalStationNumber = 401;
            if (dotUtlType.Open() != 0)
                return false;

            sqlConnection = new SqlConnection("Data Source=RBPC12;Initial Catalog=Robot22_DB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            try
            {
                sqlConnection.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                dotUtlType.Close();
                return false;
            }
            processTimeStanps = new DateTime[Parameters.StepNumbers];
            await Task.Run(() => Run(cancellToken));
            //TODO 処理が正常終了、異常終了の定義をちゃんとする
            return false;
        }

        /// <summary>
        ///  ポーリングで、PLCの値をチェックするタスク。
        /// </summary>
        /// <param name="token"> 
        ///  このトークンに対してキャンセル命令をかけることで
        ///  ループが終了する。
        /// </param>
        /// <see cref="PLC_MonitorTask.cancellToken"/>
        public void Run(CancellationTokenSource token)
        {
            bool dbWrite = false;
            int[] datas = new int[1];   //PLCから読み取った値をブロックで格納しておく
            string label = "shine";     
            Console.WriteLine("PLCの読み取り開始");

            /* プログラム最初に値を読み取って、そこからの変化を見る */
            int read = dotUtlType.ReadDeviceBlock(ref label, 1, ref datas);
            int old_data = datas[0];
            int[] resultDatas = new int[4];
            label = "Result";
            read = dotUtlType.ReadDeviceBlock(ref label, 1, ref datas);
            label = "shine";
            Task<bool> dbWriteTask = null;      //データベースへの書き込みタスク。

            while (true)
            {
                //TODO ブロック読み取りの時にマスクをかける
                read = dotUtlType.ReadDeviceBlock(ref label, 1, ref datas);
                if (old_data != datas[0])
                {
                    DateTime dateTime = DateTime.Now;
                    int diff = datas[0] ^ old_data;
                    old_data= datas[0];
                    dbWriteTask = Task.Run(() => Writedata(dateTime, diff, datas[0]));
                    dbWrite = true;
                }
                string resultRabel = "Result";
                read = dotUtlType.ReadDeviceBlock(ref resultRabel, 1, ref resultDatas);
                resultRabel = "ResultBlock";
                read = dotUtlType.ReadDeviceBlock(ref resultRabel, 4, ref resultDatas);

                // キャンセル要求
                if (token.IsCancellationRequested)
                {
                    Console.WriteLine("MyThread IsCancellationRequested");
                    break;
                }
                Thread.Sleep(2);

                if (dbWrite)
                {
                    //Console.WriteLine(dbWriteTask.Result);
                    dbWrite = false;
                }
            }
        }

        /// <summary>
        ///  サンプルで、触れたセンサ値(Xﾗﾍﾞﾙ)の種類と、
        ///  触れたか離れたかの値をデータベースに入力
        /// </summary>
        /// <param name="reactTime"> 変化を検知した時刻 </param>
        /// <param name="changeBit"> 変化していたビット </param>
        /// <param name="sensorData"> 変化していた値の元の値 </param>
        /// <returns></returns>
        public bool Task_WriteDB(DateTime nowTime, int changeBit, int sensorData)
        {
            try
            {
                int sensorNum = 0;
                int on_off = (changeBit & sensorData) != 0 ? 1 : 0;

                for (; changeBit != 0; changeBit >>= 1, sensorNum++) { ; }

                string cmd = String.Format("INSERT INTO PLC_Test (Time,sensor_Number,ON_OFF) VALUES ('{0}.{1:D3}' , {2} , {3})", nowTime, nowTime.Millisecond, sensorNum, on_off);
                using (var command = new SqlCommand(cmd, sqlConnection))
                    command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
            return true;
        }

        /// <summary>
        /// 　変化したPLCの値から、検査工程が進んでいた時はデータベースを更新する
        /// </summary>
        /// <param name="reactTime"> 変化を検知した時刻 </param>
        /// <param name="changeBit"> 変化していたビット </param>
        /// <param name="sensorData"> 変化していた値の元の値 </param>
        /// <returns> 書き込みを行ったらTrue </returns>
        
        //TODO 書き込みが失敗した場合はreturn falseとは違う処理を行うようにする
        public bool Writedata(DateTime reactTime, int changeBit, int sensorData)
        {
            bool ChangeON = (changeBit & sensorData) != 0;  //変化したビットが、1に変化したか(true)
            string sql = null;          //INSERT、UPDATEいずれかのSQL文
            int progressNum = -1;       //工程が進んだ場合、その工程番号

            try
            {
                /* 変化が工程の進みと関係なかったら何もしない */
                if (!Parameters.Bit_X.getProgressNum(changeBit, ChangeON, ref progressNum))
                {
                    return false;
                }

                /* 前回センサが反応した時間からどれだけ時間がたったか */
                TimeSpan diffReactTime = reactTime - processTimeStanps[progressNum];
                processTimeStanps[progressNum] = reactTime;

                /* 工程が進む入力は、前の入力よりChataring_time(定数)以上遅れていた時のみ反応する */
                if (diffReactTime < Chataring_time)
                {
                    return false;
                }

                ///DEBUG パワポ出すためのデバッグコード
                Console.WriteLine("工程 {0} が完了、時刻 : {1}", progressNum, reactTime.TimeOfDay);

                /* ワーク搬入工程が行われたとき、INSERT文で新しくワーク情報を追加する */
                if (progressNum == 0)
                {
                    sql = workController.AddnewWork(reactTime);
                }
                /* それ以外の工程が行われたとき、UPDATE文でワーク情報を更新 */
                else if (progressNum > 0)
                {
                    sql = workController.ProcessToSql(progressNum, reactTime.TimeOfDay);
                }
                using (var command = new SqlCommand(sql, sqlConnection))
                    command.ExecuteNonQuery();
            }
            //TODO SQL実行が失敗したときの処理を書く。エラーログへの書き込み、再実行?
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
            return true;
        }

        /// <summary>
        ///  PLC状態取得のポーリングを停止≒プログラムの終了
        /// </summary>

        ///  TODO 検査(のデータ取得)の終了ログの追加
        public void stop()
        {
            cancellToken.Cancel();
        }
    }

    /// <summary>
    ///  検査ステーションに入ってきたワークの管理を行う
    /// </summary>
    class WorkController
    {
        /// <summary>
        ///  ステーション内にあるワークの一覧を管理
        ///  (検査終了、搬出でキューから無くなる)
        /// </summary>
        private Queue<WorkData> t = null;
        public WorkController()
        {
            t = new Queue<WorkData>();
        }

        /// <summary>
        ///  行われた変更から、ワーク情報を更新するSQL文を作成
        /// </summary>
        /// <param name="progressNum"> 工程番号。Parameterクラスからとってくる </param>
        /// <see cref="Parameters.Bit_X.getProgressNum(int, bool, ref int)"/>
        /// <param name="nowTime"> センサが反応した時刻 </param>
        /// <returns> 作成したUPDATE文 </returns>
        /// TODO エラー時の返り値等を決めておく

        public string ProcessToSql(int progressNum, TimeSpan nowTime)
        {
            string sql = null;
            /* 搬出工程が行われたら、管理キューから1つ破棄する */
            if (progressNum == 9)
            {
                var StartTime = t.Dequeue().startTime;
                sql = String.Format("UPDATE Test_CycleTime SET {0} = '{1}' WHERE Carry_in BETWEEN '{2}' AND '{3}'",
                    "Carry_out", nowTime, StartTime, StartTime + TimeSpan.FromSeconds(1));
            }
            else
            {
                /* その工程がまだ行われていない、最も古いワークに変更を加える */
                foreach (var e in t)
                {
                    if (progressNum > e.progressNum)
                    {
                        e.progressNum = progressNum;
                        sql = String.Format("UPDATE Test_CycleTime SET {0} = '{1}' WHERE Carry_in BETWEEN '{2}' AND '{3}'",
                        Parameters.TIME_COLUMNAMES[progressNum], nowTime, e.startTime, e.startTime + TimeSpan.FromSeconds(1));
                        Console.WriteLine(sql);
                        break;
                    }
                }
            }
            return sql;
        }

        /// <summary>
        /// 　新しいワークが搬入された時に管理対象に追加する
        /// </summary>
        /// <param name="startTime">　搬入された時刻　</param>
        public string AddnewWork(DateTime startTime)
        {
            t.Enqueue(new WorkData(startTime, 1));
            return String.Format("INSERT INTO Test_CycleTime ({2}) VALUES ('{0}.{1:D3}')",
                startTime, startTime.Millisecond, Parameters.TIME_COLUMNAMES[0]);
        }

        /// <summary>
        ///  検査データを表すクラス
        /// </summary>
        class WorkData
        {
            /// <summary>
            ///  ワークが最後に行った工程の番号
            /// </summary>
            public int progressNum;
            /// <summary>
            ///  ワークが搬入されてきた時間。これでテーブル操作の時にワークを識別する。
            /// </summary>
            public DateTime startTime;
            public WorkData(DateTime startTime, int progressNum)
            {
                this.progressNum = progressNum;
                this.startTime = startTime;
            }
        }



    }
}


//TODO IO割付のRtoPLCを見て、X411を読み取る。 配列のサイズを10にしてブロックで読めるか確認する