using PLC_Connection.Modules;
using PLC_Connection.InspectionResultDataModel;
using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace PLC_Connection.StationMonitor
{
    public class AssemblyStationMonitor : Base_StationMonitor
    {
        private int numberOfWork = 0;

        private FunctionStationMonitor beforeStationMonitor;
        public AssemblyStationMonitor(PLC_MonitorTask plc_MonitorTask, WorkController workController, MemoryMappedViewAccessor commonMemoryAccessor,
            FunctionStationMonitor beforeStationMonitor) : base(plc_MonitorTask, workController, commonMemoryAccessor)
        {
            UpdateStationState(MEMORY_SPACE.NUMBER_OF_WORK_ASSEMBLY_STATION, numberOfWork);
            UpdateStationState(MEMORY_SPACE.STATE_OF_ASSEMBLY_STATION, 0);
            this.beforeStationMonitor = beforeStationMonitor;
        }

        override public void CheckData(PLCContactData plcDatas, DateTime checkedTime)
        {
            if (plcDatas.B15_Block.IsAnyBitStundUp)
            {
                List<DataBlock.ChangeBitData> changeData = plcDatas.B15_Block.ChangedDatas();
                foreach (var e in changeData)
                {
                    //組立工程が終わったら
                    workController.RemoveWork(checkedTime);
                    numberOfWork--;
                }

                //搬入を検知したら
                workController.WriteProcesChangeData(CommonParameters.Process_Number.Assembly_in, checkedTime);
                numberOfWork++;
                UpdateStationState(MEMORY_SPACE.NUMBER_OF_WORK_FUNCTIONAL_STATION, numberOfWork);
                beforeStationMonitor.RemoveWork();


                int numberOfStock_OK = 0;
                int numberOfStock_NG = 0;
                //ストック数が変化したら
                UpdateStationState(MEMORY_SPACE.NUMBER_OF_OKSTOCK, numberOfStock_OK);
                UpdateStationState(MEMORY_SPACE.NUMBER_OF_NGSTOCK, numberOfStock_NG);

                //ストッカの取り出し等、システム停止が行われたら
                UpdateStationState(MEMORY_SPACE.IS_SYSTEM_PAUSE, 1);
            }

            if (plcDatas.B15_Block.IsAnyBitStundUp)
            {
                List<DataBlock.ChangeBitData> changeData =
                    plcDatas.B15_Block.StandUpDatas(0, 1, 3, 4, 10, 11);

                foreach (var e in changeData)
                {
                    if (e.BitNumber == 0)
                    {
                        UpdateStationState(MEMORY_SPACE.STATE_OF_ASSEMBLY_STATION, 1);
                    }
                    if (e.BitNumber == 1)
                    {
                        UpdateStationState(MEMORY_SPACE.STATE_OF_ASSEMBLY_STATION, 2);
                    }
                    if (e.BitNumber == 4)
                    {
                        UpdateStationState(MEMORY_SPACE.STATE_OF_ASSEMBLY_STATION, 4);
                    }

                }
            }
        }

    }
}