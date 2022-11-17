using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
            int open = dotUtlType.Open();


            string rabel = "テスト";
            var buff = new int[2];
            int read = dotUtlType.ReadDeviceBlock(ref rabel,2, ref buff);
            Console.WriteLine(buff);

            rabel = "Test5";
            int device = 5;
            read = dotUtlType.GetDevice(ref rabel, ref device);

            rabel = "OutTest";
            read = dotUtlType.GetDevice(ref rabel, ref device);
           // Console.WriteLine(device);

            rabel = "テスティング";
            device = 0;
            int count = 0;
            while (device != 0 || count < 1000)
            {
                read = dotUtlType.GetDevice(ref rabel, ref device);
                if (read != 0)
                {
                    Console.WriteLine("エラー");
                    break;
                }
                else if (device != 0)
                {
                    Console.WriteLine("受け取りします");
                    break;
                }
                System.Threading.Thread.Sleep(100);
                count++;
            }
            
        }


    }
}
