using System.Collections.Generic;
using System.Text;

namespace PLC_Connection.InspectionResultDataModel
{
    /// <summary>
    ///  ICの検査結果をまとめたクラス
    ///  IC1,2のどちらもこのクラスで管理する
    /// </summary>
    public class IC : Parts
    {
        /// <summary>
        ///  エラーコードの一覧を格納した配列。
        ///  対応した文字列を取り出し、データベースに書き込む
        /// </summary>
        private readonly string[] errorCodes = new string[]
        {
            "IC11",
            "IC12",
            "IC21",
            "IC22"
        };

        /// <summary>
        ///  初期化値としてNO_CHECK 未検査を変数に格納するコンストラクタ
        /// </summary>
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

        /* 部品管理のインタフェース、Parts のメソッドの実装
         * 検査項目それぞれに対して、OK,NGを表示する。 */

        string Parts.GetResultString()
        {
            StringBuilder str = new StringBuilder("IC : ");
            str.Append("\n\tIC1_がある ");
            str.Append(IC1_OK == CHECK_RESULT.OK ? "〇" : "×");
            str.Append("\n\tIC1_の向き ");
            str.Append(IC1_DIR == CHECK_RESULT.OK ? "〇" : "×");
            str.Append("\n\tIC2_がある ");
            str.Append(IC2_OK == CHECK_RESULT.OK ? "〇" : "×");
            str.Append("\n\tIC2_の向き ");
            str.Append(IC2_DIR == CHECK_RESULT.OK ? "〇" : "×");
            return str.ToString();
        }

        /* インターフェースParts のメソッドの実装
         *エラー項目があったときにそのエラーコード文字列の
         *リストを作成して返す。全てOKの場合はNULLを返す */
        List<string> Parts.GetErrorCodes()
        {
            //返却するリスト変数。初期値NULLで、エラーがあるとインスタンス化され、リストにコードが格納される
            List<string> errors = null;
            if (IC1_DIR == CHECK_RESULT.NG)
            {
                errors = new List<string>
                {
                    errorCodes[0]
                };
            }
            if (IC1_OK == CHECK_RESULT.NG)
            {
                if (errors == null)         //エラー項目が1つもない場合、リストのインスタンスを作成
                    errors = new List<string>();
                errors.Add(errorCodes[1]);
            }
            if (IC2_DIR == CHECK_RESULT.NG)
            {
                if (errors == null)
                    errors = new List<string>();
                errors.Add(errorCodes[2]);
            }
            if (IC2_OK == CHECK_RESULT.NG)
            {
                if (errors == null)
                    errors = new List<string>();
                errors.Add(errorCodes[3]);
            }
            return errors;
        }
    }
}