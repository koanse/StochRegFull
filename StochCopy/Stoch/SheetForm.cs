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
    public partial class SheetForm : Form
    {
        public string sheet;
        public SheetForm(string[] arr)
        {
            InitializeComponent();
            listBox1.Items.AddRange(arr);
        }
        void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
                return;
            sheet = (string)listBox1.SelectedItem;
            Close();
        }
    }
}
