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

namespace PLC_Connection
{
    public static class Parameters
    {
        public static readonly string[] TIME_COLUMNAMES =
            { "Carry_in", "Position", "Shoot_S", "Shoot_E",
               "Ready", "Arm1", "Stand", "Arm2", "Installation","Carry_out"
            };

        public static class Bit_X
        {
            public const int IN_CONVARE_FIRST = 0b0000000000010000;
            public const int PT_POSITION = 0b0000000000100000;
            public const int CONVARE_END = 0b0000000001000000;
            public const int OUT_CONVARE_FIRST = 0b0000000010000000;
            public const int OUT_CONVARE_END = 0b0000001000000000;
            public const int STAND_POS = 0b0000010000000000;


            public static readonly int[] bits = {IN_CONVARE_FIRST, PT_POSITION, CONVARE_END,
                                                   OUT_CONVARE_FIRST, OUT_CONVARE_END, STAND_POS };


            public static bool getProgressNum(int bits,bool on,ref int progressNum)
            {
                switch (bits)
                {
                    case IN_CONVARE_FIRST: if (on) { progressNum = 0; return true; } return false;
                    case PT_POSITION: if (on) { progressNum = 1; return true; } return false;
                    case CONVARE_END: if (on) progressNum = 4; else progressNum = 5; return true;
                    case STAND_POS: if (on) progressNum = 6; else progressNum = 7; return true;
                    case OUT_CONVARE_FIRST: if (on) { progressNum = 8; return true; } return false;
                    case OUT_CONVARE_END: if (on) { progressNum = 9; return true; } return false;
                    default: return false;
                }
            }

            public static string GetColumName(short bit)
            {
                int index = Array.IndexOf(bits, bit);

                if(index < 0)
                {
                    return null;
                }
                switch (index)
                {
                    case 0:
                        return Parameters.TIME_COLUMNAMES[1];
                        default : return null;

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

        public static class Bits_M
        {
            public const int RFID_READ = 0b0001000000000000;
        }
    }
}