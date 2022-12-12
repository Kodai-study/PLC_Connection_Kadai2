//#define debug 

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
            int[] resultDatas = new int[4];
            Parameters.BITS_STATUS x41 = new Parameters.BITS_X41();
            Parameters.BITS_STATUS x42 = new Parameters.BITS_X42();
            Parameters.BITS_STATUS x43 = new Parameters.BITS_X43();
            Parameters.BITS_STATUS x44 = new Parameters.BITS_X44();
            var result = new ResultDatas.Results();
            for(int i = 0;i < resultDatas.Length; i++)
            {
                resultDatas[i] = -1;
            }
            DotUtlType dotUtlType = new DotUtlType();
            dotUtlType.ActLogicalStationNumber = 401;

            //InputSampleResultData(resultDatas);

            read = dotUtlType.Open();
            string resultRabel = "Result";
            read = dotUtlType.ReadDeviceBlock(ref resultRabel, 1, ref resultDatas);
            resultRabel = "ResultBlock";
            read = dotUtlType.ReadDeviceBlock(ref resultRabel, 4, ref resultDatas);

            x41.CheckResult(ref result, resultDatas[0]);
            x42.CheckResult(ref result, resultDatas[1]);
            x43.CheckResult(ref result, resultDatas[2]);
            x44.CheckResult(ref result, resultDatas[3]);

            Array.Clear(resultDatas, 0, 4);
            read = dotUtlType.ReadDeviceBlock(ref resultRabel, 4, ref resultDatas);
            dotUtlType.Close();

            /*
            read = dotUtlType.Open();
            string resultRabel = "Result";
            read = dotUtlType.ReadDeviceBlock(ref resultRabel, 1, ref resultDatas);
            resultRabel = "ResultBlock";
            read = dotUtlType.ReadDeviceBlock(ref resultRabel, 4, ref resultDatas);
             */
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