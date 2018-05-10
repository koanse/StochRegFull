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
    public partial class StageForm : Form
    {
        public string stage;
        public StageForm(string[] arr)
        {
            InitializeComponent();
            listBox1.Items.AddRange(arr);
        }
        void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
                return;
            stage = listBox1.SelectedItem.ToString();
            Close();
        }
    }
}
