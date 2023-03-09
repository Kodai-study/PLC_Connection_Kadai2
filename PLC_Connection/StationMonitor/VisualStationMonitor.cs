using PLC_Connection.Modules;
using PLC_Connection.InspectionResultDataModel;
using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;

namespace PLC_Connection.StationMonitor
{
    public class VisualStationMonitor : Base_StationMonitor
    {

        private int numberOfWork = 0;

        private readonly TimeSpan delayTime = new TimeSpan(0, 0, 5);

        /// <summary> 搬入コンベアの入口のセンサ部分 </summary>
        public const int IN_CONVARE_FIRST = 0b0000000000010000;

        /// <summary> 搬入コンベアの出口のセンサ部分 </summary>
        public const int OUT_CONVARE_END = 0b0000001000000000;

        private readonly ResultDataCreater[] resultCreaters = new ResultDataCreater[] {
            new VisualInspectionResultCreater_X41(),
            new VisualInspectionResultCreater_X42(),
            new VisualInspectionResultCreater_X43(),
            new VisualInspectionResultCreater_X44()
        };

        DateTime? lastInspectedTime = null;

        public VisualStationMonitor(PLC_MonitorTask plc_MonitorTask, WorkController workController, MemoryMappedViewAccessor commonMemoryAccessor) : base(plc_MonitorTask, workController, commonMemoryAccessor)
        {
            UpdateStationState(MEMORY_SPACE.NUMBER_OF_WORK_VISUAL_STATION, numberOfWork);
            UpdateStationState(MEMORY_SPACE.STATE_OF_VISUAL_STATION, 0);
        }

        override public void CheckData(PLCContactData plcDatas, DateTime checkedTime)
        {
            if (plcDatas.X00_Block.IsAnyBitStundUp)
            {
                foreach (var e in plcDatas.X00_Block.StandUpDatas())
                {
                    if (e.BitNumber == 4)
                    {
                        numberOfWork++;
                        UpdateStationState(MEMORY_SPACE.NUMBER_OF_WORK_VISUAL_STATION, numberOfWork);
                        workController.AddnewWork(checkedTime);
                        workController.WriteProcesChangeData(
                            CommonParameters.Process_Number.VisualStation_in, checkedTime);
                    }

                }
            }

            if (plcDatas.X40_Block.IsAnyBitStundUp)
            {
                List<DataBlock.ChangeBitData> changeData = plcDatas.X40_Block.StandUpDatas();

                foreach (var e in changeData)
                {
                    if (e.BitNumber == 0)
                    {
                        GetVisualInspectionResult();
                        lastInspectedTime = checkedTime;
                        UpdateStationState(MEMORY_SPACE.IS_VISUAL_INSPECTED_JUST_BEFORE, 1);
                    }
                }
            }

            if (plcDatas.B06_Block.IsAnyBitStundUp)
            {
                List<DataBlock.ChangeBitData> changeData =
                    plcDatas.B06_Block.StandUpDatas(0, 1, 3, 4, 10, 11);

                foreach (var e in changeData)
                {
                    if (e.BitNumber == 0)
                    {
                        UpdateStationState(MEMORY_SPACE.STATE_OF_VISUAL_STATION, 2);
                    }
                    if (e.BitNumber == 3)
                    {
                        UpdateStationState(MEMORY_SPACE.STATE_OF_VISUAL_STATION, 3);
                    }
                    if (e.BitNumber == 4)
                    {
                        UpdateStationState(MEMORY_SPACE.STATE_OF_VISUAL_STATION, 4);
                    }
                }
            }

            if (lastInspectedTime != null && lastInspectedTime + delayTime < checkedTime)
            {
                UpdateStationState(MEMORY_SPACE.IS_VISUAL_INSPECTED_JUST_BEFORE, 0);
                lastInspectedTime = null;
            }
        }

        public void GetVisualInspectionResult()
        {
            Results visualInspectionResult = new Results();
            int[] resultBlock = plc_MonitorTask.GetVisualInspectionResult();
            for (int i = 0; i < resultBlock.Length; i++)
            {
                resultCreaters[i].CheckResult(ref visualInspectionResult,
                    resultBlock[i]);
            }
            Console.WriteLine(visualInspectionResult);
            WorkData checkedWork = workController.GetVisualCheckedWork();

            if (checkedWork == null)
                return;

            foreach (var errorCode in visualInspectionResult.getErrorCodes())
            {
                string insertErrorCodeSql = String.Format("INSERT INTO VisalST (No,result_Code) VALUES ({0},'{1}')",
                  checkedWork.WorkID, errorCode);
                DatabaseController.ExecSQL(insertErrorCodeSql);
            }
            checkedWork.IsVisualInspected = true;
            visualInspectionResult.getErrorCodes();
        }

        public void RemoveWork()
        {
            numberOfWork--;
            UpdateStationState(MEMORY_SPACE.NUMBER_OF_WORK_VISUAL_STATION, numberOfWork);
        }
    }
}