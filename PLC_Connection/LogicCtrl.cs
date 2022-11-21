using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MITSUBISHI.Component;
using System.Data.SqlClient;
using ActUtlTypeLib;

namespace PLC_Connection
{
    class LogicCtrl
    {
        private DotUtlType dotUtlType;
        public void a()
        {
            dotUtlType = new DotUtlType();
            dotUtlType.ActLogicalStationNumber = 1;
            int open = dotUtlType.Open();


            string rabel = "テスト";
            var buff = new int[2];
            int read = dotUtlType.ReadDeviceBlock(ref rabel, 2, ref buff);
            Console.WriteLine(buff);

            rabel = "Test5";
            int device = 5;
            read = dotUtlType.GetDevice(ref rabel, ref device);

            rabel = "OutTest";
            read = dotUtlType.GetDevice(ref rabel, ref device);
            // Console.WriteLine(device);

        }


        public bool waitTrigger(string rabel,ref int code)
        {
            int device = 0;
            int count = 0;
            int Code = 0;
            while (device != 0 || count < 1000)
            {
                Code = dotUtlType.GetDevice(ref rabel, ref device);
                if (Code != 0)
                {
                    Console.WriteLine("エラー");
                    code = Code;
                    return false;
                }
                else if (device == 1)
                {
                    return true;
                }
                Thread.Sleep(100);
                count++;
            }
            return false;
        }
    }
}
