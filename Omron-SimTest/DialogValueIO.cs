using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Omron_SimTest
{
    public partial class DialogValueIO : Form
    {
        /// <summary>
        /// 対象のセルコレクション
        /// </summary>
        public DataGridViewSelectedCellCollection _cells;


        public DialogValueIO()
        {
            InitializeComponent();
        }


		/// <summary>
		/// 入力された値取得プロパティ
		/// </summary>
		public string InputtedValue
		{
			set { txtValue.Text = value; }
			get { return txtValue.Text; }
		}

		/// <summary>
		/// OKボタン押下イベントハンドラ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnEnter_Click(object sender, EventArgs e)
		{
            Int32 value;
            if (_cells == null) return;

			//bool castErr = false;
			foreach (DataGridViewCell cell in _cells)
			{
				switch (cell.ValueType.ToString())
				{
					case Consts.TYPE_INT:
						if (Int32.TryParse(txtValue.Text, out value)) cell.Value = value;
						//else castErr = true;

						break;

                    case Consts.TYPE_STR:
                        cell.Value = txtValue.Text;
                        break;

                    case Consts.TYPE_BOL:
                        bool bSet = false;
                        if (Int32.TryParse(txtValue.Text, out value))
                        {
                            if (value != 0) bSet = true;
                        }
                        cell.Value = bSet;
                        break;

                    default:
						//castErr = true;

						break;
				}
			}
            this.Close();
		}

		/// <summary>
		/// キー押下イベントハンドラ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ReplaceValue_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)Keys.Enter) btnEnter.PerformClick();
			if (e.KeyChar == (char)Keys.Escape) this.Close();
		}

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
