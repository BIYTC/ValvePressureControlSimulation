using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressureControlSystem
{
    /// <summary>
    /// 基于特性曲线来粗调压力
    /// </summary>
    class CurveAutoModule
    {
        public bool coarse = true;//当前是粗调模式
        public double midPressure;//范围的中间值，用于给定
        public double minPressure;
        public double maxPressure;
        double previousSign = 0;//上次调节是往大调还是往小调,1为正，-1为负；0.1为向开度增大方向进行了一次尝试，-0.1为向开度减小方向进行了一次尝试
        double pressurePercent_Close;//反馈的开度对应的关闭曲线的压力百分比
        double pressurePercent_Open;//反馈的开度对应的打开曲线的压力百分比

        public double totalPressure;//通过当前开度和压力反推出的阀门全开时的最大压力
        int unknowStrideDivide = 3;//用correct的unknowStrideDivide分之一来作为试探用的步长
        private double histPressure = -1;//历史压力值
        decimal newOpening;//利用特性曲线得到的新的开度
        double forbidCoarseThreshold = 0.1;//如果压力差小于0.03就禁止粗调
        public decimal cumulativeStep = 0;//累计步长，当连续进行尝试的时候，如果累计的步长超过了correct，就可以认为找到了对应的曲线
        double enforceCoarseThreshold = 0.2;//当压力差大于该值且此时处于细调模式时，强制进入粗调模式。这个设置要小心当曲线不准或其他原因，导致下一次粗调还大于该值，造成连续粗调
        /// <summary>
        /// 基于特性曲线来粗调压力，但只允许进行一次粗调，在粗调结束后如果压力没有到位则必须进入细调
        /// </summary>
        /// <param name="setOpening">当前设定的开度</param>
        /// <param name="backOpening">当前反馈的开度</param>
        /// <param name="pressure">当前的压力</param>
        /// <param name="correct">补偿的开度</param>
        /// <returns>返回下一次设定的开度</returns>
        public decimal PressureSteady(decimal setOpening, decimal backOpening, double pressure, decimal correct)
        {
            minPressure = MainForm.minPressure;
            maxPressure = MainForm.maxPressure;
            if (pressure <= minPressure || pressure > maxPressure)//压力不在范围内，需要进行调节
            {
                midPressure = (minPressure + maxPressure) / 2;
                if (Math.Abs(pressure - midPressure) < forbidCoarseThreshold)
                {
                    coarse = false;
                }
                if (coarse || (Math.Abs(pressure - midPressure) >= enforceCoarseThreshold && !coarse))
                {
                    //通过当前的压力和当前的开度，来计算目标压力应该在哪个开度上
                    if (previousSign == -1)//上次有效调节是往小调，可以使用关闭曲线来计算本次开度
                    {
                        pressurePercent_Close = ValveModel.closingCurve[(int)(backOpening * 2)];
                        if (pressurePercent_Close != 0)
                        {
                            totalPressure = pressure / pressurePercent_Close;
                            newOpening = CalculateNewOpening(midPressure, totalPressure, pressure);
                            coarse = false;//粗调结束
                            histPressure = pressure;//无论如何都需要在返回新开度前将当前压力保存下来
                            if (pressure > maxPressure)//说明这次是往小调节
                            {
                                previousSign = -1;
                            }
                            else
                            {
                                previousSign = 1;
                            }
                            return newOpening;//因为特性曲线本身就是基于开度补偿绘制的，所以在这里不需要再考虑开度补偿问题，但需要将调节的方向保存下来
                        }
                        else
                        {
                            if (pressure <= minPressure)//当前压力小于最小压力，需要往大调节
                            {
                                previousSign = 0.1;//标记为向开度增大方向进行了一次尝试
                                cumulativeStep += CalculateStride(correct);
                                histPressure = pressure;//将当前的压力值赋给histPressure
                                coarse = true;//粗调尚未结束
                                return setOpening + CalculateStride(correct);
                            }
                            else//当前压力大于最大压力，需要往小调节
                            {
                                previousSign = -0.1;//标记为向开度减小方向进行了一次尝试
                                cumulativeStep -= CalculateStride(correct);
                                histPressure = pressure;//将当前的压力值赋给histPressure
                                coarse = true;
                                return setOpening - CalculateStride(correct);
                            }
                        }
                    }
                    else if (previousSign == 1)//上次有效调节是往大调节，可以使用打开曲线来计算本次开度
                    {
                        pressurePercent_Open = ValveModel.openingCurve[(int)(backOpening * 2)];
                        if (pressurePercent_Open != 0)
                        {
                            totalPressure = pressure / pressurePercent_Open;
                            newOpening = CalculateNewOpening(midPressure, totalPressure, pressure);
                            coarse = false;
                            histPressure = pressure;//保存当前压力
                            if (pressure > midPressure)//说明这次是往小调节
                            {
                                previousSign = -1;
                            }
                            else
                            {
                                previousSign = 1;
                            }
                            return newOpening;
                        }
                        else
                        {
                            if (pressure <= minPressure)//当前压力小于最小压力，需要往大调节
                            {
                                previousSign = 0.1;//标记为向开度增大方向进行了一次尝试
                                cumulativeStep += CalculateStride(correct);
                                histPressure = pressure;//将当前的压力值赋给histPressure
                                coarse = true;//粗调尚未结束
                                return setOpening + CalculateStride(correct);
                            }
                            else//当前压力大于最大压力，需要往小调节
                            {
                                previousSign = -0.1;//标记为向开度减小方向进行了一次尝试
                                cumulativeStep -= CalculateStride(correct);
                                histPressure = pressure;//将当前的压力值赋给histPressure
                                coarse = true;
                                return setOpening - CalculateStride(correct);
                            }
                        }
                    }
                    else//处于未知状态，需要先对当前状态进行明确，用correct的unknowStrideDivide分之一来作为试探用的步长，并保存当前的压力值，如果压力值朝着期望的方向变化了则认为此时开度正位于某条曲线上
                    {
                        if (histPressure == -1)//说明是第一次进行调节，处于未知状态且还没有获取过历史压力
                        {
                            if (pressure <= minPressure)//当前压力小于最小压力，需要往大调节
                            {
                                previousSign = 0.1;//标记为向开度增大方向进行了一次尝试
                                cumulativeStep += CalculateStride(correct);
                                histPressure = pressure;//将当前的压力值赋给histPressure
                                coarse = true;//粗调尚未结束
                                return setOpening + CalculateStride(correct);
                            }
                            else//当前压力大于最大压力，需要往小调节
                            {
                                previousSign = -0.1;//标记为向开度减小方向进行了一次尝试
                                cumulativeStep -= CalculateStride(correct);
                                histPressure = pressure;//将当前的压力值赋给histPressure
                                coarse = true;
                                return setOpening - CalculateStride(correct);
                            }
                        }
                        else//仍处于未知状态且获取过历史压力值，需要将当前压力值与历史压力值进行比较，观察变化是否符合预期
                        {
                            if (previousSign * (pressure - histPressure) > 0 || (Math.Abs(cumulativeStep) >= correct))//说明压力变化符合预期或cumulativeStep的绝对值已经大于了correct
                            {
                                previousSign *= 10;//将previousSign置为正/负1，表示已经成功抵达某条特征曲线，并可以基于特性曲线计算本次调节的开度了
                                cumulativeStep = 0;
                                if (previousSign == -1)//上次有效调节是往小调，可以使用关闭曲线来计算本次开度
                                {
                                    pressurePercent_Close = ValveModel.closingCurve[(int)(backOpening * 2)];
                                    if (pressurePercent_Close != 0)
                                    {
                                        totalPressure = pressure / pressurePercent_Close;
                                        newOpening = CalculateNewOpening(midPressure, totalPressure, pressure);
                                        coarse = false;//粗调结束
                                        histPressure = pressure;//无论如何都需要在返回新开度前将当前压力保存下来
                                        if (pressure > maxPressure)//说明这次是往小调节
                                        {
                                            previousSign = -1;
                                        }
                                        else
                                        {
                                            previousSign = 1;
                                        }
                                        return newOpening;//因为特性曲线本身就是基于开度补偿绘制的，所以在这里不需要再考虑开度补偿问题，但需要将调节的方向保存下来
                                    }
                                    else
                                    {
                                        if (pressure <= minPressure)//当前压力小于最小压力，需要往大调节
                                        {
                                            previousSign = 0.1;//标记为向开度增大方向进行了一次尝试
                                            cumulativeStep += CalculateStride(correct);
                                            histPressure = pressure;//将当前的压力值赋给histPressure
                                            coarse = true;//粗调尚未结束
                                            return setOpening + CalculateStride(correct);
                                        }
                                        else//当前压力大于最大压力，需要往小调节
                                        {
                                            previousSign = -0.1;//标记为向开度减小方向进行了一次尝试
                                            cumulativeStep -= CalculateStride(correct);
                                            histPressure = pressure;//将当前的压力值赋给histPressure
                                            coarse = true;
                                            return setOpening - CalculateStride(correct);
                                        }
                                    }
                                }
                                else//上次有效调节是往大调节，可以使用打开曲线来计算本次开度
                                {
                                    pressurePercent_Open = ValveModel.openingCurve[(int)(backOpening * 2)];
                                    if (pressurePercent_Open != 0)
                                    {
                                        totalPressure = pressure / pressurePercent_Open;
                                        newOpening = CalculateNewOpening(midPressure, totalPressure, pressure);
                                        coarse = false;
                                        histPressure = pressure;//保存当前压力
                                        if (pressure > midPressure)//说明这次是往小调节
                                        {
                                            previousSign = -1;
                                        }
                                        else
                                        {
                                            previousSign = 1;
                                        }
                                        return newOpening;
                                    }
                                    else
                                    {
                                        if (pressure <= minPressure)//当前压力小于最小压力，需要往大调节
                                        {
                                            previousSign = 0.1;//标记为向开度增大方向进行了一次尝试
                                            cumulativeStep += CalculateStride(correct);
                                            histPressure = pressure;//将当前的压力值赋给histPressure
                                            coarse = true;//粗调尚未结束
                                            return setOpening + CalculateStride(correct);
                                        }
                                        else//当前压力大于最大压力，需要往小调节
                                        {
                                            previousSign = -0.1;//标记为向开度减小方向进行了一次尝试
                                            cumulativeStep -= CalculateStride(correct);
                                            histPressure = pressure;//将当前的压力值赋给histPressure
                                            coarse = true;
                                            return setOpening - CalculateStride(correct);
                                        }
                                    }
                                }

                            }
                            else//压力变化不符合开度变化的预期，或压力没变或压力反向变化了，说明无法保证此时已经达到了某条曲线
                            {
                                if (pressure <= minPressure)//当前压力小于最小压力，需要往大调节
                                {
                                    previousSign = 0.1;//标记为向开度增大方向进行了一次尝试
                                    cumulativeStep += CalculateStride(correct);
                                    histPressure = pressure;//将当前的压力值赋给histPressure
                                    coarse = true;
                                    return setOpening + CalculateStride(correct);
                                }
                                else//当前压力大于最大压力，需要往小调节
                                {
                                    previousSign = -0.1;//标记为向开度减小方向进行了一次尝试
                                    cumulativeStep -= CalculateStride(correct);
                                    histPressure = pressure;//将当前的压力值赋给histPressure
                                    coarse = true;
                                    return setOpening - CalculateStride(correct);
                                }
                            }
                        }
                    }
                }
                else//粗调结束，但压力没有在范围内，或压力需要调节的很小，进行细调
                {
                    if (pressure <= minPressure && previousSign == 1)//压力小于最小值，需要往大调节，且之前也是往大调节
                    {
                        previousSign = 1;//每次调节前都要保存本次的调节方向
                        histPressure = pressure;
                        return setOpening + (decimal)0.5;//以1为步长进行调节
                    }
                    else if (pressure <= minPressure && (previousSign != 1))//压力小于最小值，需要往大调节，但是之前是往小调节。或不确定是否到达打开曲线
                    {
                        if (histPressure == -1)//细调，但之前没获取过压力
                        {
                            previousSign = 0.1;//往大调节，但不确定是否到达打开曲线
                            cumulativeStep += CalculateStride(correct);
                            histPressure = pressure;//将当前的压力值赋给histPressure
                            return setOpening + CalculateStride(correct);//用1/unknowStrideDivide的开度补偿为步长进行调节
                        }
                        else//之前获取过压力了，但是不确定是否已经到达了曲线
                        {
                            if ((pressure > histPressure && previousSign > 0) || cumulativeStep >= correct)//说明压力变大了，或者累计的打开开度已经大于误差步长，证明已经到达了打开曲线
                            {
                                histPressure = pressure;
                                previousSign = 1;
                                cumulativeStep = 0;
                                return setOpening + (decimal)0.5;//以1为步长进行调节
                            }
                            else//压力没有变大
                            {
                                histPressure = pressure;
                                previousSign = 0.1;
                                cumulativeStep += CalculateStride(correct);
                                return setOpening + CalculateStride(correct);//用1/unknowStrideDivide的开度补偿为步长进行调节
                            }
                        }
                    }
                    else if (pressure > maxPressure && previousSign == -1)//压力大于最大值，需要往小调节，且之前也是往小调节
                    {
                        previousSign = -1;
                        histPressure = pressure;
                        return setOpening - (decimal)0.5;
                    }
                    else//压力大于最大值，需要往小调节，但之前是往大调节,或不确定是否到达关闭曲线
                    {
                        if (histPressure == -1)//细调，但之前没获取过压力
                        {
                            previousSign = -0.1;//往小调节，但不确定是否到达打开曲线
                            histPressure = pressure;//将当前的压力值赋给histPressure
                            cumulativeStep -= CalculateStride(correct);
                            return setOpening - CalculateStride(correct);//用1/unknowStrideDivide的开度补偿为步长进行调节
                        }
                        else//之前获取过压力了，但是不确定是否已经到达了曲线
                        {
                            if ((pressure < histPressure && previousSign < 0) || cumulativeStep <= -correct)//说明压力变小了，证明已经到达了关闭曲线
                            {
                                histPressure = pressure;
                                previousSign = -1;
                                cumulativeStep = 0;
                                return setOpening - (decimal)0.5;//以-1为步长进行调节
                            }
                            else//压力没有变小
                            {
                                histPressure = pressure;
                                previousSign = -0.1;
                                cumulativeStep -= CalculateStride(correct);
                                return setOpening - CalculateStride(correct);//用1/unknowStrideDivide的开度补偿为步长进行调节
                            }
                        }
                    }
                }

            }
            else
            {
                if (previousSign * (pressure - histPressure) > 0 && Math.Abs(previousSign) == 0.1)//说明上次的不确定调节让压力按照预期进入了设定范围
                //这里不用限定histPressure,因为Math.Abs(previousSign) == 0.1已经限定histPressure不会为-1
                {
                    previousSign *= 10;
                }
                histPressure = pressure;
                coarse = true;//当压力调节到范围后，恢复粗调模式
                cumulativeStep = 0;
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
            return Math.Max((decimal)0.5, correct / unknowStrideDivide);
            //if ((int)(correct / unknowStrideDivide) == 0)
            //{
            //    return (decimal)0.5;
            //}
            //else
            //{
            //    return (decimal)(correct / unknowStrideDivide);
            //}
        }
        /// <summary>
        /// 利用特征曲线，计算预期的开度值
        /// </summary>
        /// <param name="targetPressure">预期的压力</param>
        /// <param name="totalPressure">通过当前的压力和</param>
        /// <param name="currentPressure">当前的压力</param>
        /// <returns>返回新的开度，如果有异常则返回-1</returns>
        private decimal CalculateNewOpening(double targetPressure, double totalPressure, double currentPressure)
        {
            //Debug.Assert(previousSign == 1 || previousSign == -1,"previousSign只能为正负1，以说明此时正位于某一条特征曲线上");
            double percent = targetPressure / totalPressure;
            double error = 2;
            decimal opening = -1;
            if (targetPressure > currentPressure)//需要开度变大，在打开的曲线上来寻找开度
            {
                for (int i = 0; i < ValveModel.openingCurve.Length; i++)
                {
                    double _temp = Math.Abs(ValveModel.openingCurve[i] - percent);
                    if (_temp < error)//调大的过程，因为i是从0开始，在有相同的error的情况下，取小的开度，尽可能避免调整过度还需要再向小调的问题
                    {
                        error = _temp;
                        opening = i * (decimal)0.5;
                    }
                }
                return opening;
            }
            else//开度变小，在闭合的曲线上来寻找开度
            {
                for (int i = 0; i < ValveModel.closingCurve.Length; i++)
                {
                    double _temp = Math.Abs(ValveModel.closingCurve[i] - percent);
                    if (_temp <= error)//调小的过程，因为i是从0开始，在有相同的error的情况下，选大的开度
                    {
                        error = _temp;
                        opening = i * (decimal)0.5;
                    }
                }
                return opening;
            }
        }
    }
}
