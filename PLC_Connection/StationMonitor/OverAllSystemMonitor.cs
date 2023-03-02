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
    public class OverAllSystemMonitor : Base_StationMonitor
    {
        public OverAllSystemMonitor(PLC_MonitorTask plc_MonitorTask, WorkController workController, MemoryMappedViewAccessor commonMemoryAccessor) : base(plc_MonitorTask, workController, commonMemoryAccessor)
        {
        }

        public override void CheckData(PLCContactData plcDatas, DateTime checkedTime)
        {
            //システム状態の変化が有ったら
            int systemState = 0;
            UpdateStationState(MEMORY_SPACE.STATE_OF_OVERALL_SYSTEM, systemState);
        }
    }
}