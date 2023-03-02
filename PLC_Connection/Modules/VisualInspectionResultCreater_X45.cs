using PLC_Connection.InspectionResultDataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLC_Connection.Modules
{

    /// <summary>
    ///   X440～X44Fまでのビットブロックの管理を行う
    /// </summary>
    public class VisualInspectionResultCreater_X45 : ResultDataCreater
    {
        /// <summary> 電池ソケットの向きが正しいかどうか </summary>
        public const int REGESTER_ALL = 0b0000000010000000;
        /// <summary> ダイオードの向きが正しかった時に立つビット </summary>
        public const int REGESTER_5 = 0b0000000100000000;
        public const int REGESTER_10 = 0b0000001000000000;
        public const int REGESTER_11 = 0b0000010000000000;
        public const int REGESTER_12 = 0b0000100000000000;
        public const int REGESTER_18 = 0b0001000000000000;

        public override int MASK
        {
            get
            {
                return REGESTER_ALL | REGESTER_5 | REGESTER_10 |
                REGESTER_11 | REGESTER_12 | REGESTER_18;
            }
        }

        public override void CheckResult(ref Results checkResults, int bitData)
        {
            if ((bitData & REGESTER_ALL) != 0)
                checkResults.resistor.resistorAllResult = CHECK_RESULT.OK;
            else
                checkResults.batterySocket.SOCKET_DIR = CHECK_RESULT.NG;

            if ((bitData & REGESTER_5) != 0)
                checkResults.resistor.result_R05 = CHECK_RESULT.OK;
            else
                checkResults.resistor.result_R05 = CHECK_RESULT.NG;

            if ((bitData & REGESTER_10) != 0)
                checkResults.resistor.result_R10 = CHECK_RESULT.OK;
            else
                checkResults.resistor.result_R10 = CHECK_RESULT.NG;

            if ((bitData & REGESTER_11) != 0)
                checkResults.resistor.result_R11 = CHECK_RESULT.OK;
            else
                checkResults.resistor.result_R11 = CHECK_RESULT.NG;

            if ((bitData & REGESTER_12) != 0)
                checkResults.resistor.result_R12 = CHECK_RESULT.OK;
            else
                checkResults.resistor.result_R12 = CHECK_RESULT.NG;

            if ((bitData & REGESTER_18) != 0)
                checkResults.resistor.result_R18 = CHECK_RESULT.OK;
            else
                checkResults.resistor.result_R18 = CHECK_RESULT.NG;
        }
    }
}