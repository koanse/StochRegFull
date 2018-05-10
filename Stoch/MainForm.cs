using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Stoch
{
    public partial class MainForm : Form
    {
        DataTable table;
        public MainForm()
        {
            InitializeComponent();
        }
        void excelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
                return;
            try
            {
                string s = "provider = Microsoft.Jet.OLEDB.4.0;" +
                    "data source = " + openFileDialog1.FileName + ";" +
                    "extended properties = Excel 8.0;";
                OleDbConnection conn = new OleDbConnection(s);
                conn.Open();
                DataTable t = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                string[] arr = new string[t.Rows.Count];
                for (int i = 0; i < t.Rows.Count; i++)
                    arr[i] = t.Rows[i]["TABLE_NAME"].ToString();
                SheetForm sf = new SheetForm(arr);
                sf.ShowDialog();
                s = string.Format("SELECT * FROM [{0}]", sf.sheet);
                OleDbDataAdapter da = new OleDbDataAdapter(s, conn);
                table = new DataTable();
                da.Fill(table);
                conn.Close();
                arr = new string[table.Columns.Count];
                for (int i = 0; i < table.Columns.Count; i++)
                    arr[i] = table.Columns[i].Caption;
                lbDQ.Items.Clear();
                lbDU.Items.Clear();
                lbDQ.Items.AddRange(arr);
                lbDU.Items.AddRange(arr);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        void lbDQ_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                string s = (string)lbDQ.SelectedItem;
                lbQ.Items.Add(s);
                DataGridViewTextBoxColumn c = new DataGridViewTextBoxColumn();
                c.HeaderText = s;
                dgvU.Columns.Add(c);
                dgvQParam.Rows.Add(s, 10, -1, 1, 0);
            }
            catch { }
        }
        void lbDU_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                string s = (string)lbDU.SelectedItem;
                object[] arr = new object[lbQ.Items.Count + 2];
                arr[0] = dgvU.RowCount + 1;
                arr[1] = s;
                for (int i = 2; i < arr.Length; i++)
                    arr[i] = "x";
                dgvU.Rows.Add(arr);
                dgvUParam.Rows.Add(s, -1, 1, 0.1);
            }
            catch { }
        }
        void btnRefresh_Click(object sender, EventArgs e)
        {
            dgvData.Rows.Clear();
            Variable[] arrQ, arrU;
            int count = Import(out arrQ, out arrU);
            List<Variable> lVar = new List<Variable>(arrQ);
            lVar.AddRange(arrU);
            foreach (Variable v in lVar)
                dgvData.Columns.Add(v.name, v.ToString());
            for (int i = 0; i < count; i++)
            {
                string[] arr = new string[lVar.Count];
                for (int j = 0; j < arr.Length; j++)
                    arr[j] = string.Format("{0:g4}", lVar[j].arr[i]);
                dgvData.Rows.Add(arr);
            }
        }
        int Import(out Variable[] arrQ, out Variable[] arrU)
        {
            List<string> lVar = new List<string>();
            for (int i = 0; i < lbQ.Items.Count; i++)
                lVar.Add(lbQ.Items[i].ToString());
            for (int i = 0; i < dgvU.Rows.Count; i++)
                lVar.Add(dgvU[1, i].Value.ToString());
            List<int> lIndex = new List<int>();
            double tmp;
            for (int i = 0; i < table.Rows.Count; i++)
            {
                int j;
                for (j = 0; j < lVar.Count; j++)
                    if (!double.TryParse(table.Rows[i][lVar[j]].ToString(), out tmp))
                        break;
                if (j == lVar.Count)
                    lIndex.Add(i);
            }
            arrQ = new Variable[lbQ.Items.Count];
            for (int i = 0; i < arrQ.Length; i++)
            {
                string name = lbQ.Items[i].ToString();
                double[] arr = new double[lIndex.Count];
                for (int j = 0; j < arr.Length; j++)
                    arr[j] = double.Parse(table.Rows[lIndex[j]][name].ToString());
                arrQ[i] = new Variable()
                {
                    name = name,
                    id = string.Format("y{0}", i + 1),
                    arr = arr 
                };
                arrQ[i].Norm();
            }
            arrU = new Variable[dgvU.Rows.Count];
            for (int i = 0; i < arrU.Length; i++)
            {
                string name = dgvU[1, i].Value.ToString();
                double[] arr = new double[lIndex.Count];
                for (int j = 0; j < arr.Length; j++)
                    arr[j] = double.Parse(table.Rows[lIndex[j]][name].ToString());
                arrU[i] = new Variable()
                {
                    name = name,
                    id = string.Format("u{0}", i + 1),
                    arr = arr
                };
                arrU[i].Norm();
            }
            return lIndex.Count;
        }
        void btnOpt_Click(object sender, EventArgs e)
        {
            string rep = "";
            Variable[] arrQ, arrU;
            Import(out arrQ, out arrU);
            Sample[] arrSmp = new Sample[arrU.Length];
            double[][] arrCoeff = new double[arrQ.Length][];
            for (int i = 0; i < arrU.Length; i++)
                arrSmp[i] = new Sample(arrU[i].name, arrU[i].id, arrU[i].arr);
            for (int i = 0; i < arrQ.Length; i++)
            {
                Sample[] arrTSmp = new Sample[arrSmp.Length];
                for (int j = 0; j < arrTSmp.Length; j++)
                    arrTSmp[j] = new TranSample(arrSmp[j], dgvU[i + 2, j].Value.ToString());
                Sample smp = new Sample(arrQ[i].name, arrQ[i].id, arrQ[i].arr);
                Regression r = new Regression(smp, arrSmp);
                arrCoeff[i] = r.arrB;
                rep += r.GetRegReport() + "<br>";
            }
            string[] arrStage = new string[dgvStage.Rows.Count];
            for (int i = 0; i < arrStage.Length; i++)
                arrStage[i] = dgvStage[0, i].Value.ToString();
            StageForm sf = new StageForm(arrStage);
            sf.ShowDialog();
            List<string> lNU = new List<string>(), lU = new List<string>();
            int k;
            for (k = 0; k < arrStage.Length; k++)
                if (sf.stage == arrStage[k])
                    break;
                else
                    lNU.AddRange(dgvStage[1, k].Value.ToString().Split(new char[] { ' ', ',' },
                        StringSplitOptions.RemoveEmptyEntries));
            ExpForm ef = new ExpForm(lNU.ToArray(), arrU);
            ef.ShowDialog();
            for (int i = 0; i < arrU.Length; i++)
                if (!lNU.Contains(string.Format("{0}", i + 1)))
                    lU.Add(arrU[i].name);
            int p = lU.Count, n = arrQ.Length;
            int[] arrI = new int[p], arrNI = new int[arrU.Length - p];
            for (int i = 0; i < p; i++)
                for (int j = 0; j < arrU.Length; j++)
                    if (arrU[j].name == lU[i])
                    {
                        arrI[i] = j;
                        break;
                    }
            for (int i = 0; i < arrNI.Length; i++)
                for (int j = 0; j < arrU.Length; j++)
                    if (arrU[j].name == lNU[i])
                    {
                        arrNI[i] = j;
                        break;
                    }
            string[,] matrFunc = new string[p, n];
            for (int i = 0; i < p; i++)
                for (int j = 0; j < n; j++)
                    matrFunc[i, j] = dgvU[j + 2, arrI[i]].Value.ToString();
            double[][] arrC = new double[n][];
            for (int j = 0; j < n; j++)
            {
                arrC[j] = new double[p + 1];
                double s = arrCoeff[j][0];
                for (int i = 0; i < arrU.Length; i++)
                {
                    for (k = 0; k < p; k++)
                        if (arrI[k] == i)
                            break;
                    if (k < p)
                        arrC[j][k + 1] = arrCoeff[j][i + 1];
                    else
                    {
                        for (k = 0; k < p; k++)
                            if (arrNI[k] == i)
                                break;
                        s += arrCoeff[j][i + 1] * F(ef.arr[k], dgvU[j + 2, i].Value.ToString());
                    }
                }
                arrC[j][0] = s;
            }
            double[] arrUMin = new double[p], arrUMax = new double[p], arrUSMin = new double[p];
            for (int i = 0; i < p; i++)
            {
                arrUMin[i] = double.Parse(dgvUParam[1, arrI[i]].Value.ToString());
                arrUMax[i] = double.Parse(dgvUParam[2, arrI[i]].Value.ToString());
                arrUSMin[i] = double.Parse(dgvUParam[3, arrI[i]].Value.ToString());
            }
            double[] arrYOpt = new double[n], arrYMin = new double[n], arrYMax = new double[n], arrAlpha = new double[n];
            for (int i = 0; i < n; i++)
            {
                arrYOpt[i] = double.Parse(dgvQParam[4, i].Value.ToString());
                arrYMin[i] = double.Parse(dgvQParam[2, i].Value.ToString());
                arrYMax[i] = double.Parse(dgvQParam[3, i].Value.ToString());
                arrAlpha[i] = double.Parse(dgvQParam[1, i].Value.ToString());
            }
            double R = double.Parse(tbR.Text);
            HJInitialParams init = new HJInitialParams(double.Parse(tbDU.Text), double.Parse(tbDF.Text),
                arrCoeff, matrFunc, arrUMin, arrUMax, arrUSMin, arrYOpt, arrYMin, arrYMax, arrAlpha, R);
            int iterNum = 0;
            List<HJIteration> lIter = new List<HJIteration>();
            double[] arrX = new double[2 * p], arrXDelta = new double[2 * p];
            double xEps = double.Parse(tbDU.Text);
            for (int i = 0; i < 2 * p; i++)
            {
                arrX[i] = 0.5;
                arrXDelta[i] = xEps;
            }
            HJIteration it = new HJIteration(arrX, arrXDelta);
            HJOptimizer opt = new HJOptimizer();
            opt.Initialize(init);
            //double C = double.Parse(tbC.Text);
            double f = double.MaxValue, fEps = double.Parse(tbDF.Text);
            rep += "ОПТИМИЗАЦИЯ МЕТОДОМ ХУКА-ДЖИВСА<br><table border = 1 cellspacing = 0><tr>";
            for (int i = 0; i < p; i++)
            {
                rep += "<td>" + lU[i];
            }
            rep += "<td>Целевая функция";
            rep += it.ToHtml(init, 3);
            //do
            {
                do
                {
                    it = (HJIteration)opt.DoIteration(it);
                    if (it == null)
                        break;
                    lIter.Add(it);
                    iterNum++;
                    rep += it.ToHtml(init, 3);
                }
                while (iterNum < 1000);
                //double FNext = lIter.Last().fRes;
                //if (Math.Abs(FNext - f) < fEps)
                //    break;
                //f = FNext;
                //R *= C;
                //init = new HJInitialParams(double.Parse(tbDU.Text), double.Parse(tbDF.Text),
                //    arrCoeff, matrFunc, arrUMin, arrUMax, arrUSMin, arrYOpt, arrYMin, arrYMax, arrAlpha, R);
                //it = new HJIteration(lIter.Last().arrX, arrXDelta);
            }
            //while (true);
            rep += "</table><br>";
            rep += "РЕЗУЛЬТАТЫ ОПТИМИЗАЦИИ<br>";
            for (int i = 0; i < p; i++)
                rep += string.Format("m<sub>{0}</sub> = {1:f4}<br>", i + 1, lIter.Last().arrX[i]);
            Random rnd = new Random();
            for (int i = 0; i < p; i++)
                rep += string.Format("s<sub>{0}</sub> = {1:f4}<br>", i + 1, lIter.Last().arrX[i + p]);
            rep += string.Format("<br>Целевая функция:<br>F = {0:f4}<br>", init.GetFuncValue(lIter.Last().arrX));

            rep += "<br>Значения параметров распределений показателей качества<br>";
            for (int i = 0; i < n; i++)
            {
                rep += string.Format("mu<sub>{0}</sub> = {1:f4}<br>", i + 1, init.arrMu[i]);
                rep += string.Format("sigma<sub>{0}</sub> = {1:f4}<br>", i + 1, init.arrSigma[i]);
            }
            rep += "<br>Значения показателей качества<br>";
            for (int i = 0; i < n; i++)
            {
                rep += string.Format("y<sub>{0}</sub> = {1:f4}<br>", i + 1, init.arrY[i]);
            }
            rep += "<br>Технологические факторы до и после оптимизации<br>";
            rep += "Нормированные средние значения до и после оптимизации<table border = 1 cellspacing = 0><tr>" +
                "<td>Величина<td>Среднее до оптимизации<td>Среднее после оптимизации";
            for (int i = 0; i < p; i++)
            {
                rep += string.Format("<tr><td>{0}<td>{1:f4}<td>{2:f4}", arrU[i].name, 0,
                    lIter.Last().arrX[i]);
            }
            rep += "</table><br>Средние значения до и после оптимизации<table border = 1 cellspacing = 0><tr>" +
                "<td>Величина<td>Среднее до оптимизации<td>Среднее после оптимизации";
            for (int i = 0; i < p; i++)
            {
                rep += string.Format("<tr><td>{0}<td>{1:f4}<td>{2:f4}", arrU[i].name, arrU[i].Inv(0),
                    arrU[i].Inv(lIter.Last().arrX[i]));
            }
            rep += "</table><br>Нормированные средние квадратические отклонения до и после оптимизации<table border = 1 cellspacing = 0><tr>" +
                "<td>Величина<td>СКО до оптимизации<td>СКО после оптимизации";
            for (int i = 0; i < p; i++)
            {
                rep += string.Format("<tr><td>{0}<td>{1:f4}<td>{2:f4}", arrU[i].name, 1,
                    lIter.Last().arrX[p + i]);
            }
            rep += "</table><br>Средние квадратические отклонения до и после оптимизации<table border = 1 cellspacing = 0><tr>" +
                "<td>Величина<td>СКО до оптимизации<td>СКО после оптимизации";
            for (int i = 0; i < p; i++)
            {
                rep += string.Format("<tr><td>{0}<td>{1:f4}<td>{2:f4}", arrU[i].name, arrU[i].sigma,
                    arrU[i].sigma * lIter.Last().arrX[p + i]);
            }
            rep += "</table>";

            rep += "<br>Показатели качества до и после оптимизации<br>";
            rep += "Нормированные средние значения до и после оптимизации<table border = 1 cellspacing = 0><tr>" +
                "<td>Величина<td>Среднее до оптимизации<td>Среднее после оптимизации";
            for (int i = 0; i < n; i++)
            {
                rep += string.Format("<tr><td>{0}<td>{1:f4}<td>{2:f4}", arrQ[i].name, 0,
                    init.arrMu[i]);
            }
            rep += "</table><br>Средние значения до и после оптимизации<table border = 1 cellspacing = 0><tr>" +
                "<td>Величина<td>Среднее до оптимизации<td>Среднее после оптимизации";
            for (int i = 0; i < n; i++)
            {
                rep += string.Format("<tr><td>{0}<td>{1:f4}<td>{2:f4}", arrQ[i].name, arrQ[i].Inv(0),
                    arrQ[i].Inv(init.arrMu[i]));
            }
            rep += "</table><br>Нормированные средние квадратические отклонения до и после оптимизации<table border = 1 cellspacing = 0><tr>" +
                "<td>Величина<td>СКО до оптимизации<td>СКО после оптимизации";
            for (int i = 0; i < n; i++)
            {
                rep += string.Format("<tr><td>{0}<td>{1:f4}<td>{2:f4}", arrQ[i].name, 1,
                    init.arrSigma[i]);
            }
            rep += "</table><br>Средние квадратические отклонения до и после оптимизации<table border = 1 cellspacing = 0><tr>" +
                "<td>Величина<td>СКО до оптимизации<td>СКО после оптимизации";
            for (int i = 0; i < n; i++)
            {
                rep += string.Format("<tr><td>{0}<td>{1:f4}<td>{2:f4}", arrQ[i].name, arrQ[i].sigma,
                    arrQ[i].sigma * init.arrSigma[i]);
            }
            rep += "</table>";

            List<Sample> lSmp = new List<Sample>();
            for (int i = 1; i < arrQ.Length; i++)
            {
                lSmp.Add(new Sample(arrQ[i].name, string.Format("y{0}", i + 1), arrQ[i].arr));
            }
            lSmp.AddRange(arrSmp);
            Regression reg = new Regression(new Sample(arrQ[0].name, "y0", arrQ[0].arr), lSmp.ToArray());
            rep += reg.GetCorrReport();
            ResForm rf = new ResForm(rep);
            rf.Show();
            List<double[]> lArr = new List<double[]>();
            List<string> lName = new List<string>();
            List<double> lMu = new List<double>(), lSigma = new List<double>();
            for (int i = 0; i < arrU.Length; i++)
            {
                lName.Add(arrU[i].name);
                lMu.Add(arrU[i].Inv(lIter.Last().arrX[i]));
                lSigma.Add(arrU[i].sigma * lIter.Last().arrX[i + p]);
                double[] arr = new double[arrU[i].arr.Length];
                for (int j = 0; j < arr.Length; j++)
                {
                    arr[j] = arrU[i].Inv(arrU[i].arr[j]);
                }
                lArr.Add(arr);
            }
            for (int i = 0; i < arrQ.Length; i++)
            {
                lName.Add(arrQ[i].name);
                lMu.Add(arrQ[i].Inv(init.arrMu[i]));
                lSigma.Add(arrQ[i].sigma * init.arrSigma[i]);
                double[] arr = new double[arrQ[i].arr.Length];
                for (int j = 0; j < arr.Length; j++)
                {
                    arr[j] = arrQ[i].Inv(arrQ[i].arr[j]);
                }
                lArr.Add(arr);
            }
            DistForm df = new DistForm(lName.ToArray(), lArr.ToArray(), lMu.ToArray(), lSigma.ToArray());
            df.Show();
        }
        void SaveSettings()
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = new FileStream("config.ini", FileMode.Create);
            bf.Serialize(fs, table);
            SerializeLB(lbQ, fs, bf);
            SerializeLB(lbDQ, fs, bf);
            SerializeLB(lbDU, fs, bf);
            SerializeDGV(dgvU, fs, bf);
            SerializeDGV(dgvStage, fs, bf);
            SerializeDGV(dgvUParam, fs, bf);
            SerializeDGV(dgvQParam, fs, bf);
            bf.Serialize(fs, tbDF.Text);
            bf.Serialize(fs, tbDU.Text);
            bf.Serialize(fs, tbR.Text);
        }
        void LoadSettings()
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = new FileStream("config.ini", FileMode.Open);
            table = (DataTable)bf.Deserialize(fs);
            DeserializeLB(lbQ, fs, bf);
            DeserializeLB(lbDQ, fs, bf);
            DeserializeLB(lbDU, fs, bf);
            DeserializeDGV(dgvU, fs, bf);
            DeserializeDGV(dgvStage, fs, bf);
            DeserializeDGV(dgvUParam, fs, bf);
            DeserializeDGV(dgvQParam, fs, bf);
            tbDF.Text = (string)bf.Deserialize(fs);
            tbDU.Text = (string)bf.Deserialize(fs);
            tbR.Text = (string)bf.Deserialize(fs);
        }
        void SerializeDGV(DataGridView dgv, FileStream fs, BinaryFormatter bf)
        {
            bf.Serialize(fs, dgv.ColumnCount);
            for (int i = 0; i < dgv.Columns.Count; i++)
            {
                bf.Serialize(fs, dgv.Columns[i].HeaderText);
            }
            bf.Serialize(fs, dgv.RowCount);
            for (int i = 0; i < dgv.RowCount; i++)
            {
                for (int j = 0; j < dgv.ColumnCount; j++)
                {
                    bf.Serialize(fs, dgv[j, i].Value.ToString());
                }                
            }
        }
        void DeserializeDGV(DataGridView dgv, FileStream fs, BinaryFormatter bf)
        {
            dgv.Rows.Clear();
            dgv.Columns.Clear();
            int cCount = (int)bf.Deserialize(fs);
            for (int i = 0; i < cCount; i++)
            {
                string s = (string)bf.Deserialize(fs);
                DataGridViewTextBoxColumn c = new DataGridViewTextBoxColumn();
                c.HeaderText = s;
                dgv.Columns.Add(c);
            }
            dgv.RowCount = (int)bf.Deserialize(fs);
            for (int i = 0; i < dgv.RowCount; i++)
            {
                for (int j = 0; j < dgv.ColumnCount; j++)
                {
                    dgv[j, i].Value = bf.Deserialize(fs);
                }
            }
        }
        void SerializeLB(ListBox lb, FileStream fs, BinaryFormatter bf)
        {
            string[] arr = new string[lb.Items.Count];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = lb.Items[i].ToString();
            }
            bf.Serialize(fs, arr);
        }
        void DeserializeLB(ListBox lb, FileStream fs, BinaryFormatter bf)
        {
            string[] arr = (string[])bf.Deserialize(fs);
            lb.Items.Clear();
            lb.Items.AddRange(arr);
        }
        double F(double x, string func)
        {
            switch (func)
            {
                case "x":
                    return x;
                case "x^2":
                    return x * x;
                case "x^3":
                    return x * x * x;
                case "1/x":
                    return 1 / x;
                case "1/x^2":
                    return 1 / x / x;
                case "sqrt(x)":
                    return Math.Sqrt(x);
                case "1/sqrt(x)":
                    return 1 / Math.Sqrt(x);
                case "ln(x)":
                    return Math.Log(x);
                case "exp(x)":
                    return Math.Exp(x);
                default:
                    throw new Exception();
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                LoadSettings();
            }
            catch
            {
                dgvStage.Rows.Add("Передел1", "");
                tbR.Text = "1";
                tbDF.Text = "0,00001";
                tbDU.Text = "0,01";
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            switch (MessageBox.Show("Сохранить изменения?", "", MessageBoxButtons.YesNoCancel))
            {
                case DialogResult.Yes:
                    try
                    {
                        SaveSettings();
                    }
                    catch { }
                    break;
                case DialogResult.No:
                    return;
                case DialogResult.Cancel:
                    e.Cancel = true;
                    break;
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            dgvStage.Rows.Add();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            table = null;
            lbDQ.Items.Clear();
            lbDU.Items.Clear();
            lbQ.Items.Clear();
            int cCount = dgvU.ColumnCount;
            for (int i = 0; i < cCount - 2; i++)
            {
                dgvU.Columns.RemoveAt(2);
            }
            dgvU.Rows.Clear();
            dgvUParam.Rows.Clear();
            dgvQParam.Rows.Clear();
        }
    }
    [Serializable]
    public class Variable
    {
        public double av, av2, sigma;
        public string name, id;
        public double[] arr;
        public void Norm()
        {
            av = 0;
            av2 = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                av += arr[i];
                av2 += arr[i] * arr[i];
            }
            av /= arr.Length;
            av2 /= arr.Length;
            sigma = Math.Sqrt(av2 - av * av);
            for (int i = 0; i < arr.Length; i++)
                arr[i] = (arr[i] - av) / sigma;
        }
        public double Norm(double x)
        {
            return (x - av) / sigma;
        }
        public double Inv(double z)
        {
            return z * sigma + av;
        }
        public override string ToString()
        {
            return string.Format("{0}", name);// ({1}, av = {2:g4}, sigma = {3:g4})", name, id, av, sigma);
        }
    }
}
