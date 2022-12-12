#define debug 

using MITSUBISHI.Component;
using System;
using System.Data.SqlClient;
using System.Threading;

namespace PLC_Connection
{
    /// <summary>
    ///  メイン実行のクラス
    /// </summary>
    class Class1
    {
        public DotUtlType dotUtlType;
        public static void Main()
        {
#if debug
            debug();
#else
            var e = new PLC_MonitorTask();
            var task = e.Start();
            Thread.Sleep(5000);
            //task.Wait();
            //e.stop();
            bool IsSuccess = task.Result;
            if (IsSuccess)
            {
                Console.WriteLine("正常終了");
            }
            else
            {
                Console.WriteLine("異常終了");
            }
#endif
        }

        static void ConnectDB()
        {
            SqlConnection a = new SqlConnection("Data Source=RBPC12;Initial Catalog=Robot22_DB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            a.Open();
            var nowTime = DateTime.Now;
            string cmd = String.Format("INSERT INTO PLC_Test (Time) VALUES ('{0}.{1:D3}')", nowTime, nowTime.Millisecond);
            using (var command = new SqlCommand(cmd, a))
                command.ExecuteNonQuery();
        }

        static void debug()
        {
            int read;
            int oldX40;
            int[] resultDatas = new int[4];
            int[] processBits = new int[1];

            Parameters.Bitdata_Process x40 = new Parameters.BITs_X40();
            Parameters.BITS_STATUS x41 = new Parameters.BITS_X41();
            Parameters.BITS_STATUS x42 = new Parameters.BITS_X42();
            Parameters.BITS_STATUS x43 = new Parameters.BITS_X43();
            Parameters.BITS_STATUS x44 = new Parameters.BITS_X44();

            Parameters.BITS_STATUS[] resultBlocks = new Parameters.BITS_STATUS[] { x41, x42, x43, x44 };

            string resultRabel = "ResultBlock";
            string ProcessLabel = "Result";
            var result = new ResultDatas.Results();
            DotUtlType dotUtlType = new DotUtlType
            {
                ActLogicalStationNumber = 401
            };


            if (dotUtlType.Open() != 0)
            {
                Console.WriteLine("MX Componentのオープンエラー");
                return;
            }


            if (dotUtlType.ReadDeviceBlock(ref ProcessLabel, 1, ref processBits) != 0)
            {
                Console.WriteLine("{0}の読み取りエラー", ProcessLabel);
                return;
            }
            oldX40 = processBits[0] & x40.MASK;

            while (true)
            {
                read = dotUtlType.ReadDeviceBlock(ref ProcessLabel, 1, ref processBits);
                int x0_data = processBits[0] & x40.MASK;

                if (oldX40 != x0_data && (x0_data & Parameters.BITs_X40.END_SHOOT) != 0)
                {
                    DateTime dateTime = DateTime.Now;
                    int diff = x0_data ^ oldX40;
                    oldX40 = x0_data;

                    read = dotUtlType.ReadDeviceBlock(ref resultRabel, 4, ref resultDatas);

                    for (int i = 0; i < resultBlocks.Length; i++)
                    {
                        resultBlocks[i].CheckResult(ref result, resultDatas[i]);
                    }

                    /*
                    x41.CheckResult(ref result, resultDatas[0]);
                    x42.CheckResult(ref result, resultDatas[1]);
                    x43.CheckResult(ref result, resultDatas[2]);
                    x44.CheckResult(ref result, resultDatas[3]);
                    */
                    Console.WriteLine(result);
                    //  break;
                }
                Thread.Sleep(2);
            }
            dotUtlType.Close();
        }

        //DEBUG 結果が入るところでサンプルのデータを入れる関数
        static void InputSampleResultData(int[] datas)
        {
            if (datas.Length != 4)
            {
                Console.WriteLine("配列サイズが違う");
                return;
            }
            Parameters.BITS_STATUS x41 = new Parameters.BITS_X41();
            Parameters.BITS_STATUS x42 = new Parameters.BITS_X42();
            Parameters.BITS_STATUS x43 = new Parameters.BITS_X43();
            Parameters.BITS_STATUS x44 = new Parameters.BITS_X44();
            datas[0] = x41.MASK & (~Parameters.BITS_X41.WORK_DIR_NG);
            datas[1] = x42.MASK;
            datas[2] = x43.MASK;
            datas[3] = x44.MASK;
        }
    }
}