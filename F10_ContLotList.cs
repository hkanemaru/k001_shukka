using ClosedXML.Excel;
using System;
using System.Windows.Forms;

namespace k001_shukka
{
    public partial class F10_ContLotList : Form
    {
        #region フォーム変数
        private Boolean bClose = true;
        private string[] argVals; // 親フォームから受け取る引数
        public string[] ReturnValue;            // 親フォームに返す戻り値
        private Boolean bPHide = true;  // 親フォームを隠す = True
        private int FRM_iH; private int FRM_iW;
        private Boolean bdgvCellClk = false; // dgvでクリックする場合には必須
        DateTime loadTime; // formloadの時間
        private string sTmp = string.Empty; // dgvセルの一時格納 比較の為初期化済
        private string sSD;
        private string sED;
        //private bool bDirty = false; // 編集が行われたらtrue
        #endregion 

        public F10_ContLotList(params string[] argVals)
        {
            // 親フォームから受け取ったデータをこのインスタンスメンバに格納
            this.argVals = argVals;
            InitializeComponent();

            // ■■■ 親フォームを隠す設定 true => 隠す　default = 隠す。
            //bPHide = false; // 隠さない
            //■■■ 画面の和名
            string sTitle = "バーコード発行リスト";
            #region 画面の状態を設定
            // 画面サイズ変更の禁止
            this.MaximizeBox = false;
            string s = string.Empty;
            if (usr.iDB == 1) s += " TestDB: ";
            s += DateTime.Now.ToString("yy/MM/dd HH:mm");
            s += " " + usr.name;
            lblDT.Text = s;

            // タイトルバー表示設定
            this.Text = string.Format("【{0}】 {1}  {2} {3}"
                , this.Name
                , sTitle
                , DateTime.Now.ToShortDateString()
                , DateTime.Now.ToShortTimeString());
            #endregion
            // dgvがある時はここでソースを指定
            dgv0.DataSource = bs0;
        }

        static public string[] ShowMiniForm(Form frm, params string[] s)
        {
            F10_ContLotList f = new F10_ContLotList(s);
            f.ShowDialog(frm);

            string[] receiveText = f.ReturnValue; // -- ①
            f.Dispose();
            return receiveText;
        }

        private void FRM_Load(object sender, EventArgs e)
        {
            // 開いた元フォームを隠す設定
            if (bPHide) this.Owner.Hide();
            lblTtitle.Text = "東洋製罐出荷用Lot管理";
            loadTime = DateTime.Now;
            fn.showCenter(this);
            // 初期設定値を記憶する
            FRM_iH = this.Width; FRM_iW = this.Width;
            // タイトルラベルを中央に置く
            lblTtitle.Left = (this.Width - lblTtitle.Width) / 2;
            sSD = loadTime.AddMonths(-2).ToShortDateString();
            sED = DateTime.Today.AddMonths(1).ToShortDateString();
            GetData(dgv0, bs0, sGetList());
            if (argVals[0].Length > 0) textBox3.Text = argVals[0];
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
            }
        }
        // 2) btnClose
        private void closing()
        {
            bClose = false;
            // 戻り値をセット
            //if (this.ReturnValue == null)
            //{
            //    this.ReturnValue = new string[] { "戻り値です" };
            //}
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

        //新規ボタン
        private void button1_Click(object sender, EventArgs e)
        {
            string[] sendText = { "" };

            //string[] reT = FRM303_BarCode.ShowMiniForm(this, sendText);
        }

        private string sGetList()
        {
            string s;
            s = string.Format(
                "SELECT"
             + "   bc.SEQ"    //0
             + "   , DATE_FORMAT(bc.DUE_DATE, '%Y/%m/%d') 納入日"    //1
             + "   , bc.DUE_TIME 納入時間"    //2
             + "   , REPLACE(REPLACE(REPLACE(DELIVERY,'東洋製罐株式会社', ''), ' ', ''), '　', '') 納入先"    //3
             + "   , bc.LOT LOT"    //4
             + "   , bc.WEIGHT 数量"    //5
             + "   , bc.CONTAINER_NO コンテナNo"    //6
             + "   , bc.REF_NO 産業refNo"    //7
             + "   , bc.ORDER_NUM 発注管理番号"    //8
             + "   , UPD_PSN 更新者"    //9
             + "   , DATE_FORMAT(UPD_DATE, '%Y/%m/%d') 更新日時"    //10
             + "   , REG_PSN 登録者"    //11
             + "   , DATE_FORMAT(REG_DATE, '%Y/%m/%d') 登録日時 "    //12
             + "   , 0 'FLG'"                                    // 13
             + "  FROM T_CAN_BARCODE bc"
             + "  WHERE bc.LGC_DEL = '0' AND bc.REG_DATE > '{0}'"
             + "  AND bc.REG_DATE <= '{1}' AND bc.LOCATION IS NULL"
             + "  ORDER BY DUE_DATE DESC, SEQ DESC;"
            , sSD,sED);
            return s;
        }

        private void GetData(DataGridView dgv, BindingSource bs, string sSel)
        {
            dgv0.Visible = false;
            try
            {
                // dgvの書式設定全般
                fn.SetDGV(dgv0, true, 20, false, 9, 10, 50, true, 40, DEF_CON.DBLUE, DEF_CON.YELLOW);
                //dgv0.MultiSelect = true;

                //if(bs.DataSource != null) bs.DataSource = null;

                mydb.kyDb con = new mydb.kyDb();

                con.GetData(sSel,DEF_CON.Constr());

                bs.DataSource = con.ds.Tables[0];

                //ヘッダーとすべてのセルの内容に合わせて、列の幅を自動調整する
                dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                #region 手動でセル幅を指定する場合
                #region sel内容

                #endregion
                int[] iw;
                int[] icol;
                // 列幅を整える
                //icol = new int[] { 1, 2, 3, 4, 5, 6 };
                //iw = new int[] { 93, 40, 77, 180, 178, 68 };
                //clsFunc.setDgvWidth(dgv0, icol, iw);
                #endregion

                icol = new int[] { 11, 12, 13 };
                fn.setDgvInVisible(dgv0, icol);
                icol = new int[] { 0, 5 };
                iw = new int[] { -1, -1 };
                //fn.setDgvAlign(dgv0, icol, iw);
                dgv.ClearSelection();
                int iWidth = fn.dgvWidth(dgv);
                if (iWidth > 1300)
                {
                    dgv.Anchor = AnchorStyles.None;
                    dgv.Anchor = AnchorStyles.Left | AnchorStyles.Top;

                    this.Width = 1300;
                    dgv.Width = 1300 - dgv.Left * 2 - 20;
                    dgv.Anchor = AnchorStyles.Left | AnchorStyles.Top
                        | AnchorStyles.Right;
                }
                else
                {
                    this.Width = iWidth + dgv.Left * 2 + 20;
                    dgv.Width = iWidth;
                }
                arrageTextBW(dgv);
                FillDgvCount(dgv, label1);
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

        private void arrageTextBW(DataGridView dgv)
        {
            //bool b = false;
            //textBox1.Visible = b; textBox2.Visible = b; textBox3.Visible = b; textBox4.Visible = b;
            if (dgv.Rows.Count > 0)
            {
                #region 検索ボックスの位置
                //int iwdt = dgv.Columns[0].Width;
                //textBox0.Left = dgv.Left;
                //textBox0.Width = iwdt;

                textBox1.Left = dgv.Left + dgv.Columns[0].Width;
                int iwdt = dgv.Columns[1].Width;
                textBox1.Width = iwdt;

                textBox2.Left = textBox1.Left + textBox1.Width + dgv.Columns[2].Width + 1;
                iwdt = dgv.Columns[3].Width;
                textBox2.Width = iwdt;

                textBox3.Left = textBox2.Left + textBox2.Width + 1;
                iwdt = dgv.Columns[4].Width;
                textBox3.Width = iwdt;

                textBox4.Left = textBox3.Left + textBox3.Width + 1;
                iwdt = dgv.Columns[5].Width;
                textBox4.Width = iwdt;

                //textBox5.Left = textBox4.Left + textBox4.Width + 1;
                //iwdt = dgv.Columns[5].Width;
                //textBox5.Width = iwdt;

                //textBox6.Left = textBox5.Left + textBox5.Width + dgv.Columns[6].Width + 1;
                //iwdt = dgv.Columns[7].Width;
                //textBox6.Width = iwdt;

                //textBox7.Left = textBox6.Left + textBox6.Width + dgv.Columns[8].Width + 1;
                //iwdt = dgv.Columns[9].Width;
                //textBox7.Width = iwdt;
                #endregion
            }
            //b = true;
            //textBox1.Visible = b; textBox2.Visible = b; textBox3.Visible = b; textBox4.Visible = b;
        }

        #region dgvクリック関連
        private void dgv0_MouseDown(object sender, MouseEventArgs e)
        {
            bdgvCellClk = fn.dgvCellClk(sender, e);
        }

        private void dgv0_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            if (!bdgvCellClk) return;
            int ir = e.RowIndex;
            int ic = e.ColumnIndex;

            if (ic == 2 || ic == 6 || ic == 7 || ic == 8)
            {
                dgv.ReadOnly = false;
            }
            else
            {
                dgv.ReadOnly = true;
            }
            try
            {
                string s = dgv.Rows[ir].Cells[ic].Value.ToString();

                //アプリケーション終了後、クリップボードからデータは削除される
                Clipboard.SetDataObject(s);
                //フォーム上の座標でマウスポインタの位置を取得する
                //画面座標でマウスポインタの位置を取得する
                System.Drawing.Point sp = System.Windows.Forms.Cursor.Position;
                //画面座標をクライアント座標に変換する
                System.Drawing.Point cp = this.PointToClient(sp);
                //X座標を取得する
            }
            catch
            {
                return;
            }
        }

        private void dgv0_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            // データ欄以外は何もしない
            if (!bdgvCellClk) return;
            int ir = e.RowIndex;

            // ダブルクリック前に列の並び替えが行われていれば、その状態を記憶
            string sOrder = string.Empty;
            string sOrdColName = string.Empty;
            if (dgv.SortedColumn != null)
            {
                if (dgv.SortOrder.ToString() == "Ascending") sOrder = "ASC";
                else sOrder = "DESC";
                sOrdColName = dgv.SortedColumn.Name;
            }

            string s0 = dgv.Rows[e.RowIndex].Cells[4].Value.ToString(); // SEQ
            string[] sendText = { s0 };

            // FRMxxxxから送られてきた値を受け取る

            string[] receiveText = F11_CLotBC.ShowMiniForm(this, sendText);
            if (receiveText[0].Length == 0) return;
            GetData(dgv0, bs0, sGetList());
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
            dgv0.Rows[ir].Selected = true;
            if (ir + 1 < dgv.Rows.Count) dgv0.FirstDisplayedScrollingRowIndex = ir;
        }
        #endregion

        private void dgv0_CurrentCellChanged(object sender, EventArgs e)
        {

        }

        private void dgv0_CellValidated(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dgv0_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            var dgv = (DataGridView)sender;
            if (!bdgvCellClk) return;
            int ic = dgv.CurrentCell.ColumnIndex;
            int ir = dgv.CurrentCell.RowIndex;

            if (dgv[ic, ir].ReadOnly) return;
            if (dgv.IsCurrentCellDirty)
            {
                dgv.CommitEdit(DataGridViewDataErrorContexts.Commit);
                string s = dgv.Rows[ir].Cells[ic].Value.ToString();
                string s0 = dgv.Rows[ir].Cells[0].Value.ToString();
                string sMsg = string.Empty; //message
                bool bEdit = true;
                string sCol = "DUE_TIME";
                if (ic == 2) // 時刻
                {
                    if (s.Length > 5)
                    {
                        sMsg = "時刻は5文字以内で入力して下さい。";
                        bEdit = false;
                    }
                    if (string.IsNullOrEmpty(s)) s = "NULL";
                    else if (!fn.isHankakuEisu(s))
                    {
                        sMsg += "入力は半角英数です。";
                        bEdit = false;
                    }
                    else s = "'" + s + "'";
                    // sCol = "DUE_TIME";
                }

                if (ic == 6)          // コンテナNo
                {
                    sCol = "CONTAINER_NO";
                    if (string.IsNullOrEmpty(s)) s = "NULL";
                    else if (s.Length > 12)
                    {
                        sMsg += "12文字以下で記入して下さい。";
                        bEdit = false;
                    }
                    else if (!fn.isHankakuEisu(s))
                    {
                        sMsg += "入力は半角英数です。";
                        bEdit = false;
                    }
                    else s = "'" + s + "'";
                }

                if (ic == 7)          // 産業REF_NO
                {
                    sCol = "REF_NO";
                    if (string.IsNullOrEmpty(s)) s = "NULL";
                    else if (s.Length > 8)
                    {
                        sMsg += "8文字以下で記入して下さい。";
                        bEdit = false;
                    }
                    else s = "'" + s + "'";
                }
                if (ic == 8)          // 発注管理番号
                {
                    sCol = "ORDER_NUM";
                    if (string.IsNullOrEmpty(s)) s = "NULL";
                    else if (s.Length > 16)
                    {
                        sMsg += "16文字以下で記入して下さい。";
                        bEdit = false;
                    }
                    else if (!fn.isHankakuEisu(s))
                    {
                        sMsg += "入力は半角英数です。";
                        bEdit = false;
                    }
                    else s = "'" + s + "'";
                }

                if (bEdit)
                {
                    try
                    {
                        string sUpd = string.Format(
                            "UPDATE t_can_barcode SET {0} = {1}, UPD_PSN = '{3}', UPD_DATE = NOW() WHERE SEQ = {2};"
                            , sCol, s, s0, usr.name);
                        mydb.kyDb con = new mydb.kyDb();
                        con.ExecSql(false, DEF_CON.Constr(), sUpd);
                    }
                    catch 
                    {
                        return;
                    }
                }
            }
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
            string fileNm = string.Format("{1}東洋製罐Lotリスト{0}.xlsx", DateTime.Now.ToLongDateString(), argVals[0]);
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

        private void button2_Click(object sender, EventArgs e)
        {
            ExportXls(dgv0);
        }

        private void tB_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;

            string s = string.Empty;
            if (textBox1.Text.Length > 0) s += string.Format(" AND 納入日 LIKE '%{0}%'", textBox1.Text);
            if (textBox2.Text.Length > 0) s += string.Format(" AND 納入先 LIKE '%{0}%'", textBox2.Text);
            if (textBox3.Text.Length > 0) s += string.Format(" AND LOT LIKE '%{0}%'", textBox3.Text);
            if (textBox4.Text.Length > 0) s += string.Format(" AND 数量 = {0}", textBox4.Text);
            if (s.Length > 0) s = s.Substring(4);
            bs0.Filter = s;
            arrageTextBW(dgv0);
            FillDgvCount(dgv0, label1);
        }

        private void button3_Click(object sender, EventArgs e)
        {            
            string[] sSet = { sSD, sED, "" };
            string[] sRcv = promag_frm.F06_SelDate.ShowMiniForm(this, sSet);
            if (sRcv[0].Length == 0) return;
            sSD = sRcv[0];
            sED = sRcv[1];
            GetData(dgv0, bs0, sGetList());
            arrageTextBW(dgv0);
        }
    }
}
