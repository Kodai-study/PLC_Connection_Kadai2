using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLC_Connection.Modules
{
    public class PLCContactData
    {
        DataBlock X00_blockData = new DataBlock();
        DataBlock Y31_blockData = new DataBlock();
        DataBlock Y33_blockData = new DataBlock();
        DataBlock Y34_blockData = new DataBlock();
        DataBlock X40_blockData = new DataBlock();
        DataBlock X41_blockData = new DataBlock();
        DataBlock X42_blockData = new DataBlock();
        DataBlock X43_blockData = new DataBlock();
        DataBlock X44_blockData = new DataBlock();
        DataBlock X45_blockData = new DataBlock();

       public DataBlock X00_Block { get { return X00_blockData; } }
       public DataBlock Y31_Block { get { return Y31_blockData; } }
       public DataBlock Y33_Block { get { return Y33_blockData; } }
       public DataBlock Y34_Block { get { return Y34_blockData; } }
       public DataBlock X40_Block { get { return X40_blockData; } }
       public DataBlock X41_Block { get { return X41_blockData; } }
       public DataBlock X42_Block { get { return X42_blockData; } }
       public DataBlock X43_Block { get { return X43_blockData; } }
       public DataBlock X44_Block { get { return X44_blockData; } }
       public DataBlock X45_Block { get { return X45_blockData; } }
    }                   
}