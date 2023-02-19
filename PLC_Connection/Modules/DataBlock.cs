using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLC_Connection.Modules
{
    public abstract class DataBlock
    {
        private int data;

        private int oldData;

        public int BitMask { get; set; } = -1;

        public bool IsNeedInsertData
        {
            get { return false; }
        }

        public List<ChangeBitData> ChangedDatas() {
            List<ChangeBitData> changeBitDataList = new List<ChangeBitData>();

            return changeBitDataList;
        }

        public class ChangeBitData
        {
            int bitNumber { get; set; }
            bool isStundUp { get; set; }
            public ChangeBitData(int bitNumber, bool isStundUp) {
                this.bitNumber = bitNumber;
                this.isStundUp = isStundUp;
            }
        }
    }
}
