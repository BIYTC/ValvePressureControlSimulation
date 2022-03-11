using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressureControlSystem
{
    class PIDAutoModule
    {
        //使用增量式PID控制，输出的是增量的增量
        static double ek = 5;
        static double ek_1 = 5;
        static double ek_2 = 5;//因为压力变化达不到5，所以用5来判断是否获取了该误差值，给定量与输出量之差
        static decimal increaseD;//increase的decimal类型

        decimal increaseIncreaseD;//增量的增量的double类型

        const int maxVariation = 2;//设置一个球阀开度最大允许变化量
        static bool controling = false;//表明目前是否结束调节
        static int stableCount;//如果压力在范围内，则stableCount加一
        int controlFinishCount = 5;//如果连续controlFinishCount次压力都在范围内，则认为控制结束
        double midPressure;//范围的中间值，用于给定

        static int previousSign = 0;//在increase变号时使用,1为正，-1为负，0为未知
        private decimal outOpening;//输出的开度

        public decimal PressureSteady(decimal setOpening, double pressure, decimal correct)
        {
            if (pressure <= MainForm.minPressure || pressure > MainForm.maxPressure || controling)//压力不在范围内或处于控制模式，需要进行调节
            {
                midPressure = (MainForm.minPressure + MainForm.maxPressure) / 2;
                if (ek == 5)
                {
                    ek = midPressure - pressure;//第一次只获取了一个误差，是给定量与输出量之差，不进行修正
                    return -20;//返回-20代表还没有获取全数据，需要停止轮询，继续获取误差
                }
                else if (ek != 5 && ek_1 == 5)//第二次获取误差，不进行修正
                {
                    ek_1 = ek;
                    ek = midPressure - pressure;//更新ek和ek_1
                    return -20;
                }
                else if (ek != 5 && ek_1 != 5 && ek_2 == 5)//第三次获取误差，不进行修正
                {
                    ek_2 = ek_1;
                    ek_1 = ek;
                    ek = midPressure - pressure;//更新ek、ek_1和ek_2
                    return -20;
                }
                else//历史误差都获取到了，可以进行PID控制
                {
                    controling = true;
                    increaseIncreaseD = (decimal)(MainForm.Kp * (ek - ek_1) + MainForm.Ki * ek + MainForm.Kd * (ek - (2 * ek_1) + ek_2));//计算增量的增量

                    increaseD += increaseIncreaseD;//利用“增量的增量”来计算“增量”

                    //更新ek,ek_1,ek_2
                    ek_2 = ek_1;
                    ek_1 = ek;
                    ek = midPressure - pressure;

                    if (Math.Abs(increaseD) > maxVariation)//如果球阀开度增量绝对值大于maxVariation了，则只允许变化maxVariation
                    {
                        increaseD = (increaseD / Math.Abs(increaseD)) * maxVariation;
                    }

                    if (pressure > MainForm.minPressure && pressure <= MainForm.maxPressure)//说明压力稳定在了范围内
                    {
                        increaseD = 0;//如果在范围内就先暂时不让阀调节，并且stableCount加1
                        stableCount++;
                    }
                    else//说明压力没有稳定,将计数清零
                    {
                        stableCount = 0;
                    }

                    //如果压力已经低于最小值但是increaseD还为负数，则将increaseD置0
                    //如果压力已经高于最大值但是increaseD还为正数，则将increaseD置0，防止其过度超调，再试一试
                    if ((pressure <= MainForm.minPressure && increaseD < 0) || (pressure > MainForm.minPressure && increaseD > 0))
                    {
                        increaseD = 0;
                    }

                    if (previousSign * increaseD > 0)//同向
                    {
                        outOpening = (int)(increaseD + setOpening);//直接返回要设定的值
                    }
                    else if (previousSign * increaseD < 0)//反向
                    {
                        if (increaseD > 0)//之前是减小，现在要往大调
                        {
                            outOpening = (int)(increaseD + setOpening + correct * 2 / 3);//返回要设定的值加补偿值
                        }
                        else//之前是增大，现在要往小调
                        {
                            outOpening = (int)(increaseD + setOpening - correct * 2 / 3);//返回要设定的值减补偿值
                        }
                    }
                    else//increased等于0或者previousSign等于0
                    {
                        if (increaseD == 0)
                        {
                            outOpening = setOpening;//不做调整
                        }
                        else//第一次调节，不知道过去的方向
                        {
                            outOpening = (int)(increaseD + setOpening);//直接返回要设定的值
                        }
                    }

                    //将现在的increaseD的方向保存
                    if (increaseD > 0)
                    {
                        previousSign = 1;
                    }
                    else if (increaseD < 0)
                    {
                        previousSign = -1;
                    }//如果本次increaseD为0，则previousSign保留为上次不变


                    if (stableCount >= controlFinishCount)//只有连续稳定controlFinishCount次，才认为调节结束了
                    {
                        ResetParams();
                    }
                    return outOpening;
                }
            }
            else//压力在范围内，不需要进行调节了
            {
                ResetParams();
                return -10;//返回-10代表没在控制模式中，并且压力在范围内，不需要调节
            }
        }
        public static void ResetParams()
        {
            stableCount = 0;
            controling = false;//退出控制模式
            increaseD = 0;//将增量置0
            //重置误差
            ek = 5;
            ek_1 = 5;
            ek_2 = 5;
        }
    }
}
