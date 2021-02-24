using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;

namespace k001_shukka
{
    public partial class F06_ChkLot : Form
    {
        #region フォーム変数
        private bool bClose = true;
        private string[] argVals;    // 親フォームから受け取る引数
        public string[] ReturnValue; // 親フォームに返す戻り値
        private bool bPHide = true;  // 親フォームを隠す = True
        #endregion
        public F06_ChkLot(params string[] argVals)
        {
            // 親フォームから受け取ったデータをこのインスタンスメンバに格納
            this.argVals = argVals;
            InitializeComponent();

            // ■■■ 親フォームを隠す設定 true => 隠す　default = 隠す。
            //bPHide = false; // 隠さない
            //■■■ 画面の和名
            string sTitle = "カクテルLot商品CD確認";
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
                , DEF_CON.prjName + " " + DEF_CON.verString);
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
            F06_ChkLot f = new F06_ChkLot(s);
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
            label1.Text = "受注コード：";
            if (argVals[1].Length > 0) label1.Text += argVals[1];
            GetData(dgv0, bs0, sGetSql(argVals[0]));
        }

        private string sGetSql(string sLots)
        {
            return string.Format(
                "SELECT "
                 + " REPLACE(CONCAT(CONCAT(SUBSTR(C.HINLCDDT,3,6), '-'),C.LOTID),' ','') LOT_NO"  //0
                 + " , C.HINCD 商品CD"  //1
                 + " , REPLACE(A.HINNMA, ' ','') 商品名"  //2
                 + " , C.RELZAISU 数量"  //3
                 + " ,CONCAT(C.WRTDT,C.WRTTM)"  //4
                 + " FROM KEI_USR1.HINMTC C"
                 + " INNER JOIN"
                 + " ("
                 + " SELECT "
                 + " REPLACE(CONCAT(CONCAT(SUBSTR(C1.HINLCDDT,3,6), '-'),C1.LOTID),' ','') LotNo"
                 + " ,MAX(CONCAT(C1.WRTDT,C1.WRTTM)) MAXDATE"
                 + "  FROM KEI_USR1.HINMTC C1"
                 + " WHERE "
                 + " REPLACE(CONCAT(CONCAT(SUBSTR(C1.HINLCDDT,3,6), '-'),C1.LOTID),' ','')"
                 + " IN ({0})"
                 + " GROUP BY CONCAT(CONCAT(SUBSTR(C1.HINLCDDT,3,6), '-'),C1.LOTID)"
                 + " ,REPLACE(CONCAT(CONCAT(SUBSTR(C1.HINLCDDT,3,6), '-'),C1.LOTID),' ','')"
                 + " ) tmp"
                 + " ON tmp.LotNo = REPLACE(CONCAT(CONCAT(SUBSTR(C.HINLCDDT,3,6), '-'),C.LOTID),' ','')"
                 + "  AND tmp.MAXDATE = CONCAT(C.WRTDT,C.WRTTM)"
                 + " LEFT JOIN KEI_USR1.HINMTA A"
                 + " ON C.HINCD = A.HINCD"
                 + " WHERE "
                 + " LENGTH(REPLACE(C.LOTID,' ','')) > 0"
                 + " AND"
                 + " REPLACE(CONCAT(CONCAT(SUBSTR(C.HINLCDDT,3,6), '-'),C.LOTID),' ','')"
                 + " IN ({0})"
                 + " ORDER BY CONCAT(CONCAT(SUBSTR(C.HINLCDDT,3,6), '-'),C.LOTID)"
                ,sLots);
        }

        private void GetData(DataGridView dgv, BindingSource bs, string sSel)
        {
            dgv.Visible = false;
            try
            {
                // dgvの書式設定全般
                fn.SetDGV(dgv, true, 20, true, 9, 10, 50, true, 40, DEF_CON.BLUE, DEF_CON.YELLOW);
                dgv.MultiSelect = true;

                //if(bs.DataSource != null) bs.DataSource = null;
                //bs0.Filter = string.Empty;

                //mydb.kyDb con = new mydb.kyDb();

                //con.GetData(sSel, DEF_CON.Constr());

                try
                {
                    OracleDataAdapter oda;
                    DataSet ds;
                    using (var oracon = new OracleConnection())
                    {
                        
                        oracon.ConnectionString = "User ID=KEI_ROU;Password=P;Data Source=SDS;";
                        //+ "Connection Timeout=60;";
                        ds = new DataSet();
                        //Dataを取得
                        try
                        {
                            oda = new OracleDataAdapter(sSel, oracon.ConnectionString);

                            oda.SelectCommand.CommandTimeout = 240;
                            oda.Fill(ds);
                            bs.DataSource = ds.Tables[0];
                        }
                        catch (Exception ex) { MessageBox.Show(ex.Message, "clsMyKyoei.GetDataエラー"); }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "エラー");
                    return;
                }

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

                dgv.Columns[3].DefaultCellStyle.Format = "#,0";

                icol = new int[] { 4 };
                fn.setDgvInVisible(dgv, icol);

                //icol = new int[] { 0, 6, 7 };
                //iw = new int[] { -1, -1, 1 };
                //fn.setDgvAlign(dgv, icol, iw);
                if (fn.dgvWidth(dgv) < 600)
                {
                    this.Width = fn.dgvWidth(dgv) + 40;
                    dgv.Width = fn.dgvWidth(dgv);
                }
                else
                {
                    this.Width = 600;
                    dgv.Width = this.Width - 40;
                }
                dgv.ClearSelection();

                if(dgv.Rows.Count > 0)
                {
                    if(argVals[1].Length > 0)
                    {
                        bool b = true;
                        for(int i = 0; i < dgv.Rows.Count; i++)
                        {
                            if (argVals[1] != dgv.Rows[i].Cells[1].Value.ToString()) b = false;
                        }
                        if (b) label3.Text = "突合OK";
                        else label3.Text = "一致しない商品コードがあります。";
                    }
                    else
                    {
                        label3.Text = "受注データで作成されていません。比較不可";
                    }
                }

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
    }
}
