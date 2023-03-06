using System;
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
        private List<ChangeBitData> changes;

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
                if (changes == null)
                    return false;

                foreach (var e in changes)
                {
                    if (e.IsStundUp)
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
        public List<ChangeBitData> ChangedDatas(params int[] filterBits)
        {
            List<ChangeBitData> changeBitDataList = new List<ChangeBitData>();
            changeBitDataList.FindAll(e => { return filterBits.Contains(e.bitNumber); });
            return changeBitDataList;
        }

        public class ChangeBitData
        {
            public int bitNumber { get; set; }
            public bool IsStundUp { get; set; }
            public ChangeBitData(int bitNumber, bool isStundUp)
            {
                this.bitNumber = bitNumber;
                this.IsStundUp = isStundUp;
            }
        }
    }
}