using System;

namespace PressureControlSystem
{
    class SingleScaleAutoModule
    {
        static bool coarse = false;//当前是细调模式，不允许粗调
        public double previousSign = 0;//上次调节是往大调还是往小调,1为正，-1为负；
        int unknowStrideDivide = 2;//用correct的unknowStrideDivide分之一来作为试探用的步长
        decimal maxUnknownStride = 3; //用correct的unknowStrideDivide分之一来作为试探用的步长,最大为3
        double minPressure;
        double maxPressure;
        /// <summary>
        /// 该模式相较于特性曲线方法，只有误差补偿时会进行跨刻度调节（粗调），其余时刻均单一刻度调节
        /// </summary>
        /// <param name="setOpening">当前设定的开度</param>
        /// <param name="pressure">当前的压力</param>
        /// <param name="correct">补偿的开度</param>
        /// <returns>返回下一次设定的开度</returns>
        public decimal PressureSteady(decimal setOpening, double pressure, decimal correct)
        {
            minPressure = MainForm.minPressure;
            maxPressure = MainForm.maxPressure;
            if (pressure <= minPressure || pressure > maxPressure)//压力不在范围内，需要进行调节
            {
                //判断需要粗调的情况
                if (previousSign == 0)//未知历史方向，粗调
                {
                    //coarse = true;
                }
                else if (previousSign == -1 && pressure <= minPressure)//之前是往小调，现在压力偏小，应该往大调，粗调
                {
                    coarse = true;
                }
                else if (previousSign == 1 && pressure > maxPressure)//之前是往大调，现在压力偏大，应该往小调，粗调
                {
                    coarse = true;
                }

                if (coarse)
                {
                    if (previousSign == -1)//上次有效调节是往小调，这次往大调
                    {
                        coarse = false;//粗调结束                      
                        previousSign = 1;//已经粗调过了，记录方向
                        return CalNewCoarseOpening(setOpening, correct, 1);
                    }
                    else if (previousSign == 1)//上次有效调节是往大调节，这次往小调
                    {
                        coarse = false;//粗调结束
                        previousSign = -1;//已经粗调过了，记录方向
                        return CalNewCoarseOpening(setOpening, correct, -1); ;
                    }
                    else//处于未知状态，需要先对当前状态进行明确，用correct的unknowStrideDivide分之一来作为试探用的步长
                    {
                        if (pressure <= minPressure)//当前压力小于最小压力，需要往大调节
                        {
                            coarse = false;//粗调结束
                            previousSign = 1;//已经粗调过了，记录方向
                            return CalNewCoarseOpening(setOpening, correct, 1); ;
                        }
                        else//当前压力大于最大压力，需要往小调节
                        {
                            coarse = false;//粗调结束
                            previousSign = -1;//已经粗调过了，记录方向
                            return CalNewCoarseOpening(setOpening, correct, -1); ;
                        }
                    }
                }
                else//细调
                {
                    if (pressure <= minPressure)//压力小于最小值，需要往大调节
                    {
                        previousSign = 1;//每次调节前都要保存本次的调节方向
                        return setOpening + (decimal)0.5;//以0.5为步长进行调节
                    }
                    else//压力大于最大值，需要往小调节，但之前是往大调节,或不确定是否到达关闭曲线
                    {
                        previousSign = -1;//每次调节前都要保存本次的调节方向
                        return setOpening - (decimal)0.5;//以0.5为步长进行调节
                    }
                }
            }
            else
            {
                return -10;
            }
        }
        /// <summary>
        /// 计算预估的步长，最小值为1
        /// </summary>
        /// <param name="correct">开度补偿</param>
        /// <returns>返回试探用的步长</returns>
        private decimal CalculateStride(decimal correct)
        {
            if ((correct / unknowStrideDivide) == 0)
            {
                return 1;
            }
            else
            {
                return Math.Min(maxUnknownStride, (decimal)correct / unknowStrideDivide);
            }
        }
        /// <summary>
        /// 计算粗调的开度
        /// </summary>
        /// <param name="setOpening"> 之前的设定值</param>
        /// <param name="correct"> 机械补偿</param>
        /// <param name="sign">方向，-1为向下，1为向上</param>
        /// <returns></returns>
        private decimal CalNewCoarseOpening(decimal setOpening,decimal correct,int sign)
        {
            if (sign < 0)
            {
                return setOpening - CalculateStride(correct);
            }
            else
            {
                return setOpening + CalculateStride(correct);
            }
        }
    }
}
