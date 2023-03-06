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
            "Supply", "Visal_in", "Functional_in", "Assembly_in",
               "Assembly_end"
        };

        /// <summary>
        ///  工程の番号、種類を管理する。工程を表すコラムの文字列である
        ///  TIME_COLUMNAMES の添え字と対応する。
        /// </summary>
        public enum Process_Number
        {
            NO_CHECK = -1,
            Supply,
            VisualStation_in,
            FunctionStation_in,
            Assembly_in,
            Assembly_end
        }

        /// <summary>
        ///  工程数を返すゲッタ
        /// </summary>
        public static int StepNumbers { get { return TIME_COLUMNAMES.Length; } }

    }

    public enum MEMORY_SPACE
    {
        NUMBER_OF_WORK_VISUAL_STATION,
        NUMBER_OF_WORK_FUNCTIONAL_STATION,
        NUMBER_OF_WORK_ASSEMBLY_STATION,
        NUMBER_OF_OKSTOCK,
        NUMBER_OF_NGSTOCK,
        IS_SYSTEM_PAUSE,
        IS_VISUAL_INSPECTED_JUST_BEFORE,
        IS_FUNCTION_INSPECTED_JUST_BEFORE,
        RESULT_VISUAL_INSPECTION,
        RESULT_FREQUENCY,
        RESULT_VOLTAGE,
        STATE_OF_OVERALL_SYSTEM,
        STATE_OF_SUPPLY_ROBOT,
        STATE_OF_VISUAL_STATION,
        STATE_OF_FUNCTION_STATION,
        STATE_OF_ASSEMBLY_STATION,
        NUMNER_OF_STATE_KIND
    }

}