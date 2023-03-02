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


        public PLC_MonitorTask()
        {
            MemoryMappedFile share_mem = MemoryMappedFile.CreateNew("shared_memory",4 * (int)MEMORY_SPACE.NUMNER_OF_STATE_KIND);
            MemoryMappedViewAccessor accessor = share_mem.CreateViewAccessor();
            accessor.Write(0, 1);
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


            if (!DatabaseController.DBConnection("Data Source=tcp:192.168.96.69,54936;Initial Catalog=Robot22_2DB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"))
            {
                dotUtlType.Close();
                return false;
            }

            this.cancellToken = new CancellationTokenSource();
            workController = new WorkController();

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
            string label = "Y31";
            int[] blockData_y41 = new int[4];
            Console.WriteLine("PLCの読み取り開始");
            int testData = 0;
            //キャンセル要求されるまで無限ループ
            while (!token.IsCancellationRequested)
            {
                DateTime now = DateTime.Now;
                dotUtlType.ReadDeviceBlock(ref label, 4, ref blockData_y41);
               // plcData.Y31_Block.NewBlockData = blockData_y41[0];
               // plcData.Y33_Block.NewBlockData = blockData_y41[2];
               // plcData.Y34_Block.NewBlockData = blockData_y41[3];

                plcData.X00_Block.NewBlockData = testData;
                testData++;

                //visualStationMonitor.CheckData(plcData, now);
                //functionStationMonitor.CheckData(plcData, now);


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