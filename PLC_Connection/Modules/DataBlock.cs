﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLC_Connection.Modules
{
    public class DataBlock
    {
        public const int BLOCK_SIZE = 16;
        private int data;

        private int oldData;
        List<ChangeBitData> changes;

        public int BitMask { get; set; } = -1;
        public int NewBlockData
        {
            set
            {
                changes = new List<ChangeBitData>();
                int checkBit = 1;
                for (int i = 0; i < BLOCK_SIZE; i++)
                {
                    if ((oldData & checkBit) != (value & checkBit))
                    {
                        changes.Add(new ChangeBitData(i,
                            (value & checkBit) != 0));
                    }
                    checkBit <<= 1;
                }
                oldData = value;
            }
        }

        public bool IsChangeBit
        {
            get
            {
                return changes.Count > 0;
            }
        }

        public bool IsAnyBitStundUp
        {
            get
            {
                foreach (var e in changes)
                {
                    if (e.isStundUp)
                        return true;
                }
                return false;
            }
        }

        public List<ChangeBitData> ChangedDatas()
        {
            List<ChangeBitData> changeBitDataList = new List<ChangeBitData>();

            return changeBitDataList;
        }

        public class ChangeBitData
        {
            public int bitNumber { get; set; }
            public bool isStundUp { get; set; }
            public ChangeBitData(int bitNumber, bool isStundUp)
            {
                this.bitNumber = bitNumber;
                this.isStundUp = isStundUp;
            }
        }
    }
}