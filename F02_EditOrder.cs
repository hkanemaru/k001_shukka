using System;
using System.Windows.Forms;
using iTextSharp.text.pdf;
using iTextSharp.text;
//iTextSharp.text.FontクラスがSystem.Drawing.Fontクラスと
//混在するためiFontという別名を設定
using iFont = iTextSharp.text.Font;
using System.IO;
using System.Diagnostics;
using System.Collections;

namespace k001_shukka
{
    public partial class F02_EditOrder : Form
    {
        #region フォーム変数
        private Boolean bClose = true;
        private string[] argVals; // 親フォームから受け取る引数
        public string[] ReturnValue;            // 親フォームに返す戻り値
        private Boolean bPHide = true;  // 親フォームを隠す = True
        private Boolean bdgvCellClk = false; // dgvでクリックする場合には必須
        DateTime loadTime; // formloadの時間
        private string pPsn; // 出荷担当
        private bool btrt = false;
        private bool bSet = false;
        private string sHINCD = string.Empty;
        ToolTip ToolTip1;
        //private bool bDirty = false; // 編集が行われたらtrue
        #endregion 

        public F02_EditOrder(params string[] argVals)
        {
            // 親フォームから受け取ったデータをこのインスタンスメンバに格納
            this.argVals = argVals;
            InitializeComponent();

            // ■■■ 親フォームを隠す設定 true => 隠す　default = 隠す。
            bPHide = false; // 隠さない
            //■■■ 画面の和名
            string sTitle = "出荷情報登録";
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
            this.Text = string.Format("【{0}】 {1}  {2} {3}"
                , this.Name
                , sTitle
                , DateTime.Now.ToShortDateString()
                , DateTime.Now.ToShortTimeString());
            #endregion
            dgv0.DataSource = bs0;
            dgv1.DataSource = bs1;  
            setTags();
            
        }

        static public string[] ShowMiniForm(Form frm, params string[] s)
        {
            // params s >> 0= appID, 1 = usr.id, 2 = iDB
            F02_EditOrder f = new F02_EditOrder(s);
            f.ShowDialog(frm);

            string[] receiveText = f.ReturnValue; // -- ①
            f.Dispose();
            return receiveText;
        }

        private void F02_EditOrder_Load(object sender, EventArgs e)
        {
            // 開いた元フォームを隠す設定
            // argVals 0 >> ship_seq, 1>> ga_seq
            lblSeqg.Text = "";
            lblSeq0.Text = "";
            label9.Text = ""; label1.Text = "";
            lblDEN.Text = ""; lblLIN.Text = "";
            if (bPHide) this.Owner.Hide();
            #region EnterKeyで移動
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FRM_KeyDown);
            this.KeyPreview = true;
            #endregion
            button4.Enabled = false; // 印刷
            loadTime = DateTime.Now;
            fn.showCenter(this);
            
            textBox6.Text = "鶴田 武市";
            pPsn = "k0102";
            if (argVals[0].Length > 0) lblSeq0.Text = argVals[0] + ";";
            lblRCaption.Left = this.Width - lblRCaption.Width - 20;
            // lblSeqに値がある時はgetdata
            //GetData(dgv0, bs0, sGetList());
        }

        private void SetTooltip()
        {
            //ToolTipを作成する
            //ToolTip1 = new ToolTip(this.components);
            //フォームにcomponentsがない場合
            ToolTip1 = new ToolTip
            {

                //ToolTipの設定を行う
                //ToolTipが表示されるまでの時間
                InitialDelay = 200,
                //ToolTipが表示されている時に、別のToolTipを表示するまでの時間
                ReshowDelay = 500,
                //ToolTipを表示する時間
                AutoPopDelay = 10000,
                //フォームがアクティブでない時でもToolTipを表示する
                ShowAlways = true
            };

            //Button1とButton2にToolTipが表示されるようにする
            if (button1.Image == k001_shukka.Properties.Resources.unlock) ToolTip1.SetToolTip(button1, "編集");
            else ToolTip1.SetToolTip(button1, "登録");

            ToolTip1.SetToolTip(btnClose, "戾る");
            
            ToolTip1.SetToolTip(button6, "削除");
            ToolTip1.SetToolTip(button5, "出荷確認");
            ToolTip1.SetToolTip(button4, "印刷");
            ToolTip1.SetToolTip(button9, "SC確認");
            ToolTip1.SetToolTip(button8, "連携CSV出力");
            ToolTip1.SetToolTip(button7, "コンテナLot登録");
            ToolTip1.SetToolTip(button10, "Lot確認");
            ToolTip1.SetToolTip(button11, "出荷票設定");
            ToolTip1.SetToolTip(button12, "別ロット管理");
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
                e.Cancel = true;
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
            string s = string.Empty;
            
            this.ReturnValue = new string[] { "戻り値です", s };
            closing();
        }
        // CLOSE処理の最後
        private void FRM_Closed(object sender, FormClosedEventArgs e)
        {
            if (bPHide) this.Owner.Show();
        }
        #endregion

        private void GetData(DataGridView dgv, BindingSource bs, string sSel)
        {
            dgv.Visible = false;
            try
            {
                int[] HCOLOR;
                Label lbl;
                if (dgv.Name == "dgv0")
                {
                    HCOLOR = new int[] { 112, 173, 71 }; // ミドリ
                    lbl = label1;
                }
                else
                {
                    HCOLOR = new int[] { 0, 176, 200 }; // 水色
                    lbl = label9; textBox8.Visible = false;
                }
                // dgvの書式設定全般
                fn.SetDGV(dgv, true, 20, true, 9, 10, 40, true, 40, HCOLOR, DEF_CON.YELLOW);
                dgv.MultiSelect = true;

                //if(bs.DataSource != null) bs.DataSource = null;
                mydb.kyDb con = new mydb.kyDb();
                con.GetData(sSel,DEF_CON.Constr());

                bs.DataSource = con.ds.Tables[0];

                //ヘッダーとすべてのセルの内容に合わせて、列の幅を自動調整する
                //dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                #region 手動でセル幅を指定する場合
                #region sel内容

                #endregion
                //int[] iw;
                int[] icol;
                // 列幅を整える
                icol = new int[] { 0 };
                //iw = new int[] { 93, 40, 77, 180, 178, 68 };
                //clsFunc.setDgvWidth(dgv0, icol, iw);
                #endregion
                if (dgv.Name == "dgv0")
                {
                    icol = new int[] { 2, dgv.Columns.Count - 1 };
                    if (dgv.Rows.Count > 0) button4.Enabled = true;
                    if (btrt)
                    {
                        checkBox1.Visible = true;
                        icol = new int[] { 3, dgv.Columns.Count - 1 };
                    }
                }
                else
                {
                    icol = new int[] { 0, 1 };

                }
                fn.setDgvInVisible(dgv, icol);
                //icol = new int[] { 0, 3 };
                //iw = new int[] { -1, -1 };
                //clsFunc.setDgvAlign(dgv, icol, iw);
                dgv.ClearSelection();
                if (dgv.Name == "dgv1")
                {
                    textBox8.Width = dgv.Columns[2].Width;
                    textBox8.Visible = true;
                }
                dgvArrange(dgv);
                FillDgvCount(dgv, lbl);
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
            if(dgv.Name == "dgv1")
            {
                textBox8.Left = dgv.Left;
                textBox8.Width = dgv.Columns[2].Width;
            }
        }

        private void dgvArrange(DataGridView dgv)
        {
            if (dgv.Rows.Count <= 0) return;
            if(dgv.Name == "dgv0")
            {
                dgv.Width = fn.dgvWidth(dgv);
                // 2 3 9 10
                button2.Left = dgv.Left + dgv.Width + 8;
                button3.Left = dgv.Left + dgv.Width + 8;
                button9.Left = dgv.Left + dgv.Width + 8;
                button10.Left = dgv.Left + dgv.Width + 8;
                dgv1.Left = button2.Left + button2.Width + 8;
            }
            if (dgv.Name == "dgv1")
            {
                int i  = fn.dgvWidth(dgv);
                if (i > 566)
                {
                    if ((i + dgv.Left + 10) > 1320)
                    {
                        this.Width = 1320;
                        dgv.Width = 1320 - 25 - dgv.Left;
                    }
                    else
                    {
                        this.Width = i + dgv.Left + 25;
                        dgv.Width = i;
                    }
                }
            }
        }

        #region dgvクリック関連
        private void dgv_MouseDown(object sender, MouseEventArgs e)
        {
            bdgvCellClk = fn.dgvCellClk(sender, e);
        }

        private void dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            // データ欄以外は何もしない
            if (!bdgvCellClk) return;
        }

        private void dgv_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            //DataGridView dgv = (DataGridView)sender;
            //// データ欄以外は何もしない
            //if (!bdgvCellClk) return;
            //int ir = e.RowIndex;

            //// ダブルクリック前に列の並び替えが行われていれば、その状態を記憶
            //string sOrder = string.Empty;
            //string sOrdColName = string.Empty;
            //if (dgv.SortedColumn != null)
            //{
            //    if (dgv.SortOrder.ToString() == "Ascending") sOrder = "ASC";
            //    else sOrder = "DESC";
            //    sOrdColName = dgv.SortedColumn.Name;
            //}

            //string s0 = dgv.Rows[e.RowIndex].Cells[0].Value.ToString(); // SEQ
            //string[] sendText = { s0 };

            ////// FRMxxxxから送られてきた値を受け取る
            ////string[] receiveText = JFRM46.ShowMiniForm(this, sendText);

            //GetData(dgv0, bs0, sGetList());
            //// 並び順をダブルクリック前に戻し値を検索
            //if (sOrder.Length > 0)
            //{
            //    bs0.Sort = string.Format("{0} {1}", sOrdColName, sOrder);

            //    for (int r = 0; r < dgv.Rows.Count; r++)
            //    {
            //        if (dgv.Rows[r].Cells[0].Value.ToString() == s0)
            //        {
            //            ir = r;
            //            break;
            //        }
            //    }
            //}
            //dgv0.Rows[ir].Selected = true;
            //if (ir + 1 < dgv.Rows.Count) dgv0.FirstDisplayedScrollingRowIndex = ir;
        }
        #endregion

        // 出荷品名を返す
        private string sGetShipmentGrade(string sSeq)
        {
            string s = string.Format(
                "SELECT "
             + "  IFNULL(ipt.SIPPING_GRADE, g.GRADE)"  //0
             + "  FROM kyoei.m_grade_account ga"
             + "   LEFT JOIN kyoei.m_grade g ON g.GRADE_SEQ = ga.GRADE_SEQ"
             + "   LEFT JOIN kyoei.m_b_account ba ON ga.B_AC_SEQ = ba.B_AC_SEQ"
             + "   LEFT JOIN kyoei.m_inspect_std ipt ON ipt.GRADE_AC_SEQ = ga.SEQ"
             + "  WHERE ga.SEQ = {0}"
             + " ;", sSeq);
            return s;
        }
        // dgv1のリスト
        private string sGetSelection(string sGASEQ)
        {

            if(textBox7.Text.IndexOf("NA-BT7906") >= 0)
            {
                string sSql = string.Format(
                "SELECT"
                 + " std.INSPECT_SHAPE"        //0
                 + " FROM"
                 + " kyoei.m_grade_account ga0"
                 + " LEFT JOIN kyoei.m_grade g0"
                 + " ON g0.GRADE_SEQ = ga0.GRADE_SEQ"
                 + " LEFT JOIN kyoei.m_inspect_std std"
                 + " ON std.GRADE_AC_SEQ = ga0.SEQ"
                 + " WHERE ga0.SEQ = {0}"
                 + ";"
                , sGASEQ);
                mydb.kyDb con = new mydb.kyDb();
                if(con.iGetCount(sSql,DEF_CON.Constr()) == 2)
                {
                    sGASEQ =
                        "SELECT"
                         + " ga0.SEQ"        //0
                         + " FROM"  
                         + " kyoei.m_grade_account ga0"
                         + " LEFT JOIN kyoei.m_grade g0"
                         + " ON g0.GRADE_SEQ = ga0.GRADE_SEQ"
                         + " LEFT JOIN kyoei.m_inspect_std std"
                         + " ON std.GRADE_AC_SEQ = ga0.SEQ"
                         + " WHERE"
                         + " ga0.LGC_DEL = '0'"
                         + " AND"
                         + " g0.GRADE LIKE '%NA-BT7906%'"
                         + " AND std.INSPECT_SHAPE = 2";
                }
            }
            
            
            #region
            return string.Format(
                    "SELECT DISTINCT"
                 + "  p.PRODUCT_SEQ"  //0
                 + "  , IFNULL(ipt.GRADE_AC_SEQ,p.GRADE_AC_SEQ) GA_SEQ"  //1
                 + "  , p.LOT_NO"  //2
                 + "  , IFNULL(ba2.B_ACCOUNT, ba.B_ACCOUNT) 取引先"  //3
                 + "  , IFNULL(g2.GRADE, g.GRADE) 品名"  //4
                 + "  , CAST(p.WEIGHT AS UNSIGNED) 重量"  //5
                 + "  , p.SUCCESS 工" //6

                 //+ "  , CASE "
                 //+ "    WHEN IFNULL(ipt.GRADE_AC_SEQ,p.GRADE_AC_SEQ) = 150 AND p.CHK_MESH2 = '0' THEN '合'"
                 //+ "    WHEN IFNULL(ipt.GRADE_AC_SEQ,p.GRADE_AC_SEQ) != 150 AND p.CHK_MESH = '0' THEN '合'"
                 //+ "    WHEN IFNULL(ipt.GRADE_AC_SEQ,p.GRADE_AC_SEQ) = 150 AND p.CHK_MESH2 = '1' THEN '否'"
                 //+ "    WHEN IFNULL(ipt.GRADE_AC_SEQ,p.GRADE_AC_SEQ) != 150 AND p.CHK_MESH = '1' THEN '否'"
                 //+ "    ELSE '-' END M合"  //7
                 + "  , CASE "
                 + "    WHEN p.CHK_MESH = '0' THEN '合'"
                 + "    WHEN p.CHK_MESH = '1' THEN '否'"
                 + "    ELSE '-' END M"  //7

                 + "  , CASE "
                 + "    WHEN p.CHK_MESH2 = '0' THEN '合'"
                 + "    WHEN p.CHK_MESH2 = '1' THEN '否'"
                 + "    ELSE '-' END 品M"  //8

                 + "  , CASE "
                 + "    WHEN ipc.RESULT = '0' THEN '合'"
                 + "    WHEN ipc.RESULT = '1' THEN '否'"
                 + "    ELSE '-' END 検"   // 8 + 1
                 + "  , p.LOC 倉"          // 9 + 1
                 //+ "  FROM kyoei.t_product p"
            #region t_product 小山+栃木
                 + " FROM ("
                 + " SELECT"
                 + " PRODUCT_SEQ"
                 + " ,PRODUCT_DATE"
                 + " ,MACHINE_NAME"
                 + " ,LOT_NO"
                 + " ,GRADE_AC_SEQ"
                 + " ,WEIGHT"
                 + " ,CHK_MESH"
                 + " ,CHK_MESH2"
                 + " ,SUCCESS"
                 + " ,INSPECT_SEQ"
                 + " ,SHIP_SEQ"
                 + " , 2 LOC"
                 + " FROM kyoei.t_product"
                 + " WHERE GRADE_AC_SEQ IN ({0})"
                 + " AND SHIP_SEQ IS NULL"
                 + " UNION"
                 + " SELECT"
                 + " PRODUCT_SEQ"
                 + " ,PRODUCT_DATE"
                 + " ,MACHINE_NAME"
                 + " ,LOT_NO"
                 + " ,GRADE_AC_SEQ"
                 + " ,WEIGHT"
                 + " ,CHK_MESH"
                 + " ,CHK_MESH2"
                 + " ,SUCCESS"
                 + " ,INSPECT_SEQ"
                 + " ,SHIP_SEQ"
                 + " , 1 LOC"
                 + " FROM kyoei.t_t_product"
                 + " WHERE GRADE_AC_SEQ IN ({0})"
                 + " AND SHIP_SEQ IS NULL"
                 + " UNION"
                 + " SELECT"
                 + " mp.PRODUCT_SEQ"
                 + " , mp.PRODUCT_DATE"
                 + " , mp.MACHINE_NAME"
                 + " , mp.LOT_NO"
                 + " , mp.GRADE_AC_SEQ"
                 + " , mp.WEIGHT"
                 + " , mp.CHK_MESH"
                 + " , mp.CHK_MESH2"
                 + " , mp.SUCCESS"
                 + " , mp.INSPECT_SEQ"
                 + " , mp.SHIP_SEQ"
                 + " , 3 LOC"
                 + " FROM kyoei.t_m_product mp"
                 + " WHERE mp.GRADE_AC_SEQ IN ({0})"
                 + " AND SHIP_SEQ IS NULL"
                 + " ) p "
            #endregion
                 + "  LEFT JOIN kyoei.m_grade_account ga ON ga.SEQ = p.GRADE_AC_SEQ"
                 + "  LEFT JOIN kyoei.m_grade g ON g.GRADE_SEQ = ga.GRADE_SEQ"
                 + "  LEFT JOIN kyoei.m_b_account ba ON ga.B_AC_SEQ = ba.B_AC_SEQ"
                 + "  LEFT JOIN kyoei.m_inspect_std ipt ON ipt.GRADE_AC_SEQ = p.GRADE_AC_SEQ"
                 + "  LEFT JOIN kyoei.m_grade_account ga2 ON ga2.SEQ = ipt.GRADE_AC_SEQ"
                 + "  LEFT JOIN kyoei.m_grade g2 ON g2.GRADE_SEQ = ga2.GRADE_SEQ"
                 + "  LEFT JOIN kyoei.m_b_account ba2 ON ba2.B_AC_SEQ = ga2.B_AC_SEQ"
                 + "  LEFT JOIN kyoei.t_inspect ipc ON ipc.SEQ = p.INSPECT_SEQ"
                 //+ "  WHERE p.GRADE_AC_SEQ = {0} AND p.SHIP_SEQ IS NULL" // 
                 //+ "       AND p.INSPECT_SEQ IS NOT NULL AND ipc.RESULT = '0'" // 
                 + " WHERE"
                 + "   p.SHIP_SEQ IS NULL"
                 + "  ORDER BY p.PRODUCT_DATE DESC, p.MACHINE_NAME ASC, p.PRODUCT_SEQ ASC;"
                , sGASEQ);
            #endregion
        }

        private bool regist()
        {
            bool b = false;
            #region ロジック
            /*必須項目
                数字
                日付
                グレード 
               新規・更新
               ステータス
               　出荷情報登録済み
                 ロット選定中
                 出荷ロット選定済
             */
            #endregion
            #region 必須項目チェック >> 今のところlblSeqが抜けている
            if (textBox4.Text.Length == 0 || textBox1.Text.Length == 0 || pPsn.Length == 0
                || textBox2.Text.Length == 0 || textBox5.Text.Length == 0 
                || textBox6.Text.Length == 0 || lblSeqg.Text.Length == 0)
            {
                string[] sSet = { "必須項目が抜けています。", "false" };
                string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSet);
                return b;
            }
            string s = string.Empty;
            
            if (!fn.IsDatetime(textBox2.Text)) s = "出荷日が日付形式ではありません。";
            if (!fn.IsInt(textBox5.Text)) s += "出荷数量が文字形式ではありません。";
            if (textBox3.Text.Length > 0 && !fn.IsDatetime(textBox3.Text)) s += "納入日が日付形式ではありません。";
            if (s.Length > 0)
            {
                string[] sSet = { s, "false" };
                string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSet);
                return b;
            }
            if (lblSeqg.Text.Length == 0)
            {
                string[] sSet = { "出荷グレードが選択されていません。", "false" };
                string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSet);
                return b;
            }
            #endregion
            if (!b)
            {
                string[] sSet = { "入力内容を登録します。", "" };
                string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSet);
                if (sRcv[0].Length == 0) return b;
            }

            #region 変数のセット
            string sDueD = textBox3.Text;
            if (sDueD.Length == 0) sDueD = "NULL";
            else sDueD = string.Format("'{0}'", sDueD);
            // 出荷テーブルの番号を取得する。
            s = "SELECT COUNT(*) FROM t_shipment_inf;";
            mydb.kyDb con = new mydb.kyDb();
            string sSEQ = string.Empty;
            if (lblSeq0.Text.Length == 0) sSEQ = (con.iGetCount(s,DEF_CON.Constr()) + 1).ToString();
            else sSEQ = lblSeq0.Text;

            string sD = textBox2.Text;

            #endregion
            #region sqlの生成 >> s
            if (lblSeq0.Text.Length > 0)
            {
                s = string.Format(
                    "UPDATE t_shipment_inf SET "
                    + "DESTINATION = '{1}', SHIP_DATE = '{2}', DUE_DATE = {3}"
                    + ", SHIPMENT_QUANTITY = {4}, UPD_ID = '{5}', UPD_DATE = NOW()"
                    + ", SHIPMENT_PSN = '{6}', GA_SEQ = {7}, ITEM = '{8}'"
                    + "  WHERE SEQ = {0};"
                    , sSEQ
                    , textBox1.Text, sD, sDueD
                    , textBox5.Text, usr.id, pPsn, lblSeqg.Text, textBox4.Text);
            }
            else
            {
                s = string.Format(
                    "INSERT INTO t_shipment_inf ("
                    + "SEQ, LGC_DEL, DESTINATION, SHIP_DATE"
                    + ", SHIPMENT_QUANTITY, DUE_DATE, UPD_ID, UPD_DATE, LOC_SEQ, SHIPMENT_PSN"
                    + ", REG_ID, REG_DATE, GA_SEQ, ITEM"
                    + ") VALUES ("
                    + " {0}, '0', '{1}', '{2}'"
                    + ", {3}, {4}, '{5}', NOW(), 2, '{6}'"
                    + ", '{5}', NOW(), {7}, '{8}'"
                    + ");"
                    , sSEQ, textBox1.Text, sD
                    , textBox5.Text, sDueD, usr.id, pPsn, lblSeqg.Text, textBox4.Text);
            }
            #endregion

            #region 登録
            if (con.ExecSql(false, DEF_CON.Constr(), s).Length > 0)
            {
                string[] sSet = { "登録に失敗しました。システム管理者に連絡して下さい：出荷登録エラー", "false" };
                string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSet);
            }
            else
            {
                string[] sSet = { "登録しました。", "false" };
                string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSet);
                lblSeq0.Text = sSEQ;
                b = true;
            }
            #endregion  
            return b;
        }
        // 出荷情報を全部抽出
        private string sGetShipment(string sSeq)
        {
            return string.Format(
                    "SELECT "
                 + "  si.SEQ"  //0
                 + "  , CONCAT(ba.B_ACCOUNT, ' ', g.GRADE)"  // 1
                 + "  , IFNULL(si.ITEM,jc.HINNMA)" // 2
                 + "  , IFNULL(jc.NHSNM, si.DESTINATION)"  //3
                 + " , CASE"
                 + "   WHEN jc.SYUYTIDT IS NOT NULL"
                 + "   THEN CONCAT(SUBSTRING(jc.SYUYTIDT,1,4),'/',SUBSTRING(jc.SYUYTIDT,5,2)"
                 + "    ,'/',SUBSTRING(jc.SYUYTIDT,7,2))"
                 + "   ELSE DATE_FORMAT(si.SHIP_DATE, '%y/%m/%d') END 出荷日"  //4
                 + " , CASE"
                 + "   WHEN jc.NOKDT IS NOT NULL"
                 + "   THEN CONCAT(SUBSTRING(jc.NOKDT,1,4),'/',SUBSTRING(jc.NOKDT,5,2)"
                 + "    ,'/',SUBSTRING(jc.NOKDT,7,2))"
                 + "   ELSE DATE_FORMAT(si.SHIP_DATE, '%y/%m/%d') END 出荷日"  //5
                 + "  , IFNULL(CAST(jc.UODSU AS SIGNED),si.SHIPMENT_QUANTITY)"  //6
                 + "  , CONCAT(w.SEI, ' ', w.MEI)"  //7
                 + "  , si.GA_SEQ" // 8
                 + "  , si.JDNNO"  // 9
                 + "  , si.LINNO"  // 10
                 + "  , jc.HINCD"  // 11
                 + "  , cb.LOT"    // 12
                 + "  FROM kyoei.t_shipment_inf si"
                 + "  LEFT JOIN kyoei.sc_juchu jc"
                 + "   ON si.JDNNO = jc.JDNNO AND si.LINNO = jc.LINNO"
                 + "  LEFT JOIN kyoei.sc_juchu_ret jcr"
                 + "   ON si.JDNNO = jcr.JDNNO AND si.LINNO = jcr.LINNO"
                 + "  LEFT JOIN kyoei.m_worker w ON si.SHIPMENT_PSN = w.WKER_ID AND w.LGC_DEL = '0'"
                 + "  LEFT JOIN kyoei.m_grade_account ga ON ga.SEQ = si.GA_SEQ"
                 + "  LEFT JOIN kyoei.m_grade g ON g.GRADE_SEQ = ga.GRADE_SEQ"
                 + "  LEFT JOIN kyoei.m_b_account ba ON ga.B_AC_SEQ = ba.B_AC_SEQ"
                 + "  LEFT JOIN kyoei.t_can_barcode cb ON cb.SHIP_SEQ = si.SEQ"
                 + "  WHERE si.SEQ = {0};"
                , sSeq);
        }
        /// <summary>
        /// t_productでSHIP_SEQ = sSeqのレコードの重量合計を計算する
        /// </summary>
        /// <param name="sSeq"></param>
        /// <returns></returns>
        private int ChkWeight(string sSeq)
        {
            string s = string.Format(
                "SELECT CAST(SUM(p.WEIGHT) AS SIGNED) FROM"
                + " ("
                + " SELECT"
                + " PRODUCT_SEQ"
                + " ,WEIGHT"
                + " FROM kyoei.t_product"
                + " WHERE SHIP_SEQ = {0}"
                + " UNION"
                + " SELECT"
                + " PRODUCT_SEQ"
                + " ,WEIGHT"
                + " FROM kyoei.t_t_product"
                + " WHERE SHIP_SEQ = {0}"
                + " UNION"
                + " SELECT"
                + " PRODUCT_SEQ"
                + " ,WEIGHT"
                + " FROM kyoei.t_m_product"
                + " WHERE SHIP_SEQ = {0}"
                + " ) p "
                + ";"
                , sSeq);
            mydb.kyDb con = new mydb.kyDb();
            return con.iGetCount(s,DEF_CON.Constr());
        }
        // 東レタイプ以外のLot dgv0用
        private string sGetSlectedLots(string seq)
        {
            return string.Format(
                        "SELECT p.LOT_NO LotNo, CAST(p.WEIGHT AS UNSIGNED) 重量, p.PRODUCT_PERSON, p.Loc" // 
                                                                                      //+ " FROM kyoei.t_product p"
                        + " FROM ("
                        + " SELECT"
                        + " PRODUCT_SEQ"
                        + " ,PRODUCT_DATE"
                        + " ,MACHINE_NAME"
                        + " ,LOT_NO"
                        + " ,GRADE_AC_SEQ"
                        + " ,WEIGHT"
                        + " ,CHK_MESH"
                        + " ,CHK_MESH2"
                        + " ,SUCCESS"
                        + " ,INSPECT_SEQ"
                        + " ,SHIP_SEQ"
                        + " ,PRODUCT_PERSON"
                        + " , 2 LOC"
                        + " FROM kyoei.t_product"
                        + " WHERE SHIP_SEQ = {0}"
                        + " UNION"
                        + " SELECT"
                        + " PRODUCT_SEQ"
                        + " ,PRODUCT_DATE"
                        + " ,MACHINE_NAME"
                        + " ,LOT_NO"
                        + " ,GRADE_AC_SEQ"
                        + " ,WEIGHT"
                        + " ,CHK_MESH"
                        + " ,CHK_MESH2"
                        + " ,SUCCESS"
                        + " ,INSPECT_SEQ"
                        + " ,SHIP_SEQ"
                        + " ,PRODUCT_PERSON"
                        + " , 1 LOC"
                        + " FROM kyoei.t_t_product"
                        + " WHERE SHIP_SEQ = {0}"

                        + " UNION"
                        + " SELECT"
                        + " PRODUCT_SEQ"
                        + " ,PRODUCT_DATE"
                        + " ,MACHINE_NAME"
                        + " ,LOT_NO"
                        + " ,GRADE_AC_SEQ"
                        + " ,WEIGHT"
                        + " ,CHK_MESH"
                        + " ,CHK_MESH2"
                        + " ,SUCCESS"
                        + " ,INSPECT_SEQ"
                        + " ,SHIP_SEQ"
                        + " ,PRODUCT_PERSON"
                        + " , 3 LOC"
                        + " FROM kyoei.t_m_product"
                        + " WHERE SHIP_SEQ = {0}"

                        + " ) p "
                        + " WHERE p.SHIP_SEQ = {0}"
                        + " ORDER BY p.MACHINE_NAME, p.PRODUCT_SEQ;"
                        , seq);
        }
        // 東レタイプのLot dgv0用
        private string sGetSlectedTLots(string seq)
        {
            return string.Format(
                            "SELECT p.LOT_NO LotNo, p.BAGNO Bag, CAST(p.WEIGHT AS UNSIGNED) 重量, p.PRODUCT_PERSON, p.Loc"
                            //+ " FROM kyoei.t_product p"
                            + " FROM ("
                            + " SELECT"
                            + " PRODUCT_SEQ"
                            + " ,PRODUCT_DATE"
                            + " ,MACHINE_NAME"
                            + " ,LOT_NO"
                            + " ,BAGNO"
                            + " ,GRADE_AC_SEQ"
                            + " ,WEIGHT"
                            + " ,CHK_MESH"
                            + " ,CHK_MESH2"
                            + " ,SUCCESS"
                            + " ,INSPECT_SEQ"
                            + " ,SHIP_SEQ"
                            + " ,PRODUCT_PERSON"
                            + " , 2 LOC"
                            + " FROM kyoei.t_product"
                            + " WHERE SHIP_SEQ = {0}"
                            + " UNION"
                            + " SELECT"
                            + " PRODUCT_SEQ"
                            + " ,PRODUCT_DATE"
                            + " ,MACHINE_NAME"
                            + " ,LOT_NO"
                            + " ,BAGNO"
                            + " ,GRADE_AC_SEQ"
                            + " ,WEIGHT"
                            + " ,CHK_MESH"
                            + " ,CHK_MESH2"
                            + " ,SUCCESS"
                            + " ,INSPECT_SEQ"
                            + " ,SHIP_SEQ"
                            + " ,PRODUCT_PERSON"
                            + " , 1 LOC"
                            + " FROM kyoei.t_t_product"
                            + " WHERE SHIP_SEQ = {0}"

                            + " UNION"
                            + " SELECT"
                            + " PRODUCT_SEQ"
                            + " ,PRODUCT_DATE"
                            + " ,MACHINE_NAME"
                            + " ,LOT_NO"
                            + " ,BAGNO"
                            + " ,GRADE_AC_SEQ"
                            + " ,WEIGHT"
                            + " ,CHK_MESH"
                            + " ,CHK_MESH2"
                            + " ,SUCCESS"
                            + " ,INSPECT_SEQ"
                            + " ,SHIP_SEQ"
                            + " ,PRODUCT_PERSON"
                            + " , 3 LOC"
                            + " FROM kyoei.t_m_product"
                            + " WHERE SHIP_SEQ = {0}"

                            + " ) p "
                            + " WHERE p.SHIP_SEQ = {0}"
                            + " ORDER BY p.MACHINE_NAME, p.PRODUCT_SEQ;"
                            , seq);
        }

        private void CtrShipVote()
        {
            #region 承認印
            string Stamp = @"C:\tetra\permit.png";
            string Stamp0 = DEF_CON.FLSvrSub + @"template\permit.png";
            if (!System.IO.File.Exists(Stamp) ||
                File.GetLastWriteTime(Stamp) < File.GetLastWriteTime(Stamp0))
            {
                File.Copy(Stamp0, Stamp, true);
            }
            #endregion
            #region 作業用フォルダを確保
            string sDir = @"c:\tetra";
            if (!Directory.Exists(sDir))
            {
                Directory.CreateDirectory(sDir);
            }
            #endregion
            #region 行選択チェック
            if (dgv0.SelectedRows.Count == 0)
            {
                MessageBox.Show("選択された行がありません。印刷するLOTを選択して下さい");
                return;
            }
            #endregion
            #region 特殊な出荷票かどうか　サンビック PP-KA
            bool bSpec = false;
            // TOKCD > 0000013100300,NHSCD > 0000000000001,HINCD > F2510203Y0-KY03A
            mydb.kyDb con = new mydb.kyDb();
            string sTOKCD = string.Empty;
            string sNHSCD = string.Empty;

            string[] val = con.sGetVal(GetJuchuInf(lblDEN.Text, lblLIN.Text), DEF_CON.Constr());
            
            if(lblDEN.Text.Length > 0 && val.GetLength(0) > 0)
            {
                int i = 0;
                sTOKCD = val[0];
                sNHSCD = val[1];
                if (val[0] == "0000013100300") i++;
                if (val[1] == "0000000000001") i++;
                if (val[2] == "F2510203Y0-KY03A") i++;
                if (val[2] == "F2KA02V2SZ-KY03A") i++;
                if (i == 3) bSpec = true;
            }
            #endregion
            #region 海外か　= HUVIS CORPORATION  0000019605100 0000019607101
            bool bKaigai = false;
            if (lblDEN.Text.Length > 0 && val.GetLength(0) > 0)
            {
                if (val[0] == "0000019605100" || val[0] == "0000019607101") bKaigai = true;
            }
            #endregion
            //ファイルの出力先を設定
            string newFile = @"c:\tetra\shipvote.pdf";
            //PrintDocumentの作成
            System.Drawing.Printing.PrintDocument pd =
                new System.Drawing.Printing.PrintDocument();
            //プリンタ名の取得
            string PName = pd.PrinterSettings.PrinterName;

            //PDFドキュメント(ページサイズ)//ページサイズ設定 -> はがきサイズ
            iTextSharp.text.Rectangle new_Pagesize = new iTextSharp.text.Rectangle(284, 420);//(横,縦)
            //ドキュメントを作成
            iTextSharp.text.Document doc = new iTextSharp.text.Document(new_Pagesize, 14f, 14f, 40f, 14f); //(ページサイズ, 左マージン, 右マージン, 上マージン, 下マージン);
            //Document doc = new Document(PageSize.A4);
            //ファイル出力用のストリームを取得
            PdfWriter.GetInstance(doc, new FileStream(newFile, FileMode.Create));
            #region 本文用のフォント(MS P明朝)セット
            Font fnt = new Font(BaseFont.CreateFont
                (@"c:\windows\fonts\msmincho.ttc,1", BaseFont.IDENTITY_H, BaseFont.EMBEDDED), 12); // 14
            Font fntW = new Font(BaseFont.CreateFont
                (@"c:\windows\fonts\msmincho.ttc,1", BaseFont.IDENTITY_H, BaseFont.EMBEDDED), 14);
            fntW.SetColor(255, 255, 255);
            Font fnt2 = new Font(BaseFont.CreateFont
                (@"c:\windows\fonts\calibri.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED), 18); // 16
            Font fnt3 = new Font(BaseFont.CreateFont
                (@"c:\windows\fonts\msmincho.ttc,1", BaseFont.IDENTITY_H, BaseFont.EMBEDDED), 16);
            Font fnt4 = new Font(BaseFont.CreateFont
                (@"c:\windows\fonts\msmincho.ttc,1", BaseFont.IDENTITY_H, BaseFont.EMBEDDED), 12);
            Font fnt5 = new Font(BaseFont.CreateFont
                (@"c:\windows\fonts\msmincho.ttc,1", BaseFont.IDENTITY_H, BaseFont.EMBEDDED), 9);
            #endregion
            //文章の出力を開始します。
            doc.Open();
            int icount = dgv0.SelectedRows.Count;
            int ipara = 0;
            string sACC = textBox1.Text;
            string sGRADE = textBox4.Text;
            if (val[2] == "H2330203D0-KY03I") sGRADE = "TIP ECOKYE JT-B FC";
            string sDate = textBox2.Text;

            string sW = dgv0.CurrentRow.Cells[1].Value.ToString();
            if(btrt) sW = dgv0.CurrentRow.Cells[2].Value.ToString(); // btrt => 東レタイかどうか。
            #region
            string[] sSend = { "重量に変更があれば修正して下さい。", "", sW };
            string[] sRcv = F05_Dialog.ShowMiniForm(this, sSend);
            sRcv[0] = sRcv[0].Replace(".0", "").Replace(" ", "").Replace("kg", "");
            sW = sW.Replace(".0", "");
            if (sRcv[0] != sW) sW = sRcv[0];
            else sW = "";
            #endregion
            string sT_KOSU = "個数";
            string sT_LotNo = "LOT";
            string sSyukkaSaki = sACC;
            string sSyukkaMoto = "協栄産業(株)";
            string sProMan = string.Empty;
            string sSHIPMAN = textBox6.Text;
            bool bAntTbl = false;

            #region  別設定ありの時 ■■■ ■■■ ■■■ ■■■ ■■■
            string sGetSetting = string.Format(
                        "SELECT"
                         + " sn.SHIP_NAME"                     //0
                         + " ,sn.DESTINATION"                  //1
                         + " ,sn.DEST_FLG"                     //2
                         + " ,sn.ATBL"                         //3
                         + " FROM kyoei.m_shipment_name sn"
                         + " WHERE "
                         + " sn.LGC_DEL = '0'"
                         + " AND sn.GA_SEQ = {0}"
                         + " AND sn.TOKCD = '{1}'"
                         + " AND sn.NHSCD = '{2}'"
                         + ";"
                        , lblSeqg.Text, sTOKCD, sNHSCD);
            con.GetData(sGetSetting, DEF_CON.Constr());
            if (con.ds.Tables[0].Rows.Count > 0)
            {
                sT_KOSU = "BAG No.";
                sT_LotNo = "Lot No.";
                string tmp1 = con.ds.Tables[0].Rows[0][0].ToString();
                if (tmp1.Length > 0) sGRADE = tmp1;
                string tmp2 = con.ds.Tables[0].Rows[0][1].ToString();
                if (tmp2.Length > 0) sSyukkaSaki = tmp2;
                // 海外フラグが1の時
                if (con.ds.Tables[0].Rows[0][2].ToString() == "1")
                {
                    sSyukkaMoto = "KYOEI INDUSTRY CO.,LTD.";
                    sSHIPMAN = "NA";
                    sProMan = "OYAMA-F";
                }
                // 別出荷票フラグが1の時
                if (con.ds.Tables[0].Rows[0][3].ToString() == "1") bAntTbl = true;
            }
            #endregion ■■■ ■■■ ■■■ ■■■ ■■■

            try
            {
                foreach (DataGridViewRow r in dgv0.SelectedRows)
                {
                    ipara++;
                    #region 取引先 グレード 重量 管理No LotNOを代入
                    string sBagNo = string.Empty;
                    
                    string sLOTNO = dgv0.Rows[r.Index].Cells[0].Value.ToString();
                    string sWEIGHT = dgv0.Rows[r.Index].Cells[1].Value.ToString();
                    if (sW.Length > 0) sWEIGHT = sW;
                    if(sProMan.Length == 0) sProMan = dgv0.Rows[r.Index].Cells[2].Value.ToString();

                    string sBval = string.Empty;
                    if (bSpec)
                    {
                        try
                        {
                            string[] bVal = con.sGetVal(GetB(sLOTNO), DEF_CON.Constr());
                            if (bVal[0].Length > 0) sBval = bVal[1];
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("---\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                        }
                    }

                    if (btrt)
                    {
                        sBagNo = dgv0.Rows[r.Index].Cells[1].Value.ToString();
                        sWEIGHT = dgv0.Rows[r.Index].Cells[2].Value.ToString();
                        if (sW.Length > 0) sWEIGHT = sW;
                        sProMan = "OYAMA.F";
                        sSHIPMAN = "NA";
                    }
                    
                    if (btrt) sT_KOSU = "BAG No.";
                    if (btrt) sT_LotNo = "Lot No.";
                    // ---- ■■ con　val[0]　
                    string sConv = string.Format(
                        "SELECT OUTPUT_TXT FROM kyoei.t_sc_conversion "
                        + " WHERE TOKCD = '{0}' AND "
                        + " NHSCD = '{1}' AND T_FLG = '0';"
                        , val[0], val[1]);
                    string[] s = con.sGetVal(sConv, DEF_CON.Constr());
                    if (s[0] != null && s[0].Length > 0)
                    {
                        if (s[0] != "err") sACC = s[0];
                    }
                    
                    if (btrt) sSyukkaSaki = "THAI TORAY SYNTHETICS CO., LTD.";
                    if (btrt) sSyukkaMoto = "KYOEI INDUSTRY CO., LTD.";
                    #endregion
                    #region bKaigai ■■■ ■■■ ■■■ ■■■ ■■■ ■■■ ■■■ ■■■ ■■■
                    if (bKaigai)
                    {
                        sSyukkaMoto = "KYOEI INDUSTRY CO., LTD.";
                        //sProMan = "OYAMA";
                        sSHIPMAN = "N/A";
                    }
                    #endregion

                    float[] headerwidth = new float[] { 0.3f, 0.7f };
                    PdfPCell cell;

                    //2列からなるテーブルを作成
                    PdfPTable tbl = new PdfPTable(headerwidth)
                    {

                        //テーブル全体の幅（パーセンテージ）
                        WidthPercentage = 100,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };
                    tbl.DefaultCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    //テーブルの余白
                    tbl.DefaultCell.Padding = 2;
                    
                    //テーブルのセル間の間隔
                    //tbl.Spacing = 0;

                    //テーブルの線の色（RGB:黒）
                    tbl.DefaultCell.BorderColor = BaseColor.BLACK;
                    
                    //tbl.BorderColor = new iTextSharp.text.Color(0, 0, 0);
                    //タイトルのセルを追加（左の列）----------------------------------
                    //ヘッダ行
                    cell = new PdfPCell(new Phrase("出　荷　票", fnt3));
                    cell.FixedHeight = 60f; // <--高さ
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    //cell.BackgroundColor = BaseColor.LIGHT_GRAY; 背景の設定
                    // 全体の線の太さの設定
                    cell.BorderWidthTop = 1.0f;
                    cell.BorderWidthLeft = 1.0f;
                    cell.BorderWidthRight = 1.0f;
                    cell.BorderWidthBottom = 0.25f;　
                    cell.Colspan = 2;
                    //  ■■■ ■■■ ■■■ ■■■ ■■■ ■■■ ■■■
                    if (!bAntTbl) tbl.AddCell(cell);


                    ArrayList list = new ArrayList();

                    #region 別ロット管理 ■■■ ■■■ ■■■ ■■■ ■■■ ■■■ ■■■

                    string sGetAlot = string.Format(
                        "SELECT"
                         + " al.LOT_NO"                  //0
                         + " ,al.BAG_NO"                 //1
                         + " FROM kyoei.t_another_lot al"
                         + " WHERE "
                         + " al.BASE_LOT_NO = '{0}'"
                         + ";"
                        , sLOTNO);
                    con.GetData(sGetAlot, DEF_CON.Constr());
                    if(con.ds.Tables[0].Rows.Count > 0)
                    {
                        string tmp1 = con.ds.Tables[0].Rows[0][0].ToString();
                        string tmp2 = con.ds.Tables[0].Rows[0][1].ToString();
                        if (tmp1.Length > 0) sLOTNO = tmp1;
                        if (tmp2.Length > 0) sBagNo = tmp2;
                    }
                    #endregion

                    if (lblSeqg.Text == "552")
                    {
                        //sLOTNO += " (F)";
                        //sGRADE = "SD Chips - 100% Post-consumer Recycled Polyester T20GRMK(F)";
                        //sSyukkaSaki = "PENFIBRE SDN.BERHAD";
                        //sSyukkaMoto = "KYOEI INDUSTRY CO.,LTD.";
                        //sSHIPMAN = "NA";
                        //sProMan = "OYAMA-F";
                    }
                    // FootPrint対策
                    if (cb3.Checked)
                    {
                        if (sLOTNO.IndexOf("(F)") < 0) sLOTNO += " (F)";
                    }
                    #region 表示するデータ
                    if (!bAntTbl)
                    {
                        list.Add(new string[] { "品名", sGRADE });
                        list.Add(new string[] { "重量", string.Format("{0:#,0} kg", double.Parse(sWEIGHT)) });
                        if (bSpec)
                        {
                            list.Add(new string[] { "b値", sBval });
                        }
                        list.Add(new string[] { sT_KOSU, sBagNo });
                        list.Add(new string[] { "出荷日", sDate });
                        list.Add(new string[] { sT_LotNo, sLOTNO });
                        list.Add(new string[] { "出荷先", sSyukkaSaki });
                        list.Add(new string[] { "出荷元", sSyukkaMoto });
                    }
                    else
                    {
                        list.Add(new string[] { "Commodity", sGRADE });
                        list.Add(new string[] { sT_LotNo, sLOTNO });
                        list.Add(new string[] { sT_KOSU, sBagNo });
                        list.Add(new string[] { "Shipping date", sDate });
                        int iw = int.Parse(textBox5.Text);
                        list.Add(new string[] { "N.W.", string.Format("{0:#,0}", iw) });
                        iw = iw / 1000 * 1008;
                        list.Add(new string[] { "G.W.", string.Format("{0:#,0}", iw) });

                        list.Add(new string[] { "Ingredient/component", "Polyester" });
                        list.Add(new string[] { "Origin", "Japan" });

                        list.Add(new string[] { "Manufacture's name", sSyukkaMoto });
                        list.Add(new string[] { "Production date", "20" + sLOTNO.Substring(0,2) + "/"
                                                    + sLOTNO.Substring(2,2) + "/" + sLOTNO.Substring(4,2)
                                                    });
                    }
                    #endregion
                    float fHi = 32f; //32
                    if (bSpec) fHi = 28f;
                    #region 明細行の追加
                    foreach (string[] shiharai in list)
                    {
                        //左のセルの追加
                        //cell = new PdfPCell(new Phrase(shiharai[0], fnt));
                        //cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        //cell.FixedHeight = fHi; // <--これ
                        //cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        //// 線の太さの設定
                        //cell.BorderWidthTop = 0f;
                        //cell.BorderWidthLeft = 1.0f;
                        //cell.BorderWidthRight = 0.25f;
                        //cell.BorderWidthBottom = 0.25f;
                        cell = new PdfPCell(new Phrase(shiharai[0], fnt))
                        {
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            FixedHeight = fHi, // <--これ
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            // 線の太さの設定
                            BorderWidthTop = 0f,
                            BorderWidthLeft = 1.0f,
                            BorderWidthRight = 0.25f,
                            BorderWidthBottom = 0.25f
                        };
                        if (shiharai[0] == "Commodity") cell.BorderWidthTop = 1.0f;
                        if (shiharai[0] == "Production date") cell.BorderWidthBottom = 1.0f;
                        //  ■■■ ■■■ ■■■ ■■■ ■■■ ■■■ ■■■
                        tbl.AddCell(cell);

                        //右のセルの追加（金額なので右寄せ）
                        if (shiharai[0] == sT_LotNo)
                        {
                            cell = new PdfPCell(new Phrase(shiharai[1], fnt2));
                        }
                        else cell = new PdfPCell(new Phrase(shiharai[1], fnt));

                        if (shiharai[0].Substring(0, 2) == "出荷" && shiharai[0] != "出荷日")
                        {
                            cell = new PdfPCell(new Phrase(shiharai[1], fnt5));
                        }

                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        cell.FixedHeight = fHi; // <--これ
                        // 線の太さの設定
                        cell.BorderWidthTop = 0f;
                        cell.BorderWidthLeft = 0f;
                        cell.BorderWidthRight = 1.0f;
                        cell.BorderWidthBottom = 0.25f;
                        if (shiharai[0] == "Commodity") cell.BorderWidthTop = 1.0f;
                        if (shiharai[0] == "Production date") cell.BorderWidthBottom = 1.0f;
                        //  ■■■ ■■■ ■■■ ■■■ ■■■ ■■■ ■■■
                        tbl.AddCell(cell);
                    }
                    #endregion
                    //テーブルを追加  ■■■ ■■■ ■■■ ■■■ ■■■ ■■■ ■■■
                    doc.Add(tbl);

                    // フッタ行をテーブルとして追加
                    headerwidth = new float[] { 0.2f, 0.3f, 0.2f, 0.3f };
                    //4列からなるテーブルを作成
                    PdfPTable tbl1 = new PdfPTable(headerwidth);

                    //テーブル全体の幅（パーセンテージ）
                    tbl1.WidthPercentage = 100;

                    tbl1.HorizontalAlignment = Element.ALIGN_CENTER;
                    tbl1.DefaultCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    //テーブルの余白
                    tbl1.DefaultCell.Padding = 2;
                    //フッタ行を追加
                    fHi = 40f;
                    string[] sLVal = new string[] { "出荷\r\n担当", sSHIPMAN, "製造\r\n担当", sProMan };
                    //1のセルの追加
                    for (int i = 0; i < sLVal.GetLength(0); i++)
                    {
                        cell = new PdfPCell(new Phrase(sLVal[i], fnt4));
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell.FixedHeight = fHi; // <--これ
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        // 線の太さの設定
                        cell.BorderWidthTop = 0f;
                        cell.BorderWidthLeft = 0f;
                        cell.BorderWidthRight = 0.25f;
                        cell.BorderWidthBottom = 1.0f;
                        if (i == 0) cell.BorderWidthLeft = 1.0f;
                        if(i == 3) cell.BorderWidthRight = 1.0f;
                        tbl1.AddCell(cell);
                    }
                    //テーブルを追加  ■■■ ■■■ ■■■ ■■■ ■■■ ■■■ ■■■
                    if(!bAntTbl) doc.Add(tbl1);


                    if (!bAntTbl)
                    {
                        if (!bAntTbl) doc.Add(new Paragraph(""));

                        #region 承認印追加
                        iTextSharp.text.Image image
                                                = iTextSharp.text.Image.GetInstance(new Uri(@"C:\tetra\permit.png"));

                        image.ScalePercent(13.0f);
                        image.SetAbsolutePosition(220f, 94f); // 横 縦
                        //  ■■■ ■■■ ■■■ ■■■ ■■■ ■■■ ■■■
                        doc.Add(image);
                        #endregion

                        #region バーコード追加 c:\APPS
                        sLOTNO = sLOTNO.Replace(" (F)", "");
                        string fname = @"c:\tetra\code39.png";
                        string qrnm = @"C:\tetra\QRcode.png";
                        string sQR = sLOTNO + "," + sGRADE + "," + sWEIGHT + "," + sBagNo + "," + sDate;
                        try
                        {
                            clsBC.Save39Image(sLOTNO, fname
                                , System.Drawing.Imaging.ImageFormat.Png);
                            if (btrt)
                                clsBC.SaveQRImage(sQR, qrnm
                                    , System.Drawing.Imaging.ImageFormat.Png);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString(), "バーコード出力エラー");
                            return;
                        }
                        float pect = 10f;
                        if (btrt)
                        {
                            iTextSharp.text.Image imgQr
                                = iTextSharp.text.Image.GetInstance(new Uri(qrnm));
                            imgQr.ScalePercent(pect);
                            imgQr.SetAbsolutePosition(22f, 325f);
                            
                            doc.Add(imgQr);
                        }
                        pect = 20f;
                        iTextSharp.text.Image imgBc39
                            = iTextSharp.text.Image.GetInstance(new Uri(fname));

                        imgBc39.ScalePercent(pect);
                        imgBc39.SetAbsolutePosition(18f, 10f);

                        doc.Add(imgBc39);
                        #endregion
                    }
                    if (ipara < icount)
                    {
                        doc.NewPage();
                    }
                }
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine("---\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                MessageBox.Show(ex.Message, "エラー");
            }
            finally
            {
                //ドキュメントを閉じる
                doc.Close();
            }
        }

        private void Ctrl_Click(object sender, EventArgs e)
        {
            Control ctl = (Control)sender;
            #region button1 登録ボタン
            if (ctl.Name == "button1") //登録
            {
                if (!textBox6.Enabled) // 出荷担当ボックス
                {
                    enableCtrl(true);
                    button1.Image = k001_shukka.Properties.Resources.decide;
                    SetTooltip();
                }
                else
                {
                    if (regist())
                    {
                        enableCtrl(false);
                        button1.Image = k001_shukka.Properties.Resources.unlock;
                        SetTooltip();
                    }
                }
            }
            #endregion
            
            #region 追加ボタン
            if (ctl.Name == "button2") //追加
            {
                if (dgv1.SelectedRows.Count == 0)
                {
                    string[] sSend = { "ロットを選択して下さい。", "false" };
                    string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSend);
                    return;
                }
                mydb.kyDb con = new mydb.kyDb();

                int ir0 = dgv1.CurrentRow.Index;
                string s0 = dgv1.Rows[ir0].Cells[0].Value.ToString(); // SEQ
                string s = string.Empty;
                #region エラーチェック 出荷情報登録　ロット選択 重量オーバー
                if (lblSeq0.Text.Length == 0)
                {
                    string[] sSend = { "出荷登録されていない出荷情報にLotの紐づけは出来ません。", "false" };
                    string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSend);
                    return;
                }
                else // グレード番号の登録
                {    // グレード番号が選択されていれば、それが登録されているかを調べて未登録の場合は中止
                    if (lblSeqg.Text.Length > 0)
                    {
                        string sChkG = string.Format(
                            "SELECT COUNT(*) FROM t_shipment_inf "
                            + " WHERE LGC_DEL = '0' AND SEQ = {0} AND GA_SEQ = {1};"
                            , lblSeq0.Text, lblSeqg.Text);
                        if (con.iGetCount(sChkG,DEF_CON.Constr()) <= 0)
                        {
                            string[] sSend = { "出荷グレードを登録した後に、ロットを選択して下さい。", "false" };
                            string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSend);
                            return;
                        }
                    }
                }
                if (dgv1.SelectedRows.Count == 0)
                {
                    string[] sSend = { "紐付けする場合には<製品一覧>からLotを選択して下さい。", "false" };
                    string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSend);
                    return;
                }
                #endregion
                try
                {
                    /*  選択行で1行ずつ重量を加算し出荷情報の全重量と比較し
                        オーバーした時点で追加を中止する */
                    #region
                    string Msg = string.Empty;
                    foreach (DataGridViewRow r in dgv1.SelectedRows)
                    {
                        int ir = r.Index;
                        // 工程内合否・検査結果を確認する
                        if(dgv1.Rows[ir].Cells[6].Value.ToString() != "合")
                        {
                            if(dgv1.Rows[ir].Cells[10].Value.ToString() == "2" 
                                && dgv1.Rows[ir].Cells[9].Value.ToString() == "合")
                            {
                                Msg = "工程内合否が「否」です。検査結果が「合」なので処理は進めす。\r\n"
                                    + "必要があれば確認を行って下さい。";
                                string[] sSend = { Msg, "false" };
                                string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSend);
                            }
                            else
                            {
                                Msg = "工程内合否が「否」です。処理を中断します";
                                string[] sSend = { Msg, "false" };
                                string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSend);
                                return;
                            }
                        }
                        if(dgv1.Rows[ir].Cells[10].Value.ToString() == "2"
                            && dgv1.Rows[ir].Cells[9].Value.ToString() != "合")
                        {
                            Msg = "検査結果が「否」です。処理を中断します";
                            string[] sSend = { Msg, "false" };
                            string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSend);
                            return;
                        }
                        string slot = dgv1.Rows[ir].Cells[2].Value.ToString();
                        string sWeight = dgv1.Rows[ir].Cells[5].Value.ToString();
                        string stbl = dgv1.Rows[ir].Cells[10].Value.ToString();
                        if (sWeight.IndexOf(".") >= 0)
                        {
                            sWeight = sWeight.Substring(0, sWeight.IndexOf("."));
                        }
                        int iWeight = int.Parse(sWeight);
                        // 出荷情報よりも選択したLotの重量合計が多くなった場合
                        if (int.Parse(textBox5.Text) < (iWeight + ChkWeight(lblSeq0.Text)))
                        {
                            string[] sSend = { "重量オーバーです。追加する場合には<製品一覧>からLotを選択して下さい。", "false" };
                            string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSend);
                            s = sGetSlectedLots(lblSeq0.Text);
                            GetData(dgv0, bs0, s);
                            GetData(dgv1, bs1, sGetSelection(lblSeqg.Text));
                            return;
                        }
                        if (stbl == "1") stbl = "t_t_product";
                        else if (stbl == "3") stbl = "t_m_product";
                        else stbl = "t_product";
                        s += string.Format(
                            "UPDATE {2} SET SHIP_SEQ = {0} WHERE LOT_NO = '{1}';"
                            , lblSeq0.Text, slot, stbl);
                    }
                    if (con.ExecSql(false,DEF_CON.Constr(), s).Length > 0) return;
                    #endregion
                    // 東レタイプかどうかでBagNoを表示するかどうか決める。
                    if (!btrt) s = sGetSlectedLots(lblSeq0.Text);
                    else s = sGetSlectedTLots(lblSeq0.Text);
                    // 東レタイプの場合は表示前にBagNoが付いていないLotに対してBagNoを付与する
                    // 同一出荷日・同一納入日・同一グレードで現在の最大値を調べNULL = 0 +1する
                    #region 東レタイ向けでBagNo.有りの場合の処理
                    if (btrt)
                    {
                        string tmpS = string.Format(
                            "SELECT MAX(p.BAGNO)"
                            + " FROM ("
                            + " SELECT"
                            + " PRODUCT_SEQ"
                            + " ,PRODUCT_DATE"
                            + " ,MACHINE_NAME"
                            + " ,LOT_NO"
                            + " ,BAGNO"
                            + " ,GRADE_AC_SEQ"
                            + " ,WEIGHT"
                            + " ,SHIP_SEQ"
                            + " ,INSPECT_SEQ"
                            + " ,LGC_DEL"
                            + " , 2 LOC"
                            + " FROM kyoei.t_product"
                            + " UNION"
                            + " SELECT"
                            + " PRODUCT_SEQ"
                            + " ,PRODUCT_DATE"
                            + " ,MACHINE_NAME"
                            + " ,LOT_NO"
                            + " ,BAGNO"
                            + " ,GRADE_AC_SEQ"
                            + " ,WEIGHT"
                            + " ,SHIP_SEQ"
                            + " ,INSPECT_SEQ"
                            + " ,LGC_DEL"
                            + " , 1 LOC"
                            + " FROM kyoei.t_t_product"
                            + " ) p "

                            + " LEFT JOIN t_shipment_inf si ON p.SHIP_SEQ = si.SEQ"
                            + " WHERE p.BAGNO IS NOT NULL AND p.LGC_DEL = '0'"
                            + " AND si.LOC_SEQ = 2 AND si.GA_SEQ = {0} AND si.DESTINATION = '{1}'"
                            + " AND si.SHIP_DATE = '{2}' AND si.ITEM = '{3}'"
                            + " ORDER BY p.MACHINE_NAME, p.PRODUCT_SEQ"
                            , lblSeqg.Text, textBox1.Text, textBox2.Text, textBox4.Text);
                        int iNo = con.iGetCount(tmpS,DEF_CON.Constr());
                        tmpS = string.Format(
                            "SELECT p.LOT_NO"
                            + " FROM ("
                            + " SELECT"
                            + " PRODUCT_SEQ"
                            + " ,PRODUCT_DATE"
                            + " ,MACHINE_NAME"
                            + " ,LOT_NO"
                            + " ,BAGNO"
                            + " ,GRADE_AC_SEQ"
                            + " ,WEIGHT"
                            + " ,SHIP_SEQ"
                            + " ,INSPECT_SEQ"
                            + " ,LGC_DEL"
                            + " , 2 LOC"
                            + " FROM kyoei.t_product"
                            + " UNION"
                            + " SELECT"
                            + " PRODUCT_SEQ"
                            + " ,PRODUCT_DATE"
                            + " ,MACHINE_NAME"
                            + " ,LOT_NO"
                            + " ,BAGNO"
                            + " ,GRADE_AC_SEQ"
                            + " ,WEIGHT"
                            + " ,SHIP_SEQ"
                            + " ,INSPECT_SEQ"
                            + " ,LGC_DEL"
                            + " , 1 LOC"
                            + " FROM kyoei.t_t_product"
                            + " ) p "
                            + " LEFT JOIN t_shipment_inf si ON p.SHIP_SEQ = si.SEQ"
                            + " WHERE p.BAGNO IS NULL AND p.LGC_DEL = '0'"
                            + " AND si.LOC_SEQ = 2 AND si.GA_SEQ = {0} AND si.DESTINATION = '{1}'"
                            + " AND si.SHIP_DATE = '{2}' AND si.ITEM = '{3}'"
                            + " ORDER BY p.MACHINE_NAME, p.PRODUCT_SEQ"
                            , lblSeqg.Text, textBox1.Text, textBox2.Text, textBox4.Text);

                        con.GetData(tmpS,DEF_CON.Constr());

                        if (con.ds.Tables[0].Rows.Count > 0)
                        {
                            string sUPD = string.Empty;
                            string sLot = string.Empty;
                            for (int i = 0; i < con.ds.Tables[0].Rows.Count; i++)
                            {
                                sLot = con.ds.Tables[0].Rows[i][0].ToString();
                                sUPD += string.Format(
                                    "UPDATE t_product SET BAGNO = {0} WHERE LOT_NO = '{1}';"
                                    , iNo + 1, sLot);
                                iNo++;
                            }
                            con.ExecSql(false, sUPD);
                        }
                        checkBox1.Visible = true;
                    }
                    else checkBox1.Visible = false;
                    #endregion
                    // dgv0を表示し、dgv1に選択したLotを除いて表示する>>SHIP_SEQの有無
                    GetData(dgv0, bs0, s);
                    GetData(dgv1, bs1, sGetSelection(lblSeqg.Text));

                    // 表示位置を戻す
                    for (int r = 0; r < dgv1.Rows.Count; r++)
                    {
                        if (dgv1.Rows[r].Cells[0].Value.ToString() == s0)
                        {
                            ir0 = r;
                            break;
                        }
                    }

                    if (ir0 + 1 < dgv1.Rows.Count) dgv1.FirstDisplayedScrollingRowIndex = ir0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("出荷ロット紐づけエラー：" + ex.Message, "エラー");
                }
            }
            #endregion
            
            #region button3 除外
            if (ctl.Name == "button3") //除外
            {
                #region エラーチェック 出荷情報登録　ロット選択 重量オーバー
                if (dgv0.Rows.Count == 0)
                {
                    string[] sSend = { "<出荷Lot>がありません。", "false" };
                    string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSend);
                    return;
                }
                if (dgv0.SelectedRows.Count == 0)
                {
                    string[] sSend = { "<出荷Lot>から除外するロットを選んでください。", "false" };
                    string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSend);
                    return;
                }
                int ir0 = 0; string s0 = string.Empty;
                if (dgv1.SelectedRows.Count > 0)
                {
                    ir0 = dgv1.CurrentRow.Index;
                    ir0 = dgv1.FirstDisplayedCell.RowIndex;
                    s0 = dgv1.Rows[ir0].Cells[0].Value.ToString(); // SEQ
                }
                string s = string.Empty;

                #endregion
                try
                {
                    mydb.kyDb con = new mydb.kyDb();
                    foreach (DataGridViewRow r in dgv0.SelectedRows)
                    {
                        int ir = r.Index;
                        string slot = dgv0.Rows[ir].Cells[0].Value.ToString();
                        string stbl = dgv0.Rows[ir].Cells[dgv0.Columns.Count - 1].Value.ToString();
                        if (stbl == "1") stbl = "t_t_product";
                        else if (stbl == "3") stbl = "t_m_product";
                        else stbl = "t_product";
                        s += string.Format(
                            "UPDATE {1} SET SHIP_SEQ = NULL, BAGNO = NULL WHERE LOT_NO = '{0}';"
                            , slot, stbl);
                    }
                    if (con.ExecSql(false,DEF_CON.Constr(), s).Length == 0)
                    {
                        // 東レタイプかどうかでBagNoを表示するかどうか決める。
                        if (!btrt) s = sGetSlectedLots(lblSeq0.Text);
                        else s = sGetSlectedTLots(lblSeq0.Text);
                        GetData(dgv0, bs0, s);
                        GetData(dgv1, bs1, sGetSelection(lblSeqg.Text));
                    }
                    // 表示位置を戻す
                    if (dgv0.SelectedRows.Count == 0)
                    {
                        for (int r = 0; r < dgv1.Rows.Count; r++)
                        {
                            if (dgv1.Rows[r].Cells[0].Value.ToString() == s0)
                            {
                                ir0 = r;
                                break;
                            }
                        }
                        if (ir0 + 1 < dgv1.Rows.Count) dgv1.FirstDisplayedScrollingRowIndex = ir0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("出荷ロット解除エラー：" + ex.Message, "エラー");
                }
            }
            #endregion
            
            #region 印刷 button4
            if (ctl.Name == "button4") //印刷
            {
                // 2. 選択行の有無を確認 ->> 選択していない場合は何もしない
                // 6. 出荷票生成
                // 7. 印刷

                #region 2.3. 選択行の有無を確認 ->> 選択していない場合は何もしない
                int irow = dgv0.Rows.GetRowCount(DataGridViewElementStates.Selected);
                if (irow == 0)
                {
                    string[] sSend = { "印刷するロットを選択して下さい。", "false" };
                    string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSend);
                    return;
                }
                else
                {
                    string[] sSend = { "<出荷Lot>の選択行を印刷します。", "" };
                    string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSend);
                    if (sRcv[0].Length == 0) return;
                }
                #endregion

                // 6. 出荷票生成
                CtrShipVote();

                #region 7. 印刷
                Process p;
                //p = Process.Start(@"C:\Program Files\Tracker Software\PDF Viewer\PDFXCview.exe", @" /print c:/APPS/test1.pdf");
                p = System.Diagnostics.Process.Start(@"c:/tetra/shipvote.pdf");
                #endregion

                MessageBox.Show("出力しました。");
            }
            #endregion
            
            #region 出荷日　納品日
            if (ctl.Name == "textBox2" || ctl.Name == "textBox3") // 2出荷日 3納入日
            {
                string s = string.Empty;
                if (ctl.Text.Length == 0) s = DateTime.Today.ToShortDateString();
                else s = ctl.Text;
                string[] sendText = { s, "", "1" };
                string[] sRcv = promag_frm.F06_SelDate.ShowMiniForm(this, sendText);
                if (sRcv[0].Length > 0) ctl.Text = sRcv[0];
            }
            #endregion
            
            #region グレード選択 => textBox7
            if (ctl.Name == "textBox7")
            {
                string[] sendText = { "", "" };

                if(sHINCD.Length > 0)
                {
                    string sSql = string.Format(
                           "SELECT"
                        + " p.GRADE_AC_SEQ"
                        + " FROM kyoei.sc_lot_hincd lh"
                        + " LEFT JOIN kyoei.t_product p"
                        + " ON p.LOT_NO = lh.LOT_NO"
                        + " WHERE lh.HINCD = '{0}'"
                        + "  AND LENGTH(lh.LOT_NO)-LENGTH(REPLACE(lh.LOT_NO,'-','')) = 2"
                        + " GROUP BY p.GRADE_AC_SEQ"
                        , sHINCD);
                    mydb.kyDb con = new mydb.kyDb();
                    con.GetData(sSql, DEF_CON.Constr());
                    if(con.ds.Tables[0].Rows.Count > 0)
                    {
                        sendText = new string[con.ds.Tables[0].Rows.Count + 2];
                        sendText[0] = "";
                        sendText[1] = "";
                        string tmpSEQ = string.Empty;
                        for(int i = 0;i < con.ds.Tables[0].Rows.Count; i++)
                        {
                            sendText[i + 2]= con.ds.Tables[0].Rows[i][0].ToString();
                        }
                    }
                }
                // FRMxxxxから送られてきた値を受け取る 
                string[] rcvT = F03_SELECT_GRADE.ShowMiniForm(this, sendText);
                if ((rcvT[0] != "0") && (rcvT[1] != "0"))
                {
                    // 出荷グレードを埋める
                    textBox7.Text = rcvT[0] + " " + rcvT[1]; // 得意先名 + 品名
                    // 出荷品名を埋める
                    string[] sSed = { "出荷品名をテトラの出荷品名に変更しますか。", "" };
                    string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSed);
                    mydb.kyDb con = new mydb.kyDb();
                    string s;
                    if (sRcv[0].Length > 0)
                    {
                        s = string.Format(
                            "SELECT"
                         + "   IFNULL(IFNULL(sn.SHIP_NAME ,std.SIPPING_GRADE),g.GRADE) "    //0
                         + " FROM "
                         + "   kyoei.m_grade_account ga"
                         + "   LEFT JOIN kyoei.m_shipment_name sn"
                         + "   ON ga.SEQ = sn.GA_SEQ "
                         + "   LEFT JOIN kyoei.m_inspect_std std"
                         + "   ON std.GRADE_AC_SEQ = ga.SEQ"
                         + "   LEFT JOIN kyoei.m_grade g"
                         + "   ON g.GRADE_SEQ = ga.GRADE_SEQ"
                         + " WHERE ga.SEQ = {0}"
                         + " ;"
                        , rcvT[3]);
                        textBox4.Text = con.sColVal(s, DEF_CON.Constr());
                    }
                    else
                    {
                        s = string.Format(
                            "SELECT sj.HINNMA"
                            + " FROM kyoei.sc_juchu sj"
                            + " WHERE sj.JDNNO = '{0}'"
                            + " AND sj.LINNO = '{1}';"
                            ,lblDEN.Text, lblLIN.Text);
                        textBox4.Text = con.sColVal(s, DEF_CON.Constr());
                    }
                    s = rcvT[3] + ";";

                    lblSeqg.Text = s;
                    if (lblDEN.Text.Length == 0)
                    {
                        textBox1.Text = rcvT[0];
                        string[] sNM;
                        sNM = con.sGetVal(sGetShipmentGrade(rcvT[3]),DEF_CON.Constr());

                        if (sNM.Length > 0 && textBox4.Text.Length == 0) textBox4.Text = sNM[0];
                    }
                }
            }
            #endregion
            
            #region 連携CSV出力 => button8
            if (ctl.Name == "button8")
            {
                // 重量が一致するか確認する
                // CSVを作成する
                // filename = RJ_DENNO_LINO.csv
                // JDNNO,LINNO,TOKCD,NHSCD,HINCD,SYUYTIDT,NOKDT,UODSU,LOTID,HINLCDDT,SOUCD
                // 00006156,002,0000011100901,0000000000004,I2330203ZZ-282CP,20201107,20201107,1000,V3-01,20201007,0000000000003
                // 受注伝票番号,行番号,得意先コード,納品先コード,商品コード,出荷日,納品日,数量,ロットID,管理日付,倉庫コード
                string sMsg = string.Empty;
                #region 重量の一致と出荷確認済みかチェックする
                if (int.Parse(textBox5.Text) != (ChkWeight(lblSeq0.Text)))
                {
                    sMsg = "選択されたLot重量合計が出荷数量と一致しません。";
                }
                #region 選択されたLot数
                string s = string.Format(
                    "SELECT COUNT(*) FROM"
                    + " ("
                    + " SELECT"
                    + " PRODUCT_SEQ"
                    + " ,WEIGHT"
                    + " FROM kyoei.t_product"
                    + " WHERE SHIP_SEQ = {0}"
                    + " UNION"
                    + " SELECT"
                    + " PRODUCT_SEQ"
                    + " ,WEIGHT"
                    + " FROM kyoei.t_t_product"
                    + " WHERE SHIP_SEQ = {0}"
                    + " UNION"
                    + " SELECT"
                    + " PRODUCT_SEQ"
                    + " ,WEIGHT"
                    + " FROM kyoei.t_m_product"
                    + " WHERE SHIP_SEQ = {0}"
                    + " ) p "
                    + ";"
                    , lblSeq0.Text);
                #endregion
                #region 選択されてチェックされたロット数
                string s1 = string.Format(
                    "SELECT COUNT(*) FROM"
                    + " ("
                    + " SELECT"
                    + " PRODUCT_SEQ"
                    + " ,WEIGHT"
                    + " FROM kyoei.t_product"
                    + " WHERE SHIP_SEQ = {0} AND SHIP_CHK_DATE IS NOT NULL"
                    + " UNION"
                    + " SELECT"
                    + " PRODUCT_SEQ"
                    + " ,WEIGHT"
                    + " FROM kyoei.t_t_product"
                    + " WHERE SHIP_SEQ = {0} AND SHIP_CHK_DATE IS NOT NULL"
                    + " UNION"
                    + " SELECT"
                    + " PRODUCT_SEQ"
                    + " ,WEIGHT"
                    + " FROM kyoei.t_m_product"
                    + " WHERE SHIP_SEQ = {0} AND CHK_SHIPPING IS NOT NULL"
                    + " ) p "
                    + ";"
                    , lblSeq0.Text);
                #endregion
                mydb.kyDb con = new mydb.kyDb();
                int iCount = con.iGetCount(s, DEF_CON.Constr());
                int iCount1 = con.iGetCount(s1, DEF_CON.Constr());
                if(iCount != iCount1)
                {
                    sMsg = "出荷確認出来ていないLotがあります。出荷確認を実施してから出力して下さい。";
                }
                #endregion
                if(sMsg.Length > 0)
                {
                    string[] sSend = { sMsg, "false" };
                    string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSend);
                    return;
                }

                s = string.Format(
                    "SELECT COUNT(*) FROM t_shipment_inf "
                    + " WHERE SEQ = {0} AND SC_OUT IS NOT NULL;"
                    , lblSeq0.Text);
                iCount = con.iGetCount(s, DEF_CON.Constr());
                if(iCount > 0)
                {
                    sMsg = "既に出荷連携CSVを作成済みですが、再作成しますか？";
                    string[] sSend = { sMsg, "" };
                    string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSend);
                    if (sRcv[0].Length == 0) return;
                }

                #region 抽出SQL
                string sGetInf = string.Format(
                    "SELECT"
                     + " jc.JDNNO"     //0 受注No
                     + " ,jc.LINNO"    //1 ラインNo
                     + " ,jc.TOKCD"    //2 得意先コード
                     + " ,jc.NHSCD"    //3 納品先コード
                     + " ,jc.HINCD"    //4 商品コード
                     + " ,jc.SYUYTIDT" //5
                     + " ,jc.NOKDT"    //6
                     + " ,CAST(p.WEIGHT AS SIGNED) UODSU"  //7
                     + " ,SUBSTRING_INDEX(p.LOT_NO,'-',-2) LOTID"  //8
                     + " ,CONCAT('20',SUBSTRING_INDEX(p.LOT_NO,'-',1)) HINLCDDT"  //9
                     + " ,jc.SOUCD"  //10
                     + " FROM kyoei.t_shipment_inf si"
                     + " LEFT JOIN kyoei.sc_juchu jc"
                     + " ON si.JDNNO = jc.JDNNO AND si.LINNO = jc.LINNO AND jc.LGC_DEL = '0'"
                     + " LEFT JOIN"
                     + " ("
                     + " SELECT"
                     + " po.LOT_NO"
                     + " ,po.SHIP_SEQ"
                     + " ,po.WEIGHT"
                     + " FROM kyoei.t_product po"
                     + " WHERE po.SHIP_SEQ = {0}"
                     + " UNION"
                     + " SELECT"
                     + " pt.LOT_NO"
                     + " ,pt.SHIP_SEQ"
                     + " ,pt.WEIGHT"
                     + " FROM kyoei.t_t_product pt"
                     + " WHERE pt.SHIP_SEQ = {0}"
                     + " UNION"
                     + " SELECT"
                     + " pm.LOT_NO"
                     + " ,pm.SHIP_SEQ"
                     + " ,pm.WEIGHT"
                     + " FROM kyoei.t_m_product pm"
                     + " WHERE pm.SHIP_SEQ = {0}"
                     + " ) p ON p.SHIP_SEQ = si.SEQ"
                     + " WHERE jc.JDNNO = '{1}' AND jc.LINNO = '{2}'"
                     + " ;"
                    ,lblSeq0.Text, lblDEN.Text, lblLIN.Text);
                #endregion
                string sTmp = con.GetData(sGetInf, DEF_CON.Constr());

                string scont = string.Empty;
                for (int i = 0; i < con.ds.Tables[0].Columns.Count; i++)
                {
                    if (i == 0) scont = con.ds.Tables[0].Columns[i].ColumnName;
                    else scont += "," + con.ds.Tables[0].Columns[i].ColumnName;
                }
                scont += "\r\n";
                for (int i = 0; i < con.ds.Tables[0].Rows.Count; i++)
                {
                    for (int c = 0; c < con.ds.Tables[0].Columns.Count; c++)
                    {
                        if (c == 0) scont += con.ds.Tables[0].Rows[i][c].ToString();
                        else scont += "," + con.ds.Tables[0].Rows[i][c].ToString();
                    }
                    //if (i < con.ds.Tables[0].Rows.Count - 1) scont += "\r\n"; 最後の行は改行しない
                    scont += "\r\n"; // 最後の行も改行
                }

                string SaveDir = string.Empty;
                string RealDir = @"\\10.100.10.20\share\sc_renkei\sc_juchu_ret\";
                string TestDir = @"\\10.100.10.20\share\test_sc_renkei\sc_juchu_ret\";
                if (usr.iDB == 0) SaveDir = RealDir;
                else SaveDir = TestDir;

                string fn = "RJ_";
                fn += lblDEN.Text;
                fn += "_" + lblLIN.Text;
                fn += ".csv";

                fn = SaveDir + fn;

                System.IO.StreamWriter sw = new System.IO.StreamWriter(
                fn,
                false,
                System.Text.Encoding.GetEncoding("shift_jis"));

                //LogFileに書き込む
                sw.Write(scont);
                //閉じる
                sw.Close();

                string sUPD_SI = string.Format(
                    "UPDATE t_shipment_inf SET SC_OUT = NOW() "
                    + "WHERE SEQ = {0};"
                    ,lblSeq0.Text);
                sTmp = con.ExecSql(false, DEF_CON.Constr(), sUPD_SI);
                sMsg = "出荷連携CSVを作成しました。";
                if(sMsg.Length > 0)
                {
                    string[] sSend = { sMsg, "false" };
                    string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSend);
                }
            }
            #endregion

            #region 出荷担当選択 > textBox6
            if (ctl.Name == "textBox6")
            {
                string s =
                        "SELECT "
                     + "  w.WKER_ID"  //0
                     + "  , CONCAT(w.SEI, ' ', w.MEI) 氏名"  //1
                     + "  FROM kyoei.m_worker w"
                     + "  LEFT JOIN kyoei.v_sect_name vs ON w.SECT_SEQ = vs.SECT_SEQ"
                     + "  WHERE w.BASE_SEQ = 2 AND w.LGC_DEL = '0' "
                     + "  AND (w.SECT_SEQ = 9 OR vs.OneUp_SEQ = 9 OR vs.TwoUp_SEQ = 9);";
                string[] sendT = { "出荷担当", "", s };
                string[] sRcvT = promag_frm.F04List.ShowMiniForm(this, sendT);
                if (sRcvT[0].Length > 0)
                {
                    ctl.Text = sRcvT[0];
                    pPsn = sRcvT[1];
                }
            }
            #endregion
            
            #region コンテナ番号 > button7
            if (ctl.Name == "button7")
            {
                string sMsg = string.Empty;
                string sDueDate = textBox3.Text;
                if(sDueDate.Length == 0) sMsg = "納入日は必須です。";
                if (!fn.IsDatetime(sDueDate)) sMsg = "納入日が日付形式ではありません。";

                if(label17.Text.Length > 0) // コンテナロット番号がある場合
                {
                    string[] Snd = { label17.Text, "", sDueDate};
                    _ = F10_ContLotList.ShowMiniForm(this, Snd);
                }
                else
                {
                    // 紐づいているロットがないとだめ
                    if(lblSeq0.Text.Length == 0)
                    {
                        string[] Snd1 = { "品目確定してから登録します。", "false" };
                        _ = promag_frm.F05_YN.ShowMiniForm(Snd1);
                        return;
                    }
                    string sGetW = string.Format(
                        "SELECT"
                         + " SUM(CAST(tmp.WEIGHT as UNSIGNED))"    //0
                         + " FROM ("   
                         + " SELECT"   
                         + " SHIP_SEQ"   
                         + " ,sum(WEIGHT) WEIGHT"
                         + " FROM kyoei.t_product"    
                         + " WHERE SHIP_SEQ = {0}"   
                         + " UNION"   
                         + " SELECT"    
                         + " SHIP_SEQ"
                         + " ,sum(WEIGHT) WEIGHT"
                         + " FROM kyoei.t_t_product"
                         + " WHERE SHIP_SEQ = {0}"
                         + " UNION"
                         + " SELECT"
                         + " SHIP_SEQ"
                         + " ,sum(WEIGHT) WEIGHT"
                         + " FROM kyoei.t_m_product"
                         + " WHERE SHIP_SEQ = {0}"
                         + " ) tmp"
                         + " WHERE tmp.SHIP_SEQ IS NOT NULL"
                         + ";"
                        ,lblSeq0.Text);
                    mydb.kyDb con = new mydb.kyDb();
                    int iw = con.iGetCount(sGetW, DEF_CON.Constr());
                    if (int.Parse(textBox5.Text) != iw)
                    {
                        string[] Snd0 = { "Lot選定が完了してからコンテナロットを発行します。", "false" };
                        _ = promag_frm.F05_YN.ShowMiniForm(Snd0);
                        return;
                    }
                    string s0 = label17.Text; // Lot
                    string s1 = textBox1.Text; // 出荷先
                    string s2 = sDueDate;
                    string s3 = textBox5.Text; // 数量
                    string s4 = textBox4.Text; // 品名
                    string s5 = lblSeq0.Text;
                    string[] Snd = { s0, s1, s2, s3, s4, s5 };
                    string[] Rcv = F11_CLotBC.ShowMiniForm(this, Snd);
                    if (Rcv[0].Length > 0) label17.Text = Rcv[0];
                }
                
            }
            #endregion 

            if(ctl.Name == "button11")  // 出荷票設定
            {
                if(lblSeqg.Text.Length == 0)
                {
                    string[] snd = { "出荷グレードを選択してから設定します。", "false" };
                    _ = promag_frm.F05_YN.ShowMiniForm(snd);
                    return;
                }
                mydb.kyDb con = new mydb.kyDb();
                string[] val = con.sGetVal(GetJuchuInf(lblDEN.Text, lblLIN.Text), DEF_CON.Constr());
                // [0]TOKCD [1]NHSCD [2]HINCD textbox4 出荷品 textbox1 出荷先
                string sTok = val[0];
                string sNhs = val[1];
                string[] Snd = { lblSeqg.Text, sTok, sNhs, textBox4.Text, textBox1.Text };
                _ = F12_SHIP_SETTING.ShowMiniForm(this, Snd);
            }

            if (ctl.Name == "button12")  // 別ロット管理
            {
                if(dgv0.Rows.Count <= 0)
                {
                    string[] Snd = { "ロットを選定してから作業します。", "false" };
                    _ = promag_frm.F05_YN.ShowMiniForm(Snd);
                    return;
                }
                else
                {
                    string[] Snd = { lblSeq0.Text };
                    _ = F13_ALotCrt.ShowMiniForm(this, Snd);
                }
            }
        }

        private void lblSeq_TextChanged(object sender, EventArgs e)
        {
            Label lbl = (Label)sender;
            if (lbl.Text.Length == 0) return;
            if (lbl.Text.IndexOf(";") < 0) return;
            else
            {
                lbl.Text = lbl.Text.Substring(0, lbl.Text.Length - 1);
                if (lbl.Name == "lblSeq0")
                {
                    mydb.kyDb con = new mydb.kyDb();
                    con.GetData(sGetShipment(lblSeq0.Text),DEF_CON.Constr());
                    string sGA = con.ds.Tables[0].Rows[0][8].ToString();

                    if (sGA == "562" || sGA == "688" || sGA == "728") btrt = true;
                    if (sGA.Length > 0) lblSeqg.Text = sGA + ";";

                    textBox7.Text = con.ds.Tables[0].Rows[0][1].ToString();
                    textBox4.Text = con.ds.Tables[0].Rows[0][2].ToString();
                    textBox1.Text = con.ds.Tables[0].Rows[0][3].ToString();
                    textBox2.Text = con.ds.Tables[0].Rows[0][4].ToString();
                    textBox3.Text = con.ds.Tables[0].Rows[0][5].ToString();
                    textBox5.Text = con.ds.Tables[0].Rows[0][6].ToString();
                    textBox6.Text = con.ds.Tables[0].Rows[0][7].ToString();
                    lblDEN.Text = con.ds.Tables[0].Rows[0][9].ToString();
                    lblLIN.Text = con.ds.Tables[0].Rows[0][10].ToString();
                    sHINCD = con.ds.Tables[0].Rows[0][11].ToString();
                    label17.Text = con.ds.Tables[0].Rows[0][12].ToString();
                    enableCtrl(false);
                    //button1.Text = "編集";
                    button1.Image = k001_shukka.Properties.Resources.unlock; // k001_shukka.Properties.Resources.decide
                    SetTooltip();
                    // 東レタイプかどうかでBagNoを表示するかどうか決める。
                    string s = string.Empty;
                    if (!btrt) s = sGetSlectedLots(lblSeq0.Text);
                    else s = sGetSlectedTLots(lblSeq0.Text);

                    GetData(dgv0, bs0, s);
                }

                if (lbl.Name == "lblSeqg")
                {
                    if (!checkBox2.Checked) return;
                    GetData(dgv1, bs1, sGetSelection(lblSeqg.Text));
                    // 東レタイ向けの7906の場合フラグを立てる 20200401
                    if (lbl.Text == "562") btrt = true;
                }
            }
        }

        private void enableCtrl(bool b)
        {
            if (lblDEN.Text.Length > 0 && b)
            {
                textBox7.Enabled = b; label2.Enabled = b;textBox6.Enabled = b;
                return;
            }
            Control[] all = fn.GetAllControls(this);
            foreach (Control c in all)
            {
                if (c.GetType().Equals(typeof(TextBox)) || c.GetType().Equals(typeof(Label)))
                {
                    if (c.Name != "textBox8")
                    {
                        if (c.Tag == null) c.Enabled = b;
                    }
                }
            }
        }

        private void setTags()
        {
            //checkBox1.Tag = "1MOVE_FLG";
            //textBox7.Tag = "2WEIGHT";
            //textBox9.Tag = "1BIKOU";
            //textBox10.Tag = "1C_REASON";
            //label18.Tag = "2SEQ";
        }

        private void FRM_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (e.Shift)
                {
                    ProcessTabKey(false);
                }
                else
                {
                    ProcessTabKey(true);
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = (CheckBox)sender;
            if (chk.Checked) dgv0.ReadOnly = false;
            else dgv0.ReadOnly = true;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkBox2.Checked) return;
            if (lblSeqg.Text.Length == 0)
            {
                string[] Snd = { "品目選定がされていないとLot一覧が表示できません。", "false" };
                _ = promag_frm.F05_YN.ShowMiniForm(Snd);
                checkBox2.Checked = false;
            }
            else
            {
                GetData(dgv1, bs1, sGetSelection(lblSeqg.Text));
            }
            // 東レタイ向けの7906の場合フラグを立てる 20200401
            if (lblSeqg.Text == "562") btrt = true;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string sSeq = lblSeq0.Text;
            string[] sendText = { sSeq, textBox5.Text, lblDEN.Text, lblLIN.Text};
            string[] receiveText = F04_ChkShipment.ShowMiniForm(this, sendText);
        }
        
        private void dgv0_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            bSet = true;
        }

        private void dgv0_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            //bSet = false;
        }

        private void dgv0_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            if (!bSet) return;
            int ir = e.RowIndex;
            int ic = e.ColumnIndex;
            if (ic != 1)
            {
                string[] sendT = { "変更してもBagNo以外は変更出来ません。", "false" };
                string[] sRcvT = promag_frm.F05_YN.ShowMiniForm(sendT);
                return;
            }
            DataGridView dgv = (DataGridView)sender;
            string sLot = dgv.Rows[ir].Cells[0].Value.ToString();
            if (true)
            {
                string sVal = dgv.Rows[ir].Cells[1].Value.ToString();
                string[] sendT = { "BagNoを修正しますか？", "" };
                string[] sRcvT = promag_frm.F05_YN.ShowMiniForm(sendT);
                if (sRcvT[0].Length == 0) return;

                //   , idc.VAL 数値"
                //+ "  , DATE_FORMAT(idc.REG_DATE, '%m/%d %H:%i') 測定日"
                //+ "  , CONCAT(w.SEI, w.MEI) 担当者"
                //+ "  FROM t_indicator idc"
                string s = string.Format(
                    "UPDATE t_product SET BAGNO = {0} WHERE LOT_NO = '{1}';"
                    , sVal, sLot);
                mydb.kyDb con = new mydb.kyDb();
                if (con.ExecSql(false,DEF_CON.Constr(), s).Length == 0)
                {
                    string[] seT = { "登録しました。", "false" };
                    string[] RcvT = promag_frm.F05_YN.ShowMiniForm(seT);
                    bSet = false;
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (lblSeq0.Text.Length == 0)
            {
                string[] sSend = { "出荷登録されていない出荷情報は削除出来ません。", "false" };
                string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSend);
                return;
            }
            if (dgv0.Rows.Count > 0)
            {
                string[] sSend = { "選択されたLotがあります。全て除外して下さい。", "false" };
                string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSend);
                return;
            }
            if (true)
            {
                string[] sSend = { "表示されている出荷情報を削除しますか。一度削除すると元に戻せません。", "" };
                string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSend);
                if (sRcv[0].Length == 0) return;
            }
            string s = string.Format(
                    "UPDATE t_shipment_inf SET LGC_DEL = '1'"
                    + " WHERE SEQ = {0};"
                    , lblSeq0.Text);
            mydb.kyDb con = new mydb.kyDb();
            if (con.ExecSql(false,DEF_CON.Constr(), s).Length == 0)
            {
                string[] sSend = { "削除しました。", "false" };
                string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSend);
                this.ReturnValue = new string[] { "" };
                closing();
            }
            else
            {
                string[] sSend = { "処理中にエラーが発生しました。エラー：削除処理FRM307：システム管理者にご連絡下さい。", "false" };
                string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSend);
            }
        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            string s = tb.Text;
            string sF = string.Empty;
            if (s.Length == 0) sF = string.Empty;
            else
            {
                sF = string.Format(
                    "LOT_NO LIKE '%{0}%'", s);
            }
            bs1.Filter = sF;
            FillDgvCount(dgv1, label9);
        }

        private string GetB(string LotNo)
        {
            return string.Format(
            "SELECT"
             + " p.LOT_NO"  //0
             + " ,ROUND(IFNULL(c.C_b,c2.C_b),1) b"  //1
             + " FROM kyoei.t_product p"  
             + " LEFT JOIN kyoei.m_color c"  
             + "  ON p.LOT_NO = c.LOT_NO"  
             + " LEFT JOIN kyoei.m_color c1"
             + "  ON p.LOT_NO = c1.LOT_NO"
             + " LEFT JOIN kyoei.m_color c2"
             + "  ON c2.SEQ = c1.SUB_SEQ"
             + " WHERE p.LOT_NO = '{0}'"
             + " AND c.INSPECT_SEQ IS NOT NULL"
             + " ;"
             ,LotNo);
        }
        
        private string GetJuchuInf(string Den, string Lin)
        {
            return string.Format(
                "SELECT"
                 + " j.TOKCD"  //0
                 + " ,j.NHSCD"  //1
                 + " ,j.HINCD"  //2
                 + " FROM kyoei.sc_juchu j"
                 + " WHERE j.JDNNO = '{0}'"
                 + " AND j.LINNO = '{1}'"
                 + " AND j.LGC_DEL = '0'"
                 + " ;"
                ,Den,Lin);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (dgv0.Rows.Count == 0) return;
            string sLots = string.Empty;
            for (int i = 0; i < dgv0.Rows.Count; i++)
            {
                sLots += ",'" + dgv0.Rows[i].Cells[0].Value.ToString()+"'";
            }
            sLots = sLots.Substring(1);
            string[] sSnd = { sLots, argVals[1] };
            string[] sRcv = F06_ChkLot.ShowMiniForm(this, sSnd);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            string[] sSnd = { "" };
            string[] sRcv = F07_Chk_Lot_Shipped.ShowMiniForm(this, sSnd);
        }

        private void dgv1_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            int i = dgv.Columns[e.Column.Index].Width;
        }
    }
}
