using PLC_Connection.Modules;
using PLC_Connection.InspectionResultDataModel;
using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLC_Connection.StationMonitor
{
    public class FunctionStationMonitor : Base_StationMonitor
    {

        private int numberOfWork = 0;
        private VisualStationMonitor beforeStationMonitor;

        DateTime? lastInspectedTime = null;
       private readonly TimeSpan delayTime = new TimeSpan(0, 0, 5);

        public FunctionStationMonitor(PLC_MonitorTask plc_MonitorTask, WorkController workController, MemoryMappedViewAccessor commonMemoryAccessor,
            VisualStationMonitor beforeStationMonitor) : base(plc_MonitorTask, workController, commonMemoryAccessor)
        {
            UpdateStationState(MEMORY_SPACE.NUMBER_OF_WORK_FUNCTIONAL_STATION, numberOfWork);
            this.beforeStationMonitor = beforeStationMonitor;
        }

        override public void CheckData(PLCContactData plcDatas, DateTime checkedTime)
        {
            if (plcDatas.X40_Block.IsAnyBitStundUp)
            {
                List<DataBlock.ChangeBitData> changeData = plcDatas.X40_Block.ChangedDatas();
                foreach (var e in changeData)
                {
                    // 検査終了等のデータを読み取る
                }
                workController.WriteProcesChangeData(CommonParameters.Process_Number.FunctionStation_in, checkedTime);
                //搬入を検知したら
                numberOfWork++;
                UpdateStationState(MEMORY_SPACE.NUMBER_OF_WORK_FUNCTIONAL_STATION, numberOfWork);
                beforeStationMonitor.RemoveWork();

                //状態変化を見れたら
                int state = 0;
                UpdateStationState(MEMORY_SPACE.STATE_OF_FUNCTION_STATION, state);

                //検査終了を検知したら
                UpdateStationState(MEMORY_SPACE.IS_FUNCTION_INSPECTED_JUST_BEFORE, 1);
                lastInspectedTime   = checkedTime;
            }

            if (lastInspectedTime + delayTime < checkedTime)
            {
                UpdateStationState(MEMORY_SPACE.IS_FUNCTION_INSPECTED_JUST_BEFORE, 0);
                lastInspectedTime = null;
            }
        }

        public void GetFunctionalInspectionResult()
        {
            WorkData checkedWork = workController.GetFunctionCheckedWork();

            if (checkedWork == null)
                return;

            int volt = 0;
            int frequency = 0;

            string insertErrorCodeSql = String.Format("INSERT INTO FunctionalST (No,Volt,Freq) VALUES  ({0},{1},{2})",
                checkedWork.WorkID, volt, frequency);
            DatabaseController.ExecSQL(insertErrorCodeSql);

            UpdateStationState(MEMORY_SPACE.RESULT_VOLTAGE, volt);
            UpdateStationState(MEMORY_SPACE.RESULT_FREQUENCY, frequency);

            checkedWork.IsVisualInspected = true;
        }

        public void RemoveWork()
        {
            numberOfWork--;
            UpdateStationState(MEMORY_SPACE.NUMBER_OF_WORK_ASSEMBLY_STATION, numberOfWork);
        }

    }
}