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
using System.Reflection.Emit;

namespace PLC_Connection
{
    class Net_test
    {
        public DotUtlType plcControll;
        public Net_test()
        {
            plcControll = new DotUtlType();
            plcControll.ActLogicalStationNumber = 401;
            if (plcControll.Open() != 0)
            {
                throw new Exception("PLCとの接続に失敗");
            }
        }

        public void loop()
        {
            int[] datas = new int[1];
            string label = "shine";
            int read = plcControll.ReadDeviceBlock(ref label, 1, ref datas);
            int old_data = datas[0];
            while (read == 0)
            {
                read = plcControll.ReadDeviceBlock(ref label, 1, ref datas);
                if (datas[0] != old_data)
                {
                    int diff = datas[0] ^ old_data;
                    Console.WriteLine("0x" + Convert.ToString(diff, 16) + (((datas[0] & diff) != 0) ? " : ON" : " : OFF"));
                    old_data = datas[0];
                }
                Thread.Sleep(50);
            }
            Console.WriteLine("owari");
        }

        public void subProcess()
        {
        }

        public void writeDB()
        {

        }
    }
}