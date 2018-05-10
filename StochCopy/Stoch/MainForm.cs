using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;

namespace Stoch
{
    public partial class MainForm : Form
    {
        DataTable table;
        Random rnd = new Random();
        public MainForm()
        {
            InitializeComponent();
            //dgvStage.Rows.Add("Передел1", "");
            //tbN.Text = "1";
            //tbEMin.Text = "2";
            //tbDF.Text = "0,01";
            //tbEMax.Text = "0,01";
            tbN.Text = "10";
            tbEMin.Text = "-1";
            tbEMax.Text = "1";
            tbIQ.Text = "0";
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
                DataGridViewComboBoxColumn c = new DataGridViewComboBoxColumn();
                c.HeaderText = s;
                c.Items.AddRange(new string[] {
                    "x", "x^2", "x^3", "1/x", "1/x^2", "sqrt(x)", "1/sqrt(x)", "ln(x)", "exp(x)" });
                dgvU.Columns.Add(c);
                //dgvQParam.Rows.Add(s, 1, 0, 1);
            }
            catch { }
        }
        void lbDU_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                string s = (string)lbDU.SelectedItem;
                object[] arr = new object[lbQ.Items.Count + 1];
                arr[0] = s;
                for (int i = 1; i < arr.Length; i++)
                    arr[i] = "x";
                dgvU.Rows.Add(arr);
                dgvParam.Rows.Add(s, 0, 1, 0);
            }
            catch { }
        }
        void btnRefresh_Click(object sender, EventArgs e)
        {
            /*Variable[] arrQ, arrU;
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
            }*/
        }
        int Import(out Variable[] arrQ, out Variable[] arrU)
        {
            List<string> lVar = new List<string>();
            for (int i = 0; i < lbQ.Items.Count; i++)
                lVar.Add(lbQ.Items[i].ToString());
            for (int i = 0; i < dgvU.Rows.Count; i++)
                lVar.Add(dgvU[0, i].Value.ToString());
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
                string name = dgvU[0, i].Value.ToString();
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
                    arrTSmp[j] = new TranSample(arrSmp[j], dgvU[i + 1, j].Value.ToString());
                Sample smp = new Sample(arrQ[i].name, arrQ[i].id, arrQ[i].arr);
                Regression r = new Regression(smp, arrSmp);
                arrCoeff[i] = r.arrB;
            }
            string[][] arrF = new string[arrQ.Length][];
            for (int i = 0; i < arrF.Length; i++)
            {
                arrF[i] = new string[arrU.Length];
                for (int j = 0; j < arrU.Length; j++)
                {
                    arrF[i][j] = dgvU[i + 1, j].Value.ToString();
                }
            }
            int[] arrN = new int[arrQ.Length + arrU.Length];
            for (int i = 0; i < arrN.Length; i++)
            {
                arrN[i] = int.Parse(dgvParam[1, i].Value.ToString());
            }
            double[,] mData = Data(arrQ, arrU, arrN);
            int iQ = int.Parse(tbIQ.Text), N = int.Parse(tbN.Text);
            double eMin = double.Parse(tbEMin.Text), eMax = double.Parse(tbEMax.Text);
            DataForm df = new DataForm(mData, arrU.Length, iQ, arrU, arrQ[iQ]);
            df.ShowDialog();
            int iOpt = df.listBox1.SelectedIndex;
            List<double[]> listArrE = new List<double[]>(), listArrV = new List<double[]>();
            List<int> lIndex = new List<int>();
            for (int i = 0; i < N; i++)
			{
                double[] arr1, arr2;
                lIndex.Add(Calc(arrCoeff, arrF, mData, iOpt, iQ, eMin, eMax, out arr1, out arr2));
                listArrE.Add(arr1);
                listArrV.Add(arr2);
			}
            ResDataForm rf = new ResDataForm(mData, arrU.Length, iQ, arrU, arrQ[iQ], iOpt, lIndex.ToArray(), listArrE, listArrV);
            rf.ShowDialog();
            List<Variable> listV = new List<Variable>(arrU);
            listV.AddRange(arrQ);
            HistForm hf = new HistForm(listV.ToArray(), mData, lIndex.ToArray(), arrN);
            hf.ShowDialog();

            /*string rep = "";
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
                    arrTSmp[j] = new TranSample(arrSmp[j], dgvU[i + 1, j].Value.ToString());
                Sample smp = new Sample(arrQ[i].name, arrQ[i].id, arrQ[i].arr);
                Regression r = new Regression(smp, arrSmp);
                arrCoeff[i] = r.arrB;
                rep += r.GetRegReport() + "<br>";
            }
            string[] arrStage = new string[dgvStage.Rows.Count - 1];
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
            ExpForm ef = new ExpForm(lNU.ToArray());
            ef.ShowDialog();
            for (int i = 0; i < arrU.Length; i++)
                if (!lNU.Contains(arrU[i].name))
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
                    matrFunc[i, j] = dgvU[j + 1, arrI[i]].Value.ToString();            
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
                        s += arrCoeff[j][i + 1] * F(ef.arr[k], dgvU[j + 1, i].Value.ToString());
                    }
                }
                arrC[j][0] = s;
            }
            double[] arrUMin = new double[p], arrUMax = new double[p], arrUSMin = new double[p];
            for (int i = 0; i < p; i++)
            {
                arrUMin[i] = double.Parse(dgvParam[1, arrI[i]].Value.ToString());
                arrUMax[i] = double.Parse(dgvParam[2, arrI[i]].Value.ToString());
                arrUSMin[i] = double.Parse(dgvParam[3, arrI[i]].Value.ToString());
            }
            double[] arrMuOpt = new double[n], arrSigmaOpt = new double[n], arrAlpha = new double[n];
            for (int i = 0; i < n; i++)
            {
                arrMuOpt[i] = double.Parse(dgvQParam[1, i].Value.ToString());
                arrSigmaOpt[i] = double.Parse(dgvQParam[2, i].Value.ToString());
                arrAlpha[i] = double.Parse(dgvQParam[3, i].Value.ToString());
            }
            double R = double.Parse(tbN.Text);
            HJInitialParams init = new HJInitialParams(double.Parse(tbEMax.Text), double.Parse(tbDF.Text),
                arrCoeff, matrFunc, arrUMin, arrUMax, arrUSMin, arrMuOpt, arrSigmaOpt, arrAlpha, R);
            int iterNum = 0;
            List<HJIteration> lIter = new List<HJIteration>();
            double[] arrX = new double[2 * p], arrXDelta = new double[2 * p];
            double xEps = double.Parse(tbEMax.Text);
            for (int i = 0; i < 2 * p; i++)
			{
                arrX[i] = 0.5;
                arrXDelta[i] = xEps;
			}
            HJIteration it = new HJIteration(arrX, arrXDelta);
            HJOptimizer opt = new HJOptimizer();
            opt.Initialize(init);
            double C = double.Parse(tbEMin.Text);
            double f = double.MaxValue, fEps = double.Parse(tbDF.Text);
            rep += "ОПТИМИЗАЦИЯ МЕТОДОМ ХУКА-ДЖИВСА<br>";
            do
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
                while (iterNum < 100);
                double FNext = lIter.Last().fRes;
                if (Math.Abs(FNext - f) < fEps)
                    break;
                f = FNext;
                R *= C;
                init = new HJInitialParams(double.Parse(tbEMax.Text), double.Parse(tbDF.Text),
                    arrCoeff, matrFunc, arrUMin, arrUMax, arrUSMin, arrMuOpt, arrSigmaOpt, arrAlpha, R);
                it = new HJIteration(lIter.Last().arrX, arrXDelta);
            }
            while (true);
            rep += "РЕЗУЛЬТАТЫ ОПТИМИЗАЦИИ<br>";
            for (int i = 0; i < p; i++)
                rep += string.Format("m{0} = {1:g4}<br>", i + 1, lIter.Last().arrX[i]);
            Random rnd = new Random();
            for (int i = 0; i < p; i++)
                rep += string.Format("s{0} = {1:g4}<br>", i + 1, Math.Abs(lIter.Last().arrX[i + p]));
            rep += string.Format("<br>Целевая функция:<br>F = {0:g4}", init.GetFuncValue(lIter.Last().arrX));//rnd.Next(100) / 100.0);
            
            rep += "<br>Значения параметров распределений показателей качества<br>";
            for (int i = 0; i < n; i++)
            {
                rep += string.Format("mu{0} = {1:g4}<br>", i + 1, arrMuOpt[i] + rnd.Next(0, 100) / 1000.0);
                rep += string.Format("sigma{0} = {1:g4}<br>", i + 1, arrSigmaOpt[i] + rnd.Next(0, 100) / 1000.0);
            }
            ResForm rf = new ResForm(rep);
            rf.Show();*/
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
        double f(double x, string func)
        {
            switch (func)
            {
                case "x":
                    return 1;
                case "x^2":
                    return 2 * x;
                case "x^3":
                    return 3 * x * x;
                case "1/x":
                    return - 1 / x;
                case "1/x^2":
                    return -2 / x / x / x;
                case "sqrt(x)":
                    return 1 / 2 / Math.Sqrt(x);
                case "1/sqrt(x)":
                    return - 1 / 2 / x / Math.Sqrt(x);
                case "ln(x)":
                    return 1 / x;
                case "exp(x)":
                    return Math.Exp(x);
                default:
                    throw new Exception();
            }
        }
        double[,] Data(Variable[] arrQ, Variable[] arrU, int[] arrN)
        {
            double[,] mData = new double[arrQ[0].arr.Length, arrQ.Length + arrU.Length];
            for (int j = 0; j < arrU.Length; j++)
            {
                double min = arrU[j].arr.Min(), max = arrU[j].arr.Max(), step = (max - min) / arrN[j];
                for (int i = 0; i < arrU[j].arr.Length; i++)
                {
                    int index = (int)((arrU[j].arr[i] - min) / step);
                    if (index == arrN[j])
                        index--;
                    mData[i, j] = min + index * step + step / 2;
                }
            }
            for (int j = 0; j < arrQ.Length; j++)
            {
                double min = arrQ[j].arr.Min(), max = arrQ[j].arr.Max(), step = (max - min) / arrN[j + arrU.Length];
                for (int i = 0; i < arrQ[j].arr.Length; i++)
                {
                    int index = (int)((arrQ[j].arr[i] - min) / step);
                    if (index == arrN[j + arrU.Length])
                        index--;
                    mData[i, j + arrU.Length] = min + index * step + step / 2;
                }
            }
            return mData;
        }
        int Calc(double[][] arrCoeff, string[][] arrF, double[,] mData, int iOpt, int iQ, double eMin, double eMax,
            out double[] arrE, out double[] arrV)
        {
            List<int> lIndex = new List<int>();
            List<double> lZ = new List<double>(), lE = new List<double>(), lV = new List<double>();
            for (int i = 0; i < mData.GetLength(0); i++)
            {
                if (mData[i, 0] == mData[iOpt, 0])
                    lIndex.Add(i);
            }
            for (int i = 0; i < arrCoeff[0].Length - 1; i++)
            {
                double s = 0;
                for (int j = 0; j < lZ.Count; j++)
                {
                    double du = lZ[j] - mData[iOpt, j], uav = (lZ[j] + mData[iOpt, j]) / 2;
                    s += arrCoeff[iQ][j + 1] * f(uav, arrF[iQ][j]) * du;
                }
                double v = -s / (arrCoeff[iQ][i + 1] * f(mData[iOpt, i], arrF[iQ][i]));
                double e = eMin + rnd.NextDouble() * eMax;
                double z = mData[iOpt, i] + v + e;
                List<double> lZValues = new List<double>();
                for (int j = 0; j < lIndex.Count; j++)
                {
                    if (!lZValues.Contains(mData[lIndex[j], i]))
                        lZValues.Add(mData[lIndex[j], i]);
                }
                double zNext = z, min = double.MaxValue;
                for (int j = 0; j < lZValues.Count; j++)
                {
                    if (Math.Abs(lZValues[j] - z) < min)
                    {
                        min = Math.Abs(lZValues[j] - z);
                        zNext = lZValues[j];
                    }
                }
                List<int> lRemove = new List<int>();
                for (int j = 0; j < lIndex.Count; j++)
                {
                    if (mData[lIndex[j], i] != zNext)
                        lRemove.Add(j);
                }
                lRemove.Sort();
                lRemove.Reverse();
                for (int j = 0; j < lRemove.Count; j++)
                {
                    lIndex.RemoveAt(lRemove[j]);
                }
                lZ.Add(zNext); lE.Add(e); lV.Add(v);
            }
            arrE = lE.ToArray(); arrV = lV.ToArray();
            return lIndex[0];
        }
        private void btnRefresh_Click_1(object sender, EventArgs e)
        {
            Variable[] arrQ, arrU;
            Import(out arrQ, out arrU);
            List<Variable> lVar = new List<Variable>(arrU);
            lVar.AddRange(arrQ);
            dgvParam.Rows.Clear();           
            for (int i = 0; i < lVar.Count; i++)
            {
                dgvParam.Rows.Add(lVar[i].name, 5);
            }
        }
    }
    public class Variable
    {
        double av, av2, sigma;
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
            return string.Format("{0} ({1}, av = {2:g4}, sigma = {3:g4})", name, id, av, sigma);
        }
    }
}
