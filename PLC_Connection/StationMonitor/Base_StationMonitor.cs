using PLC_Connection.Modules;
using ResultDatas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLC_Connection.StationMonitor
{
    public abstract class Base_StationMonitor
    {

        protected PLC_MonitorTask plc_MonitorTask;
        protected WorkController workController;


        public Base_StationMonitor(PLC_MonitorTask plc_MonitorTask, WorkController workController)
        {
            this.plc_MonitorTask = plc_MonitorTask;
            this.workController = workController;
        }

        public abstract void CheckData(PLCContactData plcDatas);

    }
}