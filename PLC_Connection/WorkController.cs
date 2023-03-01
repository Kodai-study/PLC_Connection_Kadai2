using PLC_Connection.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLC_Connection
{
    /// <summary>
    ///  検査ステーションに入ってきたワークの管理を行う
    /// </summary>
    public class WorkController
    {
        /// <summary>
        ///  ステーション内にあるワークの一覧を管理
        ///  (検査終了、搬出でキューから無くなる)
        /// </summary>
        private Queue<WorkData> insideWorks = null;


        public WorkController()
        {
            insideWorks = new Queue<WorkData>();
        }

        /// <summary>
        ///  行われた変更から、ワーク情報を更新するSQL文を作成
        /// </summary>
        /// <param name="progressNum"> 工程番号。Parameterクラスからとってくる </param>
        /// <see cref="CommonParameters.Bit_X.getProgressNum(int, bool, ref int)"/>
        /// <param name="nowTime"> センサが反応した時刻 </param>
        /// <returns> 作成したUPDATE文 </returns>
        /// TODO エラー時の返り値等を決めておく

        public void ProcessToSql(CommonParameters.Process_Number progressNum, TimeSpan nowTime)
        {
            //TODO プロセス番号も列挙型で管理するようにする
            DateTime? startTime = null;
            /* 搬出工程が行われたら、管理キューから1つ破棄する */
            if (progressNum == CommonParameters.Process_Number.Finish)
            {
                startTime = insideWorks.Dequeue().startTime;
            }
            else
            {
                /* その工程がまだ行われていない、最も古いワークに変更を加える */
                foreach (var e in insideWorks)
                {
                    if (progressNum > e.progressNum)
                    {
                        e.progressNum = progressNum;
                        startTime = e.startTime;
                        break;
                    }
                }
            }
            DatabaseController.ExecSQL(String.Format("UPDATE Test_CycleTime SET {0} = '{1}' WHERE Carry_in BETWEEN '{2}' AND '{3}'",
                    CommonParameters.TIME_COLUMNAMES[(int)progressNum], nowTime, startTime, startTime + TimeSpan.FromSeconds(1)));
        }

        /// <summary>
        /// 　新しいワークが搬入された時に管理対象に追加する
        /// </summary>
        /// <param name="startTime">　搬入された時刻　</param>
        public void AddnewWork(DateTime startTime)
        {
            insideWorks.Enqueue(new WorkData(startTime, CommonParameters.Process_Number.Supply));
            DatabaseController.ExecSQL(String.Format("INSERT INTO Test_CycleTime ({2}) VALUES ('{0}.{1:D3}')",
                startTime, startTime.Millisecond, CommonParameters.TIME_COLUMNAMES[0]));
        }

    }
}