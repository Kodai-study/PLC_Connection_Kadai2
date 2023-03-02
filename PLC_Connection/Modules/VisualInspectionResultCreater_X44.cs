using ResultDatas;
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
    public class VisualInspectionResultCreater_X44 : ResultDataCreater
    {
        /// <summary> 電池ソケットの向きが正しいかどうか </summary>
        public const int SOCKET_DIR = 0b0000000000001000;
        /// <summary> ダイオードの向きが正しかった時に立つビット </summary>
        public const int DIODE_DIR = 0b0010000000000000;

        public override int MASK
        {
            get
            {
                return SOCKET_DIR | DIODE_DIR;
            }
        }

        public override void CheckResult(ref Results checkResults, int bitData)
        {
            if ((bitData & SOCKET_DIR) != 0)
                checkResults.batterySocket.SOCKET_DIR = CHECK_RESULT.OK;
            else
                checkResults.batterySocket.SOCKET_DIR = CHECK_RESULT.NG;

            if ((bitData & DIODE_DIR) != 0)
                checkResults.diode.DIODE_DIR = CHECK_RESULT.OK;
            else
                checkResults.diode.DIODE_DIR = CHECK_RESULT.NG;
        }
    }
}
