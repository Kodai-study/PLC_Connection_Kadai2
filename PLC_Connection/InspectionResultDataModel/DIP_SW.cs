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
            "DS004",
            "DS014",
            "DS024",
            "DS034",
            "DS044",
            "DS054",
            "DS064",
            "DS074",
            "DS084",
            "DS094",
            "DS104",
            "DS114",
            "DS124",
            "DS134",
            "DS144",
            "DS154"
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