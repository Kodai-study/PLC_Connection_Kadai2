//#define debug 

using System;
using System.Threading;
using System.Threading.Tasks;

namespace PLC_Connection
{
    /// <summary>
    ///  メイン実行のクラス
    /// </summary>
    class MainTask
    {
        public static void Main()
        {
#if debug
            Check_ResultOutputErrorCodes();
#else
            var e = new PLC_MonitorTask();
            Task<bool> plcMonitorTask = e.Start();
            Thread.Sleep(5000);
            if (plcMonitorTask.Result)
            {
                Console.WriteLine("正常終了");
            }
            else
            {
                Console.WriteLine("異常終了");
            }
#endif
        }


        //DEBUG サンプルの接点データから、エラー取得がうまくいくかどうかを見たやつ
        static void Check_ResultOutputErrorCodes()
        {
            Parameters.Bitdata_Process x40 = new Parameters.BITs_X40();
            Parameters.BITS_STATUS x41 = new Parameters.BITS_X41();
            Parameters.BITS_STATUS x42 = new Parameters.BITS_X42();
            Parameters.BITS_STATUS x43 = new Parameters.BITS_X43();
            Parameters.BITS_STATUS x44 = new Parameters.BITS_X44();

            Parameters.BITS_STATUS[] resultBlocks = new Parameters.BITS_STATUS[] { x41, x42, x43, x44 };

            int[] resultDatas = new int[resultBlocks.Length];

            InputAll_OKData(resultDatas);


            var result = new ResultDatas.Results();
            for (int i = 0; i < resultBlocks.Length; i++)
            {
                resultBlocks[i].CheckResult(ref result, resultDatas[i]);
            }
            Console.WriteLine(result + "\n---------エラーコード---");
            var All_OKDatas = result.getErrorCodes();
            All_OKDatas.ForEach(x => Console.WriteLine(x));

            InputAll_NGData(resultDatas);
            for (int i = 0; i < resultBlocks.Length; i++)
            {
                resultBlocks[i].CheckResult(ref result, resultDatas[i]);
            }
            Console.WriteLine(result + "\n---------エラーコード---");
            var All_NGErrorCodes = result.getErrorCodes();
            All_NGErrorCodes.ForEach(x => Console.WriteLine(x));
        }

        //DEBUG 結果が入るところでサンプルのデータを入れる関数
        static void InputAll_OKData(int[] datas)
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
            datas[0] = x41.MASK & ~(Parameters.BITS_X41.WORK_DIR_NG);
            datas[1] = x42.MASK;
            datas[2] = x43.MASK;
            datas[3] = x44.MASK;
        }

        public static void InputAll_NGData(int[] datas)
        {
            Array.Clear(datas, 0, datas.Length);
            datas[0] = Parameters.BITS_X41.WORK_DIR_NG;
            datas[2] = Parameters.BITS_X43.DIP_OK;
        }
    }
}