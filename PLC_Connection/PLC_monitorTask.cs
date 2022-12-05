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
using ActUtlTypeLib;
using System.Data;
using System.Reflection.Emit;
using System.Collections;

namespace PLC_Connection
{
    class PLC_MonitorTask
    {
        public DotUtlType dotUtlType;

        /// <summary>
        /// 　PLCの読み取りタスクを終了させる命令を出すトークン
        /// </summary>
        private CancellationTokenSource cancellToken;

        /// <summary>
        /// データベースへの書込みを行うコネクションオブジェクト
        /// </summary>
        SqlConnection sqlConnection;

        WorkController workController;

        public async Task<bool> Start()
        {
            this.cancellToken = new CancellationTokenSource();
            workController = new WorkController();
            dotUtlType = new DotUtlType();
            dotUtlType.ActLogicalStationNumber = 401;
            if (dotUtlType.Open() != 0)
                return false;

            sqlConnection = new SqlConnection("Data Source=RBPC12;Initial Catalog=Robot22_DB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            sqlConnection.Open();
            await Task.Run(() => Run(cancellToken));
            return false;
        }

        public void Run(CancellationTokenSource token)
        {
            bool dbWrite = false;
            int[] datas = new int[1];
            string label = "shine";
            Console.WriteLine("ThreadProc start.");
            int read = dotUtlType.ReadDeviceBlock(ref label, 1, ref datas);
            int old_data = datas[0];
            Task<bool> dbWriteTask = null;


            while (true)
            {
                read = dotUtlType.ReadDeviceBlock(ref label, 1, ref datas);
                if (old_data != datas[0])
                {
                    DateTime dateTime = DateTime.Now;
                    int diff = datas[0] ^ old_data;
                    old_data= datas[0];
                    dbWriteTask = Task.Run(() => writedata(dateTime, diff, datas[0]));
                    dbWrite = true;
                }


                // キャンセル要求
                if (token.IsCancellationRequested)
                {
                    Console.WriteLine("MyThread IsCancellationRequested");
                    break;
                }
                Thread.Sleep(2);

                if (dbWrite)
                {
                    Console.WriteLine(dbWriteTask.Result);
                    dbWrite = false;
                }
            }
        }

        /// <summary>
        ///  サンプルで、触れたセンサ値(Xﾗﾍﾞﾙ)の種類と、
        ///  触れたか離れたかの値をデータベースに入力
        /// </summary>
        /// <param name="nowTime">  </param>
        /// <param name="changeBit"></param>
        /// <param name="sensorData"></param>
        /// <returns></returns>
        public bool Task_WriteDB(DateTime nowTime, int changeBit, int sensorData)
        {
            try
            {
                int sensorNum = 0;
                int on_off = (changeBit & sensorData) != 0 ? 1 : 0;

                for (; changeBit != 0; changeBit >>= 1, sensorNum++) {; }

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
        /// <returns></returns>
        public bool writedata(DateTime reactTime, int changeBit, int sensorData)
        {

            bool ChangeON = (changeBit & sensorData) != 0;  //変化したビットが、1に変化したか(true)
            string sql = null;
            int progressNum = -1;       //行われた変化

            try
            {
                /* 変化が工程の進みと関係なかったら何もしない */
                if (!Parameters.Bit_X.getProgressNum(changeBit, ChangeON, ref progressNum))
                {
                    return false;
                }

                if (progressNum == 0)
                {
                    sql = workController.AddnewWork(reactTime);
                }
                else if (progressNum > 0)
                {
                    sql = workController.ProcessToSql(progressNum, reactTime.TimeOfDay);
                }
                using (var command = new SqlCommand(sql, sqlConnection))
                    command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        public void hoge()
        {
            string label = "wordSample";
            int data = 0;
            dotUtlType.GetDevice(ref label, ref data);
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
    ///  
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

        public string ProcessToSql(int progressNum, TimeSpan nowTime)
        {
            string sql = null;
            if (progressNum == 9)
            {
                sql = String.Format("UPDATE Test_CycleTime SET {0} = '{1}' WHERE Carry_in = {2}",
                    "Carry_out", nowTime, t.Dequeue().startTime);
            }
            else
            {
                foreach (var e in t)
                {
                    if (progressNum > e.progressNum)
                    {
                        e.progressNum = progressNum;
                        sql = String.Format("UPDATE Test_CycleTime SET {0} = '{1}' WHERE Carry_in = {2}",
                        Parameters.TIME_COLUMNAMES[progressNum], nowTime, t.Dequeue().startTime);
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
            public int progressNum;
            public DateTime startTime;
            public WorkData(DateTime startTime, int progressNum)
            {
                this.progressNum = progressNum;
                this.startTime = startTime;
            }
        }
    }
}

class WorkStatus
{
    public int status;
    Queue<int> workQueue;
    public WorkStatus()
    {
        workQueue = new Queue<int>();
    }
}