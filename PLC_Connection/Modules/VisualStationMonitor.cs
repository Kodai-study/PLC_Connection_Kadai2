using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLC_Connection.Modules
{
    public class VisualStationMonitor
    {

        private PLC_MonitorTask plc_MonitorTask;

        public VisualStationMonitor(PLC_MonitorTask plc_MonitorTask)
        {
            this.plc_MonitorTask = plc_MonitorTask;
        }   

        public void checkData(PLCContactData plcDatas)
        {
            if (plcDatas.X40_Block.IsAnyBitStundUp)
            {
                List<DataBlock.ChangeBitData> changeData = plcDatas.X40_Block.ChangedDatas();
                foreach(var e in changeData)
                {
                    if(e.bitNumber == 0)
                    {
                        int[] resultBlock = plc_MonitorTask.getVisualInspectionResult();
                       
                    }
                }
                
            }
            
        }   

        private bool checkOneBit(int bitBlock,int bitNumber)
        {
            bitNumber = 1 << bitNumber;
            return (bitBlock & bitNumber) != 0;
        }

    }
}
