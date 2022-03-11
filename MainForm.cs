using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace PressureControlSystem
{
    public partial class MainForm : Form
    {            
        public bool autoMode = false;
        public static MainForm mainForm;
        private int borderWidth = 2;
        public bool allowUpdataChart = true;
        private bool updataChartFinished = true;


        public static int[] SolenoidValueCounters = new int[18];
        public int autoControlMethods = 2;//0：PID反馈控制，1：基于特性曲线的反馈控制，2：简单开度控制

        private DateTime currentTime;


        public static int valvePeriod = 50;//球阀周期
        public static int controlPeriod = 250;//控制周期
        public static double totalPressure = 1.2;//总管压力
        public static double minPressure = 0.57;//最小压力
        public static double maxPressure = 0.63;//最大压力      
        public ValveModel Valve0 = new ValveModel();
        int chartExistTime = 30;//绘制30秒的
        private int queueLength = 1000 / 250 * 30;
        internal static double Kp = 50;
        internal static double Ki = 35;
        internal static double Kd = 10;
        Random rd = new Random();
        PIDAutoModule pIDAutoModule = new PIDAutoModule();
        SingleScaleAutoModule singleScaleAutoModule = new SingleScaleAutoModule();
        CurveAutoModule curveAutoModule = new CurveAutoModule();

        public MainForm()
        {
            InitializeComponent();
            mainForm = this;
            valveTimer.Start();
            controlTimer.Start();
            button1.PerformClick();
        }       

        private void button5_Click(object sender, EventArgs e)
        {
            allowUpdataChart = false;
            ChartInitialize(chart1, 0);
        }
        private void ChartInitialize(Chart chart, int ID)
        {
            chart.ChartAreas.Clear();
            chart.Titles.Clear();
            chart.Series.Clear();
            chart.Legends.Clear();

            ChartArea chartArea1 = new ChartArea("C1");
            chart.ChartAreas.Add(chartArea1);
            chart.Titles.Add("仿真");

            chart.Titles[0].DockedToChartArea = "C1";

            Series pressure = new Series("压力");
            pressure.ChartArea = "C1";
            chart.Series.Add(pressure);

            Series opening = new Series("开度");
            opening.ChartArea = "C1";
            chart.Series.Add(opening);

            Legend pressureL = new Legend("压力");
            Legend openingL = new Legend("开度");

            chart.Legends.Add(pressureL);
            chart.Legends.Add(openingL);

            chart.Legends[0].DockedToChartArea = "C1";
            chart.Legends[1].DockedToChartArea = "C1";
            chart.Legends[0].BackColor = Color.Transparent;
            chart.Legends[1].BackColor = Color.Transparent;

            chart.Series[0].ChartType = SeriesChartType.FastLine;
            chart.Series[1].ChartType = SeriesChartType.FastLine;
            chart.Series[0].Color = Color.Blue;//压力为蓝色
            chart.Series[1].Color = Color.Red;//开度红色
            chart.Series[0].XValueType = ChartValueType.Time;
            chart.Series[1].XValueType = ChartValueType.Time;

            chart.Series[0].BorderWidth = borderWidth;
            chart.Series[1].BorderWidth = borderWidth;

            //设置主副轴，氧气为主轴，燃气为副轴
            chart.ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;
            chart.Series[1].YAxisType = AxisType.Secondary;

            //设置刻度大小
            //if (ID == 0 || ID == 3 || ID == 6 || ID == 9 || ID == 12 || ID == 15)
            //{
            //    chart.ChartAreas[0].AxisY.Minimum = 0.9;
            //    chart.ChartAreas[0].AxisY.Maximum = 1.4;
            //    chart.ChartAreas[0].AxisY.Interval = 0.02;//切割氧压力
            //}
            chart.ChartAreas[0].AxisY.Minimum = 0;
            chart.ChartAreas[0].AxisY.Maximum = 1.3;
            chart.ChartAreas[0].AxisY.Interval = 0.05;//切割氧压力

            chart.ChartAreas[0].AxisY2.Minimum = 0;
            chart.ChartAreas[0].AxisY2.Maximum = 50;
            chart.ChartAreas[0].AxisY2.Interval = 5;//开度

            chart.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Seconds;
            //chart.ChartAreas[0].AxisX.Interval = 5;
            chart.ChartAreas[0].AxisX.Interval = 0;

            //设置样式
            chart.ChartAreas[0].BackColor = Color.Transparent;
            chart.ChartAreas[0].Axes[3].MajorGrid.Enabled = false;
            chart.ChartAreas[0].Axes[0].MajorGrid.Enabled = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //加载图表                      
            valveTimer.Start();
            button5.PerformClick();
            allowUpdataChart = true;
            queueLength = 1000 / controlPeriod * chartExistTime;
        }
        private void UpdataChartFunc(ChartData chartData, int TypeNumber, Chart chart)
        {
            if (chart.InvokeRequired)//线程外调用该空间时，InvokeRequired属性为True
            {
                Action<ChartData, int, Chart> updataAction = new Action<ChartData, int, Chart>(UpdataChartAction);
                chart.Invoke(updataAction, chartData, TypeNumber, chart);
            }
            else
            {
                UpdataChartAction(chartData, TypeNumber, chart);
            }
        }
        /// <summary>
        /// 用于更新图表的函数，需要被封装以实现跨线程使用
        /// type = 0为压力，type = 1为开度
        /// </summary>
        private void UpdataChartAction(ChartData chartData, int type, Chart chart)
        {
            try
            {
                if (updataChartFinished)
                {
                    updataChartFinished = false;
                    chart.ChartAreas[0].AxisX.Minimum = DateTime.Now.AddSeconds(-chartExistTime).ToOADate();
                    chart.ChartAreas[0].AxisX.Maximum = DateTime.Now.ToOADate();
                    chart.Series[type].Points.AddXY(chartData.currentTime.ToOADate(), chartData.data);
                    if (chart.Series[type].Points.Count > queueLength)
                    {
                        chart.Series[type].Points.RemoveAt(0);
                    }
                    updataChartFinished = true;
                }

            }
            catch (Exception)
            {
                updataChartFinished = true;
            }
        }       
        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1.PerformClick();
            }
        }

        
        private void timer1_Tick(object sender, EventArgs e)
        {
            //是否允许绘图
            if (allowUpdataChart)
            {
                ChartData chartData;
                chartData.currentTime = DateTime.Now;
                chartData.data = totalPressure * Valve0.pressurePercent;//支路压力信息
                UpdataChartFunc(chartData, 0, chart1);

                ChartData chartData1;
                chartData1.currentTime = DateTime.Now;
                chartData1.data = (double)numericUpDown1.Value;//球阀开度信息
                UpdataChartFunc(chartData1, 1, chart1);
            }
            if (radioButton1.Checked)
            {
                pidSettingVisible(true);
            }
            else if (radioButton5.Checked)
            {
                pidSettingVisible(false);
            }
            else if (radioButton4.Checked)
            {
                pidSettingVisible(false);
            }
            //是否启动自动模式
            if (autoMode)
            {
                if (radioButton1.Checked)
                {
                    decimal newOpening = pIDAutoModule.PressureSteady(numericUpDown1.Value, totalPressure * Valve0.pressurePercent,Valve0.error);
                    if (newOpening != -20)
                    {
                        if (newOpening >= numericUpDown1.Minimum && newOpening <= numericUpDown1.Maximum)
                        {
                            numericUpDown1.Value = newOpening;
                        }          
                    }
                }
                else if (radioButton5.Checked)
                {
                    decimal newOpening = singleScaleAutoModule.PressureSteady(numericUpDown1.Value, totalPressure * Valve0.pressurePercent, Valve0.error);
                    if (newOpening != -10)
                    {
                        if (newOpening >= numericUpDown1.Minimum && newOpening <= numericUpDown1.Maximum)
                        {
                            numericUpDown1.Value = newOpening;
                        }
                    }
                }
                else if (radioButton4.Checked)
                {
                    decimal newOpening = curveAutoModule.PressureSteady(numericUpDown1.Value, Valve0.backOpening, totalPressure * Valve0.pressurePercent, Valve0.error);
                    if (newOpening != -10)
                    {
                        if (newOpening >= numericUpDown1.Minimum && newOpening <= numericUpDown1.Maximum)
                        {
                            numericUpDown1.Value = newOpening;
                        }
                    }
                }
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {         
            totalPressure = double.Parse(textBox1.Text);
            minPressure = double.Parse(textBox2.Text);
            maxPressure = double.Parse(textBox3.Text);
            controlPeriod = int.Parse(textBox4.Text);
            //设置定时器的周期
            controlTimer.Stop();
            controlTimer.Interval = controlPeriod;
            controlTimer.Start();
            Valve0.adjustSpeed = decimal.Parse(textBox5.Text);
            queueLength = 1000 / controlPeriod * chartExistTime;
            valvePeriod = int.Parse(textBox6.Text);
            //设置球阀时钟的周期
            valveTimer.Stop();
            valveTimer.Interval = valvePeriod;
            valveTimer.Start();
        }

        private void 球阀时钟_Tick(object sender, EventArgs e)
        {
            label11.Text = (totalPressure * Valve0.pressurePercent).ToString();
            label17.Text = Valve0.backOpening.ToString();   
            label15.Text = Valve0.direction.ToString();
            label21.Text = Valve0.histMove.ToString();
            label28.Text = curveAutoModule.totalPressure.ToString();
            label30.Text = curveAutoModule.coarse.ToString();
            Valve0.ValveMove(numericUpDown1.Value);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                autoMode = true;
            }
            else
            {
                autoMode = false;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            allowUpdataChart = true;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            allowUpdataChart = false;
        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {
            if (!double.TryParse(textBox1.Text, out Kp))
            {
                MessageBox.Show("请输入正确的双精度类型数据");
            }           
        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            if (!double.TryParse(textBox2.Text, out Ki))
            {
                MessageBox.Show("请输入正确的双精度类型数据");
            }
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            if (!double.TryParse(textBox3.Text, out Kd))
            {
                MessageBox.Show("请输入正确的双精度类型数据");
            }
        }
        private void pidSettingVisible(bool visible)
        {
            label24.Visible = visible;
            label25.Visible = visible;
            label26.Visible = visible;
            textBox7.Visible = visible;
            textBox8.Visible = visible;
            textBox9.Visible = visible;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                randomNoise.Start();
            }
            else
            {
                randomNoise.Stop();
            }
        }

        private void randomNoise_Tick(object sender, EventArgs e)
        {
            totalPressure += rd.NextDouble() / 2 - 0.25;           
            if (totalPressure <= maxPressure)
            {
                totalPressure = maxPressure;
            }
            textBox1.Text = totalPressure.ToString();
            button1.PerformClick();
        }

    }
}

