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
    public partial class DataForm : Form
    {
        double[,] mData;
        int uCount, iQ;
        Variable[] arrU;
        Variable Q;
        public DataForm(double[,] mData, int uCount, int iQ, Variable[] arrU, Variable Q)
        {
            InitializeComponent();
            this.mData = mData;
            this.iQ = iQ;
            this.uCount = uCount;
            this.arrU = arrU;
            this.Q = Q;
            for (int i = 0; i < mData.GetLength(0); i++)
            {
                string s = "( ";
                for (int j = 0; j < uCount; j++)
                {
                    s += string.Format("{0:f3} ", mData[i, j]);
                }
                s += string.Format("{0:f3} )", mData[i, iQ]);
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
            for (int i = 0; i < uCount; i++)
            {
                Series s = new Series();
                s.ChartType = SeriesChartType.Line;
                s.Color = Color.Black;
                s.Points.AddXY(i, arrU[i].arr.Min()); s.Points.AddXY(i, arrU[i].arr.Max());
                s.BorderWidth = 4;
                chart1.Series.Add(s);
                ser.Points.AddXY(i, mData[listBox1.SelectedIndex, i]);
            }
            ser.Points.AddXY(uCount + 2, mData[listBox1.SelectedIndex, iQ]);
            chart1.Series.Add(ser);
            ser = new Series();
            ser.ChartType = SeriesChartType.Line;
            ser.Color = Color.Blue;
            ser.Points.AddXY(uCount + 2, Q.arr.Min()); ser.Points.AddXY(uCount + 2, Q.arr.Max());
            ser.BorderWidth = 4;
            chart1.Series.Add(ser);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
