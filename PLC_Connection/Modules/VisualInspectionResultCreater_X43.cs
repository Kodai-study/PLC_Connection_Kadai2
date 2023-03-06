using PLC_Connection.InspectionResultDataModel;

namespace PLC_Connection.Modules
{


    /// <summary>
    ///   X430～X43Fまでのビットブロックの管理を行う
    /// </summary>
    public class VisualInspectionResultCreater_X43 : ResultDataCreater
    {
        /// <summary> DIPスイッチの4つ目の状態を表すビット。1だとONになっている </summary>
        public const int DIP_SW4 = 0b0010000000000000;
        public const int DIP_SW3 = 0b0001000000000000;
        public const int DIP_SW2 = 0b0000100000000000;
        public const int DIP_SW1 = 0b0000010000000000;
        /// <summary> DIPスイッチの値が合格だった時に立つビット </summary>
        public const int DIP_OK = 0b0000001000000000;

        public override int MASK
        {
            get
            {
                return DIP_SW4 | DIP_SW3 | DIP_SW2 | DIP_SW1 | DIP_OK;
            }
        }

        public override void CheckResult(ref Results checkResults, int bitData)
        {
            if ((bitData & DIP_OK) != 0)
                checkResults.dipSwitch.DIP_OK = CHECK_RESULT.OK;
            else
                checkResults.dipSwitch.DIP_OK = CHECK_RESULT.NG;

            int pattern = 0;
            if ((bitData & DIP_SW1) == 0)
                pattern |= 1;
            if ((bitData & DIP_SW2) == 0)
                pattern |= 2;
            if ((bitData & DIP_SW3) == 0)
                pattern |= 4;
            if ((bitData & DIP_SW4) == 0)
                pattern |= 8;

            checkResults.dipSwitch.DIP_PATTERN = pattern;
        }
    }
}
