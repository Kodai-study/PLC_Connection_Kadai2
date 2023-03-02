using System.Collections.Generic;
using System.Text;

namespace PLC_Connection.InspectionResultDataModel
{
    /// <summary>
    ///  ワーク自体の検査結果をまとめるクラス
    /// </summary>  
    /// <remarks> 詳しい中身などは同じインタフェースを実装しているICクラスを参照 </remarks>
    /// <seealso cref="IC"/>
    public class RESISTOR : Parts
    {
        private readonly string[] errorCodes = new string[]
        {
            "R05",
            "R10",
            "R11",
            "R12",
            "R18"
        };
        public RESISTOR() { }
        public RESISTOR(CHECK_RESULT resigterAllResult)
        {
            this.result_R11 = resigterAllResult;
            if (resigterAllResult == CHECK_RESULT.OK)
            {
                result_R05 = CHECK_RESULT.OK;
                result_R10 = CHECK_RESULT.OK;
                result_R11 = CHECK_RESULT.OK;
                result_R12 = CHECK_RESULT.OK;
                result_R18 = CHECK_RESULT.OK;
            }
        }

        public RESISTOR(CHECK_RESULT result_R05, CHECK_RESULT result_R10,
            CHECK_RESULT result_R11, CHECK_RESULT result_R12, CHECK_RESULT result_R18)
        {
            resistorAllResult = CHECK_RESULT.NG;
            this.result_R05 = result_R05;
            this.result_R10 = result_R10;
            this.result_R11 = result_R11;
            this.result_R12 = result_R12;
            this.result_R18 = result_R18;
            if (result_R05 == CHECK_RESULT.OK && result_R10 == CHECK_RESULT.OK &&
               result_R11 == CHECK_RESULT.OK && result_R12 == CHECK_RESULT.OK &&
               result_R18 == CHECK_RESULT.OK)
                this.resistorAllResult = CHECK_RESULT.OK;
        }


        public CHECK_RESULT resistorAllResult = CHECK_RESULT.NO_CHECK;
        public CHECK_RESULT result_R05 = CHECK_RESULT.NO_CHECK;
        public CHECK_RESULT result_R10 = CHECK_RESULT.NO_CHECK;
        public CHECK_RESULT result_R11 = CHECK_RESULT.NO_CHECK;
        public CHECK_RESULT result_R12 = CHECK_RESULT.NO_CHECK;
        public CHECK_RESULT result_R18 = CHECK_RESULT.NO_CHECK;

        string Parts.GetResultString()
        {
            StringBuilder messageStr = new StringBuilder();
            messageStr.Append("抵抗 : ");
            messageStr.Append("\n\t抵抗全てが正しく認識されたか ");
            messageStr.Append(resistorAllResult == CHECK_RESULT.OK ? "〇" : "×");
            messageStr.Append("\n\tR05が正しく認識されたか ");
            messageStr.Append(result_R05 == CHECK_RESULT.OK ? "〇" : "×");
            messageStr.Append("\n\tR10が正しく認識されたか ");
            messageStr.Append(result_R10 == CHECK_RESULT.OK ? "〇" : "×");
            messageStr.Append("\n\tR11が正しく認識されたか ");
            messageStr.Append(result_R11 == CHECK_RESULT.OK ? "〇" : "×");
            messageStr.Append("\n\tR12が正しく認識されたか ");
            messageStr.Append(result_R12 == CHECK_RESULT.OK ? "〇" : "×");
            messageStr.Append("\n\tR18が正しく認識されたか ");
            messageStr.Append(result_R18 == CHECK_RESULT.OK ? "〇" : "×");
            return messageStr.ToString();
        }

        List<string> Parts.GetErrorCodes()
        {
            if (resistorAllResult != CHECK_RESULT.OK)
                return null;

            List<string> errors = new List<string>();
            if (result_R05 == CHECK_RESULT.NG)
                errors.Add(errorCodes[0]);
            if (result_R10 == CHECK_RESULT.NG)
                errors.Add(errorCodes[1]);
            if (result_R11 == CHECK_RESULT.NG)
                errors.Add(errorCodes[2]);
            if (result_R12 == CHECK_RESULT.NG)
                errors.Add(errorCodes[3]);
            if (result_R18 == CHECK_RESULT.NG)
                errors.Add(errorCodes[4]);

            return errors;
        }
    }
}