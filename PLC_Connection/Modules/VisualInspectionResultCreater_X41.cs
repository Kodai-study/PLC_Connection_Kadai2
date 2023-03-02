using ResultDatas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLC_Connection.Modules
{
    /// <summary>
    ///   X410～X41Fまでのビットブロックの管理を行う
    /// </summary>
    public class VisualInspectionResultCreater_X41 : ResultDataCreater
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


        public override int MASK
        {
            get
            {
                return WORK_OK | WORK_DIR_OK | WORK_DIR_NG | IC1_OK | IC1_DIR;
            }
        }

        public override void CheckResult(ref Results checkResults, int bitData)
        {
            if ((bitData & WORK_OK) != 0)
                checkResults.work.WORK_OK = CHECK_RESULT.OK;
            else
                checkResults.work.WORK_OK = CHECK_RESULT.NG;

            if ((bitData & WORK_DIR_OK) != 0)
                checkResults.work.WORK_DIR = CHECK_RESULT.OK;
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
}
