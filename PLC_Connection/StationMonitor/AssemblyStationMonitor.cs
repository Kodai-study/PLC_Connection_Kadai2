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
    public class AssemblyStationMonitor : Base_StationMonitor
    {

        private int numberOfWork = 0;

        public AssemblyStationMonitor(PLC_MonitorTask plc_MonitorTask, WorkController workController, MemoryMappedViewAccessor commonMemoryAccessor) : base(plc_MonitorTask, workController, commonMemoryAccessor)
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

                /*
                 numberOfWork ++;
                
                UpdateStationState(MEMORY_SPACE.NUMBER_OF_WORK_FUNCTIONAL_STATION, numberOfWork);
                 */
            }
        }


    }
}