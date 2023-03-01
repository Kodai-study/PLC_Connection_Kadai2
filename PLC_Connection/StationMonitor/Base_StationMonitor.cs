﻿using PLC_Connection.Modules;
using ResultDatas;
using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLC_Connection.StationMonitor
{
    public abstract class Base_StationMonitor
    {

        protected PLC_MonitorTask plc_MonitorTask;
        protected WorkController workController;
        protected MemoryMappedViewAccessor commonMemoryAccessor;
        private int[] writeMemoryStartAddress = new int[(int)MEMORY_SPACE.NUMNER_OF_STATE_KIND];


        public Base_StationMonitor(PLC_MonitorTask plc_MonitorTask, WorkController workController, MemoryMappedViewAccessor commonMemoryAccessor)
        {
            this.plc_MonitorTask = plc_MonitorTask;
            this.workController = workController;
            this.commonMemoryAccessor = commonMemoryAccessor;

            int sizeOfInt32 = sizeof(int);
            int sizeOfBoolean = sizeof(bool);
            int headAddress = 0;
            writeMemoryStartAddress[(int)MEMORY_SPACE.NUMBER_OF_WORK_VISUAL_STATION] = (headAddress += sizeOfInt32);
            writeMemoryStartAddress[(int)MEMORY_SPACE.NUMBER_OF_WORK_FUNCTIONAL_STATION] = (headAddress += sizeOfInt32);
            writeMemoryStartAddress[(int)MEMORY_SPACE.NUMBER_OF_WORK_ASSEMBLY_STATION] = (headAddress += sizeOfInt32);
            writeMemoryStartAddress[(int)MEMORY_SPACE.NUMBER_OF_OKSTOCK] = (headAddress += sizeOfInt32);
            writeMemoryStartAddress[(int)MEMORY_SPACE.NUMBER_OF_NGSTOCK] = (headAddress += sizeOfInt32);
            writeMemoryStartAddress[(int)MEMORY_SPACE.IS_SYSTEM_PAUSE] = (headAddress += sizeOfBoolean);
            writeMemoryStartAddress[(int)MEMORY_SPACE.IS_INSPECTED_JUST_BEFORE] = (headAddress += sizeOfBoolean);
            writeMemoryStartAddress[(int)MEMORY_SPACE.RESULT_FREQUENCY] = (headAddress += sizeOfInt32);
            writeMemoryStartAddress[(int)MEMORY_SPACE.RESULT_VOLTAGE] = (headAddress += sizeOfInt32);
            writeMemoryStartAddress[(int)MEMORY_SPACE.STATE_OF_SUPPLY_ROBOT] = (headAddress += sizeOfInt32);
            writeMemoryStartAddress[(int)MEMORY_SPACE.STATE_OF_VISUAL_STATION] = (headAddress += sizeOfInt32);
            writeMemoryStartAddress[(int)MEMORY_SPACE.STATE_OF_FUNCTION_STATION] = (headAddress += sizeOfInt32);
            writeMemoryStartAddress[(int)MEMORY_SPACE.STATE_OF_ASSEMBLY_STATION] = (headAddress += sizeOfInt32);
        }

        public abstract void CheckData(PLCContactData plcDatas);

        protected void UpdateStationState(MEMORY_SPACE kindOfState, int value)
        {
            if (kindOfState == MEMORY_SPACE.IS_SYSTEM_PAUSE || kindOfState == MEMORY_SPACE.IS_INSPECTED_JUST_BEFORE)
                commonMemoryAccessor.Write(writeMemoryStartAddress[(int)kindOfState], value != 0);
            else
                commonMemoryAccessor.Write(writeMemoryStartAddress[(int)kindOfState], value);
        }
    }
}