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
        int _dsp = 0;
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
            string sTitle = "受入確認一覧(受入単位)";

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
            GetData(dgv0, bs0, sGetList("")); // 一覧更新

            button1.Left = this.Width - button1.Width - 20;
            lblRCaption.Left = button1.Left - lblRCaption.Width;

            string picFl = "del_lred"
                + ",add"
                + ",excel"
                + ",CngCal"
                + ",reflesh"
                + ",dispCng"
                + ",soko_track"
                + ",okiba"
                + ",search2"
                ;
            fn.imageFileCopy(picFl);
            btnImageSetting();
        }

        private void btnImageSetting()
        {
            string fld = @"C:\tetra\img\";
            try
            {
                this.button7.Image = System.Drawing.Image.FromFile($"{fld}del_lred.png");
                this.button5.Image = System.Drawing.Image.FromFile($"{fld}add.png");
                this.button4.Image = System.Drawing.Image.FromFile($"{fld}excel.png");
                this.button3.Image = System.Drawing.Image.FromFile($"{fld}CngCal.png");
                this.button2.Image = System.Drawing.Image.FromFile($"{fld}reflesh.png");
                this.button8.Image = System.Drawing.Image.FromFile($"{fld}dispCng.png");
                this.button9.Image = System.Drawing.Image.FromFile($"{fld}soko_track.png");
                this.button10.Image = System.Drawing.Image.FromFile($"{fld}okiba.png");
                this.button11.Image = System.Drawing.Image.FromFile($"{fld}search2.png");
            }
            catch (Exception ex)
            {
                string[] snd = { ex.Message, "false" };
                _ = promag_frm2.F05_YN.ShowMiniForm(snd);
            }
        }

        private void ChgParameter()
        {
            string LotNM = "LotNo";
            string grd = "グレード";
            if (_dsp == 0)
            {
                LotNM = "代表LotNo";
                grd = "代表GRD";
            }
            string s =
                "確認日,120,1,-1,0;"
                + $"{LotNM},130,1,-1,0;"

                + $"{grd},80,1,-1,0;"
                + "製造元,150,1,-1,0;"
                + "確認数,50,-1,-1,0;"
                + "出荷番号,80,-1,-1,0;"
                + "倉庫名,100,1,-1,0;"
                + "置場, 100,1,-1,0;"
                + "確認者,120,1,-1,0;"
                + "ilcn,0,-1,-1,1;"
                + "SEQ,0,-1,-1,1;";
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

        }

        private void btn_Click(object sender, EventArgs e)
        {
            // button 6
            Button btn = (Button)sender;
            switch (btn.Name.Substring(6))
            {
                case "1": openURL(); break; // Manual
                case "2": Disp_reflesh(); break; // 更新
                case "3": ChangeTerm(); break;
                case "4": ExportXls(dgv0); break;
                case "5": addRec(); break;
                case "6": importXls(); break;
                case "7": delRow(); break;
                case "8": DispChange(); break;
                case "9": moveSOKO();   break;
                case "10": moveOkiba(); break;
                case "11": Find_Lot();  break;
            }

        }

        private void Find_Lot()
        {
            // 0>Title 1> 説明 2> textbox1初期値 3> 幅(選択) 4> MultiLine 0->off 1->ON
            string lot;
            if (true)
            {
                string[] snd = { "Lot検索", "", "", "", "0" };
                string[] rcv = promag_frm2.F08_BIKOU.ShowMiniForm(this, snd);
                if (rcv[0].Length == 0) return;
                lot = rcv[0];
            }
            GetData(dgv0, bs0, sGetList(lot));
        }


        private void Disp_reflesh()
        {
            GetData(dgv0, bs0, sGetList(""));
        }

        private void moveOkiba()
        {
            if (dgv0.SelectedRows.Count <= 0)
            {
                string[] snd = { "一覧から置場移動するLotを選んでください。", "false" };
                _ = promag_frm2.F05_YN.ShowMiniForm(snd); return;
            }
            // string okb0 = _okiba;
            string lot = string.Empty;
            string seqs = string.Empty;
            string iLcn = string.Empty;
            foreach (DataGridViewRow r in dgv0.SelectedRows)
            {
                string soko = $"{dgv0.Rows[r.Index].Cells["倉庫名"].Value}";
                string okiba = $"{dgv0.Rows[r.Index].Cells["置場"].Value}";
                if(okiba.Length > 0)
                {
                    string[] snd = { "置場が既に決定しているものは置場移動を実施します。", "false" };
                    _ = promag_frm2.F05_YN.ShowMiniForm(snd); return;
                }
                if(soko.IndexOf("小山工場") < 0)
                {
                    string[] snd = { "小山限定です。", "false" };
                    _ = promag_frm2.F05_YN.ShowMiniForm(snd); return;
                }
                if (_dsp == 0)
                {
                    seqs += $",{dgv0.Rows[r.Index].Cells["出荷番号"].Value}";
                    if(iLcn.Length > 0 && iLcn != $"{dgv0.Rows[r.Index].Cells["ilcn"].Value}")
                    {
                        string[] snd = { "複数登録する場合は同じ入荷先である必要があります。", "false" };
                        _ = promag_frm2.F05_YN.ShowMiniForm(snd); return;
                    }
                    iLcn = $"{dgv0.Rows[r.Index].Cells["ilcn"].Value}";
                }
                else lot += $",'{dgv0.Rows[r.Index].Cells["LotNo"].Value}'";
            }
            string s;
            s =
                "SELECT "
                 // + " -- bl.DISP_SHTNM"
                 + " okb.SEQ"
                 + " ,CASE WHEN okb.COL_CNT <= 1 THEN CONCAT(bl.DISP_SHTNM,okb.OKIBARNM)"
                 + " ELSE CONCAT(okb.OKIBARNM,'-',CAST(CHAR(d + 65) AS CHAR)) END 置場"
                 + " ,("
                 + " SELECT COUNT(*) "
                 + " FROM t_k_okiba kok "
                 + " WHERE kok.OKIBA_SEQ = CASE WHEN okb.COL_CNT <= 1  THEN okb.SEQ  ELSE CONCAT(okb.OKIBARNM, '-', CAST(CHAR (d + 65) AS CHAR)) END"
                 + " AND kok.LGC_DEL = '0'"
                 + " ) 使用済"
                 + " ,okb.CAPACITY Capa"
                 + " FROM kyoei.m_k_okiba okb "
                 + " LEFT JOIN kyoei.m_code bl   ON bl.KEY_CD = okb.BUILL_KBN AND bl.SECTION_CD = 182"
                 + "  JOIN ("
                 + " SELECT"
                 + " d1.DIGIT * 10 + d2.DIGIT d"
                 + " FROM"
                 + " m_digit d1 join m_digit d2 "
                 + " ) digit"
                 + " WHERE okb.LOCATION = 2"
                 + " AND digit.d < okb.COL_CNT"
                // + " AND bl.KEY_CD = 7" // AND bl.KEY_CD = 7
                 + " ORDER BY okb.SEQ,digit.d"
                 + " ;";
            mydb.kyDb con = new mydb.kyDb();

            if (true)
            {
                // if (okb0.Length > 0) okb0 = $" 現置場:{okb0}";
                // 移動先の一覧を表示
                string[] snd = { $"置場指定", s, "0", "", usr.id, "", $"{usr.iDB}" };
                string[] rcv = promag_frm2.F02_SEL_LIST.ShowMiniForm(this, snd);
                if (rcv[0].Length == 0) return;
                string okbSeq = rcv[0];
                string okb = rcv[1];

                string[] snd1 = { "置場を設定します。", "" };
                string[] rcv1 = promag_frm2.F05_YN.ShowMiniForm(snd1);
                if (rcv1[0].Length == 0) return;
                
                if(lot.Length > 0) lot = lot.Substring(1);
                if(seqs.Length > 0) seqs = seqs.Substring(1);
                string whr;
                if (_dsp == 0)
                {
                    if(iLcn.Length == 0) whr = $"p.SHIP_SEQ IN ({seqs}) AND p.iLocation IS NULL";
                    else whr = $"p.SHIP_SEQ IN ({seqs}) AND p.iLocation = {iLcn}";
                }
                else whr = $"p.LOT_NO IN ({lot})";
                string insOkb;
                insOkb = "INSERT INTO t_k_okiba (OKIBA_SEQ,OKIBANM,LOT_NO,QUANTITY,UPD_ID,UPD_DATE,LGC_DEL) "
                       + $"SELECT {okbSeq},'{okb}',p.LOT_NO,1,'{usr.id}',NOW(),'0'"
                       + " FROM t_receipt p"
                       + " LEFT JOIN t_k_okiba ko ON p.LOT_NO = ko.LOT_NO AND ko.LGC_DEL = '0'"
                       + $" WHERE {whr} AND ko.SEQ IS NULL AND p.INPUT_SEQ IS NULL";

                //insOkb += "INSERT INTO t_k_okiba (OKIBA_SEQ,OKIBANM,LOT_NO,QUANTITY,UPD_ID,UPD_DATE,LGC_DEL) VALUES"
                //    + $"({okbSeq},'{okb}','{lot}',1,'{usr.id}',NOW(),'0')";
                // string[] Ins = insOkb.Split(';');
                con.ExecSql(false, DEF_CON.Constr(), insOkb);
                int ifr = dgv0.FirstDisplayedScrollingRowIndex;
                GetData(dgv0, bs0, sGetList(""));
                if (ifr < dgv0.Rows.Count) dgv0.FirstDisplayedScrollingRowIndex = ifr;
            }
        }

        private void moveSOKO()
        {
            string lcn;
            if (dgv0.SelectedRows.Count == 0)
            {
                string[] snd = { "移動対象を選択して下さい。", "false" };
                _ = promag_frm2.F05_YN.ShowMiniForm(snd);return;
            }
            if (true)
            { // 0= tt, 1 = sql, 2 = vis, 3 = ali_left, 4 = wkerID, 5=SECTION_CD,6=usr.iDB
                string[] snd = { "移動先を選択して下さい。受入単位の場合は「出荷番号」Lot単位の場合は「Lot番号」を基準に移動します。", "" };
                string[] rcv = promag_frm2.F05_YN.ShowMiniForm(snd);
                if (rcv[0].Length == 0) return;

                string s = "SELECT"
                 + " b.BASE_SEQ 倉庫CD"
                 + " ,IFNULL(b.BASE_NICKNM,b.BASE_NM) 倉庫名"
                 + " FROM kyoei.m_base b"
                 + " WHERE b.LGC_DEL = '0'"
                 + " AND b.BASE_KBN IN (0,1)"
                 + " AND b.BASE_SEQ NOT IN (0,7,5,44,49,48,47,45,43)"
                 + " ;";
                string tt = "移動先選択";
                string[] sd = { tt, s, "", "", usr.id, "", $"{usr.iDB}" };
                string[] rv = promag_frm2.F02_SEL_LIST.ShowMiniForm(this, sd);
                if (rv[0].Length == 0) return;
                lcn = rv[0];
            }
            string Nos = string.Empty;
            string Cols = string.Empty;
            foreach(DataGridViewRow r in dgv0.SelectedRows)
            {
                if(_dsp == 0)
                {
                    Nos += $",{dgv0.Rows[r.Index].Cells["出荷番号"].Value}";
                    string sup = $"{dgv0.Rows[r.Index].Cells["製造元"].Value}";
                    Cols = $"SUPPLIER = '{sup}' AND SHIP_SEQ IN ";
                }
                else
                {
                    Nos += $",{dgv0.Rows[r.Index].Cells["SEQ"].Value}";
                    Cols = "SEQ IN ";
                }
            }

            if (Nos.Length > 0)
            {
                Nos = Nos.Substring(1);
                string upd = $"UPDATE t_receipt SET LOCATION = {lcn}, UPD_ID = '{usr.id}', UPD_DATE = NOW() WHERE {Cols} ({Nos});";

                upd += $"UPDATE t_k_okiba SET LGC_DEL = '1', UPD_ID = '{usr.id}', UPD_DATE = NOW() "
                    + $" WHERE LOT_NO IN (SELECT LOT_NO FROM t_receipt WHERE {Cols} ({Nos}));";
                mydb.kyDb con = new mydb.kyDb();
                int ifr = dgv0.FirstDisplayedScrollingRowIndex;
                con.ExecSql(false, DEF_CON.Constr(), upd);
                GetData(dgv0, bs0, sGetList(""));
                dgv0.FirstDisplayedScrollingRowIndex = ifr;
            }

        }

        private void DispChange()
        {
            _dsp++;
            if (_dsp == 2) _dsp = 0;

            string sTitle = "受入確認一覧(受入単位)";
            if(_dsp==1)sTitle = "受入確認一覧(Lot単位)";

            lblCaption.Text = fn.frmTxt(sTitle);
            GetData(dgv0, bs0, sGetList(""));
        }

        private void delRow()
        {
            if(dgv0.SelectedRows.Count == 0)
            {
                string[] snd = { "一覧から削除する行を選択して下さい", "false" };
                _ = promag_frm2.F05_YN.ShowMiniForm(snd);return;
            }
            if (true)
            {
                string[] snd = { "一覧から削除します。", "" };
                string[] rcv = promag_frm2.F05_YN.ShowMiniForm(snd);
                if (rcv[0].Length == 0) return;
            }
            string del = string.Empty;
            foreach (DataGridViewRow r in dgv0.SelectedRows)
            {
                /* + " DATE_FORMAT(r.REG_DATE,'%Y/%m/%d') 確認日"
             + " ,r.LOT_NO 代表LotNo"
             + " ,r.SUPPLIER 製造元"
             + " ,COUNT(*) 確認数"
             + " ,r.SHIP_SEQ 受入番号"
             + " ,CONCAT(w.SEI, w.MEI) 確認者"
             + " ,r.iLOCATION ilcn"  */
                string Sseq = $"{dgv0.Rows[r.Index].Cells["出荷番号"].Value}";
                string Supp = $"{dgv0.Rows[r.Index].Cells["製造元"].Value}";
                if (Supp.Length == 0) Supp = "SUPPLIER IS NULL";
                else Supp = $" SUPPLIER = '{Supp}'";
                del += $"UPDATE t_receipt SET LGC_DEL = '1',UPD_ID = '{usr.id}', UPD_DATE = NOW() "
                    + $" WHERE {Supp} AND SHIP_SEQ = {Sseq};";
                
            }
            if(del.Length > 0)
            {
                mydb.kyDb con = new mydb.kyDb();
                string msg = con.ExecSql(true, DEF_CON.Constr(), del);
                if(msg.IndexOf("エラー") < 0)
                {
                    string[] snd = { msg, "false" };
                    _ = promag_frm2.F05_YN.ShowMiniForm(snd);
                    int ifr = dgv0.FirstDisplayedScrollingRowIndex;
                    GetData(dgv0, bs0, sGetList("")); // 一覧更新
                    if(ifr < dgv0.Rows.Count)
                    {
                        dgv0.FirstDisplayedScrollingRowIndex = ifr;
                    }
                }
            }
        }

        private void importXls()
        {
            string lcn;
            if (true)
            { // 0= tt, 1 = sql, 2 = vis, 3 = ali_left, 4 = wkerID, 5=SECTION_CD,6=usr.iDB
                string[] snd = {"搬入先を選択して下さい。","" };
                string[] rcv = promag_frm2.F05_YN.ShowMiniForm(snd);
                if (rcv[0].Length == 0) return;

                string s = "SELECT"
                 + " b.BASE_SEQ 倉庫CD"
                 + " ,IFNULL(b.BASE_NICKNM,b.BASE_NM) 倉庫名"
                 + " FROM kyoei.m_base b"
                 + " WHERE b.LGC_DEL = '0'"
                 + " AND b.BASE_KBN IN (0,1)"
                 + " AND b.BASE_SEQ NOT IN (0,7,5,44,49,48,47,45,43)"
                 + " ;";
                string tt = "搬入先選択";
                string[] sd = { tt, s, "", "", usr.id, "", $"{usr.iDB}" };
                string[] rv = promag_frm2.F02_SEL_LIST.ShowMiniForm(this, sd);
                if (rv[0].Length == 0) return;
                lcn = rv[0];
            }
            if (true)
            {
                string[] snd = {"J&T環境から送付されたエクセルファイルを取込みます。"
                + "\r\n「製品原料紐付一覧...」を指定して下さい。",""};
                string[] rcv = promag_frm2.F05_YN.ShowMiniForm(snd);
                if (rcv[0].Length == 0) return;
            }
            //OpenFileDialogクラスのインスタンスを作成
            OpenFileDialog ofd = new OpenFileDialog();
            #region oFDの設定
            //はじめのファイル名を指定する
            //はじめに「ファイル名」で表示される文字列を指定する
            ofd.FileName = "";
            //はじめに表示されるフォルダを指定する
            //指定しない（空の文字列）の時は、現在のディレクトリが表示される
            ofd.InitialDirectory = @"C:\";
            ofd.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            //[ファイルの種類]に表示される選択肢を指定する
            //指定しないとすべてのファイルが表示される
            ofd.Filter = "EXCELファイル(*.xlsx)|*.xlsx;*.xls";
            //[ファイルの種類]ではじめに選択されるものを指定する
            //2番目の「すべてのファイル」が選択されているようにする
            ofd.FilterIndex = 1;
            //タイトルを設定する
            ofd.Title = "開くファイルを選択してください";
            //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
            ofd.RestoreDirectory = true;
            //存在しないファイルの名前が指定されたとき警告を表示する
            //デフォルトでTrueなので指定する必要はない
            ofd.CheckFileExists = true;
            //存在しないパスが指定されたとき警告を表示する
            //デフォルトでTrueなので指定する必要はない
            ofd.CheckPathExists = true;
            #endregion
            //ダイアログを表示する
            if (ofd.ShowDialog() == DialogResult.Cancel) return;
            // ワークブックの読み込み
            try
            {
                using (var wb = new ClosedXML.Excel.XLWorkbook(ofd.FileName))
                {
                    try
                    {
                        this.Refresh();
                        // シートを確認
                        int iMatch = 0;
                        foreach (var ws in wb.Worksheets)
                        {
                            if (ws.Name == "Measure") iMatch++;
                            if (ws.Name == "Inspect") iMatch++;
                            if (ws.Name == "Himoduke") iMatch++;
                        }
                        if (iMatch < 3)
                        {
                            string[] snd = { "ファイルの種類が違います。", "false" };
                            _ = promag_frm2.F05_YN.ShowMiniForm(snd);
                            return;
                        }
                        // すべてのシートについてループ
                        foreach (var ws in wb.Worksheets)
                        {
                            // ws.Nameが小山,MRF,出荷ロット一覧表 の時に処理に入る RET1 Measure Inspect Himoduke
                            // まとめて書き込むSQLを定義
                            string sSql = string.Empty; // 全体
                            string sIns = string.Empty; // Insertの頭
                            string sVal = string.Empty; // value部
                            if (ws.Name == "Measure")
                            {
                                // int irow = 0;
                                // テーブル作成
                                var table = ws.RangeUsed().AsTable();

                                #region 参考：セル指定方法／行数と列数の取得
                                // ws.Cell(ir, ic + 1).Value = 1
                                // ws.Cell("D4").Value = "Range:D4"; 
                                // 参考：行数と列数の取得
                                // var ir = ws.RangeUsed().RowCount();
                                // var ic = ws.RangeUsed().ColumnCount();
                                #endregion

                                // Measure の列は4
                                // ITEM_SEQ	LOT_NO	MVALUE	INSPECT_SEQ
                                #region Excel操作 DB登録用変数定義
                                var ic = ws.RangeUsed().ColumnCount();
                                // 書込み済みフラグを記入する列番号
                                var ir = ws.RangeUsed().RowCount();

                                sSql = string.Empty; sIns = string.Empty; sVal = string.Empty;
                                #endregion

                                sIns = "INSERT INTO t_f_measured_value ("
                                        + "LOT_NO, M_VAL, ITEM_SEQ, INSPECT_SEQ, LOCATION, UPD_ID, UPD_DATE, LGC_DEL) VALUES ";

                                mydb.kyDb con = new mydb.kyDb();

                                // データを行単位で取得
                                foreach (var dataRow in table.DataRange.Rows())
                                {
                                    // 行ごとの処理
                                    // if (irow == 0) { irow++; continue; }
                                    #region 変数定義
                                    int itemseq = int.Parse($"{dataRow.Cell(1).Value}");
                                    string LotNo = dataRow.Cell(2).Value.ToString();
                                    string val = dataRow.Cell(3).Value.ToString();
                                    string insSeq = dataRow.Cell(4).Value.ToString();
                                    #endregion
                                    #region 検査項目番号の変換 JTK => JPT
                                    switch (itemseq)
                                    {
                                        case 1: itemseq = 15; break;
                                        case 2: itemseq = 15; break;
                                        case 3: itemseq = 7; break;
                                        case 4: itemseq = 8; break;
                                        case 5: itemseq = 8; break;

                                        case 6: itemseq = 9; break;
                                        case 7: itemseq = 11; break;
                                        case 8: itemseq = 13; break;
                                        case 9: itemseq = 8; break;
                                        case 10: itemseq = 2; break;
                                        case 11: itemseq = 5; break;
                                        case 12: itemseq = 1; break;
                                    }
                                    #endregion

                                    if (val.Length == 0) val = "NULL";
                                    if (insSeq.Length == 0) insSeq = "NULL";

                                    /*
                                    SEQ	SEQ	int(10) unsigned auto_increment
                                    LOT_NO	LOT_NO	varchar(20)
                                    測定値	M_VAL	double
                                    測定日	MEASURING_DATE	datetime
                                    項目ID	ITEM_SEQ	smallint(5) unsigned
                                    採用FLG	ADOPTION	decimal(1,0)
                                    検査SEQ	INSPECT_SEQ	int(10) unsigned
                                    拠点番号	LOCATION	tinyint(3) unsigned          jtk = 24
                                    合否	SUCCESS	char(1)
                                    備考	BIKOU_SEQ	int(10) unsigned
                                    更新者ID	UPD_ID	varchar(5)
                                    更新日	UPD_DATE	datetime
                                    論理削除	LGC_DEL	char(1)                                 */
                                    if (LotNo.Length > 0)
                                    {
                                        sVal += $",('{LotNo}',{val},{itemseq},{insSeq},24,'{usr.id}',NOW(), '0')";
                                    }
                                }
                                #region foreachが完了し残りをInsする
                                if (sVal.Length > 0)
                                {
                                    sSql = sIns + sVal.Substring(1) + ";";
                                    sSql += "DELETE FROM t_f_measured_value "
                                        + " WHERE LOT_NO LIKE 'J%' AND SEQ NOT IN ("
                                        + "SELECT mxSEQ from (SELECT MAX(SEQ) mxSEQ FROM t_f_measured_value m GROUP BY m.ITEM_SEQ,m.LOT_NO,m.INSPECT_SEQ) tmp)";
                                    string[] sql = sSql.Split(';');
                                    con.ExecSql(false, DEF_CON.Constr(), sql);
                                }
                                #endregion
                            }
                            else if (ws.Name == "Inspect")
                            {
                                // int irow = 0;
                                // テーブル作成
                                var table = ws.RangeUsed().AsTable();
                                sSql = string.Empty; sIns = string.Empty; sVal = string.Empty;
                                #region 参考：セル指定方法／行数と列数の取得
                                // ws.Cell(ir, ic + 1).Value = 1
                                // ws.Cell("D4").Value = "Range:D4"; 
                                // 参考：行数と列数の取得
                                // var ir = ws.RangeUsed().RowCount();
                                // var ic = ws.RangeUsed().ColumnCount();
                                #endregion

                                // SEQ	INSPECT_WEIGHT	RET  // Inspect の列は3
                                #region Excel操作 DB登録用変数定義
                                var ic = ws.RangeUsed().ColumnCount();
                                // 書込み済みフラグを記入する列番号
                                var ir = ws.RangeUsed().RowCount();

                                #endregion

                                sIns = "INSERT INTO t_f_inspect ("
                                        + "INSPECT_WEIGHT, RET, ADD_SEQ, LOCATION,REG_ID,REG_DATE, UPD_ID, UPD_DATE, LGC_DEL) VALUES ";

                                // 行数カウント
                                mydb.kyDb con = new mydb.kyDb();

                                // データを行単位で取得
                                foreach (var dataRow in table.DataRange.Rows())
                                {
                                    // 行ごとの処理
                                    // if (irow == 0) { irow++; continue; }
                                    #region 変数定義
                                    int iSeq = int.Parse($"{dataRow.Cell(1).Value}");
                                    string iWeight = dataRow.Cell(2).Value.ToString();
                                    string ret = dataRow.Cell(3).Value.ToString();

                                    #endregion

                                    if (iWeight.Length == 0) iWeight = "NULL";
                                    if (ret.Length == 0) ret = "NULL";

                                    /*
                                    SEQ	SEQ	int(10) unsigned auto_increment
                                    検査重量	INSPECT_WEIGHT	double
                                    色目判定	COLOR_RET	char(1)
                                    判定結果	RET	tinyint(3) unsigned
                                    ランク	_RANK	char(1)
                                    備考SEQ	BIKOU_SEQ	int(10) unsigned
                                    追加SEQ	ADD_SEQ	int(10) unsigned
                                    拠点番号	LOCATION	smallint(5) unsigned
                                    登録者ID	REG_ID	char(5)
                                    登録日時	REG_DATE	datetime
                                    更新者ID	UPD_ID	char(5)
                                    更新日時	UPD_DATE	datetime
                                    削除フラグ	LGC_DEL	char(1)                          */
                                    if (iSeq > 0)
                                    {
                                        sVal += $",({iWeight},{ret},{iSeq},24,'{usr.id}',NOW(),'{usr.id}',NOW(), '0')";
                                    }
                                }
                                #region foreachが完了し残りをInsする
                                if (sVal.Length > 0)
                                {
                                    sSql = sIns + sVal.Substring(1) + ";";
                                    sSql += "DELETE FROM t_f_inspect "
                                        + " WHERE ADD_SEQ IS NOT NULL AND LOCATION IS NOT NULL"
                                        + " AND SEQ NOT IN ("
                                        + "SELECT mxSEQ from ("
                                        + "SELECT MAX(SEQ) mxSEQ FROM t_f_inspect m WHERE m.ADD_SEQ IS NOT NULL AND m.LOCATION IS NOT NULL GROUP BY m.INSPECT_WEIGHT,m.ADD_SEQ"
                                        + ") tmp)";
                                    string[] sql = sSql.Split(';');
                                    con.ExecSql(false, DEF_CON.Constr(), sql);
                                }
                                #endregion
                            }
                            else if (ws.Name == "Himoduke")
                            {
                                // int irow = 0;
                                // テーブル作成
                                var table = ws.RangeUsed().AsTable();
                                sSql = string.Empty; sIns = string.Empty; sVal = string.Empty;
                                #region 参考：セル指定方法／行数と列数の取得
                                // ws.Cell(ir, ic + 1).Value = 1
                                // ws.Cell("D4").Value = "Range:D4"; 
                                // 参考：行数と列数の取得
                                // var ir = ws.RangeUsed().RowCount();
                                // var ic = ws.RangeUsed().ColumnCount();
                                #endregion

                                #region Excel操作 DB登録用変数定義
                                var ic = ws.RangeUsed().ColumnCount();
                                // 書込み済みフラグを記入する列番号
                                var ir = ws.RangeUsed().RowCount();

                                #endregion

                                sIns = "INSERT INTO t_material_source ("
                                     + "LOT_NO, BALE_LOT_NO, oWT, iWT,LOADING_DATE,REPRNM,TYPERNM,SHAPERNM,DENNO,UPD_ID,UPD_DATE,LGC_DEL) VALUES ";

                                mydb.kyDb con = new mydb.kyDb();

                                // データを行単位で取得
                                // LOT_NO	BALE_LOT_NO	oWT	iWT	LOADING_DATE	REPRNM	TYPERNM	SHAPERNM   // Himoduke の列は8
                                foreach (var dataRow in table.DataRange.Rows())
                                {
                                    // 行ごとの処理
                                    // if (irow == 0) { irow++; continue; }
                                    #region 変数定義
                                    string lotNo = dataRow.Cell(1).Value.ToString();
                                    string bLotNo = dataRow.Cell(2).Value.ToString();
                                    string owt = dataRow.Cell(3).Value.ToString();
                                    string iwt = dataRow.Cell(4).Value.ToString();
                                    string lodDt = dataRow.Cell(5).Value.ToString();
                                    string REPNM = dataRow.Cell(6).Value.ToString();
                                    string TYPNM = dataRow.Cell(7).Value.ToString();
                                    string SHAPE = dataRow.Cell(8).Value.ToString();
                                    string DENNO;
                                    if (ic < 9) DENNO = "NULL";
                                    else DENNO = $"'{dataRow.Cell(9).Value}'";
                                    #endregion
                                    sVal += $",('{lotNo}','{bLotNo}',{owt},{iwt},'{lodDt}','{REPNM}','{TYPNM}','{SHAPE}',{DENNO},'{usr.id}',NOW(),'0')";
                                }
                                #region foreachが完了し残りをInsする
                                if (sVal.Length > 0)
                                {
                                    sSql = sIns + sVal.Substring(1) + ";";
                                    sSql += "DELETE FROM t_material_source "
                                        + " WHERE SEQ NOT IN ("
                                        + "SELECT mxSEQ from ("
                                        + "SELECT MAX(SEQ) mxSEQ FROM t_material_source m GROUP BY m.LOT_NO,m.BALE_LOT_NO,oWT,iWT"
                                        + ") tmp)";
                                    string[] sql = sSql.Split(';');
                                    con.ExecSql(false, DEF_CON.Constr(), sql);
                                }
                                #endregion
                            }
                            else if (ws.Name == "LotList")
                            {
                                // int irow = 0;
                                // テーブル作成
                                var table = ws.RangeUsed().AsTable();
                                sSql = string.Empty; sIns = string.Empty; sVal = string.Empty;
                                #region 参考：セル指定方法／行数と列数の取得
                                // ws.Cell(ir, ic + 1).Value = 1
                                // ws.Cell("D4").Value = "Range:D4"; 
                                // 参考：行数と列数の取得
                                // var ir = ws.RangeUsed().RowCount();
                                // var ic = ws.RangeUsed().ColumnCount();
                                #endregion
                                bool bFull = false;
                                if (ws.RangeUsed().ColumnCount() == 5) bFull = true;
                                #region Excel操作 DB登録用変数定義
                                var ic = ws.RangeUsed().ColumnCount();
                                // 書込み済みフラグを記入する列番号
                                var ir = ws.RangeUsed().RowCount();

                                #endregion

                                sIns = "INSERT INTO t_receipt ("
                                     + "LOT_NO, SUPPLIER, GRADE, WEIGHT, iLOCATION,LOCATION,SHIP_SEQ,INSPECT_SEQ,REG_ID,REG_DATE,UPD_ID,UPD_DATE,LGC_DEL) VALUES ";

                                /* SEQ	        SEQ	        int(10) unsigned auto_increment
                                   LotNo	    LOT_NO	    varchar(20)
                                   仕入先	    SUPPLIER	varchar(40)
                                   グレード	GRADE	    varchar(20)
                                   入荷拠点	iLOCATION	smallint(5) unsigned
                                   荷受拠点	LOCATION	smallint(5) unsigned
                                   受入番号	SHIP_SEQ	int(10) unsigned
                                   登録者ID	REG_ID	    char(5)
                                   登録日時	REG_DATE	datetime
                                   更新者ID	UPD_ID	    char(5)
                                   更新日時	UPD_DATE	datetime
                                   削除フラグ	LGC_DEL	char(1)                      */

                                mydb.kyDb con = new mydb.kyDb();

                                // データを行単位で取得
                                // LOT_NO	GRADE	SHIP_SEQ                            // LotList の列は3
                                foreach (var dataRow in table.DataRange.Rows())
                                {
                                    // 行ごとの処理
                                    // if (irow == 0) { irow++; continue; }
                                    #region 変数定義
                                    string lotNo = dataRow.Cell(1).Value.ToString();
                                    string Grade = dataRow.Cell(2).Value.ToString();
                                    string sSeq = dataRow.Cell(3).Value.ToString();
                                    string sInspect = "NULL";
                                    int ic2 = ws.RangeUsed().ColumnCount();
                                    if (ic2 > 3) sInspect = dataRow.Cell(4).Value.ToString();
                                    string Supp = "Ｊ＆Ｔ環境";
                                    string iLoc = "24";
                                    string Loc = lcn;
                                    string wt = "500";
                                    if (bFull) wt = dataRow.Cell(5).Value.ToString().Replace(".0", "");

                                    #endregion
                                    sVal += $",('{lotNo}','{Supp}','{Grade}',{wt},{iLoc},{Loc},{sSeq},{sInspect},'{usr.id}',NOW(),'{usr.id}',NOW(),'0')";
                                }
                                #region foreachが完了し残りをInsする
                                if (sVal.Length > 0)
                                {
                                    sSql = sIns + sVal.Substring(1) + ";";
                                    sSql += "DELETE FROM t_receipt "
                                        + " WHERE SEQ NOT IN ("
                                        + "SELECT mxSEQ from ("
                                        + "SELECT MAX(SEQ) mxSEQ FROM t_receipt m GROUP BY m.LOT_NO"
                                        + ") tmp)";
                                    string[] sql = sSql.Split(';');
                                    con.ExecSql(false, DEF_CON.Constr(), sql);
                                }
                                #endregion
                            }
                            else continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("エラーが発生しました。登録が完全ではありません。\r\n"
                            + "エクセルファイルで確認して下さい。" + ex.Message);
                    }
                    finally
                    {
                        wb.Save();
                        string[] snd = { "取込終了", "false" };
                        _ = promag_frm2.F05_YN.ShowMiniForm(snd);
                    }
                }
            }
            catch
            {
                string[] snd = { "エクセルファイルが正常に取り込めませんでした。", "false" };
                _ = promag_frm2.F05_YN.ShowMiniForm(snd);
                return;
            }
        }

        private void addRec()
        {
            string[] snd = { "" };
            _ = F15_Receipt.ShowMiniForm(this, snd);
            int ir = dgv0.FirstDisplayedScrollingRowIndex;
            GetData(dgv0, bs0, sGetList(""));
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

                GetData(dgv0, bs0, sGetList(""));
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

        private string sGetList(string slot)
        {
            string Grp = "";
            string count = "1";
            string LotNM = "LotNo";
            string INPUT_COND = " AND IFNULL(r.INPUT_SEQ,0) = 0";
            string lot_COND = string.Empty;
            if (_dsp == 0)
            {
                Grp = "GROUP BY DATE_FORMAT(r.REG_DATE,'%Y/%m/%d'), r.SUPPLIER,r.SHIP_SEQ,r.LOCATION";
                count = "COUNT(*)";
                LotNM = "代表LotNo";
            }

            if (slot.Length > 0)
            {
                INPUT_COND = "";
                lot_COND = $" AND r.LOT_NO LIKE '%{slot}%'";
            }

            string s =
              "SELECT"
             + " DATE_FORMAT(r.REG_DATE,'%Y/%m/%d') 確認日"
             + $" ,r.LOT_NO {LotNM}"
             + " ,r.GRADE グレード"
             + " ,r.SUPPLIER 製造元"
             + $" ,{count} 確認数"
             + " ,r.SHIP_SEQ 出荷番号"
             + " ,CONCAT(b.BASE_SEQ,IFNULL(b.BASE_NICKNM,b.BASE_NM)) 倉庫名"
             + " ,ko.OKIBANM 置場"
             + " ,CONCAT(w.SEI, w.MEI) 確認者"
             + " ,r.iLOCATION ilcn"
             + " ,r.SEQ"
             + " FROM kyoei.t_receipt r"
             + " LEFT JOIN kyoei.m_worker w ON r.REG_ID = w.WKER_ID AND w.LGC_DEL = '0'"
             + " LEFT JOIN kyoei.m_base b ON r.LOCATION = b.BASE_SEQ"
             + " LEFT JOIN kyoei.t_k_okiba ko ON ko.LOT_NO = r.LOT_NO AND ko.LGC_DEL = '0'"
             + $" WHERE 1 = 1"
             // + $" AND r.REG_DATE >= '{_sd:yyyy/MM/dd}' AND r.REG_DATE <= '{_ed:yyyy/MM/dd}'"
             + " AND r.LGC_DEL = '0'"
             + $" AND r.LOCATION != 54{INPUT_COND}{lot_COND}"

             + $" {Grp}"
             + " ORDER BY r.REG_DATE desc, r.SHIP_SEQ,r.LOT_NO;";
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

            string s0 = dgv.Rows[e.RowIndex].Cells["出荷番号"].Value.ToString(); 
            string s1 = dgv.Rows[e.RowIndex].Cells["ilcn"].Value.ToString(); 
            string[] sendText = { s0 ,s1};

            _ = F15_Receipt.ShowMiniForm(this, sendText);
            
            GetData(dgv0, bs0, sGetList(""));
            
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

            string s =
                "UPDATE kyoei.t_receipt r"
                 + " , kyoei.mj_product p"
                 + "  SET r.GRADE = p.GRADE"
                 + "  WHERE p.LOT_NO = r.LOT_NO AND r.GRADE IS NULL"
                 + " ;"
                 + " UPDATE kyoei.t_receipt r"
                 + " , kyoei.kj_t_product p"
                 + "  SET r.GRADE = p.GRADE"
                 + "  WHERE p.LOT_NO = r.LOT_NO AND r.GRADE IS NULL"
                 + " ;"
                 + " UPDATE kyoei.t_receipt r"
                 + " , kyoei.mj_u_product p"
                 + "  SET r.GRADE = p.GRADE"
                 + "  WHERE p.LOT_NO = r.LOT_NO AND r.GRADE IS NULL"
                 + " ;";
            mydb.kyDb con = new mydb.kyDb();
            con.ExecSql(false, DEF_CON.Constr(), s);
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

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
