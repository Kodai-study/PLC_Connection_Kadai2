using System.Collections.Generic;
using System.Text;

namespace PLC_Connection.InspectionResultDataModel
{
    /// <summary>
    ///  DIPスイッチの検査結果をまとめるクラス
    /// </summary>  
    /// <remarks> 詳しい中身などは同じインタフェースを実装しているICクラスを参照 </remarks>
    /// <seealso cref="IC"/>
    public class DIP_SW : Parts
    {
        private readonly string[] errorCodes = new string[]
        {
            "DS00",
            "DS01",
            "DS02",
            "DS03",
            "DS04",
            "DS05",
            "DS06",
            "DS07",
            "DS08",
            "DS09",
            "DS10",
            "DS11",
            "DS12",
            "DS13",
            "DS14",
            "DS15"
        };
        public DIP_SW() { }
        public DIP_SW(CHECK_RESULT DIP_OK, int DIP_PATTERN)
        {
            this.DIP_OK = DIP_OK;
            this.DIP_PATTERN = DIP_PATTERN;
        }
        public CHECK_RESULT DIP_OK = CHECK_RESULT.NO_CHECK;
        public int DIP_PATTERN = -1;

        string Parts.GetResultString()
        {
            StringBuilder messageStr = new StringBuilder();
            messageStr.Append("DIP_SW");
            messageStr.Append("\n\tDIPスイッチのパターンが正しい ");
            messageStr.Append(DIP_OK == CHECK_RESULT.OK ? "〇" : "×");
            messageStr.Append("\n\tDIPスイッチのパターン ");
            messageStr.Append(DIP_PATTERN);
            return messageStr.ToString();
        }

        List<string> Parts.GetErrorCodes()
        {
            if (DIP_OK == CHECK_RESULT.OK)
            {
                return null;
            }
            List<string> errors = new List<string>();
            errors.Add(errorCodes[DIP_PATTERN]);
            return errors;
        }
    }
}