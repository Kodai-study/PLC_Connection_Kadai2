using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MITSUBISHI.Component;
using System.Data.SqlClient;
using ActUtlTypeLib;
using System.Reflection.Emit;
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
            { "Carry_in", "Position", "Shoot_S", "Shoot_E",
               "Ready", "Arm1", "Stand", "Arm2", "Installation","Carry_out"
            };

        /// <summary>
        ///  工程数を返すゲッタ
        /// </summary>
        public static int StepNumbers { get { return TIME_COLUMNAMES.Length; } }

        /// <summary>
        ///  X000からとってきたPLCのデータについて管理するクラス
        /// </summary>
        public static class Bit_X
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

            /// <summary>
            ///  X000～X00Fまでの変更が、工程の進み具合と関係があるかどうかを判別して、
            ///  ある場合はその工程番号を返す
            /// </summary>
            /// <param name="bits"> 変わっていた部分のビットが1になっている値。立っているビットは1つのみ </param>
            /// <param name="on"> 変化が0→1:true  1→0:false </param>
            /// <param name="progressNum"> ビットの変化が表す工程番号 </param>
            /// <returns> bool ビットの変更が、工程の進み具合を表しているかどうか </returns>
            public static bool getProgressNum(int bits, bool on, ref int progressNum)
            {
                switch (bits)
                {
                    case IN_CONVARE_FIRST:
                        if (on) { progressNum = 0; return true; } return false;
                    case PT_POSITION:
                        if (on) { progressNum = 1; return true; } return false;
                    case CONVARE_END:
                        if (on) progressNum = 4; else progressNum = 5; return true;
                    case STAND_POS:
                        if (on) progressNum = 6; else progressNum = 7; return true;
                    case OUT_CONVARE_FIRST:
                        if (on) { progressNum = 8; return true; } return false;
                    case OUT_CONVARE_END:
                        if (on) { progressNum = 9; return true; } return false;
                    default: return false;
                }
            }

            public static string GetColumName(short bit)
            {
                int index = Array.IndexOf(bits, bit);

                if (index < 0)
                {
                    return null;
                }
                switch (index)
                {
                    case 0:
                        return Parameters.TIME_COLUMNAMES[1];
                    default: return null;
                }
            }

            /// <summary>
            ///  変化を読み取るビットを限定させるマスク。
            ///  ブロックのうちこれで指定されたビットが変化したら反応する
            /// </summary>
            public static short MASK
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
        ///  PLCから読み取れる、いろいろなブロックのデータを処理するクラス
        /// </summary>
        /// <remarks>
        ///  PLCからのデータの読み込みは、16ビット単位で行う。
        ///  それらのデータの区切りと意味の区切りは一緒でないため、
        ///  いろいろな変換を行う必要がある。
        /// </remarks>
        public abstract class BITS_STATUS
        {
            /// <summary>
            ///  読み取ったブロック
            /// </summary>
            protected abstract int MASK { get; }
            /// <summary>
            ///  読み取れたビットブロックから、検査結果を読み取る。
            /// </summary>
            /// <param name="checkResults"> 検査結果を格納するクラスのインスタンス。中身が書き直され、結果が格納される </param>
            /// <param name="bitData"> 読み込んだ16ビットのブロック </param>
            public abstract void checkResults(ref Results checkResults, int bitData);
        }

        /// <summary>
        ///   検査結果や
        /// </summary>
        /// TODO X40台の値の分布を決めて、検査結果のブロックか、工程管理のブロックかにする
        public class BITs_X40
        {
            /// <summary> 撮影終了したときのあれ </summary>
            public const int END_SHOOT = 0b0000000000000001;
        }

        public class BITS_X41 : BITS_STATUS
        {
            /// <summary> ワークが正常だった時の </summary>
            public const int WORK_OK = 0b0000000000000010;
            /// <summary> ワークの向きが正常だった時に立つビット </summary>
            public const int WORK_DIR_OK = 0b0000000000000100;
            /// <summary> ワークの向きが逆だった時に立つビット </summary>
            public const int WORK_DIR_NG = 0b0000000000001000;
            /// <summary> ICがあったときに立つビット </summary>
            public const int IC1_OK = 0b0000100000000000;
            /// <summary> ICの向きが正常だった時に立つビット </summary>
            public const int IC1_DIR = 0b0001000000000000;

            //TODO MASKの実装
            protected override int MASK
            {
                get
                {
                    return WORK_OK | WORK_DIR_OK | WORK_DIR_NG | IC1_OK | IC1_DIR;
                }
            }

            public override void checkResults(ref Results checkResults, int bitData)
            {
                if ((bitData & WORK_OK) != 0)
                    checkResults.work.WORK_OK = CHECK_RESULT.OK;
                else
                    checkResults.work.WORK_OK = CHECK_RESULT.NG;

                if ((bitData & WORK_DIR_OK) != 0)
                    checkResults.work.WORK_DIR = CHECK_RESULT.NG;
                else if ((bitData & WORK_DIR_NG) != 0)
                    checkResults.work.WORK_DIR = CHECK_RESULT.NG;
                else
                    checkResults.work.WORK_DIR = CHECK_RESULT.NO_CHECK;

                if ((bitData & IC1_OK) != 0)
                    checkResults.ic.IC1_OK = CHECK_RESULT.OK;
                else
                    checkResults.ic.IC1_OK = CHECK_RESULT.NG;

                if ((bitData & IC1_DIR) != 0)
                    checkResults.ic.IC1_DIR = CHECK_RESULT.OK;
                else
                    checkResults.ic.IC1_DIR = CHECK_RESULT.NG;
            }
        }
        public class BITS_X42 : BITS_STATUS
        {
            /// <summary> ICが正常だったら立つビット </summary>
            public const int IC2_OK = 0b0000000000100000;
            /// <summary> ICの向きが正常だった時に立つビット </summary>
            public const int IC2_DIR = 0b0000000001000000;
            /// <summary> トランジスタが正常だった時に立つビット </summary>
            public const int TR_OK = 0b1000000000000000;

            protected override int MASK
            {
                get { return IC2_OK | IC2_DIR | TR_OK; }
            }

            public override void checkResults(ref Results checkResults, int bitData)
            {
                if ((bitData & IC2_OK) != 0)
                    checkResults.ic.IC2_OK = CHECK_RESULT.OK;
                else
                    checkResults.ic.IC2_OK = CHECK_RESULT.NG;

                if ((bitData & IC2_DIR) != 0)
                    checkResults.ic.IC2_DIR = CHECK_RESULT.OK;
                else
                    checkResults.ic.IC2_DIR = CHECK_RESULT.NG;


                if ((bitData & TR_OK) != 0)
                    checkResults.transister.TR_OK = CHECK_RESULT.OK;
                else
                    checkResults.transister.TR_OK = CHECK_RESULT.NG;
            }
        }
        public class BITS_X43 : BITS_STATUS
        {
            /// <summary> DIPスイッチの4つ目の状態を表すビット。1だとONになっている </summary>
            public const int DIP_SW4 = 0b0010000000000000;
            public const int DIP_SW3 = 0b0001000000000000;
            public const int DIP_SW2 = 0b0000100000000000;
            public const int DIP_SW1 = 0b0000010000000000;
            /// <summary> DIPスイッチの値が合格だった時に立つビット </summary>
            public const int DIP_OK = 0b0000001000000000;

            protected override int MASK
            {
                get
                {
                    return DIP_SW4 | DIP_SW3 | DIP_SW2 | DIP_SW1 | DIP_OK;
                }
            }

            public override void checkResults(ref Results checkResults, int bitData)
            {
                if ((bitData & DIP_OK) != 0)
                    checkResults.dipSwitch.DIP_OK = CHECK_RESULT.OK;
                else
                    checkResults.dipSwitch.DIP_OK = CHECK_RESULT.NG;

                int pattern = 0;
                if ((bitData & DIP_SW1) != 0)
                    pattern |= 1;
                if ((bitData & DIP_SW2) != 0)
                    pattern |= 2;
                if ((bitData & DIP_SW3) != 0)
                    pattern |= 4;
                if ((bitData & DIP_SW4) != 0)
                    pattern |= 8;

                checkResults.dipSwitch.DIP_PATTERN = pattern;
            }
        }
        public class BITS_X44 : BITS_STATUS
        {
            /// <summary> 電池ソケットの向きが正しいかどうか </summary>
            public const int SOCKET_DIR = 0b0000000000001000;
            /// <summary> ダイオードの向きが正しかった時に立つビット </summary>
            public const int DIODE_DIR = 0b0010000000000000;

            protected override int MASK
            {
                get
                {
                    return SOCKET_DIR | DIODE_DIR;
                }
            }

            public override void checkResults(ref Results checkResults, int bitData)
            {
                if ((bitData & SOCKET_DIR) != 0)
                    checkResults.batterySocket.SOCKET_DIR = CHECK_RESULT.OK;
                else
                    checkResults.batterySocket.SOCKET_DIR = CHECK_RESULT.NG;

                if((bitData & DIODE_DIR) != 0)
                    checkResults.diode.DIODE_DIR = CHECK_RESULT.OK;
                else
                    checkResults.diode.DIODE_DIR = CHECK_RESULT.NG;
            }
        }
    }
}