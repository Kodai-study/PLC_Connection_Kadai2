using System.Collections.Generic;
using System.Text;

namespace PLC_Connection.InspectionResultDataModel
{
    /// <summary>
    ///  電池ソケットの検査結果をまとめるクラス
    /// </summary>  
    /// <remarks> 詳しい中身などは同じインタフェースを実装しているICクラスを参照 </remarks>
    /// <seealso cref="IC"/>
    public class BAT_SOCKET : Parts
    {
        private readonly string[] errorCodes = new string[]
        {
            "BS001",
            "BS002"
        };
        public BAT_SOCKET() { }
        public BAT_SOCKET(CHECK_RESULT SOCKET_DIR)
        {
            this.SOCKET_DIR = SOCKET_DIR;
        }
        public CHECK_RESULT SOCKET_DIR = CHECK_RESULT.NO_CHECK;

        string Parts.GetResultString()
        {
            StringBuilder messageStr = new StringBuilder();
            messageStr.Append("電池ソケット ");
            messageStr.Append("\n\t電池ソケットの向き ");
            messageStr.Append(SOCKET_DIR == CHECK_RESULT.OK ? "〇" : "×");
            return messageStr.ToString();
        }

        List<string> Parts.GetErrorCodes()
        {
            List<string> errors = null;
            if (SOCKET_DIR == CHECK_RESULT.NG)
            {
                errors = new List<string>();
                errors.Add(errorCodes[0]);
            }
            return errors;
        }
    }
}