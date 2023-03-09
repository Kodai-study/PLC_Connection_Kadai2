#define debug

using MITSUBISHI.Component;
using PLC_Connection.Modules;
using PLC_Connection.StationMonitor;
using System;
using System.IO.MemoryMappedFiles;
using System.Threading;
using System.Threading.Tasks;


namespace PLC_Connection
{

    public class PLC_MonitorTask
    {

        /// <summary>
        ///  チャタリング防止として、前のセンサ読み込みから
        ///  この時間がたつまでは読み込みを無視する。
        ///  1秒
        /// </summary>
        private readonly TimeSpan Chataring_time = new TimeSpan(0, 0, 1);

        /// <summary>
        ///  PLCのデータを読み込むに、コネクションを確立するオブジェクト
        /// </summary>
        public DotUtlType dotUtlType;

        /// <summary>
        /// 　PLCの読み取りタスクを終了させる命令を出すトークン
        /// </summary>
        private CancellationTokenSource cancellToken;

        /// <summary>
        ///  ステーション内のワークを管理するオブジェクト
        /// </summary>
        WorkController workController;

        PLCContactData plcData = new PLCContactData();
        readonly Base_StationMonitor visualStationMonitor;
        readonly Base_StationMonitor functionStationMonitor;
        MemoryMappedFile share_mem;
        MemoryMappedViewAccessor accessor;

        public PLC_MonitorTask()
        {
            share_mem = MemoryMappedFile.CreateNew("shared_memory", 4 * (int)MEMORY_SPACE.NUMNER_OF_STATE_KIND);
            accessor = share_mem.CreateViewAccessor();
            workController = new WorkController();
            visualStationMonitor = new VisualStationMonitor(this, workController, accessor);
            functionStationMonitor = new FunctionStationMonitor(this, workController, accessor, (VisualStationMonitor)visualStationMonitor);
        }

        /// <summary>
        ///  PLCの接点監視タスクを開始する。
        ///  ポーリングでPLC通信を行うサブタスクを立ち上げる
        /// </summary>
        /// <returns> 
        ///  立ち上げたサブタスクが返される。
        ///  タスクの返り値は、正常終了(true)、異常終了(false)
        /// </returns>
        public async Task<bool> Start()
        {
            dotUtlType = new DotUtlType
            {
                ActLogicalStationNumber = 401
            };
            if (dotUtlType.Open() != 0)
                return false;
#if debug
            if (!DatabaseController.DBConnection("Data Source=tcp:192.168.96.69,54936;Initial Catalog=Robot22_2DB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"))
            {
                dotUtlType.Close();
                return false;
            }
#else
            if (!DatabaseController.DBConnection())
            {
                dotUtlType.Close();
                return false;
            }
#endif
            this.cancellToken = new CancellationTokenSource();

            await Task.Run(() => Run(cancellToken));
            //TODO 処理が正常終了、異常終了の定義をちゃんとする
            return false;
        }


        /// <summary>
        ///  ポーリングで、PLCの値をチェックするタスク。
        /// </summary>
        /// <param name="token"> 
        ///  このトークンに対してキャンセル命令をかけることで
        ///  ループが終了する。
        /// </param>
        /// <see cref="PLC_MonitorTask.cancellToken"/>
        public void Run(CancellationTokenSource token)
        {
            string label = "shine";
            int[] blockData_y41 = new int[4];
            int[] operationCode = new int[1];
            operationCode[0] = 0;
            Console.WriteLine("PLCの読み取り開始");
            int testData = 0;
            //  workController.AddnewWork(DateTime.Now);
            // workController.WriteProcesChangeData(CommonParameters.Process_Number.VisualStation_in, DateTime.Now);
            //キャンセル要求されるまで無限ループ
            string operationContact = "systemOperation";
            dotUtlType.WriteDeviceBlock(ref operationContact, 1, operationCode);
            while (!token.IsCancellationRequested)
            {
                DateTime now = DateTime.Now;
                dotUtlType.ReadDeviceBlock(ref label, 4, ref blockData_y41);
                plcData.X00_Block.NewBlockData = blockData_y41[0];
                // plcData.Y31_Block.NewBlockData = blockData_y41[0];
                // plcData.Y33_Block.NewBlockData = blockData_y41[2];
                // plcData.Y34_Block.NewBlockData = blockData_y41[3];

                /*
                testData++;
                plcData.Test_Block.NewBlockData = testData;
                if (plcData.Test_Block.IsAnyBitStundUp)
                {
                    var e = plcData.Test_Block.ChangedDatas(2, 3, 4);
                    if (e.Count > 0)
                        Thread.Sleep(1);
                }
                */

                label = "visualStation_StateBlock";
                dotUtlType.ReadDeviceBlock(ref label, 2, ref blockData_y41);
                plcData.B06_Block.NewBlockData = blockData_y41[0];
                visualStationMonitor.CheckData(plcData, now);
                //functionStationMonitor.CheckData(plcData, now);
                if (plcData.B06_Block.IsChangeBit)
                {
                    Thread.Sleep(10);
                }

                if (accessor.CanWrite)
                {
                    if (accessor.ReadBoolean(visualStationMonitor.writeMemoryStartAddress[
                        (int)MEMORY_SPACE.INPUT_OPERATION_STOP]))
                    {
                        operationCode[0] = CommonParameters.OPERATION_SYSTEM_STOP;
                        dotUtlType.WriteDeviceBlock(ref operationContact, 1, operationCode);
                        operationCode[0] = CommonParameters.OPERATION_SYSTEM_STOP | CommonParameters.OPERATION_SYSTEM_CLOCK;
                        dotUtlType.WriteDeviceBlock(ref operationContact, 1, operationCode);
                    }

                    else if (accessor.ReadBoolean(visualStationMonitor.writeMemoryStartAddress[
                        (int)MEMORY_SPACE.INPUT_OPERATION_START]))
                    {
                        operationCode[0] = CommonParameters.OPERATION_SYSTEM_START;
                        dotUtlType.WriteDeviceBlock(ref operationContact, 1, operationCode);
                        operationCode[0] = CommonParameters.OPERATION_SYSTEM_START | CommonParameters.OPERATION_SYSTEM_CLOCK;
                        dotUtlType.WriteDeviceBlock(ref operationContact, 1, operationCode);
                    }
                    accessor.Write(visualStationMonitor.writeMemoryStartAddress[
                         (int)MEMORY_SPACE.INPUT_OPERATION_START], false);
                    accessor.Write(visualStationMonitor.writeMemoryStartAddress[
                        (int)MEMORY_SPACE.INPUT_OPERATION_STOP], false);
                }

                Thread.Sleep(10);
            }//キャンセルされるまで続くポーリング処理

            Console.WriteLine("スレッドのキャンセル要求が来ました");
        }


        public int[] getVisualInspectionResult()
        {
            string label = "ResultBlock";
            int[] resultsDataBlock = new int[4];
            dotUtlType.ReadDeviceBlock(ref label, 4, ref resultsDataBlock);
            return resultsDataBlock;
        }
    }
}