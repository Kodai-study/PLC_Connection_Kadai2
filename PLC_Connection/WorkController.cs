using PLC_Connection.Modules;
using System;
using System.Collections.Generic;

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
        private readonly Queue<WorkData> insideWorks = null;

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
        public void WriteProcesChangeData(CommonParameters.Process_Number progressNum, DateTime nowTime)
        {
            WorkData targetWork = null;
            /* 搬出工程が行われたら、管理キューから1つ破棄する */
            if (progressNum == CommonParameters.Process_Number.Assembly_end)
            {
                RemoveWork(nowTime);
            }
            else
            {
                /* その工程がまだ行われていない、最も古いワークに変更を加える */
                foreach (var e in insideWorks)
                {
                    if (progressNum > e.progressNum)
                    {
                        e.progressNum = progressNum;
                        targetWork = e;
                        break;
                    }
                }
            }
            DatabaseController.ExecSQL(String.Format("UPDATE SensorTimeT SET {0} = '{1}.{2:D3}' WHERE No = {3}",
                    CommonParameters.TIME_COLUMNAMES[(int)progressNum], nowTime, nowTime.Millisecond, targetWork.WorkID));
        }

        /// <summary>
        /// 　新しいワークが搬入された時に管理対象に追加する
        /// </summary>
        /// <param name="startTime">　搬入された時刻　</param>
        public void AddnewWork(DateTime startTime)
        {
            insideWorks.Enqueue(new WorkData(startTime, CommonParameters.Process_Number.Supply));
            DatabaseController.ExecSQL(String.Format("INSERT INTO SensorTimeT (Supply) VALUES ({0}.{1:D3}))",
                startTime, startTime.Millisecond));
        }

        public void RemoveWork(DateTime changedTime)
        {
            WorkData targetWork = insideWorks.Dequeue();
            DatabaseController.ExecSQL(String.Format("UPDATE SensorTimeT SET Assembly_end = '{0}.{1:D3}' WHERE No = {2}",
                changedTime, changedTime.Millisecond, targetWork.WorkID));

        }

        public WorkData GetVisualCheckedWork()
        {
            foreach (var e in insideWorks)
            {
                if (!e.IsVisualInspected && e.progressNum >= CommonParameters.Process_Number.VisualStation_in)
                {
                    return e;
                }
            }
            return null;
        }

        public WorkData GetFunctionCheckedWork()
        {
            foreach (var e in insideWorks)
            {
                if (!e.IsFunctionInspected && e.progressNum >= CommonParameters.Process_Number.FunctionStation_in)
                {
                    return e;
                }
            }
            return null;
        }

    }
}