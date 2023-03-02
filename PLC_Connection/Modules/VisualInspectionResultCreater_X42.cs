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
    /// <summary>
    ///   X420～X42Fまでのビットブロックの管理を行う
    /// </summary>
    public class VisualInspectionResultCreater_X42 : ResultDataCreater
    {
        /// <summary> ICが正常だったら立つビット </summary>
        public const int IC2_OK = 0b0000000000100000;
        /// <summary> ICの向きが正常だった時に立つビット </summary>
        public const int IC2_DIR = 0b0000000001000000;
        /// <summary> トランジスタが正常だった時に立つビット </summary>
        public const int TR_OK = 0b1000000000000000;

        public override int MASK
        {
            get { return IC2_OK | IC2_DIR | TR_OK; }
        }

        public override void CheckResult(ref Results checkResults, int bitData)
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
}
