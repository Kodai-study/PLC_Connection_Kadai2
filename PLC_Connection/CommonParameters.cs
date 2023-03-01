using ResultDatas;

namespace PLC_Connection
{
    /// <summary>
    ///  いろいろな固定値を保存しておく、
    ///  静的なクラス
    /// </summary>
    public static class CommonParameters
    {
        /// <summary>
        ///  データベースのテーブルに保存されている、各工程の名前 = コラム名
        ///  の配列。 添え字は工程につけられた番号になる
        /// </summary>
        public static readonly string[] TIME_COLUMNAMES =
        {
            "Carry_in", "Position", "Shoot_S", "Shoot_E",
               "Ready", "Arm1", "Stand", "Arm2", "Installation","Carry_out"
        };

        /// <summary>
        ///  工程の番号、種類を管理する。工程を表すコラムの文字列である
        ///  TIME_COLUMNAMES の添え字と対応する。
        /// </summary>
        public enum Process_Number
        {
            NO_CHECK = -1,
            Carry_in,
            Position,
            Shoot_Start,
            Shoot_End,
            Ready,
            Arm1,
            Stand,
            Arm2,
            Installation,
            Carry_out
        }

        /// <summary>
        ///  工程数を返すゲッタ
        /// </summary>
        public static int StepNumbers { get { return TIME_COLUMNAMES.Length; } }

    }
}