using System.Collections.Generic;
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

        /* 共通の処理を使うためインタフェースでインスタンスを持っておく */
        private Parts parts_ic;
        private Parts parts_work;
        private Parts parts_dipSW;
        private Parts parts_tr;
        private Parts parts_battery;
        private Parts parts_diode;

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
            if((buf = parts_work.GetErrorCodes()) != null)
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
            }
            return errors;
        }
    }

    /// <summary>
    ///  検査パーツごとに共通の処理をまとめたインターフェース
    /// </summary>
    interface Parts
    {
        /// <summary>
        ///  デバッグ用に表示する検査結果の文字列を作成する
        /// </summary>
        /// <returns> 結果を表す文字列。(部品名) : 結果 (○|×)の形 </returns>
        string GetResultString();

        /// <summary>
        ///  パーツごとに、エラー項目があるとそのエラーコードのリストを作成して返す。
        /// </summary>
        /// <returns> エラーコードのリスト。エラー項目がない場合はnull </returns>
        List<string> GetErrorCodes();
        //TODO 結果を入出力するアクセサの実装
        //CHECK_RESULT this[int x] { get;set; }
    }

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
            "IC101",
            "IC102",
            "IC103",
            "IC201",
            "IC202",
            "IC203"
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
                errors = new List<string>();
                errors.Add(errorCodes[0]);
            }
            if (IC1_OK == CHECK_RESULT.NG)
            {
                if (errors == null)         //エラー項目が1つもない場合、リストのインスタンスを作成
                    errors = new List<string>();
                errors.Add(errorCodes[1]);
            }
            if(IC2_DIR == CHECK_RESULT.NG){
                if (errors == null)
                    errors = new List<string>();
                errors.Add(errorCodes[3]);
            }
            if (IC2_OK == CHECK_RESULT.NG)
            {
                if (errors == null)
                    errors = new List<string>();
                errors.Add(errorCodes[4]);
            }
            return errors;
        }
    }


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