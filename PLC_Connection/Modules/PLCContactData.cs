using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLC_Connection.Modules
{
    public class PLCContactData
    {
        readonly DataBlock B03_blockData = new DataBlock();
        readonly DataBlock B06_blockData = new DataBlock();
        readonly DataBlock B15_blockData = new DataBlock();
        readonly DataBlock B0C_blockData = new DataBlock();
        readonly DataBlock B0D_blockData = new DataBlock();
        readonly DataBlock B0E_blockData = new DataBlock();
        readonly DataBlock X00_blockData = new DataBlock();
        readonly DataBlock X40_blockData = new DataBlock();
        readonly DataBlock Test_blockData = new DataBlock();

        public DataBlock B03_Block { get { return B03_blockData; } }
        public DataBlock B06_Block { get { return B06_blockData; } }
        public DataBlock B0C_Block { get { return B0C_blockData; } }
        public DataBlock B0D_Block { get { return B0D_blockData; } }
        public DataBlock B0E_Block { get { return B0E_blockData; } }
        public DataBlock B15_Block { get { return B15_blockData; } }
        public DataBlock X00_Block { get { return X00_blockData; } }
        public DataBlock X40_Block { get { return X40_blockData; } }
        public DataBlock Test_Block { get { return Test_blockData; } }
    }
}