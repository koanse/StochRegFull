using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Stoch
{
    public partial class ResDataForm : Form
    {
        double[,] mData;
        int uCount, iQ, iOpt;
        Variable[] arrU;
        Variable Q;
        int[] arrI;
        List<double[]> listArrE;
        List<double[]> listArrV;
        public ResDataForm(double[,] mData, int uCount, int iQ, Variable[] arrU, Variable Q, int iOpt,
            int[] arrI, List<double[]> listArrE, List<double[]> listArrV)
        {
            InitializeComponent();
            this.mData = mData;
            this.iQ = iQ;
            this.uCount = uCount;
            this.arrU = arrU;
            this.Q = Q;
            this.iOpt = iOpt;
            this.arrI = arrI;
            this.listArrE = listArrE;
            this.listArrV = listArrV;
            for (int i = 0; i < arrI.Length; i++)
            {
                string s = "( ";
                for (int j = 0; j < uCount; j++)
                {
                    s += string.Format("{0:f3} ", mData[arrI[i], j]);
                }
                s += string.Format("{0:f3} )", mData[arrI[i], iQ]);
                listBox1.Items.Add(s);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
                return;
            chart1.Series.Clear();
            Series ser = new Series();
            ser.ChartType = SeriesChartType.Line;
            ser.Color = Color.Green;
            ser.BorderWidth = 7;
            Series ser2 = new Series();
            ser2.ChartType = SeriesChartType.Line;
            ser2.Color = Color.Blue;
            ser2.BorderWidth = 7;
            for (int i = 0; i < uCount; i++)
            {
                Series s = new Series();
                s.ChartType = SeriesChartType.Line;
                s.Color = Color.Black;
                s.Points.AddXY(i, arrU[i].arr.Min()); s.Points.AddXY(i, arrU[i].arr.Max());
                s.BorderWidth = 4;
                chart1.Series.Add(s);
                ser.Points.AddXY(i, mData[iOpt, i]);
                ser2.Points.AddXY(i, mData[arrI[listBox1.SelectedIndex], i]);
                s = new Series();
                s.ChartType = SeriesChartType.Line;
                s.Color = Color.Green;
                s.BorderWidth = 3;
                s.Points.AddXY(i + 0.1, mData[iOpt, i]);
                s.Points.AddXY(i + 0.1, mData[iOpt, i] + listArrV[listBox1.SelectedIndex][i]);
                chart1.Series.Add(s);
                s = new Series();
                s.ChartType = SeriesChartType.Line;
                s.Color = Color.Red;
                s.BorderWidth = 3;
                s.Points.AddXY(i + 0.2, mData[iOpt, i] + listArrV[listBox1.SelectedIndex][i]);
                s.Points.AddXY(i + 0.2, mData[iOpt, i] + listArrV[listBox1.SelectedIndex][i] + listArrE[listBox1.SelectedIndex][i]);
                chart1.Series.Add(s);
            }
            ser.Points.AddXY(uCount + 2, mData[iOpt, iQ]);
            chart1.Series.Add(ser);
            ser = new Series();
            ser.ChartType = SeriesChartType.Line;
            ser.Color = Color.Blue;
            ser.Points.AddXY(uCount + 2, Q.arr.Min()); ser.Points.AddXY(uCount + 2, Q.arr.Max());
            ser.BorderWidth = 4;
            chart1.Series.Add(ser);
            ser2.Points.AddXY(uCount + 2, mData[arrI[listBox1.SelectedIndex], iQ]);
            chart1.Series.Add(ser2);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
