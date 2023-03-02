using System.Collections.Generic;
using System.Text;

namespace PLC_Connection.InspectionResultDataModel
{
    /// <summary>
    ///  シリコンダイオードの検査結果をまとめるクラス
    /// </summary>  
    /// <remarks> 詳しい中身などは同じインタフェースを実装しているICクラスを参照 </remarks>
    /// <seealso cref="IC"/>
    public class DIODE : Parts
    {
        private readonly string[] errorCodes = new string[]
        {
             "DI001",
             "DI002"
        };
        public DIODE() { }
        public DIODE(CHECK_RESULT DIODE_DIR)
        {
            this.DIODE_DIR = DIODE_DIR;
        }
        public CHECK_RESULT DIODE_DIR = CHECK_RESULT.NO_CHECK;

        string Parts.GetResultString()
        {
            StringBuilder messageStr = new StringBuilder();
            messageStr.Append("ダイオード");
            messageStr.Append("\n\tダイオードの向きが正しい ");
            messageStr.Append(DIODE_DIR == CHECK_RESULT.OK ? "〇" : "×");
            return messageStr.ToString();
        }

        List<string> Parts.GetErrorCodes()
        {
            List<string> errors = null;
            if (DIODE_DIR == CHECK_RESULT.NG)
            {
                errors = new List<string>();
                errors.Add(errorCodes[0]);
            }
            return errors;
        }
    }
}