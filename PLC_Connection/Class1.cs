using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MITSUBISHI.Component;


namespace PLC_Connection
{
   class Class1
    {
        public static void Main()
        {
            DotUtlType dotUtlType = new DotUtlType() { ActLogicalStationNumber = 1 };
            dotUtlType.Open();
        }
    }
}
