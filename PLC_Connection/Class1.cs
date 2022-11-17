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
            DotUtlType dotUtlType = new DotUtlType();
            dotUtlType.ActLogicalStationNumber = 1;
            int code = dotUtlType.Open();

            
            string rabel = "Test";
            var buff = new int[8];
            dotUtlType.ReadDeviceBlock(ref rabel, 8, ref buff);
            Console.WriteLine(buff);
        }
    }
}
