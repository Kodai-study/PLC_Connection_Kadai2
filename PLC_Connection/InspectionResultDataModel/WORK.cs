using System.Collections.Generic;
using System.Text;

namespace PLC_Connection.InspectionResultDataModel
{
    /// <summary>
    ///  ワーク自体の検査結果をまとめるクラス
    /// </summary>  
    /// <remarks> 詳しい中身などは同じインタフェースを実装しているICクラスを参照 </remarks>
    /// <seealso cref="IC"/>
    public class WORK : Parts
    {
        private readonly string[] errorCodes = new string[]
        {
            "WK001",
            "WK002"
        };
        public WORK() { }
        public WORK(CHECK_RESULT WORK_OK, CHECK_RESULT WORK_DIR)
        {
            this.WORK_OK = WORK_OK;
            this.WORK_DIR = WORK_DIR;
        }
        public CHECK_RESULT WORK_OK = CHECK_RESULT.NO_CHECK;
        public CHECK_RESULT WORK_DIR = CHECK_RESULT.NO_CHECK;

        string Parts.GetResultString()
        {
            StringBuilder messageStr = new StringBuilder();
            messageStr.Append("WORK : ");
            messageStr.Append("\n\tワークが正しいものか ");
            messageStr.Append(WORK_OK == CHECK_RESULT.OK ? "〇" : "×");
            messageStr.Append("\n\tワークの向き ");
            messageStr.Append(WORK_DIR == CHECK_RESULT.OK ? "〇" : "×");
            return messageStr.ToString();
        }

        List<string> Parts.GetErrorCodes()
        {
            List<string> errors = null;
            if (WORK_DIR == CHECK_RESULT.NG)
            {
                errors = new List<string>();
                errors.Add(errorCodes[0]);
            }
            if (WORK_OK == CHECK_RESULT.NG)
            {
                if (errors == null)
                    errors = new List<string>();
                errors.Add(errorCodes[1]);
            }
            return errors;
        }
    }
}