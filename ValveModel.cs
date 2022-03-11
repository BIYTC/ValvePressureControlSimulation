using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressureControlSystem
{
    public class ValveModel
    {
        public static double[] closingCurve = new double[91] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.001712553, 0.006376742, 0.012290086, 0.020369307, 0.03398914, 0.051588141, 0.073206604, 0.111688678, 0.155530035, 0.211329042, 0.279559169, 0.359041776, 0.454753342, 0.54032055, 0.630783644, 0.71275449, 0.782707244, 0.839412897, 0.886346923, 0.916669185, 0.941017659, 0.958576365, 0.970544088, 0.979268035, 0.986027582, 0.990319039, 0.993300896, 0.995325738, 0.996806592, 0.997864346, 0.998509071, 0.998962394, 0.999294831, 0.999506382, 0.999657489, 0.999778375, 0.999838819, 0.999889188, 0.999929483, 0.999949631, 0.999969778, 0.999979852, 0.999979852, 0.999989926, 0.999989926, 0.999989926, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
        public static double[] openingCurve = new double[91] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.0000806, 0.004492933, 0.010315613, 0.018545942, 0.031359868, 0.047951484, 0.070768735, 0.104888835, 0.14670535, 0.20025789, 0.272860064, 0.351415878, 0.446563309, 0.53714729, 0.625112071, 0.71215006, 0.780229079, 0.835866904, 0.883002408, 0.915409955, 0.93944614, 0.958304371, 0.970544088, 0.979268035, 0.985906696, 0.990127636, 0.993089345, 0.995325738, 0.996736075, 0.997793829, 0.998458702, 0.998922099, 0.999274683, 0.999496308, 0.999647416, 0.999758228, 0.999828745, 0.999879114, 0.999919409, 0.999939557, 0.999959705, 0.999969778, 0.999979852, 0.999989926, 0.999989926, 0.999989926, 0.999989926, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 ,1};

        public decimal backOpening = 19;//当前实际反馈的开度
        public decimal adjustSpeed = 6.5M;//球阀每秒动作多少开度
        public decimal error = 14M * 0.5M;//球阀的间隙误差
        public decimal histMove = 0;//记录历史运动的开度变化
        private decimal histhistMove = 0;
        public decimal direction = 1;//该值为1表示在打开曲线上，为-1-表示在关闭曲线上
        private decimal allowOpeningDiff = 0.2M;//允许的球阀设定值与反馈值之间的误差
        

        public double pressurePercent { get; set; }//用于往外传参的当前球阀压力百分比
        public ValveModel()
        {
            pressurePercent = openingCurve[(int)backOpening * 2];
        }
        /// <summary>
        /// 这个函数只听球阀时钟的触发，每次动作速度取决于adjustSpeed/球阀时钟的频率
        /// 默认初始位置在打开曲线的19开度上，即54%左右
        /// 该函数不需要返回值，当前球阀的开度百分比可以使用属性来传出来
        /// </summary>
        /// <param name="settingOpening">设置的开度值</param>
        public void ValveMove(decimal settingOpening)
        {
            if (Math.Abs(settingOpening - backOpening) > allowOpeningDiff)//只有球阀的反馈值与设定值之间的误差过大，球阀才会动作
            {
                histhistMove = histMove;
                if (Math.Abs(direction) == 1)//明确球阀当前所处的曲线
                {
                    if ((settingOpening - backOpening) * direction > 0)//与所在曲线同向运动
                    {
                        backOpening += direction * adjustSpeed / (1000 / MainForm.valvePeriod);//用dircetion来表示方向，正负1
                        histMove = 0;//不需要保存历史移动数据,direction保持不变
                    }
                    else//反向运动
                    {
                        backOpening += -1 * direction * adjustSpeed / (1000 / MainForm.valvePeriod);//用dircetion来表示方向，正负1                      
                        histMove += -1 * direction * adjustSpeed / (1000 / MainForm.valvePeriod);//用dircetion来表示方向，正负1
                        direction *= -0.1M;//direction变号
                    }
                }
                else//不明确球阀当前所处的曲线
                {
                    if ((settingOpening - backOpening) * direction > 0)//与上次的运动方向同向
                    {
                        backOpening += 10 * direction * adjustSpeed / (1000 / MainForm.valvePeriod);//用10 * dircetion来表示方向，计算得到的值为正负1
                        histMove += 10 * direction * adjustSpeed / (1000 / MainForm.valvePeriod);//计算历史移动量
                    }
                    else//与上次的运动方向反向
                    {
                        backOpening += -10 * direction * adjustSpeed / (1000 / MainForm.valvePeriod);//用10 * dircetion来表示方向，计算得到的值为正负1  
                        histMove += -10 * direction * adjustSpeed / (1000 / MainForm.valvePeriod);//计算历史移动量
                        direction *= -1;//反向后记得将direction变号
                    }
                    //判断是否在曲线上了
                    //判断是否达到某一条曲线的依据：
                    //A、histMove的绝对值大于等于error了
                    //B、histMove的前后存在变号或为0
                    if (Math.Abs(histMove) >= error)//A条件
                    {
                        histMove = 0;
                        direction *= 10;
                    }
                    else if (histMove == 0 || (histMove != 0 && histMove * histhistMove < 0))//B条件
                    {
                        histMove = 0;
                        direction *= 10;
                    }
                    else//没到特定曲线
                    {
                        //无操作
                    }
                }
            }
            //利用backOpening和direction来计算压力百分比
            //使用差值法来获得更精细的压力百分比
            if (Math.Abs(direction) == 1)//明确球阀当前所处的曲线，寻找当前的压力百分比
            {
                if (direction < 0)//处于关闭曲线上
                {
                    if (backOpening * 2 - (int)(backOpening * 2) <= 0)//是整数
                    {
                        pressurePercent = closingCurve[(int)(backOpening * 2)];
                    }
                    else//当前反馈开度介于球阀两个采样数据之间
                    {
                        pressurePercent = closingCurve[(int)(backOpening * 2)] + (-closingCurve[(int)(backOpening * 2)] + closingCurve[(int)(backOpening * 2) + 1]) * (double)(backOpening * 2 - (int)(backOpening * 2));
                    }                   
                }
                else//处于打开曲线上
                {
                    if (backOpening * 2 - (int)(backOpening * 2) <= 0)//是整数
                    {
                        pressurePercent = openingCurve[(int)(backOpening * 2)];
                    }
                    else//当前反馈开度介于球阀两个采样数据之间
                    {
                        pressurePercent = openingCurve[(int)(backOpening * 2)] + (-openingCurve[(int)(backOpening * 2)] + openingCurve[(int)(backOpening * 2) + 1]) * (double)(backOpening * 2 - (int)(backOpening * 2));
                    }
                }
            }
            else//不明确球阀当前所处的曲线,用历史的压力百分比，即压力百分比不需要更新
            {

            }
        }
    }
}