using PLC_Connection.Modules;
using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;

namespace PLC_Connection.StationMonitor
{
    public class SupplyURMonitor : Base_StationMonitor
    {

        private int numberOfWork = 0;

        DateTime lastInspectedTime;

        public SupplyURMonitor(PLC_MonitorTask plc_MonitorTask, WorkController workController, MemoryMappedViewAccessor commonMemoryAccessor) : base(plc_MonitorTask, workController, commonMemoryAccessor)
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
                        UpdateStationState(MEMORY_SPACE.IS_INSPECTED_JUST_BEFORE, 1);
                    }
                    else if (e.bitNumber == 1)
                    {

                    }
                }
            }
            if(lastInspectedTime + new TimeSpan(0,0,5) < DateTime.Now)
            {
                UpdateStationState(MEMORY_SPACE.IS_INSPECTED_JUST_BEFORE, 0);
            }
        }

    }
}