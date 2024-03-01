using System;
using System.Windows.Forms;

namespace k001_shukka
{
    public partial class F04_ChkShipment : Form
    {
        #region フォーム変数
        private Boolean bClose = true;
        private string[] argVals; // 親フォームから受け取る引数
        public string[] ReturnValue;            // 親フォームに返す戻り値
        private Boolean bPHide = true;  // 親フォームを隠す = True
        private Boolean bdgvCellClk = false; // dgvでクリックする場合には必須
        DateTime loadTime; // formloadの時間
        private bool bDirty = false; // 編集が行われたらtrue
        #endregion 
        public F04_ChkShipment(params string[] argVals)
        {
            // 親フォームから受け取ったデータをこのインスタンスメンバに格納
            this.argVals = argVals;
            InitializeComponent();

            // ■■■ 親フォームを隠す設定 true => 隠す　default = 隠す。
            bPHide = false; // 隠さない
            //■■■ 画面の和名
            string sTitle = "出荷Lot確認";
            #region 画面の状態を設定
            // 画面サイズ変更の禁止
            this.MaximizeBox = false;
            lblDT.Text = usr.name;
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
            // params s >> 0= appID, 1 = usr.id, 2 = iDB
            F04_ChkShipment f = new F04_ChkShipment(s);
            f.ShowDialog(frm);

            string[] receiveText = f.ReturnValue; // -- ①
            f.Dispose();
            return receiveText;
        }

        private void F04_ChkShipment_Load(object sender, EventArgs e)
        {
            // 開いた元フォームを隠す設定
            if (bPHide) this.Owner.Hide();

            loadTime = DateTime.Now;
            fn.showCenter(this);
            // タイトルラベルを中央に置く
            lblTtitle.Text = "出荷Lot確認";
            lblTtitle.Left = (this.Width - lblTtitle.Width) / 2;

            GetData(dgv0, bs0, sGetList());
            dgv1IniSet();
        }

        #region CLOSE処理
        // 3) btnClose
        private void FRM_Closing(object sender, FormClosingEventArgs e)
        {
            if (bClose)
            {

            }
            //CreateRetCsv(); 20210202 自動で出荷を行うと輸出計上が出来ないので止める
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
            if (bDirty)
            {
                // 連携CSV作成
            }
            closing();
        }
        // CLOSE処理の最後
        private void FRM_Closed(object sender, FormClosedEventArgs e)
        {
            if (bPHide) this.Owner.Show();
        }
        #endregion

        private void CreateRetCsv()
        {
            // 重量が一致するか確認する
            // CSVを作成する
            /* filename = RJ_DENNO_LINO.csv
               JDNNO,LINNO,TOKCD,NHSCD,HINCD,SYUYTIDT,NOKDT,UODSU,LOTID,HINLCDDT,SOUCD
               00006156,002,0000011100901,0000000000004,I2330203ZZ-282CP,20201107,20201107,1000,V3-01,20201007,0000000000003
               受注伝票番号,行番号,得意先コード,納品先コード,商品コード,出荷日,納品日,数量,ロットID,管理日付,倉庫コード */
            string sMsg = string.Empty;
            #region 重量の一致と出荷確認済みかチェックする
            if (int.Parse(argVals[1]) != (ChkWeight(argVals[0]))) return;
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
                , argVals[0]);
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
                , argVals[0]);
            #endregion
            mydb.kyDb con = new mydb.kyDb();
            int iCount = con.iGetCount(s, DEF_CON.Constr());
            int iCount1 = con.iGetCount(s1, DEF_CON.Constr());
            if (iCount != iCount1) return;
            #endregion

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
                , argVals[0], argVals[2], argVals[3]);
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
            fn += argVals[2];
            fn += "_" + argVals[3];
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
                , argVals[0]);
            sTmp = con.ExecSql(false, DEF_CON.Constr(), sUPD_SI);
            sMsg = "出荷連携CSVを作成しました。";
            if (sMsg.Length > 0)
            {
                string[] sSend = { sMsg, "false" };
                string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSend);
            }
        }

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
            return con.iGetCount(s, DEF_CON.Constr());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dgv1.Rows.Count <= 0)
            {
                string[] sSendT = { "Lotを入力してから登録します。", "false" };
                string[] sRcvT = promag_frm.F05_YN.ShowMiniForm(sSendT);
                return;
            }
            string slots = string.Empty;
            for (int i = 0; i < dgv1.Rows.Count; i++)
            {
                string s = string.Empty;
                if (dgv1.Rows[i].Cells[0].Value != null) s = dgv1.Rows[i].Cells[0].Value.ToString();

                if (s.Length > 0) slots += string.Format(",'{0}'", s);
            }
            // 入力or scanされたLotを確認済みとして登録
            // 登録後　同一SHIP_SEQのCHK_DATEが全てnot NULLであれば出荷テーブルの更新者と更新日を記録
            if (slots.Length > 0)
            {
                slots = slots.Substring(1);
                string sUPD = string.Empty;
                sUPD = string.Format(
                    "UPDATE t_product SET SHIP_CHK_DATE = NOW(), SHIP_CHK_PSN = '{2}' "
                    + "WHERE SHIP_SEQ = {0} AND "
                    + "LOT_NO IN ({1});"
                    , argVals[0], slots, usr.id
                    );
                sUPD += string.Format(
                    "UPDATE t_t_product SET SHIP_CHK_DATE = NOW(), SHIP_CHK_PSN = '{2}' "
                    + "WHERE SHIP_SEQ = {0} AND "
                    + "LOT_NO IN ({1});"
                    , argVals[0], slots, usr.id
                    );
                mydb.kyDb con = new mydb.kyDb();
                if (con.ExecSql(false,DEF_CON.Constr(), sUPD).Length == 0)
                {
                    string s = string.Format(
                        "SELECT COUNT(*) FROM "
                        + " ("
                        + "SELECT SHIP_SEQ, SHIP_CHK_DATE FROM t_product"
                        + " UNION ALL "
                        + "SELECT SHIP_SEQ, SHIP_CHK_DATE FROM t_t_product"
                        + ") p"
                        + " WHERE p.SHIP_SEQ = {0} AND p.SHIP_CHK_DATE IS NULL;"
                        , argVals[0]);
                    if (con.iGetCount(s,DEF_CON.Constr()) == 0)
                    {
                        sUPD = string.Format(
                            "UPDATE t_shipment_inf UPD_ID = '{1}', UPD_DATE = NOW()"
                            + " WHERE SEQ = {0};"
                            , argVals[0], usr.id
                            );
                    }
                    bDirty = true;
                    GetData(dgv0, bs0, sGetList());
                    string[] sSend = { "登録しました。", "false" };
                    _ = promag_frm.F05_YN.ShowMiniForm(sSend);
                }
            }
            else
            {
                string[] sSendT = { "有効な入力がありませんでした。登録していません。", "false" };
                _ = promag_frm.F05_YN.ShowMiniForm(sSendT);
                return;
            }
        }
        private string sGetList()
        {
            string s;
            s =
            "SELECT"
            + " p.LOT_NO LotNo"
            + " , CASE WHEN p.SHIP_CHK_DATE IS NULL THEN '' ELSE '確認済み' END 確認結果"
            + " FROM "
            + " ("
            + $"SELECT LOT_NO, SHIP_SEQ, SHIP_CHK_DATE FROM t_product WHERE SHIP_SEQ = {argVals[0]}"
            + " UNION "
            + $"SELECT LOT_NO, SHIP_SEQ, SHIP_CHK_DATE FROM t_t_product WHERE SHIP_SEQ = {argVals[0]}"
            + ") p;";
            return s;
        }

        private void GetData(DataGridView dgv, BindingSource bs, string sSel)
        {
            dgv0.Visible = false;
            try
            {
                // dgvの書式設定全般
                fn.SetDGV(dgv0, true, 20, true, 9, 10, 50, true, 40, DEF_CON.DRED2, DEF_CON.YELLOW);
                //dgv0.MultiSelect = true;

                //if(bs.DataSource != null) bs.DataSource = null;

                mydb.kyDb con = new mydb.kyDb();
                con.GetData(sSel, DEF_CON.Constr());

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

                //icol = new int[] { 0 };
                //clsFunc.setDgvInVisible(dgv0, icol);
                icol = new int[] { 0, 1 };
                iw = new int[] { 1, 1 };
                fn.setDgvAlign(dgv0, icol, iw);
                dgv.ClearSelection();
                //dgv.Width = 560;
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

        private void dgv1IniSet()
        {
            fn.SetDGV(dgv1, false, 500, false, 12, 12, 35, false, 40, DEF_CON.DBLUE, DEF_CON.LIGHT_BLUE);
            string[] sHeader = { "バーコード取込" };

            dgv1.ColumnCount = sHeader.Length;
            dgv1.Columns[0].Width = dgv1.Width - 15;
            dgv1.AllowUserToAddRows = true;
            dgv1.RowCount = 1;
            for (int i = 0; i < sHeader.Length; i++)
            {
                dgv1.Columns[i].HeaderText = sHeader[i];
            }
        }

        #region dgvクリック関連
        private void dgv0_MouseDown(object sender, MouseEventArgs e)
        {
            bdgvCellClk = fn.dgvCellClk(sender, e);
        }

        private void dgv0_CellClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dgv0_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            // データ欄以外は何もしない
            if (!bdgvCellClk) return;
            int ir = e.RowIndex;

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
            //string[] receiveText = JFRM46.ShowMiniForm(this, sendText);

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
    }
}
