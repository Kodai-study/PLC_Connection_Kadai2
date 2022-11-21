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
    class Class1
    {
        public DotUtlType dotUtlType;
        public static void Main()
        {
            var logicCtrl = new LogicCtrl();
            int errCode = 0;


            if (logicCtrl.waitTrigger("テスティング", ref errCode))
                ConnectDB();
        }


        static void ConnectDB()
        {
            SqlConnection a = new SqlConnection("Data Source=RBPC12;Initial Catalog=Robot22_DB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            a.Open();

            string cmd = String.Format("INSERT INTO PLC_Test (Time) VALUES ({0})", DateTime.Now);
            using (var command = new SqlCommand(cmd, a))
                command.ExecuteNonQuery();
        }
    }
}