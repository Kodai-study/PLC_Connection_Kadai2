using MITSUBISHI.Component;
using ResultDatas;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

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
        ///  ステーション内のワークを管理するオブジェクト
        /// </summary>
        WorkController workController;

        /// <summary>
        ///  DEBUG 最後に工程が終了した時間の一覧を記録しておく
        /// </summary>
        DateTime[] processTimeStanps;

        /// <summary>
        ///  工程の進みを検知するためのビットブロック管理オブジェクト
        ///  X000～X00Fまでを管理
        /// </summary>
        Parameters.Bitdata_Process block_X0;
        /// <summary> X040～X04Fまでのビットを監視し、工程の進み具合を検知 </summary>
        Parameters.Bitdata_Process x40;

        /// <summary>
        ///  検査結果を読み取るためのビットブロック管理オブジェクト
        ///  X410～X41Fまでを管理
        /// </summary>
        Parameters.BITS_STATUS x41;
        Parameters.BITS_STATUS x42;
        Parameters.BITS_STATUS x43;
        Parameters.BITS_STATUS x44;

        /// <summary>
        ///  検査結果ブロック管理オブジェクトの配列。
        ///  BITS_STATUSオブジェクトが格納され、すべての要素に
        ///  問い合わせを行うことで検査結果全てを取得することができる
        /// </summary>
        Parameters.BITS_STATUS[] resultBlocks;

        /// <summary>
        ///  PLCの接点監視タスクを開始する。
        ///  ポーリングでPLC通信を行うサブタスクを立ち上げる
        /// </summary>
        /// <returns> 
        ///  立ち上げたサブタスクが返される。
        ///  タスクの返り値は、正常終了(true)、異常終了(false)
        /// </returns>
        public async Task<bool> Start()
        {
            dotUtlType = new DotUtlType
            {
                ActLogicalStationNumber = 401
            };
            if (dotUtlType.Open() != 0)
                return false;


            x40 = new Parameters.BITs_X40();
            x41 = new Parameters.BITS_X41();
            x42 = new Parameters.BITS_X42();
            x43 = new Parameters.BITS_X43();
            x44 = new Parameters.BITS_X44();
            resultBlocks = new Parameters.BITS_STATUS[] { x41, x42, x43, x44 };

            if (!DatabaseController.DBConnection())
            {
                dotUtlType.Close();
                return false;
            }

            processTimeStanps = new DateTime[Parameters.StepNumbers];
            block_X0 = new Parameters.Bit_X();
            this.cancellToken = new CancellationTokenSource();
            workController = new WorkController();

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
            int[] dataBlock_x00 = new int[1];   //PLCから読み取った値をブロックで格納しておく
            int[] resultDatas = new int[4];
            int[] dataBlock_x40 = new int[1];
            string label = "shine";
            string ProcessLabel = "Result";
            Console.WriteLine("PLCの読み取り開始");

            /* プログラム最初に値を読み取って、そこからの変化を見る */
            int read = dotUtlType.ReadDeviceBlock(ref label, 1, ref dataBlock_x00);
            read = dotUtlType.ReadDeviceBlock(ref ProcessLabel, 1, ref dataBlock_x40);
            int oldBlockData_X00 = dataBlock_x00[0];
            int oldBlockData_X40 = dataBlock_x40[0];

            Task<bool> dbWriteTask = null;      //データベースへの書き込みタスク。



            var checkResult = new Results();

            while (true)
            {
                read = dotUtlType.ReadDeviceBlock(ref label, 1, ref dataBlock_x00);
                int x0_data = dataBlock_x00[0] & block_X0.MASK;

                if (oldBlockData_X00 != x0_data)
                {
                    DateTime nowTime = DateTime.Now;
                    int diff = x0_data ^ oldBlockData_X00;
                    oldBlockData_X00 = x0_data;
                    dbWriteTask = Task.Run(() => Writedata(nowTime, diff, dataBlock_x00[0], block_X0));
                    dbWrite = true;
                }

                read = dotUtlType.ReadDeviceBlock(ref ProcessLabel, 1, ref dataBlock_x40);
                int datas_x40 = dataBlock_x40[0] & x40.MASK;

                if (oldBlockData_X40 != x0_data)
                {
                    DateTime nowTime = DateTime.Now;
                    int diff = x0_data ^ oldBlockData_X40;
                    oldBlockData_X40 = x0_data;
                    dbWriteTask = Task.Run(() => Writedata(nowTime, diff, dataBlock_x00[0], x40));
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
                    //Console.WriteLine(dbWriteTask.Result);
                    dbWrite = false;
                }
            }
        }


        /// <summary>
        /// 　変化したPLCの値から、検査工程が進んでいた時はデータベースを更新する
        /// </summary>
        /// <param name="reactTime"> 変化を検知した時刻 </param>
        /// <param name="changeBit"> 変化していたビット </param>
        /// <param name="sensorData"> 変化していた値の元の値 </param>
        /// <returns> 書き込みを行ったらTrue </returns>

        //TODO 書き込みが失敗した場合はreturn falseとは違う処理を行うようにする
        public bool Writedata(DateTime reactTime, int changeBit, int sensorData, Parameters.Bitdata_Process bitBlock)
        {
            bool ChangeON = (changeBit & sensorData) != 0;  //変化したビットが、1に変化したか(true)
            string sql = null;          //INSERT、UPDATEいずれかのSQL文
            Parameters.Process_Number progressNum = Parameters.Process_Number.NO_CHECK;       //工程が進んだ場合、その工程番号

            try
            {
                /* 変化が工程の進みと関係なかったら何もしない */
                if (!bitBlock.getProgressNum(changeBit, ChangeON, ref progressNum))
                {
                    return false;
                }

                /* 前回センサが反応した時間からどれだけ時間がたったか */
                TimeSpan diffReactTime = reactTime - processTimeStanps[(int)progressNum];
                processTimeStanps[(int)progressNum] = reactTime;

                /* 工程が進む入力は、前の入力よりChataring_time(定数)以上遅れていた時のみ反応する */
                if (diffReactTime < Chataring_time)
                {
                    return false;
                }

                ///DEBUG パワポ出すためのデバッグコード
                Console.WriteLine("工程 {0} が完了、時刻 : {1}", progressNum, reactTime.TimeOfDay);

                /* ワーク搬入工程が行われたとき、INSERT文で新しくワーク情報を追加する */
                if (progressNum == Parameters.Process_Number.Carry_in)
                {
                    sql = workController.AddnewWork(reactTime);
                }
                /* それ以外の工程が行われたとき、UPDATE文でワーク情報を更新 */
                else if (progressNum > Parameters.Process_Number.Carry_in)
                {
                    sql = workController.ProcessToSql(progressNum, reactTime.TimeOfDay);
                    if (progressNum == Parameters.Process_Number.Shoot_End)
                    {
                        Results workResult = getResult();
                        string insertStatusSql = String.Format
                            ("INSERT INTO Test_Data (Cycle_Code) VALUES ({0})", workController.CheckedWork.CycleCode);
                        DatabaseController.ExecSQL(insertStatusSql);

                        foreach (var e in workResult.getErrorCodes())
                        {
                            string insertErrorCodeSql = String.Format("INSERT INTO Test_Result(ID,result_Code) VALUES ({0},'{1}')",
                                workController.CheckedWork.WorkID, e);
                            Console.WriteLine(insertErrorCodeSql);
                            DatabaseController.ExecSQL(insertErrorCodeSql);
                        }
                    }
                }
                //TODO データベースへの書き込みは各人が行うようにする
                DatabaseController.ExecSQL(sql);
                Console.WriteLine(sql + "\n");
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

        /// <summary>
        ///  結果が格納されているPLCのデータブロックを読みだして、
        ///  検査結果を抽出、結果オブジェクトを返す
        /// </summary>
        /// <returns> 結果をまとめたクラスであるResultクラスのオブジェクト </returns>
        public Results getResult()
        {
            //TODO ラベル名をちゃんとする
            string resultRabel = "ResultBlock";
            int[] resultDatas = new int[4];
            var checkResult = new Results();
            if (dotUtlType.ReadDeviceBlock(ref resultRabel, 4, ref resultDatas) != 0)
            {
                for (int i = 0; i < resultBlocks.Length; i++)
                {
                    resultBlocks[i].CheckResult(ref checkResult, resultDatas[i]);
                }
            }
            return checkResult;
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
        private Queue<WorkData> insideWorks = null;

        private WorkData lastCheckWork= null;

        public WorkData CheckedWork
        {
            get { return lastCheckWork;  }
        }

        public WorkController()
        {
            insideWorks = new Queue<WorkData>();
        }

        /// <summary>
        ///  行われた変更から、ワーク情報を更新するSQL文を作成
        /// </summary>
        /// <param name="progressNum"> 工程番号。Parameterクラスからとってくる </param>
        /// <see cref="Parameters.Bit_X.getProgressNum(int, bool, ref int)"/>
        /// <param name="nowTime"> センサが反応した時刻 </param>
        /// <returns> 作成したUPDATE文 </returns>
        /// TODO エラー時の返り値等を決めておく

        public string ProcessToSql(Parameters.Process_Number progressNum, TimeSpan nowTime)
        {
            //TODO プロセス番号も列挙型で管理するようにする
            DateTime? startTime = null;
            /* 搬出工程が行われたら、管理キューから1つ破棄する */
            if (progressNum == Parameters.Process_Number.Carry_out)
            {
                startTime = insideWorks.Dequeue().startTime;
            }
            else
            {
                /* その工程がまだ行われていない、最も古いワークに変更を加える */
                foreach (var e in insideWorks)
                {
                    if (progressNum > e.progressNum)
                    {
                        e.progressNum = progressNum;
                        startTime = e.startTime;
                        if (progressNum == Parameters.Process_Number.Shoot_End)
                        {
                            lastCheckWork = e;
                        }
                        break;
                    }
                    return null;
                }
            }
            return String.Format("UPDATE Test_CycleTime SET {0} = '{1}' WHERE Carry_in BETWEEN '{2}' AND '{3}'",
                    Parameters.TIME_COLUMNAMES[(int)progressNum], nowTime, startTime, startTime + TimeSpan.FromSeconds(1));
        }

        /// <summary>
        /// 　新しいワークが搬入された時に管理対象に追加する
        /// </summary>
        /// <param name="startTime">　搬入された時刻　</param>
        public string AddnewWork(DateTime startTime)
        {
            insideWorks.Enqueue(new WorkData(startTime, 0));
            return String.Format("INSERT INTO Test_CycleTime ({2}) VALUES ('{0}.{1:D3}')",
                startTime, startTime.Millisecond, Parameters.TIME_COLUMNAMES[0]);
        }

        /// <summary>
        ///  検査データを表すクラス
        /// </summary>
        public class WorkData
        {
            /// <summary>
            ///  ワークが最後に行った工程の番号
            /// </summary>
            public Parameters.Process_Number progressNum;
            /// <summary>
            ///  ワークが搬入されてきた時間。これでテーブル操作の時にワークを識別する。
            /// </summary>
            public DateTime startTime;

            private int? cycle_code = null;

            private int? id = null;


            public WorkData(DateTime startTime, Parameters.Process_Number progressNum)
            {
                this.progressNum = progressNum;
                this.startTime = startTime;
            }

            public int? CycleCode
            {
                //TODO startTimeからCycle_codeをとってくるSQL文を実行
                get
                {
                    if(cycle_code != null)
                        return cycle_code;

                    string getCycleCodeSql = String.Format
                        ("SELECT cycle_code FROM Test_CycleTime WHERE Carry_in BETWEEN '{0}' and '{1}'",
                        this.startTime, startTime + TimeSpan.FromSeconds(1));

                    if (DatabaseController.GetOneParameter<int?>(getCycleCodeSql, ref cycle_code))
                    {
                        return cycle_code;
                    }
                    else
                    {
                        cycle_code = null; 
                        return cycle_code;
                    }
                }
            }

            public int? WorkID
            {
                get
                {
                    if(id != null)
                        return id;

                    if (cycle_code == null)
                    {
                        cycle_code = this.CycleCode;
                    }
                    string getWorkIDSql = String.Format
                        ("SELECT ID FROM Test_Data WHERE Cycle_Code = {0}", cycle_code);

                    if (DatabaseController.GetOneParameter<int?>(getWorkIDSql, ref id))
                        return id;
                    else
                    {
                        id = null;
                        return null;
                    }
                }
            }
        }
    }
}