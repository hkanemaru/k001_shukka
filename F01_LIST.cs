using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using mydb;
using System.Linq;
using System;
using System.IO;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
//iTextSharp.text.FontクラスがSystem.Drawing.Fontクラスと
//混在するためiFontという別名を設定

namespace k001_shukka
{
    public partial class F01_LIST : Form
    {
        #region フォーム変数
        private bool bClose = true;
        private readonly string[] argVals; // 親フォームから受け取る引数
        public string[] ReturnValue;            // 親フォームに返す戻り値
        private readonly bool bPHide = true;  // 親フォームを隠す = True
        DateTime loadTime; // formloadの時間
        ToolTip ToolTip1;
        private string SDate = string.Empty;
        private string EDate = string.Empty;
        string sFilter = string.Empty;
        private bool bdgvCellClk = false; // dgvでクリックする場合には必須
        #endregion 

        public F01_LIST(params string[] argVals)
        {
            // 親フォームから受け取ったデータをこのインスタンスメンバに格納
            this.argVals = argVals;
            InitializeComponent();

            // ■■■ 親フォームを隠す設定 true => 隠す　default = 隠す。
            //bPHide = false; // 隠さない
            //■■■ 画面の和名
            string sTitle = "出荷情報一覧";
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
            dgv0.DataSource = bs0;
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
            F01_LIST f = new F01_LIST(s);
            f.ShowDialog(frm);

            string[] receiveText = f.ReturnValue; // -- ①
            f.Dispose();
            return receiveText;
        }

        private void F01_LIST_Load(object sender, EventArgs e)
        {
            // 開いた元フォームを隠す設定
            if (bPHide) this.Owner.Hide();
            loadTime = DateTime.Now;
            //argVals >> 0拠点番号,1WkerID, 2DBID, 3日付時間, 4FRM番号
            SetTooltip();
            lblRCaption.Left = this.Width - lblRCaption.Width - 20;
            lblRCaption.Top = lblCaption.Top;
            string s = string.Empty;

            SDate = DateTime.Today.AddMonths(-1).ToShortDateString();
            EDate = DateTime.Today.AddMonths(2).ToShortDateString();

            string sTerm = string.Empty;
            double Interval = (DateTime.Parse(EDate) - DateTime.Parse(SDate)).TotalDays;
            if (Interval == 1) sTerm = SDate;
            else sTerm = string.Format(" 期間:{0} - {1}", SDate, EDate);
            lblCaption.Text += " " + sTerm;
            fn.EnableDoubleBuffering(dgv0);

            //if (usr.author > 8)
            //{
            //    button11.Visible = true;
            //    button12.Visible = true;
            //} 
            //else
            //{
            //    button11.Visible = false;
            //    button12.Visible = false;
            //}

            CrtShipInf();

            GetData(dgv0, bs0, GetOrder());
            CheckedChanged();
            arrageTextBW(dgv0);
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
            ToolTip1.SetToolTip(button1, "エクセル出力");
            ToolTip1.SetToolTip(button2, "日付変更");
            ToolTip1.SetToolTip(button3, "新規");
            ToolTip1.SetToolTip(button4, "納品書出力");
            ToolTip1.SetToolTip(button5, "ロット明細印刷");
            ToolTip1.SetToolTip(button6, "表示更新");
            ToolTip1.SetToolTip(button7, "アンマッチ確認");
            ToolTip1.SetToolTip(button8, "売上番号登録");
            ToolTip1.SetToolTip(button9, "受領書確認");
            ToolTip1.SetToolTip(button10, "エラー表示");
            ToolTip1.SetToolTip(button11, "検査結果表示");
            ToolTip1.SetToolTip(button12, "承認依頼");
            ToolTip1.SetToolTip(button13, "メモ追加");
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

        private void CrtShipInf()
        {
            mydb.kyDb con = new mydb.kyDb();
            int iMax = con.iGetCount("SELECT MAX(SEQ) FROM t_shipment_inf;",DEF_CON.Constr());
            iMax++;
            con.GetData(sChkJuchuExist(), DEF_CON.Constr());
            if (con.ds.Tables[0].Rows.Count == 0) return;
            string sIns = "INSERT INTO t_shipment_inf (";
            sIns += "SEQ,LOC_SEQ,JC_SEQ,JDNNO,LINNO,UPD_DATE,REG_DATE,LGC_DEL) VALUES ";
            string sSql = string.Empty;
            string sVal = string.Empty;
            string sErr = string.Empty;
            
            for (int i = 0; i < con.ds.Tables[0].Rows.Count; i++)
            {
                /*
                 jc.SEQ,jc.JDNNO,jc.LINNO,jc.SOUCD
                 */
                try
                {
                    string lcn = con.ds.Tables[0].Rows[i][3].ToString();
                    lcn = lcn.Replace("0", "");
                    sErr = lcn; // エラーが起きた時の為
                    lcn = (int.Parse(lcn) - 1).ToString();
                    sVal += ",(" + iMax.ToString();
                    sVal += "," + lcn;
                    sVal += "," + con.ds.Tables[0].Rows[i][0].ToString();
                    sVal += ",'" + con.ds.Tables[0].Rows[i][1].ToString() + "'";
                    sVal += ",'" + con.ds.Tables[0].Rows[i][2].ToString() + "'";
                    sVal += ",NOW(),NOW(),'0')";
                    iMax++;
                }
                catch
                {
                    string[] sSet = { "登録出来ませんでした。", "false" };
                    string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSet);
                    continue;
                }
            }
            if(sVal.Length > 0 && sVal.Substring(sVal.Length-1) == ")")
            {
                sVal = sVal.Substring(1) + ";";
                sSql = sIns + sVal;
                string s = con.ExecSql(false, DEF_CON.Constr(), sSql);
            }
        }

        private string GetOrder()
        {
            string sMN = string.Empty;
            string sTerm = string.Empty;
            sTerm = string.Format("BETWEEN '{0}' AND '{1}'", SDate.Replace("/",""), EDate.Replace("/", ""));
            //sTerm = string.Format("BETWEEN '{0}' AND '{1}'", SDate.Substring(2), EDate.Substring(2));
            string s = string.Empty;
            string s1 = @"[0-9]{8}";
            s = string.Format(
                   "SELECT"
                 + "  si.SEQ 出荷No"                                                // 0
                 + " , si.JDNNO 伝票番号"                                           // 1
                 + " , si.LINNO 行番号"                                             // 2
                 + " , CASE"
                 + "   WHEN jc.SYUYTIDT IS NOT NULL"
                 //+ "   THEN CONCAT(SUBSTRING(jc.SYUYTIDT,3,2),'/',SUBSTRING(jc.SYUYTIDT,5,2)"
                 //+ "    ,'/',SUBSTRING(jc.SYUYTIDT,7,2))"
                 + "   THEN DATE_FORMAT(jc.SYUYTIDT,  '%y/%m/%d')"
                 + "   ELSE DATE_FORMAT(si.SHIP_DATE, '%y/%m/%d') END 出荷日"       // 3

                 + " , CASE"
                 + "   WHEN jc.NOKDT IS NOT NULL"
                 + "   THEN DATE_FORMAT(jc.NOKDT,  '%y/%m/%d')"
                 + "   ELSE DATE_FORMAT(si.DUE_DATE, '%y/%m/%d') END 納品日"       // 4

                 + " , CASE WHEN LOCATE('①',jc.LINCM) + LOCATE('②',jc.LINCM)"
                 + "    + LOCATE('③',jc.LINCM) + LOCATE('④',jc.LINCM)"
                 + "    + LOCATE('⑤',jc.LINCM) + LOCATE('⑥',jc.LINCM)"
                 + "    + LOCATE('⑦',jc.LINCM) + LOCATE('⑧',jc.LINCM)"
                 + "    > 0 THEN CONCAT(LEFT(jc.LINCM,1),IFNULL(jc.NHSNM, si.DESTINATION))"
                 + "    WHEN jc.NHSNM != '' THEN jc.NHSNM"            
                 + "    WHEN jc.TOKNM != '' THEN jc.TOKNM"
                 + "    ELSE si.DESTINATION END 出荷先"                            // 5
                 + " , IFNULL(jc.HINNMA, si.ITEM) 品名"                            // 6
                 + " , IFNULL(CAST(jc.UODSU AS SIGNED),si.SHIPMENT_QUANTITY) 数量" // 6 7
                 + " , CASE "
                 + "   WHEN si.JC_SEQ IS NOT NULL AND si.JC_SEQ != jc.SEQ "           // GA_SEQ未設定
                 + "     THEN '0【要確認】受注データ更新' "
                 + "   WHEN jc.LGC_DEL = '1'"                                         // GA_SEQ未設定
                 + "     THEN '0【要確認】受注データ削除' "
                 + "   WHEN si.DN_CHK_DATE IS NOT NULL" 
                 + "     THEN '8完了'"
                 + "   WHEN jcr.STATUS IS NOT NULL AND jcr.cm regexp '{1}' AND si.DN_CHK_DATE IS NULL" // 返却STATUS有 STATUS=1"
                 + "     THEN '7受領確認待'"
                 + "   WHEN si.GA_SEQ IS NULL "                                       // GA_SEQ未設定
                 + "     THEN '1品目選定待' "
                 + "   WHEN si.GA_SEQ IS NOT NULL AND (IFNULL(CAST(jc.UODSU AS SIGNED),si.SHIPMENT_QUANTITY) != IFNULL(tmp.wt,0) OR IFNULL(tmp.wt,0) = 0)" // GA_SEQ設定済 選定LOT重量合計<>ship_inf重量
                 + "     THEN '2Lot選定待' "
                 + "   WHEN IFNULL(CAST(jc.UODSU AS SIGNED),si.SHIPMENT_QUANTITY) = IFNULL(tmp.wt,0) AND tmp.CHK_DATE IS NULL" // 選定LOT重量合計=ship_inf重量 出荷確認無
                 + "     THEN '3出荷確認待' "
                 + "   WHEN si.JDNNO IS NOT NULL AND si.SC_OUT IS NULL"          //  受注番号有り SC_OUT無し"
                 + "     THEN '4CSV出力待'"
                 + "   WHEN si.SC_OUT IS NOT NULL AND jcr.STATUS IS NULL"        // SC_OUTあり 返却STATUS無し"
                 + "     THEN '5SC連携待'"
                 + "   WHEN jcr.STATUS IS NOT NULL AND jcr.STATUS = '1' AND si.DN_CHK_DATE IS NULL" // 返却STATUS有 STATUS=1"
                 + "     THEN '6SC連携エラー'"
                 //+ "   WHEN tmp.CHK_DATE IS NOT NULL AND si.DN_CHK_DATE IS NULL" // 出荷確認有 受領未確認
                 //+ "     THEN '7受領確認待' "
                 + "   ELSE '9未設定' "
                 + "   END STATUS"                                               // 7 8
                 + " , CASE WHEN jcr.STATUS = '0' THEN '正'"
                 + "   WHEN jcr.STATUS = '1' THEN 'E' ELSE '-' END 連"           // 8 9
                 //+ " , CASE WHEN (LOCATE(':',jcr.CM) + LOCATE('：',jcr.CM)) > 0"
                 //+ "   THEN SUBSTRING_INDEX(jcr.CM, '：', -1) "
                 //+ "   ELSE '' END 出荷番号"                                     // 9 10
/*                 + " , jcr.CM 出荷番号"      */                               // 9  11
                 + " , REPLACE(jcr.CM,'正常終了：','') 出荷番号"                 // 9  10
                 + " , CASE WHEN si.BIKOU IS NULL THEN '' ELSE '*' END メ"          //10  111
                 + " , CONCAT(w.SEI, w.MEI) 更新者"                              // 11 12
                 + " , DATE_FORMAT(si.UPD_DATE, '%y/%m/%d') 更新日 "             //  12  13
                 + " , jc.TOKCD"                                                 //  13  14
                 + " , jc.HINCD"                                                 //  14  15
                 + " , jc.SOUCD LOC_SEQ"                                              //   15  16 // excelで使用
                 + " , si.BIKOU"                                                // 16  17
                 + " , bc.LOT コLot"                                                // 17  18
                 + " FROM"
                 + "  kyoei.t_shipment_inf si "
                 + "  LEFT JOIN kyoei.m_worker w "
                 + "    ON si.UPD_ID = w.WKER_ID AND w.LGC_DEL = '0' "
                 + "  LEFT JOIN kyoei.sc_juchu jc"
                 + "   ON si.JDNNO = jc.JDNNO AND si.LINNO = jc.LINNO"
                 + "  LEFT JOIN kyoei.sc_juchu_ret jcr"
                 + "   ON si.JDNNO = jcr.JDNNO AND si.LINNO = jcr.LINNO"
                 + "  LEFT JOIN kyoei.t_memo mm"
                 + "   ON mm.SEQ = si.BIKOU AND mm.LGC_DEL = '0'"
            #region oym mrf tcg 3工場の P_SEQ,WEIGHT,SHIP_SEQ CHK_DATE
                 + "  LEFT JOIN ( "
            #region 旧コード
                 //+ "    SELECT"
                 //+ "      CAST(SUM(pdc.WEIGHT) as signed) wt"
                 //+ "      , pdc.SHIP_SEQ "
                 //+ "      , AVG(pdc.CHK_DATE) CHK_DATE"
                 //+ "    FROM"
                 //+ "    ( "
                 //+ "      SELECT"
                 //+ "        p.PRODUCT_SEQ"
                 //+ "        , p.WEIGHT"
                 //+ "        , p.SHIP_SEQ"
                 //+ "        , p.SHIP_CHK_DATE CHK_DATE"
                 //+ "      FROM"
                 //+ "        t_product p"
                 //+ "      WHERE"
                 //+ "        p.SHIP_SEQ IS NOT NULL"
                 //+ "      UNION "
                 //+ "      SELECT"
                 //+ "        tcg.PRODUCT_SEQ"
                 //+ "        , tcg.WEIGHT"
                 //+ "        , tcg.SHIP_SEQ "
                 //+ "        , tcg.SHIP_CHK_DATE CHK_DATE"
                 //+ "      FROM"
                 //+ "        t_t_product tcg"
                 //+ "      WHERE"
                 //+ "        tcg.SHIP_SEQ IS NOT NULL"
                 //+ "      UNION "
                 //+ "      SELECT"
                 //+ "        mp.PRODUCT_SEQ"
                 //+ "        , mp.WEIGHT"
                 //+ "        , mp.SHIP_SEQ "
                 //+ "        , mp.CHK_SHIPPING CHK_DATE"
                 //+ "      FROM"
                 //+ "        t_m_product mp"
                 //+ "      WHERE"
                 //+ "        mp.SHIP_SEQ IS NOT NULL"
                 //+ "    ) pdc"
                 //+ "    GROUP BY pdc.SHIP_SEQ"
            #endregion

                 + " SELECT"
                 + "  CAST(SUM(pdc.WEIGHT) as signed) wt"    //0
                 + "  , pdc.SHIP_SEQ"    //1
                 + "  , AVG(pdc.CHK_DATE) CHK_DATE "    //2
                 + " FROM"
                 + " ( "
                 + " SELECT"
                 + "   p.PRODUCT_SEQ"
                 + "   , p.WEIGHT"
                 + "   , p.SHIP_SEQ"
                 + "   , p.SHIP_CHK_DATE CHK_DATE "
                 + " FROM"
                 + "   t_product p "
                 + " WHERE"
                 + "   p.SHIP_SEQ IN("
                 + " SELECT"
                 + "   si.SEQ"
                 + " FROM"
                 + "   kyoei.t_shipment_inf si "
                 + "   LEFT JOIN kyoei.sc_juchu sj "
                 + "     ON si.JDNNO = sj.JDNNO "
                 + "     AND si.LINNO = sj.LINNO "
                 + " WHERE"
                 + "   IFNULL( "
                 + "     sj.SYUYTIDT"
                 + "     , DATE_FORMAT(si.SHIP_DATE, '%Y%m%d')"
                 + "   ) {0}"
                 + "   AND si.LGC_DEL = '0'"
                 + " )"
                 + " UNION "
                 + " SELECT"
                 + "   tcg.PRODUCT_SEQ"
                 + "   , tcg.WEIGHT"
                 + "   , tcg.SHIP_SEQ"
                 + "   , tcg.SHIP_CHK_DATE CHK_DATE "
                 + " FROM"
                 + "   t_t_product tcg "
                 + " WHERE"
                 + "   tcg.SHIP_SEQ IN("
                 + " SELECT"
                 + "   si.SEQ"
                 + " FROM"
                 + "   kyoei.t_shipment_inf si "
                 + "   LEFT JOIN kyoei.sc_juchu sj "
                 + "     ON si.JDNNO = sj.JDNNO "
                 + "     AND si.LINNO = sj.LINNO "
                 + " WHERE"
                 + "   IFNULL( "
                 + "     sj.SYUYTIDT"
                 + "     , DATE_FORMAT(si.SHIP_DATE, '%Y%m%d')"
                 + "   ) {0}"
                 + "   AND si.LGC_DEL = '0'"
                 + " )"
                 + " UNION "
                 + " SELECT"
                 + "   mp.PRODUCT_SEQ"
                 + "   , mp.WEIGHT"
                 + "   , mp.SHIP_SEQ"
                 + "   , mp.CHK_SHIPPING CHK_DATE "
                 + " FROM"
                 + "   t_m_product mp "
                 + " WHERE"
                 + "   mp.SHIP_SEQ IN("
                 + " SELECT"
                 + "   si.SEQ"
                 + " FROM"
                 + "   kyoei.t_shipment_inf si "
                 + "   LEFT JOIN kyoei.sc_juchu sj "
                 + "     ON si.JDNNO = sj.JDNNO "
                 + "     AND si.LINNO = sj.LINNO "
                 + " WHERE"
                 + "   IFNULL( "
                 + "     sj.SYUYTIDT"
                 + "     , DATE_FORMAT(si.SHIP_DATE, '%Y%m%d')"
                 + "   ) {0}"
                 + "   AND si.LGC_DEL = '0'"
                 + " )"
                 + ") pdc "
                 + "     GROUP BY  pdc.SHIP_SEQ"


                 + "  ) tmp "
                 + "    ON tmp.SHIP_SEQ = si.SEQ "
                 + "  LEFT JOIN t_can_barcode bc "
                 + "    ON bc.SHIP_SEQ = si.SEQ AND bc.LOCATION IS NULL"
            #endregion
                 + " WHERE"
                 //+ "   si.UPD_DATE > DATE_ADD(CURRENT_DATE (), interval - 6 month) "
                 //+ "   si.UPD_DATE {0}"
                 + " IFNULL(jc.SYUYTIDT,DATE_FORMAT(si.SHIP_DATE,'%Y%m%d')) {0}"
                 //+ "   AND si.LOC_SEQ IN (2,11) "
                 + "   AND si.LGC_DEL = '0' "
                 + " GROUP BY"
                 + "   si.SEQ "
                 + " ORDER BY"
                 + " IFNULL(jc.SYUYTIDT,DATE_FORMAT(si.SHIP_DATE,'%Y%m%d')) DESC"
                 + " , CASE"
                 + "   WHEN jc.NOKDT IS NOT NULL"
                 + "   THEN jc.NOKDT"
                 + "   ELSE DATE_FORMAT(si.DUE_DATE, '%Y%m%d') END DESC"

                 + "   ,si.JDNNO DESC,si.LINNO ASC, si.SEQ DESC"
                 , sTerm, s1);
            return s;
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
                
                mydb.kyDb con = new mydb.kyDb();

                con.GetData(sSel, DEF_CON.Constr());


                bs.DataSource = con.ds.Tables[0];

                // 並べ替えを禁止 >> セルが狭くなる
                //foreach (DataGridViewColumn column in dgv.Columns) // 並替禁止
                //{
                //    column.SortMode = DataGridViewColumnSortMode.NotSortable;
                //}

                //ヘッダーとすべてのセルの内容に合わせて、列の幅を自動調整する
                //dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                #region 手動でセル幅を指定する場合
                #region sel内容

                #endregion
                int[] iw;
                int[] icol;
                // 列幅を整える
                icol = new int[] { 0, 1, 2, 3, 4,     5, 6, 7,     8, 9, 10, 11 ,12,13,18};
                iw = new int[] { 48, 83, 40, 77,77, 220, 180, 70, 90, 30, 83,28, 67, 77,102 };
                fn.setDgvWidth(dgv, icol, iw);
                #endregion
                // 数量
                //dgv.Columns[6].DefaultCellStyle.Format = "#,0";
                dgv.Columns["数量"].DefaultCellStyle.Format = "#,0";

                icol = new int[] { 13, 14, 15, 16 };
                fn.setDgvInVisible(dgv, icol);

                icol = new int[] { 0, 7, 8 };
                iw = new int[] { -1, -1, 1 };
                fn.setDgvAlign(dgv, icol, iw);
                if (fn.dgvWidth(dgv) < 1360)
                {
                    this.Width = fn.dgvWidth(dgv) + 40;
                    dgv.Width = fn.dgvWidth(dgv);
                }
                else
                {
                    this.Width = 1300;
                    dgv.Width = this.Width - 40;
                }
                dgv.ClearSelection();
                //CheckedChanged();
                
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

        private void btn_Click(object sender, EventArgs e)
        {
                Button btn = (Button)sender;
            if (btn.Name == "button2") // 日付変更
            {
                if (SDate.Length == 0)
                {
                    SDate =
                        DateTime.Today.AddDays(-1 * DateTime.Today.Day + 1).ToShortDateString();
                    EDate = SDate;
                }
                string[] sSet = { SDate, DateTime.Parse(EDate).AddDays(-1).ToShortDateString(), "" };
                string[] sRcv = promag_frm.F06_SelDate.ShowMiniForm(this, sSet);
                if (sRcv[0].Length == 0) return;
                SDate = sRcv[0];
                EDate = DateTime.Parse(sRcv[1]).AddDays(1).ToShortDateString();
                GetData(dgv0, bs0, GetOrder());
                arrageTextBW(dgv0);
                string sTitle = "出荷情報一覧";
                lblCaption.Text = fn.frmTxt(sTitle);
                string sTerm = string.Empty;
                double Interval = (DateTime.Parse(EDate) - DateTime.Parse(SDate)).TotalDays;
                if (Interval == 1) sTerm = SDate;
                else sTerm = string.Format(" 期間:{0} - {1}", SDate, EDate);
                lblCaption.Text += " " + sTerm;

            }
            if (btn.Name == "button6") // 表示更新
            {
                CrtShipInf();
                GetData(dgv0, bs0, GetOrder());
                arrageTextBW(dgv0);
            }
            if (btn.Name == "button1") // excel
            {
                ExportXls(dgv0);
            }
            if (btn.Name == "button3")
            {
                string[] sendText = { "" };
                string[] reT = F02_EditOrder.ShowMiniForm(this, sendText);
                GetData(dgv0, bs0, GetOrder());
                arrageTextBW(dgv0);
            }
            // 納品書
            if (btn.Name == "button4")
            {
                string sNo = dgv0.CurrentRow.Cells[1].Value.ToString();
                string sLn = dgv0.CurrentRow.Cells[2].Value.ToString();
                string[] sSnd = { sNo, sLn };
                string[] sRcv = F05_Dialog.ShowMiniForm(this, sSnd);
                prtVoucher(sRcv[0]);
            }
            // ロット明細
            if (btn.Name == "button5")
            {
                prtLotList();
            }
            // 特殊処理 !マークボタン
            if(btn.Name == "button7")
            {
                string s = dgv0.CurrentRow.Cells["STATUS"].Value.ToString();
                if(s.Substring(0,1) == "0")
                {
                    string sMsg = string.Empty;
                    if(s.IndexOf("受注データ更新") >= 0)
                    {
                        string sNo = dgv0.CurrentRow.Cells[1].Value.ToString();
                        string sLn = dgv0.CurrentRow.Cells[2].Value.ToString();
                        string sSql = string.Format(
                            "UPDATE kyoei.t_shipment_inf SET"
                             + " JC_SEQ = ("
                             + " SELECT"
                             + " sj.SEQ"
                             + " FROM kyoei.sc_juchu sj"
                             + " WHERE sj.JDNNO = '{0}' AND sj.LINNO = '{1}' AND sj.LGC_DEL = '0'"
                             + " )"
                             + " ,ITEM = ("
                             + " SELECT"
                             + " sj.HINNMA"
                             + " FROM kyoei.sc_juchu sj"
                             + " WHERE sj.JDNNO = '{0}' AND sj.LINNO = '{1}' AND sj.LGC_DEL = '0'"
                             + " )"
                             + " WHERE JDNNO = '{0}' AND LINNO = '{1}';"
                            , sNo, sLn);

                        sMsg = "受注データが更新されています。\r\n変更した内容を反映させますか？";
                        string[] sSnd = { sMsg, "" };
                        string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSnd);
                        if (sRcv[0].Length == 0) return;
                        kyDb con = new kyDb();
                        con.ExecSql(false, DEF_CON.Constr(), sSql);
                        RefleshDgv();
                    }
                    if(s.IndexOf("受注データ削除") >= 0)
                    {
                        string sSeq = dgv0.CurrentRow.Cells[0].Value.ToString();
                        string sSql = string.Format(
                            "UPDATE kyoei.t_shipment_inf SET"
                             + " LGC_DEL = '1'"
                             + " WHERE SEQ = {0};"
                            , sSeq);

                        sMsg = "受注データがカクテル側で取消されています。。\r\n"
                            + "選択した出荷情報を削除しますか？";
                        string[] sSnd = { sMsg, "" };
                        string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSnd);
                        if (sRcv[0].Length == 0) return;

                        string sChkSql = string.Format(
                            "SELECT COUNT(*) FROM kyoei.t_product p "
                            + " WHERE p.SHIP_SEQ = {0};"
                            , sSeq);
                        kyDb con = new kyDb();
                        if(con.iGetCount(sChkSql,DEF_CON.Constr()) > 0)
                        {
                            sMsg = "製品Lotがこの出荷に紐付けされています。\r\n"
                            + "紐付けを解除後オーダーの取消を行ってく下さい。";
                            string[] sSnd1 = { sMsg, "false" };
                            string[] sRcv1 = promag_frm.F05_YN.ShowMiniForm(sSnd);
                            return;
                        }
                        con.ExecSql(false, DEF_CON.Constr(), sSql);
                        RefleshDgv();
                    }
                }
            }
            // //SEQ,受注番号,STATUS,LINO
            // 売上番号登録
            if (btn.Name == "button8")
            {
                if(dgv0.SelectedRows.Count == 1)
                {
                    string s0 = dgv0.CurrentRow.Cells[0].Value.ToString();
                    string s1 = dgv0.CurrentRow.Cells[1].Value.ToString();
                    
                    string s2 = dgv0.CurrentRow.Cells["STATUS"].Value.ToString();
                    if (s2.Length > 0) s2 = s2.Substring(0, 1);
                    string s3 = dgv0.CurrentRow.Cells[2].Value.ToString();
                    string s4 = dgv0.CurrentRow.Cells["コLot"].Value.ToString();

                    if (s2 == "6" || s2 == "1" || s2 == "2" || s2 == "4" 
                                            || (s2 == "3" && s4.Length > 0))
                    {
                        if (s1.Length > 0 && s2.Length > 0)
                        {
                            string[] sSnd = { s0, s1, s2, s3 };
                            string[] sRcv = F08_ChkShipment.ShowMiniForm(this, sSnd);
                        }
                    }
                    else
                    {
                        string[] snd = { "売上番号登録出来るSTATUSではありません。", "false" };
                        string[] reT = promag_frm.F05_YN.ShowMiniForm(snd);
                        return;
                    }
                }
                else
                {
                    string[] snd = { "出荷情報は1行だけ選択して下さい","false" };
                    string[] reT = promag_frm.F05_YN.ShowMiniForm(snd);
                    return;
                }   
            }
            // 受領書確認
            if (btn.Name == "button9")
            {
                if (dgv0.SelectedRows.Count == 1)
                {
                    string s0 = dgv0.CurrentRow.Cells[0].Value.ToString();
                    string s1 = dgv0.CurrentRow.Cells[1].Value.ToString();
                    
                    string s2 = dgv0.CurrentRow.Cells["STATUS"].Value.ToString();
                    if (s2.Length > 0) s2 = s2.Substring(0, 1);
                    string s3 = dgv0.CurrentRow.Cells[2].Value.ToString();
                    if (s2 == "7")
                    {
                        if (s1.Length > 0 && s2.Length > 0)
                        {
                            string[] sSnd = { s0, s1, s2, s3 };
                            string[] sRcv = F08_ChkShipment.ShowMiniForm(this, sSnd);
                        }
                    }
                    else
                    {
                        string[] snd = { "受領書確認登録出来るSTATUSではありません。", "false" };
                        string[] reT = promag_frm.F05_YN.ShowMiniForm(snd);
                        return;
                    }
                }
                else
                {
                    string[] snd = { "出荷情報は1行だけ選択して下さい", "false" };
                    string[] reT = promag_frm.F05_YN.ShowMiniForm(snd);
                    return;
                }
            }
            //　00018156_002_取込結果　
            if (btn.Name == "button10")
            {
                string s = string.Empty;
                if (dgv0.SelectedRows.Count != 1) s = "1";
                
                if (dgv0.CurrentRow.Cells["連"].Value.ToString() != "E") s = "2";
                if (s.Length > 0)
                {
                    string[] snd = { "エラーの行を1行選択してからクリックして下さい。", "false" };
                    string[] reT = promag_frm.F05_YN.ShowMiniForm(snd);
                    return;
                }
                string s1 = dgv0.CurrentRow.Cells[1].Value.ToString();
                string s2 = dgv0.CurrentRow.Cells[2].Value.ToString();
                string sTarget = string.Format("*{0}_{1}_取込結果*", s1, s2);
                string dirPath = @"\\10.100.10.20\share\sc_renkei\sc_juchu_bk\";
                var fileInfo = new DirectoryInfo(dirPath).EnumerateFiles(sTarget)
                    .OrderByDescending(_file => _file.CreationTime)
                    .FirstOrDefault();
                if(fileInfo == null)
                {
                    string[] sSend = { "連携ファイルが見つかりません。", "false" };
                    string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSend);
                    return;
                }
                s = fileInfo.ToString();
                if (s.Substring(s.Length - 4) != ".csv")
                {
                    s = dirPath + fileInfo.ToString();
                    s1 = s + ".csv";
                    File.Move(s, s1);
                }
                else s1 = dirPath + fileInfo.ToString();
                System.Diagnostics.Process.Start(s1);
            }
            // Labo
            if(btn.Name == "button11")
            {
                string sSeq = dgv0.CurrentRow.Cells[0].Value.ToString();
                fn.CrtUsrIni("2", "011" + sSeq);
                var proc = new System.Diagnostics.Process();
                // C:\Users\h-kanemaru\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\kProductPlan
                try
                {
                    string s = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    s += @"\Microsoft\Windows\Start Menu\Programs\HINKAN\HINKAN.appref-ms";
                    if (System.IO.File.Exists(s))
                    {
                        proc.StartInfo.FileName = s;
                        proc.Start();
                    }
                    else
                    {
                        string[] sSet = { "フォルダが開きますので、「setup」を実行して下さい。", "false" };
                        _ = promag_frm.F05_YN.ShowMiniForm(sSet);
                        System.Diagnostics.Process.Start(@"\\10.100.10.20\tetra\HINKAN");
                    }
                }
                catch
                {
                    string[] sSet = { "フォルダが開きますので、「setup」を実行して下さい。", "false" };
                    _ = promag_frm.F05_YN.ShowMiniForm(sSet);
                    System.Diagnostics.Process.Start(@"\\10.100.10.20\tetra\HINKAN");
                }
            }
            // 出荷承認　　0 si.SEQ 出荷No  1 i.JDNNO 伝票番号  2 si.LINNO 行番号
            if (btn.Name == "button12")
            {
                if(dgv0.SelectedRows.Count != 1)
                {
                    string[] snd = { "出荷情報を1行選択して下さい。", "false" };
                    string[] reT = promag_frm.F05_YN.ShowMiniForm(snd);
                    return;
                }
                string sSipSEQ = dgv0.CurrentRow.Cells[0].Value.ToString();
                
                if (dgv0.CurrentRow.Cells["STATUS"].Value.ToString().Substring(0,1) == "0" ||
                    dgv0.CurrentRow.Cells["STATUS"].Value.ToString().Substring(0, 1) == "1" ||
                    dgv0.CurrentRow.Cells["STATUS"].Value.ToString().Substring(0, 1) == "2")
                {
                    string[] snd = { "Lot選定したものが対象となります。", "false" };
                    _ = promag_frm.F05_YN.ShowMiniForm(snd);
                    return;
                }
                if (dgv0.SelectedRows.Count == 1)
                {
                    string sGetCount = string.Format(
                        "SELECT COUNT(*)"
                        + " FROM kyoei.t_ship_permission sp"
                        + " WHERE sp.S_SEQ = {0} AND sp.LGC_DEL = '0';"
                        , sSipSEQ);

                    kyDb con = new kyDb();

                    if(con.iGetCount(sGetCount,DEF_CON.Constr()) == 0)  //　承認依頼していない場合
                    {
                        string[] snd = { "選択した出荷Lotの出荷承認を依頼しますか", "" };
                        string[] reT = promag_frm.F05_YN.ShowMiniForm(snd);
                        if (reT[0].Length == 0) return;
                        string sIns = string.Format(
                            "INSERT INTO t_ship_permission ("
                            + "S_SEQ,REQUEST_ID, REQUEST_DATE,LGC_DEL,LOCATION"
                            + ") VALUES ("
                            + "{0}, '{1}', NOW(), '0', 2"
                            + ");"
                            , sSipSEQ, usr.id);
                        if(con.ExecSql(false,DEF_CON.Constr(), sIns).Length == 0)
                        {
                            string[] snd1 = { "出荷承認を依頼しました。", "false" };
                            string[] reT1 = promag_frm.F05_YN.ShowMiniForm(snd1);
                        }
                    }
                    if (true)
                    {
                        string[] snd = { "承認画面を開きますか？", "" };
                        string[] reT = promag_frm.F05_YN.ShowMiniForm(snd);
                        if (reT[0].Length == 0) return;
                    }
                    string sSEQ = dgv0.CurrentRow.Cells[0].Value.ToString();
                    
                    string sDate = dgv0.CurrentRow.Cells["出荷日"].Value.ToString();
                    sDate = "20" + sDate;
                    string sDest = dgv0.CurrentRow.Cells["出荷先"].Value.ToString();
                    string sGrade = dgv0.CurrentRow.Cells["品名"].Value.ToString();
                    string sWeight = dgv0.CurrentRow.Cells["数量"].Value.ToString();

                    // ダブルクリック前に列の並び替えが行われていれば、その状態を記憶
                    string sOrder = string.Empty;
                    string sOrdColName = string.Empty;
                    int iFr = dgv0.FirstDisplayedScrollingRowIndex;
                    if (dgv0.SortedColumn != null)
                    {
                        if (dgv0.SortOrder.ToString() == "Ascending") sOrder = "ASC";
                        else sOrder = "DESC";
                        sOrdColName = dgv0.SortedColumn.Name;
                    }

                    string[] sRel = { sSEQ, sDate, sDest, sGrade, sWeight };
                    _ = F09_Permit.ShowMiniForm(this, sRel);


                    GetData(dgv0, bs0, GetOrder());
                    arrageTextBW(dgv0);
                    // 並び順をダブルクリック前に戻し値を検索
                    int ir = 0;
                    if (sOrder.Length > 0)
                    {
                        bs0.Sort = string.Format("{0} {1}", sOrdColName, sOrder);

                        for (int r = 0; r < dgv0.Rows.Count; r++)
                        {
                            if (dgv0.Rows[r].Cells[0].Value.ToString() == sSEQ)
                            {
                                ir = r;
                                break;
                            }
                        }
                    }
                    if (ir < dgv0.Rows.Count) dgv0.Rows[ir].Selected = true;
                    dgv0.FirstDisplayedScrollingRowIndex = iFr;
                }
            }
            // メモ登録
            if (btn.Name == "button13")
            {
                string sNo = dgv0.CurrentRow.Cells["BIKOU"].Value.ToString();
                string[] sSnd = { "memo",sNo };
                string[] sRcv = F05_Dialog.ShowMiniForm(this, sSnd);
                if(sRcv[0].Length > 0)
                {
                    string sLock = "SELECT COUNT(*) FROM kyoei.t_tlock WHERE TBL_NAME = 't_memo';"; // t_memo >> 長さは20字以内
                    mydb.kyDb con = new kyDb();
                    if (con.iGetCount(sLock,DEF_CON.Constr()) > 0)
                    {
                        string[] seT = { "現在他のユーザーが更新中です。登録をやり直して下さい。\r\n"
                                + "改善しない場合はシステム管理者にご連絡下さい。", "false" };
                        string[] rcvT = promag_frm.F05_YN.ShowMiniForm(seT);
                        return;
                    }
                    // ロックテーブルに作業を登録
                    sLock = "INSERT INTO t_tlock (TBL_NAME) VALUES ('t_memo');";
                    bool bcontinue = true;
                    if (con.ExecSql(false,DEF_CON.Constr(), sLock).Length > 0)
                    {
                        string[] seT = { "登録に失敗しました。連続する場合はシステム管理者にご連絡下さい。", "false" };
                        string[] rcvT = promag_frm.F05_YN.ShowMiniForm(seT);
                        bcontinue = false;
                        return;
                    }
                    // 登録処理
                    if (bcontinue)
                    {
                        string sSql = string.Empty;
                        // memo_tableの数を数える
                        sSql = "SELECT COUNT(*) FROM kyoei.T_memo;";
                        int iCnt = con.iGetCount(sSql, DEF_CON.Constr());
                        iCnt++;
                        // SHIP_INFとmemo_tableに書き込む
                        string subSEQ = string.Empty;
                        if (sRcv[1].Length == 0) subSEQ = iCnt.ToString();
                        else subSEQ = sRcv[1];
                        sSql = string.Format(
                            "UPDATE kyoei.t_shipment_inf SET BIKOU = {0} WHERE SEQ = {1};"
                            , iCnt.ToString(), dgv0.CurrentRow.Cells[0].Value.ToString());
                        sSql += string.Format(
                            "INSERT INTO kyoei.t_memo (SEQ, SUB_SEQ, CONTENT, UPD_DATE, UPD_ID, LGC_DEL) VALUES "
                            + " ({0}, {3}, '{1}', NOW(), '{2}', '0');"
                            , iCnt, sRcv[0], usr.id, subSEQ);
                        if (con.ExecSql(false, DEF_CON.Constr(), sSql).Length > 0)
                        {
                            string[] seT = { "登録に失敗しました。連続する場合はシステム管理者にご連絡下さい。", "false" };
                            string[] rcvT = promag_frm.F05_YN.ShowMiniForm(seT);
                            // ロックテーブルを抹消
                            sLock = "DELETE FROM t_tlock WHERE TBL_NAME = 't_memo';";
                            con.ExecSql(false,DEF_CON.Constr(), sLock);
                            return;
                        }
                    }
                    // ロックテーブルを抹消
                    sLock = "DELETE FROM t_tlock WHERE TBL_NAME = 't_memo';";
                    con.ExecSql(false, DEF_CON.Constr(), sLock);

                    //ひゅう字更新
                    DataGridView dgv = this.dgv0;
                    // データ欄以外は何もしない
                    if (!bdgvCellClk) return;

                    int ir = dgv.CurrentCell.RowIndex;
                    int ifr = dgv.FirstDisplayedScrollingRowIndex;
                    // ダブルクリック前に列の並び替えが行われていれば、その状態を記憶
                    string sOrder = string.Empty;
                    string sOrdColName = string.Empty;
                    if (dgv.SortedColumn != null)
                    {
                        if (dgv.SortOrder.ToString() == "Ascending") sOrder = "ASC";
                        else sOrder = "DESC";
                        sOrdColName = dgv.SortedColumn.Name;
                    }
                    string sFilter = string.Empty;
                    if (bs0.Filter.Length > 0) sFilter = bs0.Filter; 

                    string s0 = dgv.Rows[ir].Cells[0].Value.ToString(); // SEQ
                    //string s1 = dgv.Rows[ir].Cells[14].Value.ToString(); // HINCD
                    

                    GetData(dgv0, bs0, GetOrder());
                    this.Visible = false;
                    arrageTextBW(dgv);

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

                    if (sFilter.Length > 0) bs0.Filter = sFilter;

                    if (ir + 1 < dgv.Rows.Count)
                    {
                        dgv.Rows[ir].Selected = true;
                        dgv.FirstDisplayedScrollingRowIndex = ifr;
                    }
                    this.Visible = true;
                }
            }
        }

        private void arrageTextBW(DataGridView dgv)
        {
            this.Visible = false;
            if (dgv.Rows.Count > 0)
            {
                #region 検索ボックスの位置
                int iwdt = dgv.Columns["出荷No"].Width;
                textBox0.Left = dgv.Left;
                textBox0.Width = iwdt;

                textBox1.Left = textBox0.Left + textBox0.Width + 1;
                iwdt = dgv.Columns["伝票番号"].Width;
                textBox1.Width = iwdt;

                textBox2.Left = textBox1.Left + textBox1.Width + 1;
                iwdt = dgv.Columns["行番号"].Width;
                textBox2.Width = iwdt;

                textBox3.Left = textBox2.Left + textBox2.Width + 1;
                iwdt = dgv.Columns["出荷日"].Width;
                textBox3.Width = iwdt;

                textBox8.Left = textBox3.Left + textBox3.Width + 1;
                iwdt = dgv.Columns["納品日"].Width;
                textBox8.Width = iwdt;

                textBox4.Left = textBox8.Left + textBox8.Width + 1;
                iwdt = dgv.Columns["出荷先"].Width;
                textBox4.Width = iwdt;

                textBox5.Left = textBox4.Left + textBox4.Width + 1;
                iwdt = dgv.Columns["品名"].Width;
                textBox5.Width = iwdt;

                textBox6.Left = textBox5.Left + textBox5.Width + dgv.Columns["数量"].Width + 1;
                iwdt = dgv.Columns["STATUS"].Width;
                textBox6.Width = iwdt;

                textBox7.Left = textBox6.Left + textBox6.Width + dgv.Columns["連"].Width + 1;
                iwdt = dgv.Columns["出荷番号"].Width;
                textBox7.Width = iwdt;

                textBox9.Left = dgv.Left + dgv.Width - dgv.Columns["コLot"].Width;
                iwdt = dgv.Columns["コLot"].Width;
                textBox9.Width = iwdt;
                #endregion
            }
            this.Visible = true;
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
            string fileNm = string.Format("{1}出荷管理表{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHmmss"), argVals[0]);
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

        private string sChkJuchuExist()
        {
            string s = string.Empty;
            s =
            "SELECT"
             + " jc.SEQ"  //0
             + " ,jc.JDNNO"  //1
             + " ,jc.LINNO"  //2
             + " ,jc.SOUCD"
             + " FROM kyoei.sc_juchu jc"  //3
             + " LEFT JOIN kyoei.t_shipment_inf si"  //4
             + " ON jc.JDNNO = si.JDNNO AND jc.LINNO = si.LINNO"  //5
             + " WHERE si.JDNNO IS NULL AND si.LINNO IS NULL AND jc.LGC_DEL = '0'"  //6
             + " ;";
            return s;
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            textboxFilter();
            FillDgvCount(dgv0, label1);
            arrageTextBW(dgv0);
        }

        private void textboxFilter()
        {
            string sFilter = string.Empty;
            if (textBox0.Text.Length > 0)
            {
                sFilter += string.Format(" AND (出荷No = '{0}')", textBox0.Text);
            }
            if (textBox1.Text.Length > 0)
            {
                sFilter += string.Format(" AND (伝票番号 LIKE '%{0}%')", textBox1.Text);
            }
            if (textBox2.Text.Length > 0)
            {
                sFilter += string.Format(" AND (行番号 LIKE '%{0}%')", textBox2.Text);
            }
            if (textBox3.Text.Length > 0)
            {
                sFilter += string.Format(" AND (出荷日 LIKE '%{0}%')", textBox3.Text);
            }
            if (textBox4.Text.Length > 0)
            {
                sFilter += string.Format(" AND (出荷先 LIKE '%{0}%')", textBox4.Text);
            }
            if (textBox5.Text.Length > 0)
            {
                sFilter += string.Format(" AND (品名 LIKE '%{0}%')", textBox5.Text);
            }
            if (textBox6.Text.Length > 0)
            {
                sFilter += string.Format(" AND (STATUS LIKE '%{0}%')", textBox6.Text);
            }
            if (textBox7.Text.Length > 0)
            {
                sFilter += string.Format(" AND (出荷番号 LIKE '%{0}%')", textBox7.Text);
            }
            if (textBox8.Text.Length > 0)
            {
                sFilter += string.Format(" AND (納品日 LIKE '%{0}%')", textBox8.Text);
            }
            if (textBox9.Text.Length > 0)
            {
                sFilter += string.Format(" AND (コLot LIKE '%{0}%')", textBox9.Text);
            }
            if (sFilter.Length > 0)
            {
                sFilter = sFilter.Substring(4);
                bs0.Filter = sFilter;
            }
            else bs0.Filter = string.Empty;
        }

        #region dgvクリック関連
        private void dgv0_MouseDown(object sender, MouseEventArgs e)
        {
            bdgvCellClk = fn.dgvCellClk(sender, e);
        }

        private void dgv0_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            button7.Enabled = false;
            button10.Enabled = false;
            DataGridView dgv = (DataGridView)sender;
            // データ欄以外は何もしない
            if (!bdgvCellClk) return;
            
            string s = dgv.Rows[e.RowIndex].Cells["STATUS"].Value.ToString();
            s = s.Substring(0, 1);
            if (s == "0") button7.Enabled = true;
            if (s == "6") button10.Enabled = true;

            int ir = e.RowIndex;
            int ic = e.ColumnIndex;
            try
            {
                s = dgv.Rows[ir].Cells[ic].Value.ToString();

                //アプリケーション終了後、クリップボードからデータは削除される
                Clipboard.SetDataObject(s);
                //フォーム上の座標でマウスポインタの位置を取得する
                //画面座標でマウスポインタの位置を取得する
                System.Drawing.Point sp = System.Windows.Forms.Cursor.Position;
                //画面座標をクライアント座標に変換する
                System.Drawing.Point cp = this.PointToClient(sp);
                //X座標を取得する
                int x = cp.X;
                //Y座標を取得する
                int y = cp.Y;
                label2.Left = x; label2.Top = y;
                label2.Text = "コピーしました。";
                this.Refresh();
                System.Threading.Thread.Sleep(10);
                label2.Text = ""; label2.Left = 0; label2.Top = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
            
            string s0 = dgv.Rows[e.RowIndex].Cells[0].Value.ToString(); // SEQ
            string s1 = dgv.Rows[e.RowIndex].Cells["HINCD"].Value.ToString(); // HINCD
            string[] sendText = { s0, s1 };

            //// FRMxxxxから送られてきた値を受け取る
            string[] receiveText = F02_EditOrder.ShowMiniForm(this, sendText);

            GetData(dgv0, bs0, GetOrder());
            this.Visible = false;
            arrageTextBW(dgv);
            
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
                dgv0.FirstDisplayedScrollingRowIndex = ir;
            }
            this.Visible = true;
        }
        #endregion

        private void dgv0_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            if (fn.dgvWidth(dgv) < 1300)
            {
                this.Width = fn.dgvWidth(dgv) + 40;
                dgv.Width = fn.dgvWidth(dgv);
            }
            else
            {
                this.Width = 1300;
                dgv.Width = this.Width - 40;
            }
        }

        private string sGetJuchu(string sDNNO)
        {
            return string.Format(
                  "SELECT"
                + "   si.GA_SEQ"  //0
                + " , CASE"
                + "   WHEN jc.SYUYTIDT IS NOT NULL"
                + "   THEN CONCAT(SUBSTRING(jc.NOKDT,1,4),'/',SUBSTRING(jc.NOKDT,5,2)"
                + "    ,'/',SUBSTRING(jc.NOKDT,7,2))"
                + "   ELSE DATE_FORMAT(si.DUE_DATE, '%y/%m/%d') END 納品日"  //1
                + "   , jc.TOKCD 得意先コード"  //2
                + "   , IFNULL(jc.NHSNM,si.DESTINATION) 得意先"  //3
                + "   , jc.TANNM 営業担当"  //4 
                + "   , jc.JDNNO 伝票番号"  //5
                + "   , jc.NHSAD 住所"  //6
                + "   , jc.NHSTL 電話"  //7
                + "   , IFNULL(si.ITEM,jc.HINNMA) 品名"  //8
                + "   , IFNULL(jc.UODSU,si.SHIPMENT_QUANTITY) 数量"  //9
                + "   , jc.UODTK 単価"  //10
                + "   , jc.LINCM 摘要"  //11
                + "   , jc.NHSZP 郵便"  //12
                + "   , jc.UODTK 単価"  //13
                + "   , jc.LINCM 摘要"  //14
                + "   , jc.UNTNM 単位 "  //15
                + "  FROM kyoei.t_shipment_inf si"
                + "  LEFT JOIN kyoei.sc_juchu jc"
                + "   ON si.JDNNO = jc.JDNNO AND si.LINNO = jc.LINNO"
                + "  WHERE si.JDNNO = '{0}' AND si.LGC_DEL = '0' "
                + "  ORDER BY si.LINNO;"
                , sDNNO);
        }

        private string GetSEQ(string DENNO)
        {
            return string.Format(
                    "SELECT"
                 + " si.SEQ"  //0
                 + " FROM kyoei.t_shipment_inf si"
                 + " LEFT JOIN kyoei.sc_juchu sj"
                 + " ON sj.JDNNO = si.JDNNO AND sj.LINNO = si.LINNO"
                 + " AND si.LGC_DEL = '0'"
                 + " WHERE sj.JDNNO = '{0}';"
                 , DENNO
                );
        }

        private void prtVoucher(string sKBN)
        {
            // ここに至る前に、dagagridviewの選択状態を確認する
            #region 作業フォルダ作成
            string path = @"C:\tetra";
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            string fn = @"C:\tetra\kyoei_stamp.png";
            string fn0 = DEF_CON.FLSvrSub + @"template\kyoei_stamp.png";
            if (!System.IO.File.Exists(fn) ||
                File.GetLastWriteTime(fn) < File.GetLastWriteTime(fn0))
            {
                File.Copy(fn0, fn, true);
            }
            fn = @"C:\tetra\kyoei_kakuin.png";
            fn0 = DEF_CON.FLSvrSub + @"template\kyoei_kakuin.png";
            if (!System.IO.File.Exists(fn) ||
                File.GetLastWriteTime(fn) < File.GetLastWriteTime(fn0))
            {
                File.Copy(fn0, fn, true);
            }
            #endregion
            System.Diagnostics.Process p;

            #region 出力先を指定
            string newFile0 = @"C:\tetra\";   //ファイルの出力先を設定
            //if (usr.iDB == 1) newFile0 = @"\\10.100.10.23\tetra\test\";
            //else newFile0 = @"\\10.100.10.23\tetra\delivery_note\";
            #endregion

            mydb.kyDb con = new mydb.kyDb();
            // 得意先コードを納品書の住所にする場合
            string SCTOK = string.Empty;
            string SCZP = string.Empty;
            string SCAD = string.Empty;
            string SCTL = string.Empty;
            if(sKBN == "1")
            {
                try
                {                    
                    #region 得意先コードを得るSQL
                    string sTOKCD = dgv0.CurrentRow.Cells["TOKCD"].Value.ToString();
                    string GetTOK = string.Format(
                        "SELECT"
                         + " CONCAT(tok.TOKNMA,tok.TOKNMB) A"  //0
                         + " ,tok.TOKZP"  //1
                         + " ,CONCAT(CONCAT(tok.TOKADA,tok.TOKADB),tok.TOKADC) B"  //2
                         + " ,tok.TOKTL"  //3
                         + " FROM KEI_USR1.TOKMTA tok"
                         + " WHERE tok.TOKCD = '{0}'"
                        , sTOKCD);
                    #endregion
                    using (var oracon = new OracleConnection())
                    {
                        #region コードに直接指定する場合
                        //ユーザ端末にはクライアントをインストールしないのでtnsnames.oraが存在しない
                        //tnsnamesは直接指定する
                        //var DataSource = "ORCL=" +
                        //"(DESCRIPTION =" +
                        //"(ADDRESS = (PROTOCOL = TCP)(HOST = 10.100.10.11)(PORT = 1521))" +
                        //"(CONNECT_DATA =" +
                        //"(SERVER = DEDICATED)" + 
                        ////"(SERVER = SHARED)" + 
                        //"(SERVICE_NAME = ORCL)" +
                        //")" +
                        //")";
                        //oracon.ConnectionString = "User ID=KEI_ROU; Password=P; Data Source=" + DataSource + ";";
                        #endregion
                        oracon.ConnectionString = "User ID=KEI_ROU;Password=P;Data Source=SDS;";
                            //+ "Connection Timeout=60;";
                        OracleCommand oracom = new OracleCommand(GetTOK, oracon);
                        
                        oracon.Open();
                        OracleDataReader ordr;
                        ordr = oracom.ExecuteReader();

                        while (ordr.Read())
                        {
                            SCTOK = ordr.GetString(0);
                            SCZP = ordr.GetString(1);
                            SCAD = ordr.GetString(2);
                            SCTL = ordr.GetString(3);
                        }
                        ordr.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "エラー");
                    return;
                }
            }
            for (int i = 0; i < dgv0.Rows.Count; i++)
            {
                if (dgv0.Rows[i].Selected
                    && dgv0.Rows[i].Cells[2].Value.ToString() == "001") // 選択行の納品書を作成する
                {
                    #region iTextでドキュメントを新規作成
                    iTextSharp.text.Rectangle new_Pagesize = new iTextSharp.text.Rectangle(PageSize.A4);//(横,縦)
                                                                                                        //ドキュメントを作成
                    iTextSharp.text.Document doc = new iTextSharp.text.Document(new_Pagesize, 16f, 16f, 12f, 12f); //(ページサイズ, 左マージン, 右マージン, 上マージン, 下マージン);
                    #endregion
                    // 行番号が001のものに限定して伝票番号でデータを抽出し配列vに格納する
                    #region gaSeq, 1納品日, 2得意先CD, 3得意先, 営業, 5伝番, 6住所, 電話, 8品名, 数量, 10単価, 摘要, 12〒
                    string sDenNo = dgv0.Rows[i].Cells[1].Value.ToString();
                    // ■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■　納品情報抽出
                    con.GetData(sGetJuchu(sDenNo), DEF_CON.Constr());
                    int iCount = con.ds.Tables[0].Rows.Count; // >5 毎に1枚伝票が増える
                    int iDenCount = 0;
                    if (iCount % 5 == 0) iDenCount = (iCount - 1) / 5;
                    else iDenCount = iCount / 5 + 1; // (icount-1) ÷ 5 = 0.x +1 >> int にすれば1
                    // 必ず5行揃える
                    int plusRow = 5 - (iCount % 5);

                    object[,] v
                     = new object[con.ds.Tables[0].Rows.Count + plusRow, con.ds.Tables[0].Columns.Count];
                    #region 一般的なvの格納
                    //for (int ir = 0; ir < con.ds.Tables[0].Rows.Count; ir++)
                    //{
                    //    for (int ic = 0; ic < con.ds.Tables[0].Columns.Count; ic++)
                    //    {
                    //        v[ir, ic] = con.ds.Tables[0].Rows[ir][ic];
                    //    }
                    //}　3,12,6,7
                    #endregion  
                    for (int ir = 0; ir < v.GetLength(0); ir++)
                    {
                        if (ir < con.ds.Tables[0].Rows.Count)
                        {
                            for (int ic = 0; ic < con.ds.Tables[0].Columns.Count; ic++)
                            {
                                string stmp = con.ds.Tables[0].Rows[ir][ic].ToString();
                                if (ic == 10) // 単価
                                {
                                    if (stmp == "0.00" || stmp.Length == 0) v[ir, ic] = "";
                                    else v[ir, ic] = stmp;
                                    if(usr.author < 9)
                                    {
                                        if (usr.id != "k0134" && usr.id != "k0107") v[ir, ic] = "";
                                    }
                                }
                                else v[ir, ic] = stmp;
                                if (sKBN == "1")
                                {
                                    if (ic == 3) v[ir, ic] = SCTOK;
                                    if (ic == 6) v[ir, ic] = SCAD;
                                    if (ic == 7) v[ir, ic] = SCTL;
                                    if (ic == 12) v[ir, ic] = SCZP;
                                }
                            }
                        }
                        else
                        {
                            for (int ic = 0; ic < con.ds.Tables[0].Columns.Count; ic++)
                            {
                                v[ir, ic] = "";
                            }
                        }
                    }
                    #endregion
                    #region ファイル名を生成
                    // delivery note 納品書 >>  delntXXX####
                    string newFile = newFile0 + "delnt_" + v[0, 3].ToString() + "_" + v[0, 5];
                    newFile += ".pdf";
                    #endregion
                    #region ファイル出力用のストリームを取得
                    try
                    {
                        PdfWriter.GetInstance(doc, new FileStream(newFile, FileMode.Create));
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show(ex.Message, "エラー");
                        MessageBox.Show("受注伝票が開かれています。閉じてから作業して下さい。");
                        return;
                    }
                    #endregion
                    #region 本文用のフォント(MS P明朝) 設定
                    Font fnt8 = new Font(BaseFont.CreateFont
                       (@"c:\windows\fonts\msmincho.ttc,1", BaseFont.IDENTITY_H, true), 8, iTextSharp.text.Font.NORMAL);
                    Font fnt9 = new Font(BaseFont.CreateFont
                       (@"c:\windows\fonts\msmincho.ttc,0", BaseFont.IDENTITY_H, true), 9, iTextSharp.text.Font.NORMAL);
                    Font fnt10 = new Font(BaseFont.CreateFont
                       (@"c:\windows\fonts\msmincho.ttc,1", BaseFont.IDENTITY_H, true), 10, iTextSharp.text.Font.NORMAL);
                    Font fnt16 = new Font(BaseFont.CreateFont
                       (@"c:\windows\fonts\msmincho.ttc,1", BaseFont.IDENTITY_H, true), 16, iTextSharp.text.Font.NORMAL);
                    Font fnt11 = new Font(BaseFont.CreateFont
                       (@"c:\windows\fonts\msmincho.ttc,1", BaseFont.IDENTITY_H, true), 11, iTextSharp.text.Font.NORMAL);
                    Font fnt12 = new Font(BaseFont.CreateFont
                       (@"c:\windows\fonts\msmincho.ttc,1", BaseFont.IDENTITY_H, true), 12, iTextSharp.text.Font.NORMAL);
                    Font fnt4 = new Font(BaseFont.CreateFont
                           (@"c:\windows\fonts\msmincho.ttc,1", BaseFont.IDENTITY_H, true), 6, iTextSharp.text.Font.BOLD);
                    #endregion
                    //文章の出力を開始します。

                    doc.Open();
                    #region 本体作成
                    try
                    {

                        for (int ir = 0; ir < iDenCount; ir++)
                        {
                            for (int ip = 0; ip < 3; ip++) // 3枚連ちゃんなので
                            {
                                #region 先頭行 = 「納品書」　18f
                                float[] headerwidth = new float[] { 0.3f, 0.4f, 0.3f };
                                PdfPCell cell;
                                //3列からなるテーブルを作成
                                PdfPTable tbl = new PdfPTable(headerwidth);
                                //テーブル全体の幅（パーセンテージ）
                                tbl.WidthPercentage = 100;
                                tbl.HorizontalAlignment = Element.ALIGN_CENTER;
                                tbl.DefaultCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                //テーブルの余白
                                tbl.DefaultCell.Padding = 2;
                                #region ヘッダ　= タイトル設定
                                string sTitle = "納 品 書（控）";
                                if (ip == 1) sTitle = "納 品 書";
                                else if (ip == 2) sTitle = "納 品 受 領 書";
                                #endregion
                                // テーブルの中身
                                string[] sHeader = new string[] { "", "", sTitle };
                                //テーブルのセル間の間隔
                                //tbl.Spacing = 0;
                                //テーブルの線の色（RGB:黒）
                                tbl.DefaultCell.BorderColor = BaseColor.BLACK;
                                //タイトルのセルを追加
                                for (int j = 0; j < sHeader.GetLength(0); j++)
                                {
                                    cell = new PdfPCell(new Phrase(sHeader[j], fnt16));
                                    //if (j == 1) cell = new PdfPCell(new Phrase(sHeader[j], fnt12));
                                    //if (j == 2) cell = new PdfPCell(new Phrase(sHeader[j], fnt14));
                                    cell.BorderColor = BaseColor.WHITE;

                                    cell.FixedHeight = 20f; // <--高さ
                                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                    if (j == 2) cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                    //cell.BackgroundColor = BaseColor.LIGHT_GRAY; // セルの拝啓
                                    //cell.Colspan = 2; // セルのマージ
                                    tbl.AddCell(cell);
                                }
                                doc.Add(tbl);
                                #endregion 先頭行 10f

                                #region 最初の表
                                //4列からなるテーブルを作成
                                headerwidth = new float[] { 0.14f, 0.26f, 0.25f, 0.25f };
                                PdfPTable tbl1 = new PdfPTable(headerwidth);
                                //テーブル全体の幅（パーセンテージ）
                                tbl1.WidthPercentage = 60;
                                tbl1.HorizontalAlignment = Element.ALIGN_RIGHT;
                                tbl1.DefaultCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                //テーブルの余白>> 設定済み tbl.DefaultCell.Padding = 2;
                                string sDue = v[0, 1].ToString();
                                string sTokCD = v[0, 2].ToString();
                                string sTanto = v[0, 4].ToString();
                                string sDen = v[0, 5].ToString();
                                sDen += string.Format(" ({0}/{1})", (ir + 1).ToString(), iDenCount.ToString());
                                string[] item = new string[] { "年月日", "お得意様コード", "担当", "伝票番号" };
                                string[] item2 = new string[] { sDue, sTokCD, sTanto, sDen };
                                float fHi = 15f;

                                #region テーブル1行目
                                for (int j = 0; j < item.Length; j++)
                                {
                                    //セルの追加
                                    cell = new PdfPCell(new Phrase(item[j], fnt9));
                                    // 全体の線の太さの設定
                                    cell.BorderWidthTop = 0.8f;
                                    cell.BorderWidthLeft = 0f;
                                    cell.BorderWidthRight = 0.25f;
                                    cell.BorderWidthBottom = 0.25f;
                                    // 最初のセル
                                    if (j == 0) cell.BorderWidthLeft = 0.8f;
                                    // 最後のセル
                                    if (j == item.Length - 1) cell.BorderWidthRight = 0.8f;
                                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                                    cell.FixedHeight = fHi; // <--これ
                                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                    tbl1.AddCell(cell);
                                }
                                //テーブルを追加
                                doc.Add(tbl1);
                                #endregion
                                #region テーブル2行目
                                PdfPTable tbl2 = new PdfPTable(headerwidth);
                                tbl2.WidthPercentage = 60;
                                tbl2.HorizontalAlignment = Element.ALIGN_RIGHT;
                                tbl2.DefaultCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                for (int j = 0; j < item.Length; j++)
                                {
                                    //セルの追加
                                    cell = new PdfPCell(new Phrase(item2[j], fnt9));
                                    // 全体の線の太さの設定
                                    cell.BorderWidthTop = 0f;
                                    cell.BorderWidthLeft = 0f;
                                    cell.BorderWidthRight = 0.25f;
                                    cell.BorderWidthBottom = 0.8f;
                                    // 最初のセル
                                    if (j == 0) cell.BorderWidthLeft = 0.8f;
                                    // 最後のセル
                                    if (j == item.Length - 1) cell.BorderWidthRight = 0.8f;
                                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                                    cell.FixedHeight = fHi; // <--これ
                                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                    tbl2.AddCell(cell);
                                }
                                //テーブルを追加
                                doc.Add(tbl2);
                                #endregion
                                #endregion 終 最初の表

                                #region 宛先等
                                string sPostCD = v[0, 12].ToString();
                                string sAcc = v[0, 3].ToString();
                                sAcc += " 様";
                                sAcc = sAcc.Replace(" ", "");
                                string sTEL = v[0, 7].ToString();
                                string sADD = v[0, 6].ToString();
                                sADD = sADD.Replace(" ", "");
                                headerwidth = new float[] { 0.5f }; // ----------------------------
                                PdfPTable tbl3 = new PdfPTable(headerwidth);
                                item = new string[] { sPostCD, sADD, sAcc, sTEL };
                                tbl3.WidthPercentage = 40;

                                tbl3.DefaultCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                fHi = 15f;
                                if (ip == 2) tbl3.HorizontalAlignment = Element.ALIGN_RIGHT;
                                else tbl3.HorizontalAlignment = Element.ALIGN_LEFT;
                                tbl3.DefaultCell.Padding = 0;

                                for (int irow = 0; irow < item.GetLength(0); irow++)
                                {
                                    if (irow == 2)
                                    {
                                        cell = new PdfPCell(new Phrase(item[irow], fnt12));
                                        cell.FixedHeight = 34f;
                                    }
                                    else
                                    {
                                        cell = new PdfPCell(new Phrase(item[irow], fnt10));
                                        cell.FixedHeight = 14f; // <--これ
                                    }

                                    cell.BorderWidth = 0f;
                                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    tbl3.AddCell(cell);
                                }
                                doc.Add(tbl3);
                                #endregion
                                #region ロゴを追加
                                // c:\apps\kyoei_kakuin.png c:\apps\kyoei_stamp.png

                                if (ip == 2)
                                {
                                    if (sTokCD != "0000014101601") // 髙安の時は不要
                                    {
                                        iTextSharp.text.Image image
                                            = iTextSharp.text.Image.GetInstance(new Uri(@"C:\tetra\kyoei_kakuin.png"));
                                        iTextSharp.text.Image image2
                                            = iTextSharp.text.Image.GetInstance(new Uri(@"C:\tetra\kyoei_stamp.png"));
                                        image.ScalePercent(20.0f / 96.0f * 100f);
                                        image.SetAbsolutePosition(510f, 425f); // 横 縦

                                        image2.ScalePercent(25.0f / 100.0f * 100f);
                                        image2.SetAbsolutePosition(440f, 715f); // 横 縦
                                        doc.Add(image2);
                                        image2.SetAbsolutePosition(394f, 430f); // 横 縦
                                        doc.Add(image2);
                                        image2.SetAbsolutePosition(30f, 155f); // 横 縦
                                        doc.Add(image2);

                                        doc.Add(image);
                                        // 位置を調査
                                        //iTextSharp.text.Image img
                                        //= iTextSharp.text.Image.GetInstance(new Uri(@"c:\apps\line.png"));
                                        //img.SetAbsolutePosition(0f, 280f); // 横 縦
                                        //doc.Add(img);
                                        //img.SetAbsolutePosition(0f, 560f); // 横 縦
                                        //doc.Add(img);
                                        //img.SetAbsolutePosition(0f, 841f); // 横 縦
                                        //doc.Add(img);
                                    }
                                }
                                #endregion
                                #region メインの表
                                #region メイン-ヘッダ行
                                if (ip != 2)
                                {
                                    headerwidth = new float[] { 0.35f, 0.2f, 0.15f, 0.15f, 0.15f };
                                    item = new string[] { "品　名　　・　　規　格", "数　量 ・ 単　位", "単　　価", "金　　額", "消 費 税 等" };
                                }
                                else
                                {
                                    headerwidth = new float[] { 0.35f, 0.2f, 0.45f };
                                    item = new string[] { "品　名　　・　　規　格", "数　量 ・ 単　位", "受　　　領　　　印" };
                                }
                                PdfPTable tbl4 = new PdfPTable(headerwidth);

                                tbl4.WidthPercentage = 100;
                                fHi = 15f;

                                #region メイン-ヘッダ行書き込み
                                for (int j = 0; j < item.Length; j++)
                                {
                                    //セルの追加
                                    cell = new PdfPCell(new Phrase(item[j], fnt9));
                                    // 全体の線の太さの設定
                                    cell.BorderWidthTop = 0.8f;
                                    cell.BorderWidthLeft = 0f;
                                    cell.BorderWidthRight = 0.25f;
                                    cell.BorderWidthBottom = 0.8f;
                                    // 最初のセル
                                    if (j == 0) cell.BorderWidthLeft = 0.8f;
                                    // 最後のセル
                                    if (j == item.Length - 1) cell.BorderWidthRight = 0.8f;
                                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                                    cell.FixedHeight = fHi; // <--これ
                                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                    tbl4.AddCell(cell);
                                }
                                //テーブルを追加
                                doc.Add(tbl4);
                                #endregion
                                #endregion
                                //gaSeq, 1納品日, 2得意先CD, 3得意先, 営業, 5伝番, 6住所, 電話, 8品名, 数量, 10単価, 摘要, 12〒
                                #region 内容行 tbl5
                                if (ip != 2)
                                {
                                    #region 1,2枚目
                                    headerwidth = new float[] { 0.35f, 0.2f, 0.15f, 0.15f, 0.15f };
                                    PdfPTable tbl5 = new PdfPTable(headerwidth);
                                    tbl5.WidthPercentage = 100;
                                    fHi = 18f;
                                    // ir = iDencount
                                    for (int irow = ir * 5; irow < 5 + ir * 5; irow++)
                                    {
                                        string s1 = string.Empty;
                                        if (v[irow, 9].ToString().Length > 0) s1 = string.Format("{0:#,0.00}{1}", decimal.Parse(v[irow, 9].ToString()), v[irow, 15].ToString());
                                        string s2 = string.Empty;
                                        if (v[irow, 10].ToString().Length > 0) s2 = string.Format("{0:#.00}", decimal.Parse(v[irow, 10].ToString()));
                                        string s3 = string.Empty;
                                        if (s1.Length > 0 && s2.Length > 0)
                                        {
                                            s3 = (decimal.Parse(v[irow, 9].ToString()) * decimal.Parse(v[irow, 10].ToString())).ToString();
                                            s3 = string.Format("{0:#,0}", decimal.Parse(s3));
                                        }
                                        item = new string[] { v[irow, 8].ToString()
                                        , s1, s2, s3, "" };
                                        #region テーブル1行目
                                        for (int j = 0; j < item.Length; j++)
                                        {
                                            //セルの追加
                                            if (j == 0)
                                            {
                                                cell = new PdfPCell(new Phrase(item[j], fnt9));
                                                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                                            }
                                            else
                                            {
                                                cell = new PdfPCell(new Phrase(item[j], fnt10));
                                                if (j == 1) cell.HorizontalAlignment = Element.ALIGN_CENTER;
                                                else cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                            }
                                            cell.FixedHeight = fHi; // <--これ
                                            // 全体の線の太さの設定
                                            cell.BorderWidthTop = 0.25f;
                                            cell.BorderWidthLeft = 0f;
                                            cell.BorderWidthRight = 0.25f;
                                            cell.BorderWidthBottom = 0f;
                                            if (irow == 0) cell.BorderWidthTop = 0f;
                                            // 最初のセル
                                            if (j == 0) cell.BorderWidthLeft = 0.8f;
                                            // 最後のセル
                                            if (j == item.Length - 1) cell.BorderWidthRight = 0.8f;
                                            

                                            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                            tbl5.AddCell(cell);
                                        }
                                    }
                                    doc.Add(tbl5);
                                    #endregion
                                    #endregion
                                }
                                else
                                {
                                    #region 3枚目
                                    headerwidth = new float[] { 0.35f, 0.2f, 0.45f };
                                    PdfPTable tbl5 = new PdfPTable(headerwidth);
                                    tbl5.WidthPercentage = 100;
                                    fHi = 20f;
                                    // ir = iDencount
                                    for (int irow = ir * 5; irow < 5 + ir * 5; irow++)
                                    {
                                        string s1 = string.Empty;
                                        if (v[irow, 9].ToString().Length > 0) s1 = string.Format("{0:#.00}{1}", decimal.Parse(v[irow, 9].ToString()), v[irow, 15].ToString());
                                        item = new string[] {
                                            v[irow, 8].ToString()
                                            , s1,  "" };
                                        #region テーブル1行目
                                        for (int j = 0; j < item.Length; j++)
                                        {
                                            //セルの追加
                                            if(j == 1) cell = new PdfPCell(new Phrase(item[j], fnt10));
                                            else cell = new PdfPCell(new Phrase(item[j], fnt9));
                                            // 全体の線の太さの設定
                                            cell.BorderWidthTop = 0.25f;
                                            cell.BorderWidthLeft = 0f;
                                            cell.BorderWidthRight = 0.25f;
                                            cell.BorderWidthBottom = 0f;
                                            if (irow == 0) cell.BorderWidthTop = 0f;
                                            // 最初のセル
                                            if (j == 0) cell.BorderWidthLeft = 0.8f;
                                            // 最後のセル
                                            if (j == item.Length - 1)
                                            {
                                                cell.BorderWidthTop = 0f;
                                                cell.BorderWidthBottom = 0f;
                                                cell.BorderWidthRight = 0.8f;
                                            }
                                            if (irow == 5 + ir * 5 - 1) cell.BorderWidthBottom = 0.8f;
                                            cell.HorizontalAlignment = Element.ALIGN_CENTER;
                                            cell.FixedHeight = fHi; // <--これ
                                            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                            tbl5.AddCell(cell);
                                        }
                                    }
                                    doc.Add(tbl5);
                                    #endregion
                                }
                                #endregion
                                #endregion

                                #endregion

                                #region 最終行
                                if (ip != 2)
                                {
                                    headerwidth = new float[] { 0.35f, 0.05f, 0.02f, 0.18f, 0.02f, 0.18f, 0.02f, 0.18f };
                                    PdfPTable tbl6 = new PdfPTable(headerwidth);
                                    tbl6.WidthPercentage = 100;
                                    tbl6.DefaultCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                    fHi = 26f;
                                    decimal dTotal = 0;
                                    for (int irow = ir * 5; irow < 5 + ir * 5; irow++)
                                    {
                                        decimal dQ = 0;
                                        decimal dP = 0;
                                        if (v[irow, 10].ToString().Length > 0)
                                        {
                                            dP = decimal.Parse(v[irow, 13].ToString());
                                            dQ = decimal.Parse(v[irow, 9].ToString());
                                            dTotal += decimal.Parse((dP * dQ).ToString());  // 切捨て
                                        }
                                    }
                                    string s4 = string.Empty;
                                    if (v[0, 11].ToString().Length > 0) s4 = "摘要:" + v[0, 11].ToString();
                                    string s5 = "税\r\n抜";
                                    string s6 = "税\r\n額";
                                    string s7 = "総\r\n額";
                                    string sTotal = string.Empty;
                                    if (dTotal > 0) sTotal = string.Format("{0:#,0}", decimal.Parse(dTotal.ToString()));
                                    item = new string[] { s4, "合計", s5, sTotal, s6, "", s7, sTotal };
                                    for (int j = 0; j < item.Length; j++)
                                    {
                                        //セルの追加
                                        if (j == 3 || j == 7)
                                        {
                                            cell = new PdfPCell(new Phrase(item[j], fnt10));
                                        }
                                        else
                                        {
                                            cell = new PdfPCell(new Phrase(item[j], fnt8));
                                        }

                                        cell.FixedHeight = fHi; // <--これ
                                        // 全体の線の太さの設定
                                        cell.BorderWidthTop = 0.25f;
                                        cell.BorderWidthLeft = 0f;
                                        cell.BorderWidthRight = 0.25f;
                                        cell.BorderWidthBottom = 0.8f;
                                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                                        // 最初のセル
                                        if (j == 0)
                                        {
                                            cell.BorderWidthLeft = 0.8f;
                                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                        }
                                        // 最後のセル
                                        if (j == item.Length - 1) cell.BorderWidthRight = 0.8f;


                                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                        tbl6.AddCell(cell);
                                    }
                                    //テーブルを追加
                                    doc.Add(tbl6);
                                    Paragraph para = new Paragraph(" ", fnt9);
                                    doc.Add(para);
                                    doc.Add(para);
                                }
                                #endregion
                            }
                            if (ir != iDenCount - 1)
                            {
                                doc.NewPage();
                            }
                        } // iDenCount++
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("---\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                        System.Windows.Forms.MessageBox.Show(ex.Message, "エラー");
                    }
                    doc.Close();
                    p = System.Diagnostics.Process.Start(newFile);
                } // 選択行&& [001]の処理　この手前で続きがあればnewpage


            }
            #endregion

            //p = System.Diagnostics.Process.Start(newFile);

            //p.WaitForExit();              // プロセスの終了を待つ
            //int iExitCode = p.ExitCode;   // 終了コード
            //lb_refreshData();
        }

        private string LotList(string shipSEQ)
        {
            string s = string.Format(
                "SELECT"
             + " si.SEQ"  //0
             + " ,IFNULL(si.ITEM,jc.HINNMA)"  //1
             + " ,IFNULL(jc.UODSU,si.SHIPMENT_QUANTITY)"  //2
             + " ,tmp.WEIGHT"  //3
             + " ,tmp.LOT_NO"  //4
             + " ,SUBSTRING_INDEX(tmp.LOT_NO,'-',1) dDate"  //5
             + " ,SUBSTRING_INDEX(tmp.LOT_NO,'-',-2) dLot"  //6
             + " ,cb.LOT" //7 コンテナ番号
             + " FROM"
             + "  kyoei.t_shipment_inf si"
             + "  LEFT JOIN kyoei.sc_juchu jc"
             + "  ON si.JC_SEQ = jc.SEQ"
             + "  LEFT JOIN kyoei.t_can_barcode cb"
             + "  ON cb.SHIP_SEQ = si.SEQ"
             + "  LEFT JOIN "
             + "  ("
             + "  SELECT"
             + "  p.SHIP_SEQ"
             + "  ,p.WEIGHT"
             + "  ,p.LOT_NO "
             + "  FROM kyoei.t_product p"
             + "  WHERE p.SHIP_SEQ IN ({0})"
             + "  UNION"
             + "  SELECT"
             + "  mp.SHIP_SEQ"
             + "  ,mp.WEIGHT"
             + "  ,mp.LOT_NO"
             + "  FROM kyoei.t_m_product mp"
             + "  WHERE mp.SHIP_SEQ IN ({0})"
             + "  UNION"
             + "  SELECT"
             + "  tp.SHIP_SEQ"
             + "  ,tp.WEIGHT"
             + "  ,tp.LOT_NO"
             + "  FROM kyoei.t_t_product tp"
             + "  WHERE tp.SHIP_SEQ IN ({0})"
             + "  ) tmp"
             + "  ON tmp.SHIP_SEQ = si.SEQ"
             + " WHERE si.SEQ IN ({0})"
             + " GROUP BY si.SEQ,tmp.LOT_NO"
             + " ORDER BY"
             + "   jc.LINNO"
             + "   ,IFNULL(jc.HINNMA,si.ITEM)"
             + "  ,SUBSTRING_INDEX(tmp.LOT_NO,'-',1)"
             + "  ,CAST(SUBSTRING_INDEX(tmp.LOT_NO,'-',-1) AS SIGNED);"
                , shipSEQ);
            return s;
        }

        private string GetJuchuInf(string shipSEQ)
        {
            string s = string.Format(
                "SELECT"
                 + " si.SEQ"  //0
                 + " ,CONCAT(" 
                 + " SUBSTRING(jc.NOKDT,1,4)"  
                 + " ,'/',SUBSTRING(jc.NOKDT,5,2)"  
                 + " ,'/',SUBSTRING(jc.NOKDT,7,2)"  
                 + " )"                          // 1
                 + " ,jc.TOKCD"                  // 2
                 + " ,jc.TANNM "                 // 3
                 + " ,RIGHT(jc.TANCD,2)"         // 4
                 + " ,CONCAT(jc.JDNNO,' (1/1)')" // 5
                 + " , jc.NHSZP"                 // 6
                 + " , jc.NHSAD"                 // 7
                 + " , jc.NHSNM"                 // 8
                 + " , jc.NHSTL"                 // 9
                 + " , jc.SOUCD"                 //10
                 + " , bc.LOT"                   //11
                 + " FROM kyoei.t_shipment_inf si"
                 + " LEFT JOIN kyoei.sc_juchu jc"
                 + " ON si.JDNNO = jc.JDNNO AND si.LINNO = jc.LINNO"
                 + " LEFT JOIN t_can_barcode bc "
                 + " ON bc.SHIP_SEQ = si.SEQ"
                 + " WHERE si.SEQ = {0};"
                 , shipSEQ
                );
            return s;
        }

        private void prtLotList()
        {
            string sMsg = string.Empty;
            int ir = -1;
            #region 印刷まえ確認
            if (dgv0.SelectedRows.Count <= 0)
            {
                sMsg = "出荷情報を選択して下さい。";
            }
            
            if (sMsg.Length > 0)
            {
                string[] sSnd = { sMsg, "false" };
                string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSnd);
                return;
            }
            #endregion  
            
            #region 作業フォルダの作成
            string path = @"c:\tetra";
            fn.CrtDir(path);
            string flnm = @"C:\tetra\kyoei_stamp.png";
            string fn0 = DEF_CON.FLSvrSub + @"template\kyoei_stamp.png";
            if (!System.IO.File.Exists(flnm) ||
                File.GetLastWriteTime(flnm) < File.GetLastWriteTime(fn0))
            {
                File.Copy(fn0, flnm, true);
            }
            #endregion
            #region 印刷の確認
            sMsg = "ロット明細書を印刷しますか？";
            string[] sendText = { sMsg };
            string[] reT = promag_frm.F05_YN.ShowMiniForm(sendText);

            if (reT[0].Length == 0) return;
            #endregion


            for (int i = 0; i < dgv0.Rows.Count; i++)
            {
                if (dgv0.Rows[i].Selected)
                {
                    ir = i;
                    #region 変数の定義
                    string sNouhinbi = string.Empty; // 納品日
                    string sTokCD = string.Empty; // お得意様コード
                    string sTanTo = string.Empty; // 担当
                    string sDenNo = string.Empty; // 伝票番号
                    string sZIPCD = string.Empty;
                    string sNOHAD = string.Empty;
                    string sNOUNM = string.Empty;
                    string sNOUTL = string.Empty;
                    string sConLot = string.Empty; // コンテナロット

                    string ShipSeq = dgv0.Rows[i].Cells[0].Value.ToString(); // SHIP_SEQ
                    string sQty = dgv0.Rows[i].Cells["数量"].Value.ToString(); // 数量
                    #endregion

                    kyDb con = new kyDb();
                    #region 伝票情報を取得
                    string tmpDb = con.GetData(GetJuchuInf(ShipSeq), DEF_CON.Constr());
                    sNouhinbi = con.ds.Tables[0].Rows[0][1].ToString();
                    sTokCD = con.ds.Tables[0].Rows[0][2].ToString();
                    string sSOUCD = sNOUTL = con.ds.Tables[0].Rows[0][10].ToString(); // 20201216追記
                    sTanTo = con.ds.Tables[0].Rows[0][3].ToString();
                    sTanTo += "                    ";
                    sTanTo = sTanTo.Substring(0, 18);
                    sTanTo += sSOUCD.Substring(sSOUCD.Length - 2); // 20201216追記
                    //sTanTo += con.ds.Tables[0].Rows[0][4].ToString();
                    sDenNo = con.ds.Tables[0].Rows[0][5].ToString();
                    sZIPCD = con.ds.Tables[0].Rows[0][6].ToString();
                    sNOHAD = con.ds.Tables[0].Rows[0][7].ToString();
                    sNOUNM = con.ds.Tables[0].Rows[0][8].ToString();
                    sNOUTL = con.ds.Tables[0].Rows[0][9].ToString();
                    sConLot = con.ds.Tables[0].Rows[0][11].ToString();
                    #endregion

                    // 同一受注番号のものがあるか探す。
                    tmpDb = con.GetData(GetSEQ(sDenNo.Substring(0,sDenNo.Length-6)), DEF_CON.Constr());
                    string sShipSeqs = string.Empty;
                    for(int r = 0;r < con.ds.Tables[0].Rows.Count; r++)
                    {
                        sShipSeqs += "," + con.ds.Tables[0].Rows[r][0].ToString();
                    }
                    sShipSeqs = sShipSeqs.Substring(1);

                    tmpDb = con.GetData(LotList(sShipSeqs), DEF_CON.Constr());
                    object[,] v = new object[con.ds.Tables[0].Rows.Count, con.ds.Tables[0].Columns.Count];
                    for(int row = 0;row < v.GetLength(0); row++)
                    {
                        for (int col = 0; col < v.GetLength(1); col++)
                        {
                            //if(con.ds.Tables[0].Rows[row][col].ToString())
                            v[row, col] = con.ds.Tables[0].Rows[row][col].ToString();
                        }
                    }

                    System.Diagnostics.Process p;
                    #region ドキュメントを作成 d.ToString("yyyyMMddHHmmss"))
                    string sCrtDate = DateTime.Now.ToString("yyMMddHHmmss");
                    iTextSharp.text.Document doc =
                        new iTextSharp.text.Document(PageSize.A4, 12f, 14f, 14f, 12f);
                    //(ページサイズ, 左, 右, 上, 下マージン);
                    //ファイルの出力先を設定
                    string newFile = @"C:\tetra\dlv_nt_LotTbl";
                    newFile += ShipSeq + sCrtDate + ".pdf";
                    #endregion
                    #region ファイル出力用のストリームを取得
                    try
                    {
                        PdfWriter.GetInstance(doc, new FileStream(newFile, FileMode.Create));
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show(ex.Message, "エラー");
                        MessageBox.Show("誰かがロット明細用ファイルを使用中です。閉じてから作業して下さい。");
                        return;
                    }
                    #endregion
                    #region 本文用のフォント(MS P明朝) 設定
                    iTextSharp.text.Font fnt16 = new iTextSharp.text.Font(BaseFont.CreateFont
                       (@"c:\windows\fonts\msmincho.ttc,1", BaseFont.IDENTITY_H, true), 16, iTextSharp.text.Font.NORMAL);
                    iTextSharp.text.Font fnt12 = new iTextSharp.text.Font(BaseFont.CreateFont
                       (@"c:\windows\fonts\msmincho.ttc,1", BaseFont.IDENTITY_H, true), 12, iTextSharp.text.Font.NORMAL);
                    iTextSharp.text.Font fnt10 = new iTextSharp.text.Font(BaseFont.CreateFont
                       (@"c:\windows\fonts\msmincho.ttc,1", BaseFont.IDENTITY_H, true), 10, iTextSharp.text.Font.NORMAL);
                    iTextSharp.text.Font fnt11 = new iTextSharp.text.Font(BaseFont.CreateFont
                       (@"c:\windows\fonts\msmincho.ttc,1", BaseFont.IDENTITY_H, true), 11, iTextSharp.text.Font.NORMAL);
                    iTextSharp.text.Font fnt9 = new iTextSharp.text.Font(BaseFont.CreateFont
                       (@"c:\windows\fonts\msmincho.ttc,1", BaseFont.IDENTITY_H, true), 9, iTextSharp.text.Font.NORMAL);
                    #endregion
                    doc.Open();
                    try
                    {
                        #region 先頭行 = 「納品書」　18f
                        float[] headerwidth = new float[] { 0.3f, 0.4f, 0.3f };
                        PdfPCell cell;
                        //3列からなるテーブルを作成
                        PdfPTable tbl = new PdfPTable(headerwidth);
                        //テーブル全体の幅（パーセンテージ）
                        tbl.WidthPercentage = 100;
                        tbl.HorizontalAlignment = Element.ALIGN_CENTER;
                        tbl.DefaultCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        //テーブルの余白
                        tbl.DefaultCell.Padding = 2;
                        #region ヘッダ　= タイトル設定
                        string sTitle = "ロ ッ ト 明 細";
                        
                        #endregion
                        // テーブルの中身
                        string[] sHeader = new string[] { "", "", sTitle };
                        //テーブルのセル間の間隔
                        //tbl.Spacing = 0;
                        //テーブルの線の色（RGB:黒）
                        tbl.DefaultCell.BorderColor = BaseColor.BLACK;
                        //タイトルのセルを追加
                        for (int j = 0; j < sHeader.GetLength(0); j++)
                        {
                            cell = new PdfPCell(new Phrase(sHeader[j], fnt16));
                            cell.BorderColor = BaseColor.WHITE;
                            cell.FixedHeight = 20f;                         // <--高さ
                            cell.HorizontalAlignment = Element.ALIGN_CENTER;
                            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                            if (j == 2) cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                            tbl.AddCell(cell);
                        }
                        doc.Add(tbl);
                        #endregion 先頭行 10f

                        #region 最初の表
                        //4列からなるテーブルを作成
                        headerwidth = new float[] { 0.14f, 0.26f, 0.25f, 0.25f };
                        PdfPTable tbl1 = new PdfPTable(headerwidth);
                        //テーブル全体の幅（パーセンテージ）
                        tbl1.WidthPercentage = 70;
                        tbl1.HorizontalAlignment = Element.ALIGN_RIGHT;
                        tbl1.DefaultCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        //テーブルの余白>> 設定済み tbl.DefaultCell.Padding = 2;
                        //string sDen = v[0, 5].ToString();
                        //sDen += string.Format(" ({0}/{1})", (ir + 1).ToString(), iDenCount.ToString());
                        string[] item = new string[] { "年月日", "お得意様コード", "担当", "伝票番号" };
                        string[] item2 = new string[] { sNouhinbi, sTokCD, sTanTo, sDenNo };
                        float fHi = 16f;

                        #region テーブル1行目
                        for (int j = 0; j < item.Length; j++)
                        {
                            //セルの追加
                            cell = new PdfPCell(new Phrase(item[j], fnt9));
                            // 全体の線の太さの設定
                            cell.BorderWidthTop = 0.8f;
                            cell.BorderWidthLeft = 0f;
                            cell.BorderWidthRight = 0.25f;
                            cell.BorderWidthBottom = 0.25f;
                            // 最初のセル
                            if (j == 0) cell.BorderWidthLeft = 0.8f;
                            // 最後のセル
                            if (j == item.Length - 1) cell.BorderWidthRight = 0.8f;
                            cell.HorizontalAlignment = Element.ALIGN_CENTER;
                            cell.FixedHeight = fHi; // <--これ
                            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                            tbl1.AddCell(cell);
                        }
                        //テーブルを追加
                        doc.Add(tbl1);
                        #endregion
                        #region テーブル2行目
                        PdfPTable tbl2 = new PdfPTable(headerwidth);
                        tbl2.WidthPercentage = 70;
                        tbl2.HorizontalAlignment = Element.ALIGN_RIGHT;
                        tbl2.DefaultCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        for (int j = 0; j < item.Length; j++)
                        {
                            //セルの追加
                            cell = new PdfPCell(new Phrase(item2[j], fnt9));
                            // 全体の線の太さの設定
                            cell.BorderWidthTop = 0f;
                            cell.BorderWidthLeft = 0f;
                            cell.BorderWidthRight = 0.25f;
                            cell.BorderWidthBottom = 0.8f;
                            // 最初のセル
                            if (j == 0) cell.BorderWidthLeft = 0.8f;
                            // 最後のセル
                            if (j == item.Length - 1) cell.BorderWidthRight = 0.8f;
                            cell.HorizontalAlignment = Element.ALIGN_CENTER;
                            cell.FixedHeight = fHi; // <--これ
                            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                            tbl2.AddCell(cell);
                        }
                        //テーブルを追加
                        doc.Add(tbl2);
                        #endregion

                        #endregion 終 最初の表

                        #region 宛先等
                        headerwidth = new float[] { 0.4f };
                        PdfPTable tbl3 = new PdfPTable(headerwidth);
                        item = new string[] { sZIPCD, sNOHAD, sNOUNM + " 様", sNOUTL };
                        tbl3.WidthPercentage = 35;

                        tbl3.DefaultCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        fHi = 15f;
                        tbl3.HorizontalAlignment = Element.ALIGN_LEFT;
                        tbl3.DefaultCell.Padding = 0;

                        for (int irow = 0; irow < item.GetLength(0); irow++)
                        {
                            if (irow == 2)
                            {
                                cell = new PdfPCell(new Phrase(item[irow], fnt12));
                                cell.FixedHeight = 34f;
                            }
                            else
                            {
                                cell = new PdfPCell(new Phrase(item[irow], fnt10));
                                cell.FixedHeight = 14f; // <--これ
                            }

                            cell.BorderWidth = 0f;
                            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            tbl3.AddCell(cell);
                        }
                        doc.Add(tbl3);
                        #endregion

                        #region ロゴを追加
                        if (sTokCD != "0000014101601") // 髙安の時は表示しない
                        {
                            iTextSharp.text.Image image2
                                = iTextSharp.text.Image.GetInstance(new Uri(@"C:\tetra\kyoei_stamp.png"));
                            image2.ScalePercent(25.0f / 100.0f * 100f);
                            image2.SetAbsolutePosition(440f, 715f); // 横 縦
                            doc.Add(image2);
                        }
                        #endregion

                        #region メイン-ヘッダ行
                        headerwidth = new float[] { 0.5f, 0.125f, 0.084f, 0.125f, 0.166f };
                        item = new string[] { "品　名　　・　　規　格　　・　　個数", "数　量", "日 付", "ロ ッ ト №", "充 填 ロ ッ ト №" };
                        bool bCon = false;
                        if (v[0, 7].ToString().Length > 0) bCon = true;
                        if (bCon) item = new string[] { "品　名　　・　　規　格　　・　　個数", "数　量", "日 付", "管 理 番 号", "充 填 ロ ッ ト №" };
                        PdfPTable tbl4 = new PdfPTable(headerwidth);

                        tbl4.WidthPercentage = 100;
                        fHi = 15f;

                        #region メイン-ヘッダ行書き込み
                        for (int j = 0; j < item.Length; j++)
                        {
                            //セルの追加
                            cell = new PdfPCell(new Phrase(item[j], fnt9));
                            // 全体の線の太さの設定
                            cell.BorderWidthTop = 0.8f;
                            cell.BorderWidthLeft = 0f;
                            cell.BorderWidthRight = 0.25f;
                            cell.BorderWidthBottom = 0.8f;
                            // 最初のセル
                            if (j == 0) cell.BorderWidthLeft = 0.8f;
                            // 最後のセル
                            if (j == item.Length - 1) cell.BorderWidthRight = 0.8f;
                            cell.HorizontalAlignment = Element.ALIGN_CENTER;
                            cell.FixedHeight = fHi; // <--これ
                            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                            tbl4.AddCell(cell);
                        }
                        //テーブルを追加
                        doc.Add(tbl4);
                        #endregion
                        #endregion

                        #region 表中身
                        /*
                        0 shipSEQ
                        1 品名
                        2 出荷数量
                        3 Lot数量
                        4 Lot_No
                        5 生産日
                        6 Lot枝番
                        7 コンテナ番号
                         */
                        //headerwidth = new float[] { 0.35f, 0.2f, 0.15f, 0.15f, 0.15f };
                        PdfPTable tbl5 = new PdfPTable(headerwidth);
                        tbl5.WidthPercentage = 100;
                        fHi = 20.5f;
                        // ir = iDencount
                        
                        for (int irow = 0; irow < 32; irow++ )
                        {
                            string s1 = string.Empty; // 全体出荷数量
                            string s2 = string.Empty; // Lot重量
                            string s3 = string.Empty; // 品名等   --------------------->> 2021/1/21 元々forの上にあった。
                            string sHIDUKE = string.Empty;
                            string sBRANCH = string.Empty;
                            string sCONTENA = string.Empty;
                            if (irow < v.GetLength(0))
                            {
                                s1 = v[irow, 2].ToString(); // 全体出荷数量
                                if (s1.IndexOf(".") > 0) s1 = s1.Substring(0, s1.IndexOf(".") );
                                if (s1.Length > 0) s1 = string.Format("{0:#,0.00}", int.Parse(s1));
                                s2 = v[irow, 3].ToString(); // Lot重量
                                if (s2.IndexOf(".") > 0) s2 = s2.Substring(0, s2.IndexOf("."));
                                if (s2.Length > 0) s2 = string.Format("{0:#,0.00}", int.Parse(s2));
                                
                                s3 = v[irow, 1].ToString();
                                s3 += "                                                            ";
                                s3 = s3.Substring(0, 60 - s1.Length);
                                s3 += s1;
                                if (irow > 0)
                                {
                                    if (v[irow, 1].ToString() == v[irow - 1, 1].ToString()
                                        && v[irow, 2].ToString() == v[irow - 1, 2].ToString()) s3 = "";
                                    if (v[irow, 1].ToString().Length == 0) s3 = "";
                                }
                                
                                sHIDUKE = v[irow, 5].ToString();
                                sBRANCH = v[irow, 6].ToString();
                                if (bCon) sBRANCH = "";
                                sCONTENA = v[irow, 7].ToString();
                                if (bCon && irow > 0) sCONTENA = "";
                            }

                            item = new string[] { s3, s2, sHIDUKE, sBRANCH, sCONTENA };
                            
                            #region テーブル1行目
                            for (int j = 0; j < item.Length; j++)
                            {
                                //セルの追加
                                if (j == 1)
                                {
                                    cell = new PdfPCell(new Phrase(item[j], fnt9));
                                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                }
                                else
                                {
                                    cell = new PdfPCell(new Phrase(item[j], fnt9));
                                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                                }
                                cell.FixedHeight = fHi; // <--これ
                                                        // 全体の線の太さの設定
                                cell.BorderWidthTop = 0f;
                                cell.BorderWidthLeft = 0f;
                                cell.BorderWidthRight = 0.25f;
                                cell.BorderWidthBottom = 0.8f;
                                if (irow == 0) cell.BorderWidthTop = 0f;
                                // 最初のセル
                                if (j == 0) cell.BorderWidthLeft = 0.8f;
                                // 最後のセル
                                if (j == item.Length - 1) cell.BorderWidthRight = 0.8f;


                                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                tbl5.AddCell(cell);
                            }
                        }
                        doc.Add(tbl5);
                        #endregion
                        #endregion

                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("---\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                        System.Windows.Forms.MessageBox.Show(ex.Message, "エラー");
                    }
                    doc.Close();
                    p = System.Diagnostics.Process.Start(newFile);
                }
            }

            if (ir < dgv0.Rows.Count)
            {
                dgv0.Rows[ir].Selected = true;
                dgv0.FirstDisplayedScrollingRowIndex = ir;
            }
        }

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckedChanged();
            RefleshDgv();
        }

        private void CheckedChanged()
        {
            string s = string.Empty;
            textboxFilter();
            if(bs0.Filter != null)s = bs0.Filter;
            if (s.Length > 0 && !checkBox1.Checked ) s += " AND ";
            if (!checkBox1.Checked) s += "LOC_SEQ IN ('0000000000003','0000000000011')";

            bs0.Filter = s;
            FillDgvCount(dgv0, label1);
        }

        private void RefleshDgv()
        {
            if (dgv0.Rows.Count <= 0) return;
            int irow = 0;
            if (dgv0.CurrentCell.RowIndex >= 0) irow = dgv0.CurrentCell.RowIndex;
            // ダブルクリック前に列の並び替えが行われていれば、その状態を記憶
            string sOrder = string.Empty;
            string sOrdColName = string.Empty;
            string sFilter = string.Empty;
            if (bs0.Filter.Length > 0) sFilter = bs0.Filter;
            
            if (dgv0.SortedColumn != null)
            {
                if (dgv0.SortOrder.ToString() == "Ascending") sOrder = "ASC";
                else sOrder = "DESC";
                sOrdColName = dgv0.SortedColumn.Name;
            }

            string s0 = dgv0.Rows[irow].Cells[0].Value.ToString(); // SEQ
            string s1 = dgv0.Rows[irow].Cells["HINCD"].Value.ToString(); // HINCD
            


            this.Visible = false;
            arrageTextBW(dgv0);

            bs0.Filter = sFilter;
            // 並び順をダブルクリック前に戻し値を検索
            if (sOrder.Length > 0)
            {
                bs0.Sort = string.Format("{0} {1}", sOrdColName, sOrder);

                for (int r = 0; r < dgv0.Rows.Count; r++)
                {
                    if (dgv0.Rows[r].Cells[0].Value.ToString() == s0)
                    {
                        irow = r;
                        break;
                    }
                }
            }

            if (irow + 1 < dgv0.Rows.Count)
            {
                dgv0.Rows[irow].Selected = true;
                dgv0.FirstDisplayedScrollingRowIndex = irow;
            }
            this.Visible = true;
        }
    }
}
