using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MITSUBISHI.Component;

namespace ResultDatas
{
    /// <summary>
    ///  検査結果に入るデータの種類を定義した列挙型
    /// </summary>
    public enum CHECK_RESULT
    {
        NG = -1,
        NO_CHECK,
        OK
    }

    /// <summary>
    ///  ワークの検査結果をまとめたクラス
    /// </summary>
    public class Results
    {
        public IC ic;
        public WORK work;
        public TR transister;
        public DIP_SW dipSwitch;
        public BAT_SOCKET batterySocket;
        public DIODE diode;
    }
    public class IC
    {
        public IC(CHECK_RESULT IC1_OK, CHECK_RESULT IC1_DIR, CHECK_RESULT IC2_OK, CHECK_RESULT IC2_DIR)
        {
            this.IC1_OK = IC1_OK;
            this.IC1_DIR = IC1_DIR;
            this.IC2_OK = IC2_OK;
            this.IC2_DIR = IC2_DIR;
        }
        public CHECK_RESULT IC1_OK = CHECK_RESULT.NO_CHECK;
        public CHECK_RESULT IC1_DIR = CHECK_RESULT.NO_CHECK;
        public CHECK_RESULT IC2_OK = CHECK_RESULT.NO_CHECK;
        public CHECK_RESULT IC2_DIR = CHECK_RESULT.NO_CHECK;
    }
    public class WORK
    {
        public WORK(CHECK_RESULT WORK_OK, CHECK_RESULT WORK_DIR)
        {
            this.WORK_OK = WORK_OK;
            this.WORK_DIR = WORK_DIR;
        }
        public CHECK_RESULT WORK_OK = CHECK_RESULT.NO_CHECK;
        public CHECK_RESULT WORK_DIR = CHECK_RESULT.NO_CHECK;
    }
    public class TR
    {
        public TR(CHECK_RESULT TR_OK)
        {
            this.TR_OK = TR_OK;
        }
        public CHECK_RESULT TR_OK = CHECK_RESULT.NO_CHECK;
    }
    public class DIP_SW
    {
        public DIP_SW(CHECK_RESULT DIP_OK,int DIP_PATTERN)
        {
            this.DIP_OK = DIP_OK;
            this.DIP_PATTERN = DIP_PATTERN;
        }
        public CHECK_RESULT DIP_OK = CHECK_RESULT.NO_CHECK;
        public int DIP_PATTERN = -1;
    }
    public class BAT_SOCKET
    {
        public BAT_SOCKET(CHECK_RESULT SOCKET_DIR)
        {
            this.SOCKET_DIR = SOCKET_DIR;
        }
        public CHECK_RESULT SOCKET_DIR = CHECK_RESULT.NO_CHECK;
    }
    public class DIODE
    {
        public DIODE(CHECK_RESULT DIODE_DIR)
        {
            this.DIODE_DIR = DIODE_DIR;
        }
        public CHECK_RESULT DIODE_DIR = CHECK_RESULT.NO_CHECK;
    }
}