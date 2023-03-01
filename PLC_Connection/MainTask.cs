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
            PLC_MonitorTask mainPLCMonitorTask = new PLC_MonitorTask();
            Task<bool> plcMonitorTask = mainPLCMonitorTask.Start();
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
    }
}