using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MathNet.Numerics.Distributions;

namespace Stoch
{
    public partial class DistForm : Form
    {
        double[][] arrArr;
        double[] arrAv, arrSigma;
        public DistForm(string[] arrName, double[][] arrArr, double[] arrAv, double[] arrSigma)
        {
            InitializeComponent();
            listBox1.Items.AddRange(arrName);
            this.arrArr = arrArr;
            this.arrAv = arrAv;
            this.arrSigma = arrSigma;
        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int i = listBox1.SelectedIndex;
                Sample s = new Sample("", "", arrArr[i]);
                s.DoHistogram(true);
                double[] arrX = s.CloneHistX(), arrY = s.CloneHistY(), arrF = new double[arrY.Length];
                double step2 = (arrX[1] - arrX[0]) / 2, H = 0, HOpt = 0;
                NormalDistribution dist = new NormalDistribution(arrAv[i], arrSigma[i]);
                chart1.Series[0].Points.Clear();
                chart1.Series[1].Points.Clear();
                for (int j = 0; j < arrF.Length; j++)
                {
                    arrF[j] = dist.CumulativeDistribution(arrX[j] + step2) - dist.CumulativeDistribution(arrX[j] - step2);
                    arrY[j] = arrY[j] / arrArr[i].Length;
                    if (arrF[j] > 0)
                        HOpt -= arrF[j] * Math.Log(arrF[j]);
                    if (arrY[j] > 0) 
                        H -= arrY[j] * Math.Log(arrY[j]);
                    chart1.Series[0].Points.AddXY(arrX[j], arrY[j]);
                    chart1.Series[1].Points.AddXY(arrX[j], arrF[j]);
                }
                chart1.Series[0].Name = string.Format("До опт., H = {0:g5}", H);
                chart1.Series[1].Name = string.Format("После опт., H = {0:g5}", HOpt);
                chart1.Titles[0].Text = listBox1.Items[i].ToString();
            }
            catch { }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
