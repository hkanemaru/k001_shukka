using ClosedXML.Excel;
using System;
using System.Windows.Forms;

namespace k001_shukka
{
    public partial class F14_SHIP_PERMIT_LIST : Form
    {
        #region フォーム変数
        private bool bClose = true;
        private string[] argVals;    // 親フォームから受け取る引数
        public string[] ReturnValue; // 親フォームに返す戻り値
        private bool bPHide = true;  // 親フォームを隠す = True
        ToolTip ToolTip1;
        private Boolean bdgvCellClk = false; // dgvがクリックされた際にデータ欄だとtrue
        private string sSD = string.Empty;
        private string sED = string.Empty;
        private int iDisp = 0;
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

        #endregion

        public F14_SHIP_PERMIT_LIST(params string[] argVals)
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
            F14_SHIP_PERMIT_LIST f = new F14_SHIP_PERMIT_LIST(s);
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
            string sTitle = "出荷承認依頼一覧";
            #region 画面の状態を設定
            // ■■■ 親フォームを隠す設定 true => 隠す　default = 隠す。
            //bPHide = false; // 隠さない
            // 画面サイズ変更の禁止
            //this.MaximizeBox = false;

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

            // 開いた元フォームを隠す設定
            if (bPHide) this.Owner.Hide();

            // リスト抽出　>> 初期設定は直近1カ月
            sSD = DateTime.Today.AddDays(-14).ToShortDateString();
            sED = DateTime.Today.AddDays(60).ToShortDateString();
            label1.Text = string.Format("期間：{0} ~ {1}", sSD, sED);
            GetData(dgv0, bs0, sGetList()); // 一覧更新
        }

        private void ChgParameter()
        {
            if (iDisp == 0)
            {
                // ColAli  書式　1 LEFT 0CENTER,-1RIGHT
                // ColZezo 書式 -1 何もなし、0 小数点0, 1=小数点１ 2小数点2
                ColNM = new string[] { "SEQ", "出荷日", "納品日", "出荷先", "品名", "出荷数量", "依頼日", "依頼者", "業務承認", "業務承認者", "業務承認日", "品管承認", "品管承認者", "品管承認日", "製造承認", "製造承認者", "製造承認日" };
                ColWID = new int[] { 0, 90, 90, 200, 150, 100, 60, 60, 50, 60, 60, 50, 60, 60, 50, 60, 60 };
                ColAli = new int[] { -1, 1, 1, 1, 1, -1, 0, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1 };
                ColZezo = new int[] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
                ColInV = new int[] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
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
            dgvSelf.Columns[1].Frozen = true;
        }

        private void dgvF_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            string s = string.Empty;
            string s1 = string.Empty;
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
                        if (s1 == "0")
                        {
                            sFil += string.Format("AND ([{0}] = '' OR [{0}] IS NULL)", s);
                        }
                        else if (s1 == "1")
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

            if (sFil.Length > 0)
            {
                checkBox1.Checked = false;
                checkBox2.Checked = false;
                sFil = sFil.Substring(4);
            }

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
                fn.SetDGV(dgv, false, 80, true, 10, 10, 50, true, 40, DEF_CON.DBLUE, DEF_CON.LGreen);
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
                    _ = promag_frm.F05_YN.ShowMiniForm(Snd);
                    return;
                }
                bs.DataSource = con.ds.Tables[0];

                #region dgv書式設定                


                dgv.Columns[1].Frozen = true;
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
                for (int i = 0; i < iCols.Length; i++)
                {
                    if (iCols[i] == 19)
                    {
                        dgv0.Columns[i].ToolTipText = "出荷Flg:0 未, 1 準備済"; // , 2 済
                        break;
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
                        dgv0.Columns[i].ToolTipText = "フィルター：0 = 空白のみ選択, 1 = 空白を除外";
                    }
                    // 0の場合は中央揃え
                    if (iWs[i] == 0)
                    {
                        dgv.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        dgv0.Columns[i].ToolTipText = "フィルター：0 = 空白のみ選択, 1 = 空白を除外";
                    }
                    if (iCols[i] == 26)
                    {
                        dgv0.Columns[i].ToolTipText = "出荷Flg:0 未, 1 準備済"; // , 2 済
                    }
                    // 負の場合は右揃え
                    //if (iAlign[i] < 0) dgv.Columns[icol[i]].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                int iWd = fn.dgvWidth(dgv0);
                if (iWd + 21 > 1366)
                {
                    this.Width = 1366;
                    dgv.Width = this.Width - dgv.Left - 30;
                }
                else
                {
                    dgv.Width = iWd;
                    this.Width = dgv.Left + dgv.Width + 21;
                }
                #endregion
                dgv.ClearSelection();
                cb_Check();
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

        }

        private void btn_Click(object sender, EventArgs e)
        {
            // button 6
            Button btn = (Button)sender;
            switch (btn.Name.Substring(6))
            {
                case "1":

                    break; // Serch

                case "2": // 
                    GetData(dgv0, bs0, sGetList());
                    break;
                case "3": // AC
                    string[] Snd = { sSD, sED, "" };
                    string[] Rcv = promag_frm.F06_SelDate.ShowMiniForm(this, Snd);
                    if (Rcv[0].Length > 0)
                    {
                        sSD = Rcv[0];
                        sED = Rcv[1];

                        GetData(dgv0, bs0, sGetList());
                    }
                    label1.Text = string.Format("期間：{0} ~ {1}", sSD, sED);
                    break;
            }

        }

        private string sGetList()
        {
            string s = string.Empty;
            if (iDisp == 0)
            {
                s = string.Format(
                    "SELECT"
                 + "   si.SEQ"                            //0
                 + "   , DATE_FORMAT(si.SHIP_DATE, '%y/%m/%d') 出荷日"                            //1
                 + "   , DATE_FORMAT(si.DUE_DATE, '%y/%m/%d') 納品日"                            //2
                 + "   , si.DESTINATION 出荷先"                            //3
                 + "   , si.ITEM 品名"                            //4
                 + "   , si.SHIPMENT_QUANTITY 出荷数量"                            //5
                 + "   , DATE_FORMAT(sp.REQUEST_DATE, '%m/%d') 依頼日"                            //6
                 + "   , CONCAT(w.SEI, w.MEI) 依頼者"                            //7
                 + "   , CASE "
                 + "     WHEN sp.PERMISSION = '0' THEN '承認'"
                 + "     WHEN sp.PERMISSION = '1' THEN '否認'"
                 + "     ELSE '-' "
                 + "     END 業務承認"                            //8
                 + "   , CONCAT(pw.SEI, pw.MEI) 業務承認者"                            //9
                 + " , DATE_FORMAT(sp.PERMIT_DATE, '%m/%d') 業務承認日"                            //10
                 + "   , CASE "
                 + "     WHEN sp.I_PERMIT = '0' THEN '承認'"
                 + "     WHEN sp.I_PERMIT = '1' THEN '否認'"
                 + "     ELSE '-' "
                 + "     END 品管承認"                            //11
                 + "   , CONCAT(iw.SEI, iw.MEI) 品管承認者"                            //12
                 + " , DATE_FORMAT(sp.i_PERMIT_DATE, '%m/%d') 品管承認日"                            //13
                 + "   , CASE "
                 + "     WHEN sp.APPROVAL = '0' THEN '承認' "
                 + "     WHEN sp.APPROVAL = '1' THEN '否認'"
                 + "     ELSE '-' "
                 + "     END 製造承認"                            //14
                 + "   , CONCAT(aw.SEI, aw.MEI) 製造承認者"                            //15
                 + " , DATE_FORMAT(sp.APPROVE_DATE, '%m/%d') 製造承認日"                            //16
                 + " FROM"
                 + "   kyoei.t_shipment_inf si "
                 + "   LEFT JOIN kyoei.t_ship_permission sp "
                 + "     ON si.SEQ = sp.S_SEQ "
                 + "   LEFT JOIN kyoei.m_worker w "
                 + "     ON sp.REQUEST_ID = w.WKER_ID AND w.LGC_DEL = '0' "
                 + "   LEFT JOIN kyoei.m_worker pw "
                 + "     ON sp.PERMIT_ID = pw.WKER_ID AND pw.LGC_DEL = '0' "
                 + "   LEFT JOIN kyoei.m_worker iw "
                 + "     ON sp.I_PERMIT_ID = iw.WKER_ID AND iw.LGC_DEL = '0'"
                 + "   LEFT JOIN kyoei.m_worker aw "
                 + "     ON sp.APPROVE_ID = aw.WKER_ID AND aw.LGC_DEL = '0'"
                 + " WHERE "
                 + " si.SHIP_DATE >= '{0}'"
                 + " AND si.DUE_DATE >= '2022/06/01'"
                 + " AND si.SHIP_DATE <= '{1}'"
                 + " ORDER BY Si.SHIP_DATE,si.DUE_DATE "
                 + " ;"
                    , sSD, sED);
            }

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

            string s0 = dgv.Rows[e.RowIndex].Cells[0].Value.ToString(); //
            string s1 = dgv.Rows[e.RowIndex].Cells[1].Value.ToString();
            string s2 = dgv.Rows[e.RowIndex].Cells[3].Value.ToString();
            string s3 = dgv.Rows[e.RowIndex].Cells[4].Value.ToString();
            string s4 = dgv.Rows[e.RowIndex].Cells[5].Value.ToString();
            string s5 = dgv.Rows[e.RowIndex].Cells[0].Value.ToString();
            // "SEQ", "出荷日", "納品日", "出荷先", "品名", "出荷数量", "依頼日", "依頼者", "承認", "承認者" };
            string[] sRel = { s0, "20" + s1, s2, s3, s4 };
            _ = F09_Permit.ShowMiniForm(this, sRel);

            GetData(dgv0, bs0, sGetList()); // 一覧更新

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
            TextBox tb = (TextBox)sender;
            if (tb.Text.Length == 0)
            {
                bs0.Filter = "";
                lblFillVal(label8, dgv0);
                return;
            }
            if (tb.Name == "textBox1")
            {
                string s = tb.Text;
                bs0.Filter = string.Format("充填時間 LIKE '%{0}%'", s);
                lblFillVal(label8, dgv0);
            }
        }

        private void dgv0_Scroll(object sender, ScrollEventArgs e)
        {
            dgv0.HorizontalScrollingOffset = dgvF.HorizontalScrollingOffset;
        }

        private void dgv0_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            if (bLoad) return;
            SetWDgvF(dgv0, dgvF);
        }

        private void rB_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            if (!rb.Checked) return;
            if (rb.Name == "rb1") iDisp = 0;
            if (rb.Name == "rb2") iDisp = 1;
            if (rb.Name == "rb3") iDisp = 2;
            GetData(dgv0, bs0, sGetList()); // 一覧更新
        }

        private void cB_CheckedChanged(object sender, EventArgs e)
        {
            //CheckBox cb = (CheckBox)sender;
            cb_Check();
            lblFillVal(label8, dgv0);
        }

        private void cb_Check()
        {
            string s = string.Empty;
            if (checkBox1.Checked) s = " AND 依頼日 <> ''";
            if (checkBox2.Checked) s += " AND 製造承認日 IS NULL";
            if (s.Length > 0) s = s.Substring(5);
            bs0.Filter = s;
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
            string fileNm = string.Format("{1}出荷承認一覧{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHmmss"), argVals[0]);
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

        private void button1_Click(object sender, EventArgs e)
        {
            ExportXls(dgv0);
        }
    }
}
