using ResultDatas;
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
        private WorkController workController;

        private ResultDataCreater[] resultCreaters = new ResultDataCreater[] {
            new BlockToResultChanger_X41(),
            new BlockToResultChanger_X42(),
            new BlockToResultChanger_X43(),
            new BlockToResultChanger_X44()
        };

        public VisualStationMonitor(PLC_MonitorTask plc_MonitorTask, WorkController workController)
        {
            this.plc_MonitorTask = plc_MonitorTask;
            this.workController = workController;
        }

        public void CheckData(PLCContactData plcDatas)
        {
            if (plcDatas.X40_Block.IsAnyBitStundUp)
            {
                List<DataBlock.ChangeBitData> changeData = plcDatas.X40_Block.ChangedDatas();
                foreach (var e in changeData)
                {
                    if (e.bitNumber != 0)
                        continue;

                    Results visualInspectionResult = new Results();
                    int[] resultBlock = plc_MonitorTask.getVisualInspectionResult();
                    for (int i = 0; i < resultBlock.Length; i++)
                    {
                        resultCreaters[i].CheckResult(ref visualInspectionResult,
                            resultBlock[i]);
                    }
                    Console.WriteLine(visualInspectionResult);
                }
            }
        }

        private bool checkOneBit(int bitBlock, int bitNumber)
        {
            bitNumber = 1 << bitNumber;
            return (bitBlock & bitNumber) != 0;
        }

    }
}