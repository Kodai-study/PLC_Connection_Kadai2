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
        readonly Base_StationMonitor supplyStationMonitor;
        readonly Base_StationMonitor visualStationMonitor;
        readonly Base_StationMonitor functionStationMonitor;
        readonly Base_StationMonitor assemblyStationMonitor;
        MemoryMappedFile share_mem;
        MemoryMappedViewAccessor accessor;

        public PLC_MonitorTask()
        {
            share_mem = MemoryMappedFile.CreateNew("shared_memory", 4 * (int)MEMORY_SPACE.NUMNER_OF_STATE_KIND);
            accessor = share_mem.CreateViewAccessor();
            workController = new WorkController();
            supplyStationMonitor = new SupplyURMonitor(this, workController, accessor);
            visualStationMonitor = new VisualStationMonitor(this, workController, accessor);
            functionStationMonitor = new FunctionStationMonitor(this, workController, accessor, (VisualStationMonitor)visualStationMonitor);
            assemblyStationMonitor = new AssemblyStationMonitor(this, workController, accessor, (FunctionStationMonitor)functionStationMonitor);
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
            int[] buffer = new int[4];
            Console.WriteLine("PLCの読み取り開始");

            //キャンセル要求されるまで無限ループ
            while (!token.IsCancellationRequested)
            {
                DateTime now = DateTime.Now;
                dotUtlType.ReadDeviceBlock(ref ContactLabels.visualStationSensor, 1, ref buffer);
                plcData.X00_Block.NewBlockData = buffer[0];

                dotUtlType.ReadDeviceBlock(ref ContactLabels.visualInspectionState, 1, ref buffer);
                plcData.X40_Block.NewBlockData = buffer[0];


                dotUtlType.ReadDeviceBlock(ref ContactLabels.functionStationSensor, 2, ref buffer);
                plcData.B0D_Block.NewBlockData = buffer[0];
                plcData.B0E_Block.NewBlockData = buffer[1];

                dotUtlType.ReadDeviceBlock(ref ContactLabels.supplyStationState, 1, ref buffer);
                plcData.B03_Block.NewBlockData = buffer[0];

                dotUtlType.ReadDeviceBlock(ref ContactLabels.assemblyStationState, 1, ref buffer);
                plcData.B15_Block.NewBlockData = buffer[0];

                dotUtlType.ReadDeviceBlock(ref ContactLabels.functionStationState, 1, ref buffer);
                plcData.B0C_Block.NewBlockData = buffer[0];

                dotUtlType.ReadDeviceBlock(ref ContactLabels.visualStationState, 1, ref buffer);
                plcData.B06_Block.NewBlockData = buffer[0];

                supplyStationMonitor.CheckData(plcData, now);
                visualStationMonitor.CheckData(plcData, now);
                functionStationMonitor.CheckData(plcData, now);
                assemblyStationMonitor.CheckData(plcData, now);

                CheckSystemOperation();

                Thread.Sleep(10);
            }//キャンセルされるまで続くポーリング処理

            Console.WriteLine("スレッドのキャンセル要求が来ました");
        }


        public int[] GetVisualInspectionResult()
        {
            int[] resultsDataBlock = new int[4];
            dotUtlType.ReadDeviceBlock(ref ContactLabels.visualInspectionResult, 4, ref resultsDataBlock);
            return resultsDataBlock;
        }

        public bool GetFunctionInspectionResult(out float voltage, out int frequency)
        {
            int buffer = -1;
            int errCode;
            errCode = dotUtlType.GetDevice(ref ContactLabels.functionInspectionVoltage, ref buffer);
            if (errCode != 0 || buffer == 0)
            {
                voltage = -1;
                frequency = -1;
                return false;
            }
            voltage = buffer / 1000;
            errCode = dotUtlType.GetDevice(ref ContactLabels.functionInspectionFrequency, ref buffer);
            if (errCode != 0)
            {
                frequency = -1;
                return false;
            }
            frequency = buffer;
            return true;
        }


        private void CheckSystemOperation()
        {
            if (accessor.CanWrite)
            {
                var operationCode = new int[1];
                if (accessor.ReadBoolean(visualStationMonitor.writeMemoryStartAddress[
                    (int)MEMORY_SPACE.INPUT_OPERATION_STOP]))
                {
                    operationCode[0] = CommonParameters.OPERATION_SYSTEM_STOP;
                    dotUtlType.WriteDeviceBlock(ref ContactLabels.systemOperation, 1, operationCode);
                    operationCode[0] = CommonParameters.OPERATION_SYSTEM_STOP | CommonParameters.OPERATION_SYSTEM_CLOCK;
                    dotUtlType.WriteDeviceBlock(ref ContactLabels.systemOperation, 1, operationCode);
                }

                else if (accessor.ReadBoolean(visualStationMonitor.writeMemoryStartAddress[
                    (int)MEMORY_SPACE.INPUT_OPERATION_START]))
                {
                    operationCode[0] = CommonParameters.OPERATION_SYSTEM_START;
                    dotUtlType.WriteDeviceBlock(ref ContactLabels.systemOperation, 1, operationCode);
                    operationCode[0] = CommonParameters.OPERATION_SYSTEM_START | CommonParameters.OPERATION_SYSTEM_CLOCK;
                    dotUtlType.WriteDeviceBlock(ref ContactLabels.systemOperation, 1, operationCode);
                }
                accessor.Write(visualStationMonitor.writeMemoryStartAddress[
                     (int)MEMORY_SPACE.INPUT_OPERATION_START], false);
                accessor.Write(visualStationMonitor.writeMemoryStartAddress[
                    (int)MEMORY_SPACE.INPUT_OPERATION_STOP], false);
            }
        }

    }
}