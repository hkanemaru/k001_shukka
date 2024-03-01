using ClosedXML.Excel;
using System;
using System.Windows.Forms;

namespace k001_shukka
{
    public partial class F16_ReciptList : Form
    {
        #region フォーム変数
        private bool bClose = true;
        private string[] argVals;    // 親フォームから受け取る引数
        public string[] ReturnValue; // 親フォームに返す戻り値
        ToolTip ToolTip1;
        private Boolean bdgvCellClk = false; // dgvがクリックされた際にデータ欄だとtrue
        private DateTime _sd;
        private DateTime _ed;


        private bool bLoad = false;
        private string[] ColNM;
        private int[] ColWID;
        // 書式　1 LEFT 2CENTER,-1RIGHT
        private int[] ColAli;
        // 書式 -1 何もなし、0 小数点0, 1=小数点１ 2小数点2
        private int[] ColZezo;
        private int[] ColInV;
        int[] iCols;
        int[] iWs;

        private int _c = -1;
        
        #endregion

        public F16_ReciptList(params string[] argVals)
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
            F16_ReciptList f = new F16_ReciptList(s);
            f.ShowDialog(frm);

            string[] receiveText = f.ReturnValue; // -- ①
            f.Dispose();
            return receiveText;
        }

        private void FRM_Load(object sender, EventArgs e)
        {
            // _lot = argVals[0];
            // 登録用
            SetTooltip();

            //■■■ 画面の和名
            string sTitle = "受入確認一覧";

            #region 画面のキャプション設定
            lblCaption.Text = fn.frmTxt(sTitle);

            DateTime bldTime = DEF_CON.GetBuildDateTime(DEF_CON.exepath);
            this.Text = $"【{this.Name}】{DEF_CON.prjName} {bldTime:yyyy'/'MM'/'dd}";
            string scap = string.Empty;
            if (usr.iDB == 1) scap += " TestDB: ";
            scap += DateTime.Now.ToString("yy/MM/dd HH:mm");
            scap += " " + usr.name;
            lblRCaption.Text = scap;
            #endregion

            #region dgv設定のここでバインド
            dgv0.DataSource = bs0;
            #endregion


            // リスト抽出　>> 初期設定は直近1カ月
            _ed = DateTime.Today.AddDays(1);
            _sd = _ed.AddMonths(-1);

            label1.Text = $"期間：{_sd:yy/MM/dd} ~ {_ed:yy/MM/dd}";
            // 固定列 = _c を設定
            _c = 1;
            GetData(dgv0, bs0, sGetList()); // 一覧更新

            button1.Left = this.Width - button1.Width - 20;
            lblRCaption.Left = button1.Left - lblRCaption.Width;
        }

        private void ChgParameter()
        {
            string s =
                "確認日,120,1,-1,0;"
                + "代表LotNo,150,1,-1,0;"
                + "製造元,150,1,-1,0;"
                + "確認数,60,1,-1,0;"
                + "受入番号,80,-1,-1,0;"
                + "確認者,120,1,-1,0;"
                + "ilcn,0,-1,-1,1;";
            if (s.Substring(s.Length - 1) == ";") s = s.Substring(0, s.Length - 1);
            string[] lines = s.Split(';');
            int icount = lines.GetLength(0);
            string[,] values = new string[icount, 5];
            for (int i = 0; i < icount; i++)
            {
                string[] clm = lines[i].Split(',');
                for (int c = 0; c < clm.GetLength(0); c++) values[i, c] = clm[c];
            }

            ColNM = new string[icount];
            ColWID = new int[icount];
            ColAli = new int[icount];
            ColZezo = new int[icount];
            ColInV = new int[icount];
            for (int i = 0; i < icount; i++)
            {
                ColNM[i] = values[i, 0].Replace(" ", "");
                ColWID[i] = int.Parse(values[i, 1].Replace(" ", ""));
                ColAli[i] = int.Parse(values[i, 2].Replace(" ", ""));
                ColZezo[i] = int.Parse(values[i, 3].Replace(" ", ""));
                ColInV[i] = int.Parse(values[i, 4].Replace(" ", ""));
            }
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

            ToolTip1.SetToolTip(button2, "表示更新");
            ToolTip1.SetToolTip(button3, "期間変更");
            //ToolTip1.SetToolTip(button4, "納品書出力");
        }

        private void SetDgvF(DataGridView dgvTarget, DataGridView dgvSelf)
        {
            #region フィルター項目を引き継ぐ 前準備
            string sVals = string.Empty;
            string sCols = string.Empty;
            string sCols1 = string.Empty;
            if (dgvSelf.Rows.Count > 0)
            {
                for (int i = 0; i < dgvSelf.Columns.Count; i++)
                {
                    if (dgvSelf.Rows[0].Cells[i].Value != null)
                        sVals += "," + dgvSelf.Rows[0].Cells[i].Value.ToString();
                    else sVals += ",";
                    sCols += "," + dgvSelf.Columns[i].HeaderText;
                }
            }
            #endregion

            dgvSelf.Columns.Clear();
            dgvSelf.Rows.Clear();
            // Columuns.Add([列名称],[表示するテキスト])
            for (int i = 0; i < dgvTarget.Columns.Count; i++)
            {
                dgvSelf.Columns.Add("", "");
            }

            fn.SetDGV(dgvSelf, false, 80, false, 10, 10, 50, false, 40, DEF_CON.DBLUE, DEF_CON.LGreen);
            dgvSelf.ScrollBars = ScrollBars.None;
            dgvSelf.Rows.Add(1);
            //列ヘッダーを非表示にする
            dgvSelf.ColumnHeadersVisible = false;

            ////行ヘッダーを非表示にする
            //dgvSelf.RowHeadersVisible = false;
            dgvSelf.ClearSelection();
            SetWDgvF(dgv0, dgvF);

            #region  フィルター項目を引き継ぐ 実処理
            if (dgvSelf.Rows.Count > 0)
            {
                for (int i = 0; i < dgvSelf.Columns.Count; i++)
                {
                    sCols1 += "," + dgvSelf.Columns[i].HeaderText;
                }
                if (sCols == sCols1)
                {
                    string[] v = sVals.Substring(1).Split(',');
                    // 配列の長さと文字数が同じ時は、値の指定がない。
                    if (sVals.Length == v.Length) return;
                    for (int i = 0; i < dgvSelf.Columns.Count; i++)
                    {
                        dgvSelf.Rows[0].Cells[i].Value = v[i];
                    }
                }
            }
            #endregion
        }

        private void SetWDgvF(DataGridView dgvTarget, DataGridView dgvSelf)
        {
            if (dgvSelf.Columns.Count == 0) return;
            for (int i = 0; i < dgvTarget.Columns.Count; i++)
            {
                if (dgvTarget.Columns[i].Visible) dgvSelf.Columns[i].Width = dgvTarget.Columns[i].Width;
                else dgvSelf.Columns[i].Visible = false;
            }
            if (dgvTarget.Height - 50 - (40 * dgvTarget.Rows.Count) < 0) dgvSelf.Width = dgvTarget.Width - 20;
            else dgvSelf.Width = dgvTarget.Width;
            if (_c >= 0) dgvSelf.Columns[_c].Frozen = true;
        }

        private void dgvF_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            string s = string.Empty;
            string s1;
            // ターゲットとなるdgv= dgv0とする。
            string sFil = string.Empty;
            // ColAli
            for (int i = 0; i < dgv.ColumnCount; i++)
            {
                if (dgv.Rows[e.RowIndex].Cells[i].Value == null
                    || dgv.Rows[e.RowIndex].Cells[i].Value.ToString() == "") continue;

                // 抽出データのヘッダーテキスト
                s = dgv0.Columns[i].HeaderCell.Value.ToString();
                // 検索ボックスのデータ                
                s1 = dgv.Rows[e.RowIndex].Cells[i].Value.ToString();
                if (s1.Trim().Length > 0)
                {
                    if (ColAli[iCols[i]] >= 0)
                    {
                        if (s1 == "=\"\"")
                        {
                            sFil += string.Format("AND ([{0}] = '' OR [{0}] IS NULL)", s);
                        }
                        else if (s1 == "<>\"\"")
                        {
                            sFil += string.Format("AND ([{0}] <> '')", s);
                        }
                        else sFil += string.Format("AND ({0} LIKE '%{1}%')", s, s1);
                    }
                    else
                    {
                        if (s1.IndexOf(">") >= 0 || s1.IndexOf("<") >= 0 || s1.IndexOf("=") >= 0)
                        {
                            sFil += string.Format("AND ({0} {1})", s, s1);
                        }
                        else sFil += string.Format("AND ({0} = {1})", s, s1);
                    }

                }
            }

            if (s.Length == 0) sFil = "";
            if (sFil.Length > 0) sFil = sFil.Substring(4);

            try
            {
                bs0.Filter = sFil;

                lblFillVal(label8, dgv0);
            }
            catch (Exception ex)
            {
                MessageBox.Show("検索出来ない文字列を含んでいます。", ex.GetType().FullName);
            }
        }

        private void dgvF_Scroll(object sender, ScrollEventArgs e)
        {
            dgv0.HorizontalScrollingOffset = dgvF.HorizontalScrollingOffset;
        }

        private void GetData(DataGridView dgv, BindingSource bs, String sSel)
        {
            ChgParameter();
            try
            {
                bLoad = true;
                dgv.Visible = false;
                Label Lbl = label8; Lbl.Text = "";
                bs0.RemoveFilter();

                #region dgvの書式設定全般
                fn.SetDGV(dgv, false, 80, true, 10, 10, 50, true, 40, DEF_CON.STLBLUE, DEF_CON.LLGray);
                //dgv.EnableHeadersVisualStyles = false;
                dgv.DefaultCellStyle.BackColor = System.Drawing.Color.GhostWhite;
                dgv.MultiSelect = true;
                //dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                dgv.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                //dgv.ColumnHeadersDefaultCellStyle.Font
                //    = new System.Drawing.Font("メイリオ", 11, System.Drawing.FontStyle.Bold);

                #endregion
                bs.DataSource = null;
                mydb.kyDb con = new mydb.kyDb();
                
                con.GetData(sSel, DEF_CON.Constr());

                if (con.ds.Tables[0].Rows.Count == 0)
                {
                    string[] Snd = { "該当がありません。", "false" };
                    _ = promag_frm2.F05_YN.ShowMiniForm(Snd);
                    return;
                }
                bs.DataSource = con.ds.Tables[0];

                #region dgv書式設定                


                if (_c >= 0) dgv.Columns[_c].Frozen = true;
                iCols = new int[con.ds.Tables[0].Columns.Count];
                iWs = new int[iCols.Length];


                // iCols に ColNM の順番を入れる
                for (int i = 0; i < iCols.Length; i++)
                {
                    string s = con.ds.Tables[0].Columns[i].ColumnName;
                    for (int n = 0; n < ColNM.Length; n++)
                    {
                        if (s == ColNM[n])
                        {
                            iCols[i] = n;
                            break;
                        }
                    }
                }

                // iWs に iWsの順番に基づいた数値を代入する
                for (int i = 0; i < iCols.Length; i++)
                {
                    iWs[i] = ColWID[iCols[i]];
                }

                for (int i = 0; i < iCols.Length; i++)
                {
                    dgv.Columns[i].Width = iWs[i];
                }

                // 非表示
                for (int i = 0; i < iCols.Length; i++)
                {
                    iWs[i] = ColInV[iCols[i]];
                }
                for (int i = 0; i < iCols.Length; i++)
                {
                    if (iWs[i] == 0) dgv.Columns[i].Visible = true;
                    else dgv.Columns[i].Visible = false;
                }
                // 書式　小数点　// 書式 -1 何もなし、0 小数点0, 1=小数点１ 2小数点2
                for (int i = 0; i < iCols.Length; i++)
                {
                    iWs[i] = ColZezo[iCols[i]];
                }
                for (int i = 0; i < iCols.Length; i++)
                {
                    if (iWs[i] == 0) dgv.Columns[i].DefaultCellStyle.Format = "0";
                    if (iWs[i] == 1) dgv.Columns[i].DefaultCellStyle.Format = "0.0";
                    if (iWs[i] == 2) dgv.Columns[i].DefaultCellStyle.Format = "0.00";
                    if (iWs[i] == 3) dgv.Columns[i].DefaultCellStyle.Format = "0.000";
                    if (iWs[i] == 10) dgv.Columns[i].DefaultCellStyle.Format = "#,0";
                }
                // 書式　揃え
                for (int i = 0; i < iCols.Length; i++)
                {
                    iWs[i] = ColAli[iCols[i]];
                }
                for (int i = 0; i < iCols.Length; i++)
                {
                    // 正の場合は左揃え
                    if (iWs[i] > 0)
                    {
                        dgv.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                    }
                    // 0の場合は中央揃え
                    if (iWs[i] == 0)
                    {
                        dgv.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    }
                    // 負の場合は右揃え
                    //if (iAlign[i] < 0) dgv.Columns[icol[i]].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }

                #endregion
                dgv.ClearSelection();

                int w = System.Windows.Forms.Screen.GetWorkingArea(this).Width;
                int dgvw = fn.dgvWidth(dgv);
                int thisW = dgvw + dgv.Left + 40;
                if (thisW <= w)
                {
                    this.Width = thisW;
                    dgv.Width = dgvw;
                }
                else
                {
                    this.Width = w;
                    dgv.Width = w - dgv.Left - 40;
                }

                dgv.Visible = true;

                bLoad = false;
                SetDgvF(dgv0, dgvF);
                lblFillVal(Lbl, dgv);
            }
            catch (Exception ex)
            {
                MessageBox.Show("データ抽出の過程でエラーが発生しました。", ex.GetType().FullName);
                bLoad = false;
            }
        }

        private void lblFillVal(Label lbl, DataGridView dgv)
        {
            lbl.Text = dgv.Rows.Count.ToString();
            lbl.Left = dgv.Left + dgv.Width - lbl.Width;
            label2.Left = lbl.Left - label2.Width;
        }

        private void btn_Click(object sender, EventArgs e)
        {
            // button 6
            Button btn = (Button)sender;
            switch (btn.Name.Substring(6))
            {
                case "1": openURL(); break; // Manual
                case "2": GetData(dgv0, bs0, sGetList()); break; // 更新
                case "3": ChangeTerm(); break;
                case "4": ExportXls(dgv0); break;
                case "5": addRec(); break;
            }

        }

        private void addRec()
        {
            string[] snd = { "" };
            _ = F15_Receipt.ShowMiniForm(this, snd);
            int ir = dgv0.FirstDisplayedScrollingRowIndex;
            GetData(dgv0, bs0, sGetList());
            dgv0.FirstDisplayedScrollingRowIndex = ir;
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

            string fileNm = $"{lblCaption.Text}{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
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
                wb.SaveAs(fp);
                string smsg = string.Format(
                    "マイドキュメントに、「{0}」を作成しました。ファイルを開きますか？", fileNm);
                string[] sSet = { smsg, "" };
                string[] sRcv = promag_frm2.F05_YN.ShowMiniForm(sSet);
                fileNm = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\" + fileNm;
                if (sRcv[0].Length > 0) System.Diagnostics.Process.Start(fileNm);

            }
        }


        private void ChangeTerm()
        {
            string[] Snd = { $"{_sd:yyyy/MM/dd}", $"{_ed:yyyy/MM/dd}", "" };
            string[] Rcv = promag_frm.F06_SelDate.ShowMiniForm(this, Snd);
            if (Rcv[0].Length > 0)
            {
                _sd = DateTime.Parse(Rcv[0]);
                _ed = DateTime.Parse(Rcv[1]).AddDays(1);

                GetData(dgv0, bs0, sGetList());
            }
            label1.Text = $"期間：{_sd:yy/MM/dd} ~ {_ed:yy/MM/dd}";
        }


        private void openURL()
        {
            string s = @"\\10.100.10.20\share\www\manual\cont\";
            s += $"{DEF_CON.prjName}-{this.Name.Substring(0, 4)}.html";
            bool b = System.IO.File.Exists(s);
            if (!b) return;
            System.Diagnostics.Process ps = new System.Diagnostics.Process();
            ps.StartInfo.FileName = s;
            ps.Start();
        }

        private string sGetList()
        {
            string s =
              "SELECT"
             + " DATE_FORMAT(r.REG_DATE,'%Y/%m/%d') 確認日"
             + " ,r.LOT_NO 代表LotNo"
             + " ,r.SUPPLIER 製造元"
             + " ,COUNT(*) 確認数"
             + " ,r.SHIP_SEQ 受入番号"
             + " ,CONCAT(w.SEI, w.MEI) 確認者"
             + " ,r.iLOCATION ilcn"
             + " FROM kyoei.t_receipt r"
             + " LEFT JOIN kyoei.m_worker w ON r.REG_ID = w.WKER_ID AND w.LGC_DEL = '0'"
             + $" WHERE r.REG_DATE >= '{_sd:yyyy/MM/dd}' AND r.REG_DATE <= '{_ed:yyyy/MM/dd}'"
             + "  GROUP BY DATE_FORMAT(r.REG_DATE,'%Y/%m/%d'), r.SUPPLIER,r.SHIP_SEQ"
             + " ORDER BY r.REG_DATE desc;";
            return s;
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
            int ir = e.RowIndex; int ic = e.ColumnIndex;
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

            string s0 = dgv.Rows[e.RowIndex].Cells["受入番号"].Value.ToString(); 
            string s1 = dgv.Rows[e.RowIndex].Cells["ilcn"].Value.ToString(); 
            string[] sendText = { s0 ,s1};

            _ = F15_Receipt.ShowMiniForm(this, sendText);
            
            GetData(dgv0, bs0, sGetList());
            
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
                string[] snd = { "「戻る」ボタンで画面を閉じてください。", "false" };
                _ = promag_frm2.F05_YN.ShowMiniForm(snd);
                // return;

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

        }

        #endregion

        private void dgv0_Scroll(object sender, ScrollEventArgs e)
        {
            //dgv0.HorizontalScrollingOffset = dgvF.HorizontalScrollingOffset;
            dgvF.HorizontalScrollingOffset = dgv0.HorizontalScrollingOffset;
        }

        private void dgv0_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            if (bLoad) return;
            SetWDgvF(dgv0, dgvF);
        }
    }
}
