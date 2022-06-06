using ClosedXML.Excel;
using System;
using System.Windows.Forms;

namespace k001_shukka
{
    public partial class F13_ALotCrt : Form
    {
        #region フォーム変数
        private bool bClose = true;
        private string[] argVals;    // 親フォームから受け取る引数
        public string[] ReturnValue; // 親フォームに返す戻り値
        private bool bPHide = true;  // 親フォームを隠す = True
        ToolTip ToolTip1;
        private Boolean bdgvCellClk = false; // dgvがクリックされた際にデータ欄だとtrue
        #endregion

        public F13_ALotCrt(params string[] argVals)
        {
            // 親フォームから受け取ったデータをこのインスタンスメンバに格納
            this.argVals = argVals;
            InitializeComponent();
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
            F13_ALotCrt f = new F13_ALotCrt(s);
            f.ShowDialog(frm);

            string[] receiveText = f.ReturnValue; // -- ①
            f.Dispose();
            return receiveText;
        }

        private void FRM_Load(object sender, EventArgs e)
        {
            // 登録用
            SetTooltip();

            //■■■ 画面の和名
            string sTitle = "別ロット管理";
            #region 画面の状態を設定
            // ■■■ 親フォームを隠す設定 true => 隠す　default = 隠す。
            //bPHide = false; // 隠さない
            // 画面サイズ変更の禁止
            this.MaximizeBox = false;

            lblCaption.Text = fn.frmLTxt(sTitle);
            lblRCaption.Text = fn.frmRTxt();
            // タイトルバー表示設定
            this.Text = string.Format("【{0}】 {1}"
                , this.Name
                , DEF_CON.prjName + " " + DEF_CON.GetVersion());
            #endregion

            #region dgv設定のここでバインド
            dgv0.DataSource = bs0;
            #endregion

            // 開いた元フォームを隠す設定
            if (bPHide) this.Owner.Hide();

            // リスト抽出　>> 初期設定は直近1カ月
            GetData(dgv0, bs0, sGetList(argVals[0])); // 一覧更新
        }

        private void SetTooltip()
        {
            //ToolTipを作成する
            //ToolTip1 = new ToolTip(this.components);

            //フォームにcomponentsがない場合
            ToolTip1 = new ToolTip();

            #region ToolTipの設定を行う
            //ToolTipが表示されるまでの時間
            ToolTip1.InitialDelay = 200;
            //ToolTipが表示されている時に、別のToolTipを表示するまでの時間
            ToolTip1.ReshowDelay = 500;
            //ToolTipを表示する時間
            ToolTip1.AutoPopDelay = 10000;
            //フォームがアクティブでない時でもToolTipを表示する
            ToolTip1.ShowAlways = true;
            #endregion

            //Button1とButton2にToolTipが表示されるようにする
            ToolTip1.SetToolTip(btnClose, "戾る");

            ToolTip1.SetToolTip(button2, "登録");
            ToolTip1.SetToolTip(button3, "自動採番");
            ToolTip1.SetToolTip(button1, "エクセル出力");
        }


        private void GetData(DataGridView dgv, BindingSource bs, String sSel)
        {
            try
            {
                dgv.Visible = false;
                // dgvの書式設定全般
                fn.SetDGV(dgv, false, 80, false, 11, 11, 50, false, 45, DEF_CON.BLUE, DEF_CON.YELLOW);
                //dgv.EnableHeadersVisualStyles = false;
                dgv.DefaultCellStyle.BackColor = System.Drawing.Color.GhostWhite;

                //dgv.ColumnHeadersDefaultCellStyle.Font
                //    = new System.Drawing.Font("メイリオ", 11, System.Drawing.FontStyle.Bold);
                bs.DataSource = null;

                mydb.kyDb con = new mydb.kyDb();
                con.GetData(sSel, DEF_CON.Constr());

                bs.DataSource = con.ds.Tables[0];

                #region dgv書式設定
                dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

                //if (fn.dgvWidth(dgv0) > 680)
                //{
                //    dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                //    dgv.Columns[2].Width = dgv.Columns[2].Width
                //        - (fn.dgvWidth(dgv0) - 680);
                //}
                // dgv 列幅設定
                // iw に列幅　icol に指定する列番号を設定する
                // 1,GRADE_SEQ , 2GRADE, 3GRADE_KANA, 4BIKOU 
                int[] iw = { dgv.Width - 20 };
                //if (dgv.Name == "dgv0") iw = new int[] { 380 };
                //else iw = new int[] { 320 };


                int[] icol = new int[] { 1 };
                //fn.setDgvWidth(dgv, icol, iw);
                // dgv 列非表示設定
                int[] ic = { dgv.Columns.Count - 1 };
                //fn.setDgvInVisible(dgv, ic);

                // dgv テキスト配置設定
                icol = new int[] { 2 };
                int[] it = { -1 };
                fn.setDgvAlign(dgv, icol, it);

                #endregion
                Label Lbl = label8;

                lblFillVal(Lbl, dgv);
                dgv.ClearSelection();
                dgv.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("データ抽出の過程でエラーが発生しました。", ex.GetType().FullName);
            }
        }

        private void lblFillVal(Label lbl, DataGridView dgv)
        {
            lbl.Text = dgv.Rows.Count.ToString();
            lbl.Left = dgv.Left + dgv.Width - lbl.Width;
            textBox1.Width = dgv.Columns[0].Width;
        }

        private void ExportXls(DataGridView dgv)
        {
            // DataGridViewのセルデータ格納変数 -> v[,]
            //object[,] v = new object[dgv.Rows.Count + 1, dgv.Columns.Count];
            object[,] v = new object[dgv.Rows.Count + 1, dgv.Columns.Count];
            #region DataGridViewのセルデータ取得 -> v に値を格納
            // ヘッダ
            for (int c = 0; c <= dgv.Columns.Count - 1; c++)
            {
                v[0, c] = dgv.Columns[c].HeaderCell.Value.ToString();
            }
            // データ
            for (int r = 0; r < dgv.Rows.Count; r++)
            {
                for (int c = 0; c <= dgv.Columns.Count - 1; c++)
                {

                    if (dgv.Rows[r].Cells[c].Value != null)
                    {
                        if (dgv.Rows[r].Cells[c].Value == null
                            || dgv.Rows[r].Cells[c].Value.ToString().Length == 0) continue;
                        v[r + 1, c] = dgv.Rows[r].Cells[c].Value;
                    }
                }
            }
            #endregion
            //string filePath = @"C:\app\temp.xlsx";
            string fileNm = string.Format("{1}Lot管理表{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHmmss"), argVals[0]);
            var fp = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileNm);

            //using (var book = new XLWorkbook(XLEventTracking.Disabled))
            //{
            //    var sheet1 = book.AddWorksheet("シート1");

            using (var wb = new ClosedXML.Excel.XLWorkbook(XLEventTracking.Disabled))
            {

                var ws = wb.AddWorksheet("DATA0");

                for (int i = 0; i < v.GetLength(0); i++)
                {
                    for (int j = 0; j < v.GetLength(1); j++)
                    {
                        ws.Cell(i + 1, j + 1).Value = v[i, j];
                    }
                }
                // 表全体をまとめて調整する場合は
                ws.ColumnsUsed().AdjustToContents();
                wb.SaveAs(fp);
                string smsg = string.Format(
                    "マイドキュメントに、「{0}」を作成しました。ファイルを開きますか？", fileNm);
                string[] sSet = { smsg, "" };
                string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSet);
                fileNm = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\" + fileNm;
                if (sRcv[0].Length > 0) System.Diagnostics.Process.Start(fileNm);
            }
        }

        private void btn_Click(object sender, EventArgs e)
        {
            // button 6
            Button btn = (Button)sender;
            switch (btn.Name.Substring(6))
            {
                case "1":  // エクセル
                    ExportXls(dgv0);
                    break; // Serch

                case "2": // 登録
                    if (true)
                    {
                        string[] Snd = { "入力内容を登録します。", "" };
                        string[] Rcv = promag_frm.F05_YN.ShowMiniForm(Snd);
                        if (Rcv[0].Length == 0) return;
                    }
                    string sIns = "INSERT INTO t_another_lot (LOT_NO,BASE_LOT_NO,BAG_NO,LOCATION,UPD_ID,UPD_DATE,LGC_DEL"
                        +") VALUES ";
                    string sVals = string.Empty;
                    for (int i = 0; i < dgv0.Rows.Count; i++)
                    {
                        string sLot = dgv0.Rows[i].Cells[0].Value.ToString();
                        string sAlot = dgv0.Rows[i].Cells[1].Value.ToString(); ;
                        string sBag = dgv0.Rows[i].Cells[2].Value.ToString(); ;
                        if (string.IsNullOrWhiteSpace(sAlot)) sAlot = "NULL";
                        else sAlot = string.Format("'{0}'", sAlot);
                        if (string.IsNullOrWhiteSpace(sBag) || !fn.IsInt(sBag)) sBag = "NULL";
                        sVals += string.Format(
                            ",({0},'{1}',{2},2,'{3}',NOW(), '0')"
                            ,sAlot,sLot,sBag,usr.id);
                    }
                    if(sVals.Length > 0)
                    {
                        sVals = sVals.Substring(1) + ";";
                        sIns += sVals;
                        sIns += "DELETE FROM kyoei.t_another_lot "
                             + " WHERE SEQ NOT IN(SELECT MAX_SEQ from (SELECT MAX(SEQ) MAX_SEQ "
                             + " FROM t_another_lot GROUP BY BASE_LOT_NO) tmp);";
                        mydb.kyDb con = new mydb.kyDb();
                        string sMsg = con.ExecSql(true, DEF_CON.Constr(), sIns);
                        string[] Snd = { sMsg, "false"};
                        _ = promag_frm.F05_YN.ShowMiniForm(Snd);
                    }
                    break;
                case "3": // 自動採番
                    if(string.IsNullOrEmpty(textBox1.Text))
                    {
                        string[] Snd = { "Lotは必ず指定します。","false"};
                        _ = promag_frm.F05_YN.ShowMiniForm(Snd);
                        return;
                    }
                    if(textBox2.Text.Length > 0 && !fn.IsInt(textBox2.Text))
                    {
                        string[] Snd = { "連番開始番号は数字のみ入力可能です。", "false" };
                        _ = promag_frm.F05_YN.ShowMiniForm(Snd);
                        return;
                    }
                    string s = textBox1.Text;
                    if (s.Substring(s.Length - 1) != "-") s += "-";
                    string sNo = string.Empty;
                    if (textBox2.Text.Length > 0) sNo = textBox2.Text;
                    
                    for (int i = 0; i < dgv0.Rows.Count; i++)
                    {
                        if (sNo == "0" || sNo.Length == 0) sNo = "";
                        else
                        {
                            if (sNo.Length > 0 && i > 0) sNo = (int.Parse(sNo) + 1).ToString();
                        }
                        if (radioButton1.Checked) dgv0.Rows[i].Cells[1].Value = s + sNo;
                        else if (radioButton2.Checked)
                        {
                            if (sNo.Length == 0) dgv0.Rows[i].Cells[2].Value = DBNull.Value;
                            else dgv0.Rows[i].Cells[2].Value = sNo;
                        }
                    }
                    break;
            }

        }

        private static string sGetList(string sSEQ)
        {
            return string.Format(
                "SELECT"
                 + " tmp.LOT_NO '生産時LotNo.'"          //0
                 + " ,al.LOT_NO '出荷LotNo.'"         //1
                 + " ,al.BAG_NO 'BagNo.'"          //2
                 + " FROM"
                 + " ("
                 + " SELECT"
                 + " p.LOT_NO"
                 + " FROM kyoei.t_product p"
                 + " WHERE p.SHIP_SEQ = {0}"
                 + " ) tmp"
                 + " LEFT JOIN kyoei.t_another_lot al"
                 + " ON tmp.LOT_NO = al.BASE_LOT_NO"
                 + " ORDER BY"
                 + " SUBSTRING_INDEX(tmp.LOT_NO, '-',1)"
                 + " ,SUBSTRING_INDEX(tmp.LOT_NO, '-',2)"
                 + " ,CAST(SUBSTRING_INDEX(tmp.LOT_NO, '-',-1) AS UNSIGNED)"
                 + ";"
                , sSEQ);
        }

        #region dgvクリック
        private void dgv_MouseDown(object sender, MouseEventArgs e)
        {
            bdgvCellClk = fn.dgvCellClk(sender, e);
        }

        private void dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (!bdgvCellClk) return;
            DataGridView dgv = (DataGridView)sender;
            try
            {

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.GetType().FullName, "一覧クリックエラー");
            }
        }

        private void dgv_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            // データ欄以外は何もしない
            if (!bdgvCellClk) return;
            int ir = e.RowIndex;
            int iFr = dgv.FirstDisplayedScrollingRowIndex;

            this.Visible = false;
            // ダブルクリック前に列の並び替えが行われていれば、その状態を記憶
            string sOrder = string.Empty;
            string sOrdColName = string.Empty;
            if (dgv.SortedColumn != null)
            {
                if (dgv.SortOrder.ToString() == "Ascending") sOrder = "ASC";
                else sOrder = "DESC";
                sOrdColName = dgv.SortedColumn.Name;
            }

            string s0 = dgv.Rows[e.RowIndex].Cells[0].Value.ToString(); // LotNo

            //string[] sendText = { s0 };

            //_ = F002_LOADING.ShowMiniForm(this, sendText);
            //GetData(dgv0, bs0, sGetInf());

            //arrageTextBW(dgv);
            // 並び順をダブルクリック前に戻し値を検索
            if (sOrder.Length > 0)
            {
                bs0.Sort = string.Format("{0} {1}", sOrdColName, sOrder);

                for (int r = 0; r < dgv.Rows.Count; r++)
                {
                    if (dgv.Rows[r].Cells[0].Value.ToString() == s0)
                    {
                        ir = r;
                        break;
                    }
                }
            }

            if (ir + 1 < dgv.Rows.Count)
            {
                dgv0.Rows[ir].Selected = true;
                dgv0.FirstDisplayedScrollingRowIndex = iFr;
            }
            this.Visible = true;
        }
        #endregion

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
            if (this.ReturnValue == null) this.ReturnValue = new string[] { "" };
            closing();
        }
        // CLOSE処理の最後
        private void FRM_Closed(object sender, FormClosedEventArgs e)
        {
            if (bPHide) this.Owner.Show();
        }

        #endregion
        private void tB_TextChanged(object sender, EventArgs e)
        {

        }

    }
}
