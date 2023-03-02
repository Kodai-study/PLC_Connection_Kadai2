using PLC_Connection.Modules;
using ResultDatas;
using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;

namespace PLC_Connection.StationMonitor
{
    public class VisualStationMonitor : Base_StationMonitor
    {

        private int numberOfWork = 0;

        private ResultDataCreater[] resultCreaters = new ResultDataCreater[] {
            new VisualInspectionResultCreater_X41(),
            new VisualInspectionResultCreater_X42(),
            new VisualInspectionResultCreater_X43(),
            new VisualInspectionResultCreater_X44()
        };

        DateTime lastInspectedTime;

        public VisualStationMonitor(PLC_MonitorTask plc_MonitorTask, WorkController workController, MemoryMappedViewAccessor commonMemoryAccessor) : base(plc_MonitorTask, workController, commonMemoryAccessor)
        {
        }

        override public void CheckData(PLCContactData plcDatas, DateTime checkedTime)
        {
            if (plcDatas.X40_Block.IsAnyBitStundUp)
            {
                List<DataBlock.ChangeBitData> changeData = plcDatas.X40_Block.ChangedDatas();
                foreach (var e in changeData)
                {
                    if (e.bitNumber == 0 && e.IsStundUp)
                    {
                        GetVisualInspectionResult();
                        lastInspectedTime = DateTime.Now;
                        UpdateStationState(MEMORY_SPACE.IS_INSPECTED_JUST_BEFORE, 1);
                    }
                    else if (e.bitNumber == 1)
                    {
                        workController.WriteProcesChangeData(CommonParameters.Process_Number.VisualStation_in, checkedTime);
                    }
                }
            }
            if (lastInspectedTime + new TimeSpan(0, 0, 5) < DateTime.Now)
            {
                UpdateStationState(MEMORY_SPACE.IS_INSPECTED_JUST_BEFORE, 0);
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

            WorkData checkedWork = workController.GetFunctionCheckedWork();

            if (checkedWork == null)
                return;

            foreach (var errorCode in visualInspectionResult.getErrorCodes())
            {
                string insertErrorCodeSql = String.Format("INSERT INTO VisalST (No,result_Code) VALUES ({0},{1})",
                  checkedWork.WorkID, errorCode);
                DatabaseController.ExecSQL(insertErrorCodeSql);
            }
            checkedWork.IsVisualInspected = true;
            visualInspectionResult.getErrorCodes();
        }
    }
}