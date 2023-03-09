using PLC_Connection.Modules;
using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;

namespace PLC_Connection.StationMonitor
{
    public class SupplyURMonitor : Base_StationMonitor
    {
        public SupplyURMonitor(PLC_MonitorTask plc_MonitorTask, WorkController workController, MemoryMappedViewAccessor commonMemoryAccessor) : base(plc_MonitorTask, workController, commonMemoryAccessor)
        {
        }

        override public void CheckData(PLCContactData plcDatas, DateTime checkedTime)
        {
            if (plcDatas.B03_Block.IsAnyBitStundUp)
            {
                List<DataBlock.ChangeBitData> changeData = plcDatas.B03_Block.ChangedDatas();
                foreach (var e in changeData)
                {
                    if (e.bitNumber == 2 && e.IsStundUp)
                    {
                        UpdateStationState(MEMORY_SPACE.IS_FUNCTION_INSPECTED_JUST_BEFORE, 1);
                    }
                    else if (e.bitNumber == 1)
                    {
                        //workController.AddnewWork(checkedTime);
                    }
                }
            }
        }

    }
}