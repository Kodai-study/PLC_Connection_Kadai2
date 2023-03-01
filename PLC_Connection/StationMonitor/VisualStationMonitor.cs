using PLC_Connection.Modules;
using ResultDatas;
using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLC_Connection.StationMonitor
{
    public class VisualStationMonitor : Base_StationMonitor
    {

        private ResultDataCreater[] resultCreaters = new ResultDataCreater[] {
            new BlockToResultChanger_X41(),
            new BlockToResultChanger_X42(),
            new BlockToResultChanger_X43(),
            new BlockToResultChanger_X44()
        };

        public VisualStationMonitor(PLC_MonitorTask plc_MonitorTask, WorkController workController, MemoryMappedViewAccessor commonMemoryAccessor) : base(plc_MonitorTask, workController, commonMemoryAccessor)
        {
        }

        override public void CheckData(PLCContactData plcDatas)
        {
            if (plcDatas.X40_Block.IsAnyBitStundUp)
            {
                List<DataBlock.ChangeBitData> changeData = plcDatas.X40_Block.ChangedDatas();
                foreach (var e in changeData)
                {
                    if (e.bitNumber == 0 && e.IsStundUp)
                    {
                        GetVisualInspectionResult();
                    }
                    else if (e.bitNumber == 1)
                    {

                    }
                }
            }
        }

        public void GetVisualInspectionResult()
        {
            Results visualInspectionResult = new Results();
            int[] resultBlock = plc_MonitorTask.getVisualInspectionResult();
            for (int i = 0; i < resultBlock.Length; i++)
            {
                resultCreaters[i].CheckResult(ref visualInspectionResult,
                    resultBlock[i]);
            }
            Console.WriteLine(visualInspectionResult);

            WorkData checkedWork = workController.getFunctionCheckedWork();

            if (checkedWork == null)
                return;

            foreach (var e in visualInspectionResult.getErrorCodes())
            {
                string insertErrorCodeSql = String.Format("INSERT INTO Test_Result(ID,result_Code) VALUES ({0},'{1}')",
                  checkedWork.WorkID, e);
                Console.WriteLine(insertErrorCodeSql);
                DatabaseController.ExecSQL(insertErrorCodeSql);
            }
            checkedWork.IsVisualInspected = true;
        }
    }
}