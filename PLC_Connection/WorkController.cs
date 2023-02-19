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
    class WorkController
    {
        /// <summary>
        ///  ステーション内にあるワークの一覧を管理
        ///  (検査終了、搬出でキューから無くなる)
        /// </summary>
        private Queue<WorkData> insideWorks = null;

        private WorkData lastCheckWork = null;

        public WorkData CheckedWork
        {
            get { return lastCheckWork; }
        }

        public WorkController()
        {
            insideWorks = new Queue<WorkData>();
        }

        /// <summary>
        ///  行われた変更から、ワーク情報を更新するSQL文を作成
        /// </summary>
        /// <param name="progressNum"> 工程番号。Parameterクラスからとってくる </param>
        /// <see cref="Parameters.Bit_X.getProgressNum(int, bool, ref int)"/>
        /// <param name="nowTime"> センサが反応した時刻 </param>
        /// <returns> 作成したUPDATE文 </returns>
        /// TODO エラー時の返り値等を決めておく

        public string ProcessToSql(Parameters.Process_Number progressNum, TimeSpan nowTime)
        {
            //TODO プロセス番号も列挙型で管理するようにする
            DateTime? startTime = null;
            /* 搬出工程が行われたら、管理キューから1つ破棄する */
            if (progressNum == Parameters.Process_Number.Carry_out)
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
                        if (progressNum == Parameters.Process_Number.Shoot_End)
                        {
                            lastCheckWork = e;
                        }
                        break;
                    }
                    return null;
                }
            }
            return String.Format("UPDATE Test_CycleTime SET {0} = '{1}' WHERE Carry_in BETWEEN '{2}' AND '{3}'",
                    Parameters.TIME_COLUMNAMES[(int)progressNum], nowTime, startTime, startTime + TimeSpan.FromSeconds(1));
        }

        /// <summary>
        /// 　新しいワークが搬入された時に管理対象に追加する
        /// </summary>
        /// <param name="startTime">　搬入された時刻　</param>
        public string AddnewWork(DateTime startTime)
        {
            insideWorks.Enqueue(new WorkData(startTime, 0));
            return String.Format("INSERT INTO Test_CycleTime ({2}) VALUES ('{0}.{1:D3}')",
                startTime, startTime.Millisecond, Parameters.TIME_COLUMNAMES[0]);
        }

        /// <summary>
        ///  検査データを表すクラス
        /// </summary>
        public class WorkData
        {
            /// <summary>
            ///  ワークが最後に行った工程の番号
            /// </summary>
            public Parameters.Process_Number progressNum;
            /// <summary>
            ///  ワークが搬入されてきた時間。これでテーブル操作の時にワークを識別する。
            /// </summary>
            public DateTime startTime;

            private int? cycle_code = null;

            private int? id = null;


            public WorkData(DateTime startTime, Parameters.Process_Number progressNum)
            {
                this.progressNum = progressNum;
                this.startTime = startTime;
            }

            public int? CycleCode
            {
                //TODO startTimeからCycle_codeをとってくるSQL文を実行
                get
                {
                    if (cycle_code != null)
                        return cycle_code;

                    string getCycleCodeSql = String.Format
                        ("SELECT cycle_code FROM Test_CycleTime WHERE Carry_in BETWEEN '{0}' and '{1}'",
                        this.startTime, startTime + TimeSpan.FromSeconds(1));

                    if (DatabaseController.GetOneParameter<int?>(getCycleCodeSql, ref cycle_code))
                    {
                        return cycle_code;
                    }
                    else
                    {
                        cycle_code = null;
                        return cycle_code;
                    }
                }
            }

            public int? WorkID
            {
                get
                {
                    if (id != null)
                        return id;

                    if (cycle_code == null)
                    {
                        cycle_code = this.CycleCode;
                    }
                    string getWorkIDSql = String.Format
                        ("SELECT ID FROM Test_Data WHERE Cycle_Code = {0}", cycle_code);

                    if (DatabaseController.GetOneParameter<int?>(getWorkIDSql, ref id))
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
}
