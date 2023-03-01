using PLC_Connection.Modules;
using ResultDatas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLC_Connection.StationMonitor
{
    public class FunctionStationMonitor : Base_StationMonitor
    {


        public FunctionStationMonitor(PLC_MonitorTask plc_MonitorTask, WorkController workController) : base(plc_MonitorTask, workController)
        {

        }

        override public void CheckData(PLCContactData plcDatas)
        {
            if (plcDatas.X40_Block.IsAnyBitStundUp)
            {
                List<DataBlock.ChangeBitData> changeData = plcDatas.X40_Block.ChangedDatas();
                foreach (var e in changeData)
                {
                    // 検査終了等のデータを読み取る
                }
            }
        }

        public void getResult()
        {
            WorkData checkedWork = workController.getFunctionCheckedWork();

            if (checkedWork == null)
                return;

            string insertErrorCodeSql = String.Format("");
            Console.WriteLine(insertErrorCodeSql);
            DatabaseController.ExecSQL(insertErrorCodeSql);

            checkedWork.IsVisualInspected = true;
        }
    }
}