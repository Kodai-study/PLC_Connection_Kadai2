using ResultDatas;

namespace PLC_Connection
{
    /// <summary>
    ///  いろいろな固定値を保存しておく、
    ///  静的なクラス
    /// </summary>
    public static class Parameters
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

        /// <summary>
        ///  ロボットの状態や工程の変更に関係のあるビットブロックを管理する。
        /// </summary>
        public abstract class Bitdata_Process
        {
            /// <summary>
            ///  処理に関係するビットだけを抜き出すためのマスク。
            ///  1になっているビットだけ監視して、他のビットはマスクしてしまう
            /// </summary>
            public abstract int MASK { get; }

            /// <summary>
            ///  変化したビットが、工程の進み具合と関係があるかを判別して、
            ///  工程に変化がある場合はその工程番号を返す
            /// </summary>
            /// <param name="bits"> 変わっていた部分のビットが1になっている値。立っているビットは1つのみ </param>
            /// <param name="changeTo1"> 変化が0→1:true  1→0:false </param>
            /// <param name="progressNum"> ビットの変化が表す工程番号 </param>
            /// <returns> bool ビットの変更が、工程の進み具合を表しているかどうか </returns>
            public abstract bool getProgressNum(int bits, bool changeTo1, ref Process_Number progressNum);
        }

        /// <summary>
        ///  X000からとってきたPLCのデータについて管理するクラス
        /// </summary>
        public class Bit_X : Bitdata_Process
        {
            /// <summary> 搬入コンベアの入口のセンサ部分 </summary>
            public const int IN_CONVARE_FIRST = 0b0000000000010000;
            /// <summary> 位置決め部のセンサ </summary>
            public const int PT_POSITION = 0b0000000000100000;
            /// <summary> 搬入コンベアの終端センサ </summary>
            public const int CONVARE_END = 0b0000000001000000;
            /// <summary> 搬出コンベアの入口のセンサ部分 </summary>
            public const int OUT_CONVARE_FIRST = 0b0000000010000000;
            /// <summary> 搬入コンベアの出口のセンサ部分 </summary>
            public const int OUT_CONVARE_END = 0b0000001000000000;
            /// <summary> 持ち帰部分のセンサ値 </summary>
            public const int STAND_POS = 0b0000010000000000;

            public static readonly int[] bits = {IN_CONVARE_FIRST, PT_POSITION, CONVARE_END,
                                                   OUT_CONVARE_FIRST, OUT_CONVARE_END, STAND_POS };


            public override bool getProgressNum(int bits, bool changeTo1, ref Process_Number progressNum)
            {
                switch (bits)
                {
                    case IN_CONVARE_FIRST:
                        if (changeTo1) { progressNum = Process_Number.Carry_in; return true; }
                        return false;
                    case PT_POSITION:
                        if (changeTo1) { progressNum = Process_Number.Position; return true; }
                        return false;
                    case CONVARE_END:
                        if (changeTo1) progressNum = Process_Number.Ready; else progressNum = Process_Number.Arm1; return true;
                    case STAND_POS:
                        if (changeTo1) progressNum = Process_Number.Stand; else progressNum = Process_Number.Arm2; return true;
                    case OUT_CONVARE_FIRST:
                        if (changeTo1) { progressNum = Process_Number.Installation; return true; }
                        return false;
                    case OUT_CONVARE_END:
                        if (changeTo1) { progressNum = Process_Number.Carry_out; return true; }
                        return false;
                    default: return false;
                }
            }


            public override int MASK
            {
                get
                {
                    return
                    IN_CONVARE_FIRST | PT_POSITION | CONVARE_END | OUT_CONVARE_FIRST |
                    OUT_CONVARE_END | STAND_POS;
                }
            }
        }

        /// <summary>
        ///  
        /// </summary>
        public static class Bits_M
        {
            public const int RFID_READ = 0b0001000000000000;
        }



        /// <summary>
        ///   X400～X40Fまでのビットブロックの管理を行う
        /// </summary>
        public class BITs_X40 : Bitdata_Process
        {
            /// <summary> 撮影、画像処理が終了した時に1になるビット </summary>
            public const int END_SHOOT = 0b0000000000000001;

            /// <summary> 撮影が開始された時に1になるビット </summary>
            public const int START_SHOOT = 0b0000000000000010;

            public override int MASK
            {
                get { return END_SHOOT | START_SHOOT; }
            }

            public override bool getProgressNum(int bits, bool changeTo1, ref Process_Number progressNum)
            {
                if (!changeTo1) return false;

                switch (bits)
                {
                    case START_SHOOT: progressNum = Process_Number.Shoot_Start; break;
                    case END_SHOOT: progressNum = Process_Number.Shoot_End; break;
                    default: return false;
                }
                return true;
            }
        }

       




    }
}