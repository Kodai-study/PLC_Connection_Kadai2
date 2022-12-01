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
using System.Data;
using System.Reflection.Emit;

namespace PLC_Connection
{
    class PLC_MonitorTask
    {
        public DotUtlType dotUtlType;

        private CancellationTokenSource cancellToken;

        SqlConnection sqlConnection;

        bool b = false;
        public async Task<bool> Start()
        {
            this.cancellToken = new CancellationTokenSource();
            dotUtlType = new DotUtlType();
            dotUtlType.ActLogicalStationNumber = 401;
            if (dotUtlType.Open() != 0)
                return false;

            sqlConnection = new SqlConnection("Data Source=RBPC12;Initial Catalog=Robot22_DB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            sqlConnection.Open();
            await Task.Run(() => Run(cancellToken));
            return false;
        }

        public void Run(CancellationTokenSource token)
        {
            bool dbWrite = false;
            int[] datas = new int[1];
            string label = "shine";
            Console.WriteLine("ThreadProc start.");
            int read = dotUtlType.ReadDeviceBlock(ref label, 1, ref datas);
            int old_data = datas[0];
            Task<bool> dbWriteTask = null;
            while (true)
            {
                read = dotUtlType.ReadDeviceBlock(ref label, 1, ref datas);


                if (old_data != datas[0])
                {
                    DateTime dateTime = DateTime.Now;
                    int diff = datas[0] ^ old_data;
                    dbWriteTask = Task.Run(() => Task_WriteDB(dateTime,diff,datas[0]));
                    dbWrite = true;
                    old_data = datas[0];
                }


                // キャンセル要求
                if (token.IsCancellationRequested)
                {
                    Console.WriteLine("MyThread IsCancellationRequested");
                    break;
                }
                Thread.Sleep(10);

                if (dbWrite)
                {
                    Console.WriteLine(dbWriteTask.Result);
                    dbWrite = false;
                }
            }
        }

        public bool Task_WriteDB(DateTime nowTime,int changeBit,int sensorData)
        {
            try
            {
                int sensorNum = 0;
                int on_off = (changeBit & sensorData) != 0 ? 1 : 0;

                for(;changeBit != 0;changeBit >>= 1,sensorNum++) {; }

                string cmd = String.Format("INSERT INTO PLC_Test (Time,sensor_Number,ON_OFF) VALUES ('{0}.{1:D3}' , {2} , {3})", nowTime, nowTime.Millisecond,sensorNum, on_off);
                using (var command = new SqlCommand(cmd, sqlConnection))
                    command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }



        public void hoge()
        {
            string label = "wordSample";
            int data = 0;
            dotUtlType.GetDevice(ref label, ref data);

        }

        public void stop()
        {
            cancellToken.Cancel();
        }
    }
}