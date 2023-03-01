﻿using MITSUBISHI.Component;
using PLC_Connection.Modules;
using ResultDatas;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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



        /// <summary>
        ///  工程の進みを検知するためのビットブロック管理オブジェクト
        ///  X000～X00Fまでを管理
        /// </summary>
        Parameters.Bitdata_Process block_X0;
        /// <summary> X040～X04Fまでのビットを監視し、工程の進み具合を検知 </summary>
        Parameters.Bitdata_Process x40;

        PLCContactData plcData = new PLCContactData();

        VisualStationMonitor visualStationMonitor;

        public PLC_MonitorTask()
        {
            visualStationMonitor = new VisualStationMonitor(this);
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


            if (!DatabaseController.DBConnection())
            {
                dotUtlType.Close();
                return false;
            }

            block_X0 = new Parameters.Bit_X();
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
            bool dbWrite = false;
            string label = "Y31";
            string ProcessLabel = "Result";
            int[] blockData_y41 = new int[4];
            Console.WriteLine("PLCの読み取り開始");
            while (true)
            {

                dotUtlType.ReadDeviceBlock(ref label, 4, ref blockData_y41);
                plcData.Y31_Block.NewBlockData = blockData_y41[0];
                plcData.Y33_Block.NewBlockData = blockData_y41[2];
                plcData.Y34_Block.NewBlockData = blockData_y41[3];

                visualStationMonitor.checkData(plcData);
                // キャンセル要求
                if (token.IsCancellationRequested)
                {
                    Console.WriteLine("スレッドのキャンセル要求が来ました");
                    break;
                }
                Thread.Sleep(2);
            }
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