using System;
//using System.IO.Ports;
//using System.Windows.Forms;
//using System.Drawing;
//using System.Timers;
//using System.Collections;
//using System.Collections.Generic;
//using System.Windows.Forms.DataVisualization.Charting;

//namespace PressureControlSystem
//{
//    class DataReceiver
//    {
//        private static ushort BaudRate = 9600;//设置波特率
//        private static ushort DataBits = 8;//设置数据位
//        private static StopBits StopBit = StopBits.One;//设置停止位
//        private static SerialPort PressurePort = new SerialPort();//压力变送器
//        private static SerialPort ValvePort = new SerialPort();//球阀开度       
//        private static readonly ushort PollingInterval = 500;//轮询间隔

//        public const ushort CommTimes = 5;
//        public const ushort PressureGaugeNum = 24;
//        public const ushort ValveNum = 18;
//        public const ushort qgyrrq = 3;

//        private static int ValveFeedbackGetMessageLength = 7;
//        private static byte[] ValveFeedbackGetMessageBuffer = new byte[ValveFeedbackGetMessageLength * 2];
//        private static int ValveFeedbackGetMessageCurrentLength = 0;
//        private static bool ValveFeedbackGetMessageMessageCombine = false;

//        public static decimal[] ValveOpeningValue = new decimal[ValveNum];
//        private static int[] ValveOpeningReceiveFlag = new int[ValveNum];

//        public static bool InitializingMode = true;

//        //定时器相关
//        public static System.Timers.Timer MainTimer = new System.Timers.Timer(PollingInterval);


//        private static double maxPressure = 2;//超过2的压力认为是无效的压力值

public struct ChartData
{
    public DateTime currentTime;
    public double data;
}
//        private static Queue histValveOpeningQueue0 = new Queue();
//        private static Queue histValveOpeningQueue1 = new Queue();
//        private static Queue histValveOpeningQueue2 = new Queue();
//        private static Queue[] HistValveOpeningValue = new Queue[qgyrrq]
//        {
//            histValveOpeningQueue0, histValveOpeningQueue1, histValveOpeningQueue2
//        };
//        //设置委托
//        public delegate void UpdataChartDelegate(ChartData chartData, int TypeNumber, Chart chart);
//        //设置事件
//        public static event UpdataChartDelegate UpdataChartEvent;
//        //自动控制类
//        private static CurveAutoModule curveModule0 = new CurveAutoModule();
//        private static CurveAutoModule curveModule1 = new CurveAutoModule();
//        private static CurveAutoModule curveModule2 = new CurveAutoModule();
//        public static CurveAutoModule[] curveAutoModules = new CurveAutoModule[qgyrrq] { curveModule0, curveModule1, curveModule2 };
//        private static SingleScaleAutoModule singleModule = new SingleScaleAutoModule();
//        private static int histPressureQueueLength = 12;//保存历史数据

//        private static bool InTurnQueryFlag = true;
//        public static bool InturnQueryFlag_Auto = true;//当自动控制时，用该变量控制是否轮询
//        private static int CircleID = 0;
//        private static int CurrentID = 0;
//        public static int histDiffCounter;//记录该支管的压力已经偏离几次了
//        public static int initinalCounter;
//        private static int QueueLength = 10;
//        private static decimal ValveFeedbackValueVariation = 3;

//        static private void MainTimerOperaion(object sender, ElapsedEventArgs e) //主定时器，500ms
//        {          

//            if (InitializingMode)
//            {
//                ValvePort.Write(ValveSettingGetCmd[CurrentID], 0, ValveSettingGetCmd[CurrentID].Length);
//                ValveSettingGetMessageCurrentLength = 0;
//                ValveSettingGetMessageMessageCombine = true;
//            }
//            else
//            {
//                if (!ValveSettingMode)
//                {
//                    ValvePort.Write(ValveFeedbackGetCmd[CurrentID], 0, ValveFeedbackGetCmd[CurrentID].Length);
//                    ValveOpeningReceiveFlag[CurrentID]--;
//                    //添加485通讯异常的判断
//                    if (ValveOpeningReceiveFlag[CurrentID] < 0)
//                    {
//                        MainForm.PanelsValveOpening[CircleID].BackColor = Color.Red;
//                        ValveOpeningReceiveFlag[CurrentID] = 0;
//                    }
//                    else
//                    {
//                        MainForm.PanelsValveOpening[CircleID].BackColor = Color.Gray;
//                    }


//                    for (int i = 0; i < ValveFeedbackGetMessageBuffer.Length; i++)
//                    {
//                        ValveFeedbackGetMessageBuffer[i] = 0x00;
//                    }
//                    ValveFeedbackGetMessageCurrentLength = 0;
//                    ValveFeedbackGetMessageMessageCombine = true;

//                }
//                else
//                {
//                    byte[] ValveSettingMessageBuffer = new byte[ValveSettingMessageLength];

//                    ValveSettingMessageBuffer[0] = ValveSettingAddress[CurrentID];
//                    ValveSettingMessageBuffer[1] = 0x06; ValveSettingMessageBuffer[2] = 0x00; ValveSettingMessageBuffer[3] = ValveSettingChannel[CurrentID];
//                    int ValveSettingValue;
//                    ValveSettingValue = (int)(MainForm.NumericUpDownValveSetting[CircleID].Value * 160 + 4000);
//                    ValveSettingMessageBuffer[4] = (byte)(ValveSettingValue / 256); ValveSettingMessageBuffer[5] = (byte)(ValveSettingValue % 256);

//                    int CRCResult = CRC(ValveSettingMessageBuffer, ValveSettingMessageLength);

//                    ValveSettingMessageBuffer[6] = (byte)(CRCResult % 256); ValveSettingMessageBuffer[7] = (byte)(CRCResult / 256);

//                    ValvePort.Write(ValveSettingMessageBuffer, 0, ValveSettingMessageLength);
//                    ValveSettingSuccessFlag[CurrentID]--;

//                    ValveSettingMessageCurrentLength = 0;
//                    ValveSettingMessageMessageCombine = true;
//                }
//            }
//            //为每一个CounterDown都做减1操作，直至减到0,用作倒计时
//            for (int i = 0; i < ValveNum; i++)
//            {
//                if (MainForm.SolenoidValueCounters[i] > 0)
//                {
//                    MainForm.SolenoidValueCounters[i]--;
//                }
//            }
//        }

//        public static void CommunityInitialization()
//        {
//            //初始化
//            for (int i = 0; i < PressureGaugeNum; i++)
//            {
//                PressureValue[i] = 0.0;
//                PressureReceiveFlag[i] = CommTimes;
//            }
//            for (int i = 0; i < ValveNum; i++)
//            {
//                ValveSettingValue[i] = 0; ValveOpeningValue[i] = 0;
//                ValveSettingSuccessFlag[i] = CommTimes; ValveOpeningReceiveFlag[i] = CommTimes;
//            }
//            //检查是否含有串口
//            string[] str = SerialPort.GetPortNames();
//            if (str.Length == 0)
//            {
//                MessageBox.Show("本机没有串口！", "Error");
//                return;
//            }
//            PressurePort.PortName = "COM8";//压力变送器
//            PressurePort.BaudRate = BaudRate;
//            PressurePort.DataBits = DataBits;
//            PressurePort.StopBits = StopBit;

//            ValvePort.PortName = "COM7";//球阀
//            ValvePort.BaudRate = BaudRate;
//            ValvePort.DataBits = DataBits;
//            ValvePort.StopBits = StopBit;

//            Control.CheckForIllegalCrossThreadCalls = false;    //这个类中我们不检查跨线程的调用是否合法(因为.net 2.0以后加强了安全机制,，不允许在winform中直接跨线程访问控件的属性)

//            PressurePort.DataReceived += new SerialDataReceivedEventHandler(PressurePort_DataReceived);

//            //准备就绪              
//            PressurePort.DtrEnable = true;
//            PressurePort.RtsEnable = true;
//            //设置数据读取超时为199ms
//            PressurePort.ReadTimeout = 199;

//            PressurePort.Open();

//            ValvePort.DataReceived += new SerialDataReceivedEventHandler(ValvePort_DataReceived);
//            //准备就绪              
//            ValvePort.DtrEnable = true;
//            ValvePort.RtsEnable = true;
//            //设置数据读取超时为199ms
//            ValvePort.ReadTimeout = 199;

//            ValvePort.Open();
//            //定时器相关
//            MainTimer.AutoReset = true;
//            MainTimer.Elapsed += new ElapsedEventHandler(MainTimerOperaion);
//            MainTimer.Start();
//        }
//        static void PressurePort_DataReceived(object sender, SerialDataReceivedEventArgs e)
//        {
//            if (PressurePort.IsOpen)     //此处可能没有必要判断是否打开串口，但为了严谨性，我还是加上了
//            {
//                try
//                {
//                    if (PressureGetMessageCombine)
//                    {
//                        Byte[] receivedData = new Byte[PressurePort.BytesToRead];        //创建接收字节数组                 
//                        PressurePort.Read(receivedData, 0, receivedData.Length);         //读取数据
//                        PressurePort.DiscardInBuffer();                                  //清空SerialPort控件的Buffer

//                        for (int i = PressureGetMessageCurrentLength; i < PressureGetMessageCurrentLength + receivedData.Length; i++)
//                        {
//                            PressureGetMessageBuffer[i] = receivedData[i - PressureGetMessageCurrentLength];
//                        }

//                        PressureGetMessageCurrentLength += receivedData.Length;

//                        if (PressureGetMessageCurrentLength >= PressureGetMessageLength)
//                        {
//                            PressureGetMessageCombine = false;

//                            int CRCResult = CRC(PressureGetMessageBuffer, PressureGetMessageLength);

//                            if (CRCResult != PressureGetMessageBuffer[PressureGetMessageLength - 2] + PressureGetMessageBuffer[PressureGetMessageLength - 1] * 256)
//                            {
//                                return;
//                            }
//                            else
//                            {
//                                if (PressureGetCmd[CurrentID][0] == PressureGetMessageBuffer[0])
//                                {
//                                    PressureValue[CurrentID] = 1 / 100.0 * (PressureGetMessageBuffer[3] * 256 + PressureGetMessageBuffer[4]);
//                                    //2021.1.12更新：添加一个判断，限制压力示数的最大值，避免因负数或其他原因导致的压力示数异常
//                                    if (PressureValue[CurrentID] < maxPressure)
//                                    {
//                                        MainForm.labelsPressure[CircleID].Text = PressureValue[CurrentID].ToString();
//                                        MainForm.PanelsPressure[CircleID].BackColor = Color.Green;
//                                        PressureReceiveFlag[CurrentID] = CommTimes;

//                                        ChartData chartData;
//                                        chartData.currentTime = DateTime.Now;
//                                        chartData.data = PressureValue[CurrentID];//封装绘图所需的数据，如实记录实时的数据

//                                        //if (MainForm.SolenoidValueCounters[CurrentID] == 0 && MainForm.SolenoidValueStatus[CurrentID])
//                                        //{
//                                        //    //绘图
//                                        //    if (MainForm.mainForm.allowUpdataChart)
//                                        //    {
//                                        //        UpdataChartEvent(chartData, 0, MainForm.Charts[CircleID]);//触发事件，实时的电磁阀打开后的压力
//                                        //    }

//                                        //    if (MainForm.mainForm.autoMode)
//                                        //    {
//                                        //        if (PressureValue[CurrentID] <= MainForm.AutoMinAllowPressure[CircleID] || PressureValue[CurrentID] > MainForm.AutoMaxAllowPressure[CircleID])//压力不在允许范围内
//                                        //        {
//                                        //            InturnQueryFlag_Auto = false;//存在偏离，停止轮询，继续查询该支管压力
//                                        //            histDiffCounter++;
//                                        //        }
//                                        //        else
//                                        //        {
//                                        //            curveAutoModules[CircleID].coarse = true;//压力在范围内，置为粗调
//                                        //            curveAutoModules[CircleID].cumulativeStep = 0;
//                                        //            InturnQueryFlag_Auto = true;
//                                        //            histDiffCounter = 0;                                                 
//                                        //        }

//                                        //        if (histDiffCounter >= histPressureQueueLength)//历史数据已经收集齐了
//                                        //        {
//                                        //            if (histDiffCounter % MainForm.mainForm.controlInterval == 0)//每隔controlInterval才控制一次
//                                        //            {
//                                        //                if (!InitializingMode)
//                                        //                {
//                                        //                    if (MainForm.mainForm.autoControlMethods == 1)//特性曲线控制
//                                        //                    {
//                                        //                        decimal newOpening = curveAutoModules[CircleID].PressureSteady(MainForm.NumericUpDownValveSetting[CircleID].Value, ValveOpeningValue[CurrentID], PressureValue[CurrentID], (decimal)MainForm.correctOpening[CircleID], CircleID);
//                                        //                        if (newOpening == -10)
//                                        //                        {
//                                        //                            InturnQueryFlag_Auto = true;
//                                        //                            histDiffCounter = 0;
//                                        //                        }
//                                        //                        else
//                                        //                        {
//                                        //                            //if (newOpening >= MainForm.MaxAllowOpening[CircleID] || newOpening <= MainForm.MinAllowOpening[CircleID])
//                                        //                            //{
//                                        //                            //    InturnQueryFlag_Auto = true;//已经调解到极限位置，放弃调节
//                                        //                            //    histDiffCounter = 0;
//                                        //                            //}
//                                        //                            //else
//                                        //                            //{
//                                        //                            //    MainForm.NumericUpDownValveSetting[CircleID].Value = newOpening;
//                                        //                            //}
//                                        //                            if (newOpening >= MainForm.MaxAllowOpening[CircleID])
//                                        //                            {
//                                        //                                MainForm.NumericUpDownValveSetting[CircleID].Value = MainForm.MaxAllowOpening[CircleID];
//                                        //                                InturnQueryFlag_Auto = true;//已经调解到极限位置，放弃调节
//                                        //                                histDiffCounter = 0;
//                                        //                            }
//                                        //                            else if (newOpening <= MainForm.MinAllowOpening[CircleID])
//                                        //                            {
//                                        //                                MainForm.NumericUpDownValveSetting[CircleID].Value = MainForm.MinAllowOpening[CircleID];
//                                        //                                InturnQueryFlag_Auto = true;//已经调解到极限位置，放弃调节
//                                        //                                histDiffCounter = 0;
//                                        //                            }
//                                        //                            else
//                                        //                            {
//                                        //                                MainForm.NumericUpDownValveSetting[CircleID].Value = newOpening;
//                                        //                            }
//                                        //                        }
//                                        //                    }
//                                        //                    else if (MainForm.mainForm.autoControlMethods == 2)//简单开度控制
//                                        //                    {
//                                        //                        decimal newOpening = singleModule.PressureSteady(MainForm.NumericUpDownValveSetting[CircleID].Value, chartData.data, MainForm.correctOpening[CurrentID], CircleID);
//                                        //                        if (newOpening == -10)//在压力范围内
//                                        //                        {
//                                        //                            InturnQueryFlag_Auto = true;
//                                        //                            histDiffCounter = 0;
//                                        //                        }
//                                        //                        else//新的开度
//                                        //                        {
//                                        //                            //if (newOpening >= MainForm.MaxAllowOpening[CircleID] || newOpening <= MainForm.MinAllowOpening[CircleID])
//                                        //                            //{
//                                        //                            //    InturnQueryFlag_Auto = true;//已经调解到极限位置，放弃调节
//                                        //                            //    histDiffCounter = 0;
//                                        //                            //}
//                                        //                            if (newOpening >= MainForm.MaxAllowOpening[CircleID])
//                                        //                            {
//                                        //                                MainForm.NumericUpDownValveSetting[CircleID].Value = MainForm.MaxAllowOpening[CircleID];
//                                        //                                InturnQueryFlag_Auto = true;//已经调解到极限位置，放弃调节
//                                        //                                histDiffCounter = 0;
//                                        //                            }
//                                        //                            else if (newOpening <= MainForm.MinAllowOpening[CircleID])
//                                        //                            {
//                                        //                                MainForm.NumericUpDownValveSetting[CircleID].Value = MainForm.MinAllowOpening[CircleID];
//                                        //                                InturnQueryFlag_Auto = true;//已经调解到极限位置，放弃调节
//                                        //                                histDiffCounter = 0;
//                                        //                            }
//                                        //                            else
//                                        //                            {
//                                        //                                MainForm.NumericUpDownValveSetting[CircleID].Value = newOpening;
//                                        //                            }
//                                        //                        }
//                                        //                    }
//                                        //                }
//                                        //            }
//                                        //        }
//                                        //    }
//                                        //    else
//                                        //    {
//                                        //        InturnQueryFlag_Auto = true;
//                                        //        histDiffCounter = 0;
//                                        //    }
//                                        //}
//                                        //else
//                                        //{
//                                        //    InturnQueryFlag_Auto = true;
//                                        //    histDiffCounter = 0;
//                                        //}
//                                    }
//                                }
//                            }
//                        }
//                    }
//                }
//                catch (Exception ex)
//                {
//                    throw;
//                }
//            }
//        }
//        static void ValvePort_DataReceived(object sender, SerialDataReceivedEventArgs e)
//        {
//            if (ValvePort.IsOpen)     //此处可能没有必要判断是否打开串口，但为了严谨性，我还是加上了
//            {
//                try
//                {
//                    if (ValveSettingMessageMessageCombine || ValveFeedbackGetMessageMessageCombine || ValveSettingGetMessageMessageCombine)
//                    {
//                        byte[] receivedData = new Byte[ValvePort.BytesToRead];        //创建接收字节数组
//                        ValvePort.Read(receivedData, 0, receivedData.Length);         //读取数据
//                        ValvePort.DiscardInBuffer();                                  //清空SerialPort控件的Buffer

//                        if (InitializingMode)
//                        {
//                            if (ValveSettingGetMessageMessageCombine)
//                            {
//                                for (int i = ValveSettingGetMessageCurrentLength; i < ValveSettingGetMessageCurrentLength + receivedData.Length; i++)
//                                {
//                                    ValveSettingGetMessageBuffer[i] = receivedData[i - ValveSettingGetMessageCurrentLength];
//                                }

//                                ValveSettingGetMessageCurrentLength += receivedData.Length;

//                                if (ValveSettingGetMessageCurrentLength >= ValveSettingGetMessageLength)
//                                {
//                                    ValveSettingGetMessageMessageCombine = false;

//                                    int CRCResult = CRC(ValveSettingGetMessageBuffer, ValveSettingGetMessageLength);

//                                    if (CRCResult != ValveSettingGetMessageBuffer[ValveSettingGetMessageLength - 2] + ValveSettingGetMessageBuffer[ValveSettingGetMessageLength - 1] * 256)
//                                    {
//                                        return;
//                                    }
//                                    else
//                                    {
//                                        if (ValveSettingGetMessageBuffer[0] == ValveSettingGetCmd[CurrentID][0])
//                                        {
//                                            ValveSettingValue[CurrentID] = ValveSettingGetMessageBuffer[3] * 256 + ValveSettingGetMessageBuffer[4];

//                                            MainForm.NumericUpDownValveSetting[CircleID].Value = (ValveSettingValue[CurrentID] - 4000) / (decimal)160.0;
//                                            initinalCounter++;
//                                            if (initinalCounter >= 3 * qgyrrq)
//                                            {
//                                                InitializingMode = false;
//                                            }
//                                        }
//                                    }
//                                }
//                            }
//                        }
//                        else
//                        {
//                            if (ValveSettingMode && ValveSettingMessageMessageCombine)
//                            {
//                                for (int i = ValveSettingMessageCurrentLength; i < ValveSettingMessageCurrentLength + receivedData.Length; i++)
//                                {
//                                    ValveSettingMessageBuffer[i] = receivedData[i - ValveSettingMessageCurrentLength];
//                                }

//                                ValveSettingMessageCurrentLength += receivedData.Length;

//                                if (ValveSettingMessageCurrentLength >= ValveSettingMessageLength)
//                                {
//                                    ValveSettingMessageMessageCombine = false;

//                                    int CRCResult = CRC(ValveSettingMessageBuffer, ValveSettingMessageLength);

//                                    if (CRCResult != ValveSettingMessageBuffer[ValveSettingMessageLength - 2] + ValveSettingMessageBuffer[ValveSettingMessageLength - 1] * 256)
//                                    {
//                                        return;
//                                    }
//                                    else
//                                    {
//                                        if (ValveSettingMessageBuffer[0] == ValveSettingAddress[CurrentID] && ValveSettingMessageBuffer[3] == ValveSettingChannel[CurrentID])
//                                        {
//                                            ValveSettingValue[CurrentID] = ValveSettingMessageBuffer[4] * 256 + ValveSettingMessageBuffer[5];
//                                            ValveSettingMode = false;
//                                            settingOpening = (ValveSettingValue[CurrentID] - 4000) / (decimal)160.0;

//                                            MainForm.NumericUpDownValveSetting[CircleID].Value = settingOpening;
//                                            ++ValveSettingSuccessFlag[CurrentID];
//                                        }
//                                    }
//                                }
//                            }

//                            if (!ValveSettingMode && ValveFeedbackGetMessageMessageCombine)
//                            {
//                                for (int i = ValveFeedbackGetMessageCurrentLength; i < ValveFeedbackGetMessageCurrentLength + receivedData.Length; i++)
//                                {
//                                    ValveFeedbackGetMessageBuffer[i] = receivedData[i - ValveFeedbackGetMessageCurrentLength];
//                                }

//                                ValveFeedbackGetMessageCurrentLength += receivedData.Length;

//                                if (ValveFeedbackGetMessageCurrentLength >= ValveFeedbackGetMessageLength)
//                                {
//                                    ValveFeedbackGetMessageMessageCombine = false;

//                                    int CRCResult = CRC(ValveFeedbackGetMessageBuffer, ValveFeedbackGetMessageLength);

//                                    if (CRCResult != ValveFeedbackGetMessageBuffer[ValveFeedbackGetMessageLength - 2] + ValveFeedbackGetMessageBuffer[ValveFeedbackGetMessageLength - 1] * 256)
//                                    {
//                                        return;
//                                    }
//                                    else
//                                    {
//                                        if (ValveFeedbackGetMessageBuffer[0] == ValveFeedbackGetCmd[CurrentID][0])
//                                        {
//                                            decimal temp;
//                                            temp = (decimal)((ValveFeedbackGetMessageBuffer[3] * 256 + ValveFeedbackGetMessageBuffer[4] - 4000) / 160.0);

//                                            if (HistValveOpeningValue[CircleID].Count < QueueLength)
//                                            {
//                                                InTurnQueryFlag = false;//阀设置模式后，队列被清空，此时强制进入非轮询模式
//                                            }
//                                            else
//                                            {
//                                                decimal histValue = (decimal)HistValveOpeningValue[CircleID].Dequeue();
//                                                //这里的判断，主要目的是判断球阀是否还在调节，可以和球阀几秒前的开度做比较，用一个队列实现                                              
//                                                if (Math.Abs(temp - histValue) > ValveFeedbackValueVariation)
//                                                {
//                                                    InTurnQueryFlag = false;
//                                                }
//                                                else
//                                                {
//                                                    InTurnQueryFlag = true;
//                                                }
//                                            }
//                                            //不管结果如何都要把最新的开度值压入队列
//                                            if (HistValveOpeningValue[CircleID].Count < QueueLength)
//                                            {
//                                                HistValveOpeningValue[CircleID].Enqueue(temp);
//                                            }
//                                            ValveOpeningValue[CurrentID] = temp;
//                                            if (temp >= 0 && temp <= 100)//只有开度值是正常的数才允许显示并认为通讯成功
//                                            {
//                                                feedbackOpening = (0.5 * (int)(ValveOpeningValue[CurrentID] / (decimal)0.5));
//                                                MainForm.labelsValveOpening[CircleID].Text = feedbackOpening.ToString();
//                                                MainForm.PanelsValveOpening[CircleID].BackColor = Color.Green;
//                                                ValveOpeningReceiveFlag[CurrentID] = CommTimes;
//                                                if (MainForm.mainForm.allowUpdataChart)
//                                                {
//                                                    ChartData chartData;
//                                                    chartData.currentTime = DateTime.Now;
//                                                    chartData.data = (double)ValveOpeningValue[CurrentID];//封装绘图所需的数据

//                                                    UpdataChartEvent(chartData, 1, MainForm.Charts[CircleID]);//触发事件，开度
//                                                }
//                                            }
//                                        }
//                                    }
//                                }
//                            }
//                        }
//                    }
//                }
//                catch (System.Exception ex)
//                {
//                }
//            }
//        }
//        private static int CRC(byte[] data, int datalength)
//        {
//            int CRCResult = 0xFFFF;
//            for (int i = 0; i < datalength - 2; i++)
//            {
//                CRCResult = CRCResult ^ data[i];
//                for (int j = 0; j < 8; j++)
//                {
//                    if ((CRCResult & 1) == 1)
//                        CRCResult = (CRCResult >> 1) ^ 0xA001;
//                    else
//                        CRCResult >>= 1;
//                }
//            }
//            return CRCResult;
//        }

//        public static void CurrentValueIDSetting(int circleID)
//        {
//            if (InitializingMode)
//            {
//                InTurnQueryFlag = true;
//                ValveSettingMode = false;
//            }
//            else //只有在非初始化模式下才能进行阀设定的相关操作
//            {
//                CircleID = circleID;
//                ValveSettingMode = true;
//                InTurnQueryFlag = false;
//                HistValveOpeningValue[circleID].Clear();
//            }
//        }
//    }
//}