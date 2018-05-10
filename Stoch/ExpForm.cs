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
    public partial class ExpForm : Form
    {
        public double[] arr;
        public ExpForm(string[] arrI, Variable[] arrU)
        {
            InitializeComponent();
            for (int i = 0; i < arrI.Length; i++)
            {
                int j = int.Parse(arrI[i]) - 1;
                dgv.Rows.Add(arrU[j], 0);
            }
        }
        void button1_Click(object sender, EventArgs e)
        {
            try
            {
                arr = new double[dgv.Rows.Count];
                for (int i = 0; i < arr.Length; i++)
                    arr[i] = double.Parse(dgv[1, i].Value.ToString());
                Close();
            }
            catch
            {
                MessageBox.Show("Ошибка ввода");
            }
        }
    }
}
