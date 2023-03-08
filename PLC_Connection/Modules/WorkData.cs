using PLC_Connection.InspectionResultDataModel;
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
    ///  検査データを表すクラス
    /// </summary>
    public class WorkData
    {
        /// <summary>
        ///  ワークが最後に行った工程の番号
        /// </summary>
        public CommonParameters.Process_Number progressNum;
        /// <summary>
        ///  ワークが搬入されてきた時間。これでテーブル操作の時にワークを識別する。
        /// </summary>
        public DateTime startTime;

        private int? cycle_code = null;

        private int? id = null;

        public bool IsVisualInspected { get; set; } = false;
        public bool IsFunctionInspected { get; set; } = false;


        public WorkData(DateTime startTime, CommonParameters.Process_Number progressNum)
        {
            this.progressNum = progressNum;
            this.startTime = startTime.AddTicks((startTime.Ticks % TimeSpan.TicksPerSecond) * -1); ;
        }



        public int? WorkID
        {
            get
            {
                if (id != null)
                    return id;

                string getWorkIDSql = String.Format
                    ("SELECT No FROM SensorTimeT WHERE Supply BETWEEN '{0}' AND '{1}'",
                    startTime, startTime + TimeSpan.FromSeconds(1));

                if (DatabaseController.GetOneParameter(getWorkIDSql, ref id))
                    return id;
                else
                {
                    id = null;
                    return null;
                }
            }
        }
    }
}