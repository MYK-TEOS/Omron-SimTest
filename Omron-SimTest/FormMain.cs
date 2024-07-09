using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Omron_SimTest
{

    public partial class FormMain : Form
    {
        #region 定数
        static readonly int iMachineNum = 31;
        static int iTimerCount = 0;
        readonly Random rnd = new Random();

        const string dn_NO = "番号";
        const string dn_ENABLE = "有効";
        const string dn_Particle_little = "小粒子";
        const string dn_Particle_Middle = "中粒子";
        const string dn_Particle_Big = "大粒子";
        const string dn_Temp = "温度";
        const string dn_Con = "湿度";
        const string dn_DpTemp = "露点湿度";
        const string dn_Port = "アドレス末尾";
        const string dn_Error_Hard = "ハードエラー";
        const string dn_Error_Memory = "メモリエラー";
        const string dn_Mode_Run = "RUN";
        const string dn_Mode_Thr = "THR";
        const string dn_Mode_Fun = "FUN";
        const string dn_ID_TempEnable = "温湿度センサ有";
        const string dn_ID_PD50 = "PD50";
        const string dn_Error_From = "From異常";
        const string dn_UnCalc = "未計測";
        #endregion

        #region 変数

        DataSet _ds;
        DataTable _dt;
        private static readonly Object thisLock = new Object();
        static readonly StringBuilder sbLog = new StringBuilder();

        public struct MData
        {
            public int iParticle_little;        //小粒子
            public int iParticle_Middle;      //中粒子
            public int iParticle_Big;    //大粒子
            public int iTemp;    //温度
            public int iCon;    //湿度
            public int iDpTemp;    //露点湿度
            public int iPort;    //アドレス末尾


            public bool bError_Hard;    // ハードエラー
            public bool bError_Memory;  // メモリエラー
            public bool bMode_Run;      // RUN
            public bool bMode_Thr;      // THR
            public bool bMode_Fun;      // FUN
            public bool bID_TempEnable;  // 温湿度センサ有
            public bool bID_PD50;       // PD50
            public bool bError_From;    // From異常
            public bool bEnable;        // 有効
            public bool bUnCalc;        // 未計測

        }

        public static MData[] stData = new MData[iMachineNum];
        readonly List<ConnectClass> clList;

        static bool bDataChange;

        #endregion


        public FormMain()
        {
            InitializeComponent();
            clList = new List<ConnectClass>();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            bDataChange = false;
            for (int i = 0; i < iMachineNum; i++)
            {
                stData[i].bEnable = true;
                stData[i].iPort = 150 + i;
                stData[i].bMode_Run = true;
            }

            ConnectClass cl = new ConnectClass
            {
                PortRead = 2323,
                Node = 0
            };
            clList.Add(cl);

            for (int i = 0; i < 60; i++)
            {
                comboBox1.Items.Add(i + 1);
            }
            comboBox1.SelectedIndex = 60 - 1;

            // データグリッド設定
            Datainit();
            timer1.Interval = 1000;
            timer1.Start();
        }

        void Datainit()
        {
            _ds = new DataSet("PLCSetting");
            _dt = _ds.Tables.Add("PLCTable");

            _dt.Columns.Add(dn_NO, Type.GetType(Consts.TYPE_INT));
            _dt.Columns.Add(dn_ENABLE, Type.GetType(Consts.TYPE_BOL));
            _dt.Columns.Add(dn_Particle_little, Type.GetType(Consts.TYPE_INT));
            _dt.Columns.Add(dn_Particle_Middle, Type.GetType(Consts.TYPE_INT));
            _dt.Columns.Add(dn_Particle_Big, Type.GetType(Consts.TYPE_INT));
            _dt.Columns.Add(dn_Temp, Type.GetType(Consts.TYPE_INT));
            _dt.Columns.Add(dn_Con, Type.GetType(Consts.TYPE_INT));
            _dt.Columns.Add(dn_DpTemp, Type.GetType(Consts.TYPE_INT));
            _dt.Columns.Add(dn_Port, Type.GetType(Consts.TYPE_INT));
            _dt.Columns.Add(dn_Error_Hard, Type.GetType(Consts.TYPE_BOL));
            _dt.Columns.Add(dn_Error_Memory, Type.GetType(Consts.TYPE_BOL));
            _dt.Columns.Add(dn_Mode_Run, Type.GetType(Consts.TYPE_BOL));
            _dt.Columns.Add(dn_Mode_Thr, Type.GetType(Consts.TYPE_BOL));
            _dt.Columns.Add(dn_Mode_Fun, Type.GetType(Consts.TYPE_BOL));

            _dt.Columns.Add(dn_ID_TempEnable, Type.GetType(Consts.TYPE_BOL));
            _dt.Columns.Add(dn_ID_PD50, Type.GetType(Consts.TYPE_BOL));
            _dt.Columns.Add(dn_Error_From, Type.GetType(Consts.TYPE_BOL));
            _dt.Columns.Add(dn_UnCalc, Type.GetType(Consts.TYPE_BOL));


            DataRow dr;
            for (int i = 0; i < iMachineNum; i++)
            {
                dr = _dt.Rows.Add();

                dr[dn_NO] = i + 1;
                dr[dn_ENABLE] = stData[i].bEnable;
                dr[dn_Particle_little] = stData[i].iParticle_little;
                dr[dn_Particle_Middle] = stData[i].iParticle_Middle;
                dr[dn_Particle_Big] = stData[i].iParticle_Big;

                dr[dn_Temp] = stData[i].iTemp;
                dr[dn_Con] = stData[i].iCon;
                dr[dn_DpTemp] = stData[i].iDpTemp;
                dr[dn_Port] = stData[i].iPort;
                dr[dn_Error_Hard] = stData[i].bError_Hard;
                dr[dn_Error_Memory] = stData[i].bError_Memory;
                dr[dn_Mode_Run] = stData[i].bMode_Run;
                dr[dn_Mode_Thr] = stData[i].bMode_Thr;
                dr[dn_Mode_Fun] = stData[i].bMode_Fun;

                dr[dn_ID_TempEnable] = stData[i].bID_TempEnable;
                dr[dn_ID_PD50] = stData[i].bID_PD50;
                dr[dn_Error_From] = stData[i].bError_From;
                dr[dn_UnCalc] = stData[i].bUnCalc;
            }

            dataGridView1.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Raised;
            dataGridView1.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
            dataGridView1.ColumnHeadersHeight = dataGridView1.Size.Height;
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            dataGridView1.DataSource = _dt;
            dataGridView1.Columns[dn_NO].ReadOnly = true;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            try
            {

                button1.Enabled = false;
                foreach (var cl in clList)
                {
                    lock (thisLock)
                    {
                        LogWrite(DateTime.Now.ToLongTimeString() + "  : ポート" + cl.PortRead + "をオープンしました。" + "\r\n");
                        //sbLog.Append(DateTime.Now.ToLongTimeString() + "  : ポート" + cl.PortRead + "をオープンしました。" + "\r\n");
                        //textBox1.Text = sbLog.ToString();
                    }
                    cl.StartSocket();
                }
            }
            catch (Exception ex)
            {

                lock (thisLock)
                {
                    LogWrite(ex.Message + "\r\n");
                    //sbLog.Append(ex.Message + "\r\n");
                    //textBox1.Text = sbLog.ToString();
                }
                foreach (var cl in clList)
                {
                    cl.StopSocket();
                }

            }
        }

        public static void LogWrite(string s)
        {
            lock (thisLock)
            {
                sbLog.Append(s + "\r\n");
            }
        }




        /// <summary>
        /// 値変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            DataRow dr;
            DataGridView dgv = (DataGridView)sender;
            int x = e.ColumnIndex;
            int y = e.RowIndex;
            switch (dgv.Columns[x].Name)
            {
                case dn_Particle_little:
                    stData[y].iParticle_little = (int)dgv[x, y].Value;
                    break;
                case dn_Particle_Middle:
                    stData[y].iParticle_Middle = (int)dgv[x, y].Value;
                    break;
                case dn_Particle_Big:
                    stData[y].iParticle_Big = (int)dgv[x, y].Value;
                    break;
                case dn_Port:
                    stData[y].iPort = (int)dgv[x, y].Value;
                    break;
                case dn_Error_Hard:
                    stData[y].bError_Hard = (bool)dgv[x, y].Value;
                    if (stData[y].bError_Hard == false) break;
                    if (stData[y].bError_Memory == true)
                    {
                        _dt.Rows[y][dn_Error_Memory] = false;
                        stData[y].bError_Memory = false;
                    }
                    break;
                case dn_Error_Memory:
                    stData[y].bError_Memory = (bool)dgv[x, y].Value;
                    if (stData[y].bError_Memory == false) break;
                    if (stData[y].bError_Hard == true)
                    {
                        _dt.Rows[y][dn_Error_Hard] = false;
                        stData[y].bError_Hard = false;
                    }
                    break;
                case dn_Mode_Run:
                    stData[y].bMode_Run = (bool)dgv[x, y].Value;
                    if (stData[y].bMode_Run == false) break;
                    if (stData[y].bMode_Thr == true || stData[y].bMode_Fun == true)
                    {
                        dr = _dt.Rows[y];
                        dr[dn_Mode_Thr] = false;
                        dr[dn_Mode_Fun] = false;
                        stData[y].bMode_Thr = false;
                        stData[y].bMode_Fun = false;
                    }
                    break;
                case dn_Mode_Thr:
                    stData[y].bMode_Thr = (bool)dgv[x, y].Value;
                    if (stData[y].bMode_Thr == false) break;
                    if (stData[y].bMode_Run == true || stData[y].bMode_Fun == true)
                    {
                        dr = _dt.Rows[y];
                        dr[dn_Mode_Run] = false;
                        dr[dn_Mode_Fun] = false;
                        stData[y].bMode_Run = false;
                        stData[y].bMode_Fun = false;
                    }
                    break;
                case dn_Mode_Fun:
                    stData[y].bMode_Fun = (bool)dgv[x, y].Value;
                    if (stData[y].bMode_Fun == false) break;
                    if (stData[y].bMode_Thr == true || stData[y].bMode_Run == true)
                    {
                        dr = _dt.Rows[y];
                        dr[dn_Mode_Thr] = false;
                        dr[dn_Mode_Run] = false;
                        stData[y].bMode_Thr = false;
                        stData[y].bMode_Run = false;
                    }
                    break;
                case dn_ENABLE:
                    stData[y].bEnable = (bool)dgv[x, y].Value;
                    break;


                case dn_Temp:
                    stData[y].iTemp = (int)dgv[x, y].Value;
                    break;
                case dn_Con:
                    stData[y].iCon = (int)dgv[x, y].Value;
                    break;
                case dn_DpTemp:
                    stData[y].iDpTemp = (int)dgv[x, y].Value;
                    break;


                case dn_ID_TempEnable:
                    stData[y].bID_TempEnable = (bool)dgv[x, y].Value;
                    break;

                case dn_ID_PD50:
                    stData[y].bID_PD50 = (bool)dgv[x, y].Value;
                    break;

                case dn_Error_From:
                    stData[y].bError_From = (bool)dgv[x, y].Value;
                    break;
                case dn_UnCalc:
                    stData[y].bUnCalc = (bool)dgv[x, y].Value;
                    break;

            }
            dataGridView1.Invalidate();
            dataGridView1.Update();
        }

        private void ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count == 0) return;
            if (dataGridView1.SelectedCells.IsReadOnly) return;
            DialogValueIO diag = new DialogValueIO
            {
                _cells = dataGridView1.SelectedCells
            };
            diag.ShowDialog();
        }

        /// <summary>
        /// タイマー処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer1_Tick(object sender, EventArgs e)
        {
            DataRow dr;

            if (checkBox1.Checked == true)
            {
                // ログ処理
                if (textBox1.TextLength != sbLog.Length)
                {
                    string sTmp;
                    lock (thisLock)
                    {
                        sTmp = sbLog.ToString();
                    }
                    string sTmp_textbox = textBox1.Text;

                    int iLength = sTmp.Length - sTmp_textbox.Length;
                    if (iLength > 0)
                    {
                        string sOut = sTmp.Substring(sTmp_textbox.Length, sTmp.Length - sTmp_textbox.Length);
                        textBox1.AppendText(sOut);
                        if (textBox1.TextLength == sTmp_textbox.Length)
                        {
                            sbLog.Clear();
                            textBox1.Text = "";
                        }
                    }
                    else
                    {
                        sbLog.Clear();
                        textBox1.Text = "";
                    }

                    //カレット位置を末尾に移動
                    this.textBox1.SelectionStart = textBox1.Text.Length;
                    //カレット位置までスクロール
                    this.textBox1.ScrollToCaret();
                }
            }
            // 値のランダム更新
            if (checkBox2.Checked)
            {
                iTimerCount++;

                if (iTimerCount >= comboBox1.SelectedIndex)
                {
                    iTimerCount = 0;
                    //60秒毎に値を変化させる
                    for (int i = 0; i < iMachineNum; i++)
                    {
                        stData[i].iParticle_little = rnd.Next(10000);
                        stData[i].iParticle_Middle = rnd.Next(10000);
                        stData[i].iParticle_Big = rnd.Next(10000);
                        stData[i].iTemp = 200 + rnd.Next(150);
                        stData[i].iCon = 100 + rnd.Next(900);
                        stData[i].iDpTemp = 50 + rnd.Next(200);
                    }
                    bDataChange = true;
                }
            }

            // 画面更新
            if (bDataChange == true)
            {
                for (int i = 0; i < iMachineNum; i++)
                {
                    dr = _dt.Rows[i];

                    dr[dn_Particle_little] = stData[i].iParticle_little;
                    dr[dn_Particle_Middle] = stData[i].iParticle_Middle;
                    dr[dn_DpTemp] = stData[i].iDpTemp;

                    dr[dn_Temp] = stData[i].iTemp;
                    dr[dn_Con] = stData[i].iCon;
                    dr[dn_Particle_Big] = stData[i].iParticle_Big;

                    dr[dn_Port] = stData[i].iPort;
                    dr[dn_Error_Hard] = stData[i].bError_Hard;
                    dr[dn_Error_Memory] = stData[i].bError_Memory;
                    dr[dn_Mode_Run] = stData[i].bMode_Run;
                    dr[dn_Mode_Thr] = stData[i].bMode_Thr;
                    dr[dn_Mode_Fun] = stData[i].bMode_Fun;

                    dr[dn_ID_TempEnable] = stData[i].bID_TempEnable;
                    dr[dn_ID_PD50] = stData[i].bID_PD50;
                    dr[dn_Error_From] = stData[i].bError_From;
                    dr[dn_UnCalc] = stData[i].bUnCalc;

                }
            }
            bDataChange = false;

        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            dataGridView1.Width = this.Width - dataGridView1.Left * 2;
        }

        private void Button2_Click(object sender, EventArgs e)
        {

            lock (thisLock)
            {
                sbLog.Clear();
            }
            textBox1.Text = "";
        }

        /// <summary>
        /// クリップボードへの書き出し
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button3_Click(object sender, EventArgs e)
        {
            StringBuilder sOut = new StringBuilder();
            for (int y = 0; y < iMachineNum; y++)
            {
                for (int x = 0; x < dataGridView1.ColumnCount; x++)
                {
                    sOut.Append(dataGridView1[x, y].Value);
                    if (x == dataGridView1.ColumnCount - 1) break;
                    sOut.Append("\t");
                }
                if (y == iMachineNum - 1) break;
                sOut.Append("\n");
            }

            Clipboard.SetDataObject(sOut.ToString(), true);

        }

        /// <summary>
        /// クリップボードからの読み込み
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button4_Click(object sender, EventArgs e)
        {

            textBox1.Text = "";

            IDataObject data = Clipboard.GetDataObject();

            if (data.GetDataPresent(DataFormats.Text))
            {
                string sText = (string)data.GetData(DataFormats.Text);
                string[] stArrayData = sText.Split('\n');
                int yMax = stArrayData.Length;
                if (yMax > iMachineNum) yMax = iMachineNum;

                // データを確認する
                for (int y = 0; y < yMax; y++)
                {
                    string stData = stArrayData[y];
                    string[] stxData = stData.Split('\t');
                    int xLength = stxData.Length;
                    if (xLength > dataGridView1.ColumnCount) xLength = dataGridView1.ColumnCount;

                    for (int x = 0; x < xLength; x++)
                    {
                        Type t = dataGridView1[x, y].ValueType;
                        if (stxData[x] == "") continue;

                        if (t == typeof(Int32))
                        {
                            dataGridView1[x, y].Value = Int32.Parse(stxData[x]);
                        }
                        if (t == typeof(Boolean))
                        {
                            bool b = false;
                            if (string.Compare(stxData[x], "true", true) == 0) b = true;
                            dataGridView1[x, y].Value = b;
                        }
                        if (t == typeof(Double))
                        {
                            dataGridView1[x, y].Value = double.Parse(stxData[x]);
                        }

                    }

                }
            }
        }

        private void DataGridView1_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentCell.ValueType == typeof(Boolean))
            {
                //コミットする
                dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void TextBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            //0～9と、バックスペース以外の時は、イベントをキャンセルする
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
            ChangeSleepWait();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            foreach (var cl in clList)
            {
                cl.StopSocket();
            }

        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            ChangeSleepWait();
        }

        private void ChangeSleepWait()
        {
            bool btry = int.TryParse(textBox2.Text, out int n);
            if (!btry) return;
            if (checkBox4.Checked == false) n = 0;
            foreach (var cl in clList)
            {
                cl.SleepSec = n;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            ChangeSleepWait();
        }
    }
    static class Consts
    {
        public const string TYPE_INT = "System.Int32";
        public const string TYPE_STR = "System.String";
        public const string TYPE_BOL = "System.Boolean";
    }
}
