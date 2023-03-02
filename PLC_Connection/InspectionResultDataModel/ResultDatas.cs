using System.Collections.Generic;
using System.Text;

namespace PLC_Connection.InspectionResultDataModel
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
        /// <summary>
        ///  検査パーツごとの結果は、初期値"未検査"
        ///  で初期化される。
        /// </summary>
        public Results()
        {
            ic = new IC();
            work = new WORK();
            dipSwitch = new DIP_SW();
            transister = new TR();
            batterySocket = new BAT_SOCKET();
            diode = new DIODE();
            resistor = new RESISTOR();

            parts_ic = ic;
            parts_dipSW = dipSwitch;
            parts_work = work;
            parts_battery = batterySocket;
            parts_diode = diode;
            parts_tr = transister;
        }
        /* パーツごとの検査結果 */
        public IC ic;
        public WORK work;
        public TR transister;
        public DIP_SW dipSwitch;
        public BAT_SOCKET batterySocket;
        public DIODE diode;
        public RESISTOR resistor;

        /* 共通の処理を使うためインタフェースでインスタンスを持っておく */
        private Parts parts_ic;
        private Parts parts_work;
        private Parts parts_dipSW;
        private Parts parts_tr;
        private Parts parts_battery;
        private Parts parts_diode;

        public bool result_AllOK { get; set; } = false;

        /// <summary>
        ///  デバッグ、見せる用。
        ///  結果を文字列で表示する
        /// </summary>
        /// <returns> パーツ、それぞれの検査結果を表示する文字列 </returns>
        public override string ToString()
        {
            StringBuilder str = new StringBuilder();

            str.Append(parts_work.GetResultString());
            str.Append("\n\n");
            str.Append(parts_ic.GetResultString());
            str.Append("\n\n");
            str.Append(parts_dipSW.GetResultString());
            str.Append("\n\n");
            str.Append(parts_diode.GetResultString());
            str.Append("\n\n");
            str.Append(parts_battery.GetResultString());
            str.Append("\n\n");
            str.Append(parts_tr.GetResultString());

            return str.ToString();
        }

        /// <summary>
        ///  検査結果から、データベースに書き込むエラーコード
        ///  のリストを作成する
        /// </summary>
        /// <returns></returns>
        public List<string> getErrorCodes()
        {
            List<string> errors = new List<string>();
            List<string> buf;
            if ((buf = parts_work.GetErrorCodes()) != null)
                errors.AddRange(buf);
            if ((buf = parts_ic.GetErrorCodes()) != null)
                errors.AddRange(buf);
            if ((buf = parts_dipSW.GetErrorCodes()) != null)
                errors.AddRange(buf);
            if ((buf = parts_diode.GetErrorCodes()) != null)
                errors.AddRange(buf);
            if ((buf = parts_battery.GetErrorCodes()) != null)
                errors.AddRange(buf);
            if ((buf = parts_tr.GetErrorCodes()) != null)
                errors.AddRange(buf);

            if (errors.Count == 0)
            {
                errors.Add("OK   ");
                result_AllOK = true;
            }
            return errors;
        }
    }
}