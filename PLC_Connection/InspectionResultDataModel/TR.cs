using System.Collections.Generic;
using System.Text;

namespace PLC_Connection.InspectionResultDataModel
{
    /// <summary>
    ///  トランジスタの検査結果をまとめるクラス
    /// </summary>  
    /// <remarks> 詳しい中身などは同じインタフェースを実装しているICクラスを参照 </remarks>
    /// <seealso cref="IC"/>
    public class TR : Parts
    {
        private readonly string[] errorCodes = new string[]
        {
            "TR001",
            "TR002"
        };
        public TR() { }
        public TR(CHECK_RESULT TR_OK)
        {
            this.TR_OK = TR_OK;
        }
        public CHECK_RESULT TR_OK = CHECK_RESULT.NO_CHECK;

        string Parts.GetResultString()
        {
            StringBuilder messageStr = new StringBuilder();
            messageStr.Append("トランジスタ : ");
            messageStr.Append("\n\tトランジスタが正しく実装されている ");
            messageStr.Append(TR_OK == CHECK_RESULT.OK ? "〇" : "×");
            return messageStr.ToString();
        }

        List<string> Parts.GetErrorCodes()
        {
            List<string> errors = null;
            if (TR_OK == CHECK_RESULT.NG)
            {
                errors = new List<string>();
                errors.Add(errorCodes[0]);
            }
            return errors;
        }
    }
}