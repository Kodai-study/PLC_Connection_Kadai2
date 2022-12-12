using System.Text;

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
        public Results()
        {
            ic = new IC();
            work = new WORK();
            dipSwitch = new DIP_SW();
            transister = new TR();
            batterySocket = new BAT_SOCKET();
            diode = new DIODE();
        }
        public IC ic;
        public WORK work;
        public TR transister;
        public DIP_SW dipSwitch;
        public BAT_SOCKET batterySocket;
        public DIODE diode;

        public override string ToString()
        {
            StringBuilder str = new StringBuilder("IC : ");
            str.Append("\n\tIC1_がある ");
            str.Append(ic.IC1_OK == CHECK_RESULT.OK ? "〇" : "×");
            str.Append("\n\tIC1_の向き ");
            str.Append(ic.IC1_DIR == CHECK_RESULT.OK ? "〇" : "×");
            str.Append("\n\tIC2_がある ");
            str.Append(ic.IC2_OK == CHECK_RESULT.OK ? "〇" : "×");
            str.Append("\n\tIC2_の向き ");
            str.Append(ic.IC2_DIR == CHECK_RESULT.OK ? "〇" : "×");

            str.Append("\n\nWORK : ");
            str.Append("\n\tワークが正しいものか ");
            str.Append(work.WORK_OK == CHECK_RESULT.OK ? "〇" : "×");
            str.Append("\n\tワークの向き ");
            str.Append(work.WORK_DIR == CHECK_RESULT.OK ? "〇" : "×");

            /*
            str.Append("\n\nTR");
            str.Append("\n\tトランジスタが正しい ");
            str.Append(transister.TR_OK == CHECK_RESULT.OK ? "〇" : "×");
            */

            str.Append("\n\nDIP_SW");
            str.Append("\n\tDIPスイッチのパターンが正しい ");
            str.Append(dipSwitch.DIP_OK == CHECK_RESULT.OK ? "〇" : "×");
            str.Append("\n\tDIPスイッチのパターン ");
            str.Append(dipSwitch.DIP_PATTERN);

            str.Append("\n\n電池ソケット ");
            str.Append("\n\t電池ソケットの向き ");
            str.Append(batterySocket.SOCKET_DIR == CHECK_RESULT.OK ? "〇" : "×");

            str.Append("\n\nダイオード");
            str.Append("\n\tダイオードの向きが正しい ");
            str.Append(diode.DIODE_DIR == CHECK_RESULT.OK ? "〇" : "×");
            return str.ToString();
        }
    }
    public class IC
    {
        public IC() { }
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
        public WORK() { }
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
        public TR() { }
        public TR(CHECK_RESULT TR_OK)
        {
            this.TR_OK = TR_OK;
        }
        public CHECK_RESULT TR_OK = CHECK_RESULT.NO_CHECK;
    }
    public class DIP_SW
    {
        public DIP_SW() { }
        public DIP_SW(CHECK_RESULT DIP_OK, int DIP_PATTERN)
        {
            this.DIP_OK = DIP_OK;
            this.DIP_PATTERN = DIP_PATTERN;
        }
        public CHECK_RESULT DIP_OK = CHECK_RESULT.NO_CHECK;
        public int DIP_PATTERN = -1;
    }
    public class BAT_SOCKET
    {
        public BAT_SOCKET() { }
        public BAT_SOCKET(CHECK_RESULT SOCKET_DIR)
        {
            this.SOCKET_DIR = SOCKET_DIR;
        }
        public CHECK_RESULT SOCKET_DIR = CHECK_RESULT.NO_CHECK;
    }
    public class DIODE
    {
        public DIODE() { }
        public DIODE(CHECK_RESULT DIODE_DIR)
        {
            this.DIODE_DIR = DIODE_DIR;
        }
        public CHECK_RESULT DIODE_DIR = CHECK_RESULT.NO_CHECK;
    }
}