using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Stoch
{
    public partial class HistForm : Form
    {
        Variable[] arrV;
        double[,] mData, mData2;
        int[] arrN;
        public HistForm(Variable[] arrV, double[,] mData, int[] arrI, int[] arrN)
        {
            InitializeComponent();
            this.arrV = arrV;
            this.mData = mData;
            this.arrN = arrN;
            mData2 = new double[arrI.Length, mData.GetLength(1)];
            for (int i = 0; i < arrI.Length; i++)
            {
                for (int j = 0; j < mData.GetLength(1); j++)
                {
                    mData2[i, j] = mData[arrI[i], j];
                }
            }
            for (int i = 0; i < arrV.Length; i++)
            {
                listBox1.Items.Add(arrV[i]);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
                return;
            int index = listBox1.SelectedIndex;
            SortedDictionary<double, int> dic = new SortedDictionary<double, int>();
            double min = arrV[index].arr.Min(), max = arrV[index].arr.Max(), step = (max - min) / arrN[index];
            for (int i = 0; i < arrN[index]; i++)
            {
                //dic.Add(min + (i + 0.5) * step, 0);
            }
            for (int i = 0; i < mData.GetLength(0); i++)
            {
                double key = mData[i, index];
                if (dic.ContainsKey(key))
                {
                    int n = dic[key];
                    dic.Remove(key);
                    dic.Add(key, n + 1);
                }
                else
                    dic.Add(key, 1);
            }
            chart1.Series[0].Points.Clear();
            //chart1.Series[0].Color = Color.Blue;
            chart1.ChartAreas[0].AxisX.Minimum = min;
            chart1.ChartAreas[0].AxisX.Maximum = max;
            //chart1.Series[0].BorderWidth = 6;
            for (int i = 0; i < dic.Count; i++)
            {
                chart1.Series[0].Points.AddXY(dic.Keys.ToList()[i], dic.Values.ToList()[i]);
            }

            dic = new SortedDictionary<double, int>();
            for (int i = 0; i < arrN[index]; i++)
            {
                //dic.Add(min + (i + 0.5) * step, 0);
            }
            for (int i = 0; i < mData2.GetLength(0); i++)
            {
                double key = mData2[i, index];
                if (dic.ContainsKey(key))
                {
                    int n = dic[key];
                    dic.Remove(key);
                    dic.Add(key, n + 1);
                }
                else
                    dic.Add(key, 1);
            }
            chart2.Series[0].Points.Clear();
            //chart2.Series[0].Color = Color.Blue;
            //chart2.Series[0].BorderWidth = 6;
            chart2.ChartAreas[0].AxisX.Minimum = min;
            chart2.ChartAreas[0].AxisX.Maximum = max;
            for (int i = 0; i < dic.Count; i++)
            {
                    chart2.Series[0].Points.AddXY(dic.Keys.ToList()[i], dic.Values.ToList()[i]);
            }
        }
    }
}
