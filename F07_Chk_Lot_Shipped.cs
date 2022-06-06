using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace k001_shukka
{
    public partial class F07_Chk_Lot_Shipped : Form
    {
        #region フォーム変数
        private bool bClose = true;
        private string[] argVals;    // 親フォームから受け取る引数
        public string[] ReturnValue; // 親フォームに返す戻り値
        private bool bPHide = true;  // 親フォームを隠す = True
        ToolTip ToolTip1;
        #endregion
        public F07_Chk_Lot_Shipped(params string[] argVals)
        {
            // 親フォームから受け取ったデータをこのインスタンスメンバに格納
            this.argVals = argVals;
            InitializeComponent();

            // ■■■ 親フォームを隠す設定 true => 隠す　default = 隠す。
            //bPHide = false; // 隠さない
            //■■■ 画面の和名
            string sTitle = "Lot確認";
            #region 画面の状態を設定
            // 画面サイズ変更の禁止
            this.MaximizeBox = false;

            lblCaption.Text = fn.frmTxt(sTitle);
            string s = string.Empty; ;
            if (usr.iDB == 1) s += " TestDB: ";
            s += DateTime.Now.ToString("yy/MM/dd HH:mm");
            s += " " + usr.name;
            lblRCaption.Text = s;
            // タイトルバー表示設定
            this.Text = string.Format("【{0}】 {1}"
                , this.Name
                , DEF_CON.prjName + " " + DEF_CON.GetVersion());
            #endregion

            #region dgv設定のここでバインド
            dgv0.DataSource = bs0;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="frm">Form</param>
        /// <param name="s">0= appID, 1 = usr.id, 2 = iDB</param>
        /// <returns></returns>
        static public string[] ShowMiniForm(Form frm, params string[] s)
        {
            // params s >> 0= appID, 1 = usr.id, 2 = iDB
            F07_Chk_Lot_Shipped f = new F07_Chk_Lot_Shipped(s);
            f.ShowDialog(frm);
            //f.Show(frm);

            string[] receiveText = f.ReturnValue; // -- ①
            f.Dispose();
            return receiveText;
        }

        private void Fxx_Load(object sender, EventArgs e)
        {
            // 開いた元フォームを隠す設定
            if (bPHide) this.Owner.Hide();
            SetTooltip();
        }

        private void SetTooltip()
        {
            //ToolTipを作成する
            //ToolTip1 = new ToolTip(this.components);
            //フォームにcomponentsがない場合
            ToolTip1 = new ToolTip();

            //ToolTipの設定を行う
            //ToolTipが表示されるまでの時間
            ToolTip1.InitialDelay = 200;
            //ToolTipが表示されている時に、別のToolTipを表示するまでの時間
            ToolTip1.ReshowDelay = 500;
            //ToolTipを表示する時間
            ToolTip1.AutoPopDelay = 10000;
            //フォームがアクティブでない時でもToolTipを表示する
            ToolTip1.ShowAlways = true;

            //Button1とButton2にToolTipが表示されるようにする
            ToolTip1.SetToolTip(btnClose, "戾る");
            ToolTip1.SetToolTip(button1, "蘇り");

            //ToolTip1.SetToolTip(button7, "表示切替");

        }

        #region CLOSE処理
        // 3) btnClose
        private void FRM_Closing(object sender, FormClosingEventArgs e)
        {
            if (bClose)
            {
                DialogResult result = MessageBox.Show(
                        "「戻る」ボタンで画面を閉じてください。", "",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //e.Cancel = true;
                if (this.ReturnValue == null) this.ReturnValue = new string[] { "" };
            }
        }
        // 2) btnClose
        private void closing()
        {
            bClose = false;
            // 戻り値をセット
            if (this.ReturnValue == null) this.ReturnValue = new string[] { "" };
            this.Close();
        }
        // 1) btnClose  -> 2) -> 3) -> 2) -> 1) -> ShowMiniForm.f.showdialog
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.ReturnValue = new string[] { "" };
            closing();
        }
        // CLOSE処理の最後
        private void FRM_Closed(object sender, FormClosedEventArgs e)
        {
            if (bPHide) this.Owner.Show();
        }
        #endregion

        private string sGetSql(string s)
        {
            return string.Format(
                "SELECT "
                 + " p.LOT_NO LotNo"  //0
                 + " ,CASE WHEN p.SHIP_SEQ IS NULL THEN '未出荷'"  //1
                 + " WHEN p.SHIP_SEQ = 0 THEN '0出荷'"  
                 + " ELSE p.SHIP_SEQ END 出荷No"  
                 + " ,p.GRADE_AC_SEQ GRD番号"  //2
                 + " ,g.GRADE"  //3
                 + " FROM kyoei.t_product p"
                 + " LEFT JOIN kyoei.m_grade_account ga"
                 + " ON p.GRADE_AC_SEQ = ga.SEQ"
                 + " LEFT JOIN kyoei.m_grade g"
                 + " ON ga.GRADE_SEQ = g.GRADE_SEQ"
                 + " WHERE  p.LOT_NO LIKE '%{0}%';"
                , s);
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            // 210105-v
            if (tb.Text.Length < 8) return;
            GetData(dgv0, bs0, sGetSql(tb.Text));
        }

        private void GetData(DataGridView dgv, BindingSource bs, string sSel)
        {
            dgv.Visible = false;
            try
            {
                // dgvの書式設定全般
                fn.SetDGV(dgv, true, 20, true, 9, 10, 50, true, 40, DEF_CON.DBLUE, DEF_CON.LBLUE);
                dgv.MultiSelect = true;

                //if(bs.DataSource != null) bs.DataSource = null;
                //bs0.Filter = string.Empty;

                mydb.kyDb con = new mydb.kyDb();

                con.GetData(sSel, DEF_CON.Constr());


                bs.DataSource = con.ds.Tables[0];

                // 並べ替えを禁止 >> セルが狭くなる
                //foreach (DataGridViewColumn column in dgv.Columns) // 並替禁止
                //{
                //    column.SortMode = DataGridViewColumnSortMode.NotSortable;
                //}

                // ヘッダーとすべてのセルの内容に合わせて、列の幅を自動調整する
                dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                #region 手動でセル幅を指定する場合
                #region sel内容

                #endregion
                int[] iw;
                int[] icol;
                // 列幅を整える
                //icol = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
                //iw = new int[] { 52, 83, 45, 77, 250, 200, 70, 90, 30, 83, 67, 77 };
                //fn.setDgvWidth(dgv, icol, iw);
                #endregion

                //dgv.Columns[6].DefaultCellStyle.Format = "#,0";

                //icol = new int[] { 12, 13 };
                //fn.setDgvInVisible(dgv, icol);

                icol = new int[] { 1,2 };
                iw = new int[] { -1, -1 };
                fn.setDgvAlign(dgv, icol, iw);
                //if (fn.dgvWidth(dgv) < 1300)
                //{
                //    this.Width = fn.dgvWidth(dgv) + 40;
                //    dgv.Width = fn.dgvWidth(dgv);
                //}
                //else
                //{
                //    this.Width = 1300;
                //    dgv.Width = this.Width - 40;
                //}
                dgv.ClearSelection();



                FillDgvCount(dgv, label2);
            }
            catch (Exception ex)
            {
                MessageBox.Show("データ抽出の過程でエラーが発生しました。", ex.GetType().FullName);
            }
            dgv.Visible = true;
        }

        private void FillDgvCount(DataGridView dgv, Label lbl)
        {
            lbl.Text = string.Format("{0} 件", dgv.Rows.Count.ToString());
            lbl.Left = dgv.Left + dgv.Width - lbl.Width;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dgv0.SelectedRows.Count == 0) return;

            string sLots = string.Empty;
            foreach (DataGridViewRow r in dgv0.Rows) // 並替禁止
            {
                if (dgv0.Rows[r.Index].Selected)
                {
                    sLots += ",'";
                    sLots += dgv0.Rows[r.Index].Cells[0].Value.ToString();
                    sLots += "'";
                }
            }
            if (sLots.Length == 0) return;
            sLots = sLots.Substring(1);
            string sUPD = string.Format(
            "UPDATE kyoei.t_product SET SHIP_SEQ = NULL "
            + "WHERE SHIP_SEQ = 0 AND LOT_NO IN({0});"
            ,sLots);
            if (true)
            {
                string[] sSend = { "未出荷に戻せるのは「0出荷」扱いのものに限られます。\r\n選択したLotを未出荷に戻しますか", "" };
                string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSend);
                if (sRcv[0].Length == 0) return;
            }
            mydb.kyDb con = new mydb.kyDb();
            string sRet = con.ExecSql(false, DEF_CON.Constr(), sUPD);
            GetData(dgv0, bs0, sGetSql(textBox5.Text));
        }
    }
}
