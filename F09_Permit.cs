using ClosedXML.Excel;
using System;
using System.Windows.Forms;

namespace k001_shukka
{
    public partial class F09_Permit : Form
    {
        #region フォーム変数
        private bool bClose = true;
        private string[] argVals;    // 親フォームから受け取る引数
        public string[] ReturnValue; // 親フォームに返す戻り値
        private bool bPHide = true;  // 親フォームを隠す = True
        private ToolTip ToolTip1;
        #endregion
        public F09_Permit(params string[] argVals)
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

            #region dgv設定のここでバインド
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
            F09_Permit f = new F09_Permit(s);
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
            // sSEQ, sDate出荷日, sDest, sGrade, sWeight
            lbl5.Text = argVals[0]; // 出荷SEQ
            if (argVals.Length > 1)
            {
                lbl1.Text = argVals[1];
                if (argVals[4].Length > 0) lbl2.Text = int.Parse(argVals[4]).ToString("#,#0kg");
                lbl3.Text = argVals[2];
                lbl4.Text = argVals[3];
            }
            else
            {

            }
            SetTooltip();
            // label8
            PermitLabelReflesh();
            GetData(dgv0, bs0, sGetInspect(lbl5.Text));
        }

        private void PermitLabelReflesh()
        {
            string sSql = string.Format(  // I_PERMIT // APPROVAL PERMISSION
                "SELECT "
                 + " CASE WHEN sp.APPROVAL = '0' THEN '承認済'"
                 + " WHEN sp.APPROVAL = '1' THEN '否認'"
                 + " WHEN sp.I_PERMIT = '0' THEN '品管承認'"
                 + " WHEN sp.I_PERMIT = '1' THEN '品管否認'"
                 + " WHEN sp.PERMISSION = '0' THEN '業務承認'"
                 + " WHEN sp.PERMISSION = '1' THEN '業務否認'"
                 + " ELSE '未承認' END"
                 + " ,m.CONTENT"
                 + " FROM kyoei.t_ship_permission sp"
                 + " LEFT JOIN kyoei.t_memo m ON sp.BIKOU_SEQ = m.SEQ "
                 + " WHERE sp.S_SEQ = {0}"
                 + " ;"
                , argVals[0]);
            mydb.kyDb con = new mydb.kyDb();
            label8.Text = con.sGetVal(sSql, DEF_CON.Constr())[0];
            if (label8.Text != "err")
            {
                textBox4.Text = con.sGetVal(sSql, DEF_CON.Constr())[1];
            }
            else label8.Text = "";
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
            ToolTip1.SetToolTip(button1, "否認");
            ToolTip1.SetToolTip(button2, "承認");
            ToolTip1.SetToolTip(button3, "投入紐付け確認");
            ToolTip1.SetToolTip(button4, "エクセル出力");
            ToolTip1.SetToolTip(button5, "非結晶ロット検査結果");
            //ToolTip1.SetToolTip(button6, "表示更新");
            //ToolTip1.SetToolTip(button7, "アンマッチ確認");
            //ToolTip1.SetToolTip(button8, "売上番号登録");
            //ToolTip1.SetToolTip(button9, "受領書確認");
            //ToolTip1.SetToolTip(button10, "エラー表示");
            ToolTip1.SetToolTip(button11, "検査結果表示");
            //ToolTip1.SetToolTip(button12, "承認依頼");
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

        private string sGetInspect(string sShip)
        {
            string s = string.Empty;
            s = string.Format(
                "SELECT"
                 + " DATE_FORMAT(p.PRODUCT_DATE,'%Y/%m/%d') 生産日"  //0
                 + " ,p.MACHINE_NAME '装置名'"  //1
                 + " ,CONCAT(p.CUSTOMER, p.GOODS_NAME) 品名"  //2
                 + " ,tmp2.InLot 投入ロット"  //3
                 + " ,p.LOT_NO LoNo "  //4
                 + " ,p.CONTROL_NUM 管理No"  //5
                 + " ,CASE"
                 + "  WHEN i.RESULT = '0' THEN '合'"
                 + "  WHEN i.RESULT = '1' THEN '否'"
                 + "  ELSE '-' END 検査結果"  //6


                 + " , p.PRODUCT_COMMENT 'コメント'"  //7 +1
                 + " , i.BIKOU '品管コメント'"  //8 +1
                 + " , iv.VAL 'IV'"  //9 +1
                 + " , mp.MELTINGPOINT '融点'"  //10 +1
                 + " , bf.BFLOW"  //11  +1
                 + " , iFNULL(cc.C_L,rc.C_L) 'L'"  //12 +1
                 + " , IFNULL(cc.C_a,rc.C_a) 'a'"  //13 +1
                 + " , IFNULL(cc.c_b,rc.C_b) 'b'"  //14 +1
                 + " , m.MOISTURE '水分'"  //15 +1
                 + " , truncate (ac.ASH_CONTENT * 100, 2) '灰分'"  //16 +1
                 + " , c.CONTAMINATION '異物'"  //17 +1
                 + " , ps.LENGTH '長'"  //18 +1
                 + " , ps.MAJOR_AXIS '長径'"  //19 +1
                 + " , ps.MINOR_AXIS '短径'"  //20 +1
                 + " , mi.MFR"  //21 +1
                 + " , g1.WEIGHT '100粒'"  //22 +1
                 + " , g3.WEIGHT '30粒'"  //23 +1
                 + " , gn.NUMBER '粒数'"  //24 +1
                 + " , rtn.RETENTION 'IV保持率'"  //25 +1
                 + " , dn.DENSITY '密度'"  //26 +1
                 + " , fu.FUSION '融着率' "  //27 +1
                 + " FROM"

                 + "   ("                            //0
                 + "   SELECT"                            //1
                 + "   PRODUCT_SEQ"                            //2
                 + "   ,PRODUCT_DATE"                            //3
                 + "   ,MACHINE_NAME"                            //4
                 + "   ,CUSTOMER"                            //5
                 + "   ,GOODS_NAME"                            //6
                 + "   ,LOT_NO"                            //7
                 + "   ,CONTROL_NUM"                            //8
                 + "   ,PRODUCT_COMMENT"                            //9
                 + "   ,SHIP_SEQ"                            //10
                 + "   ,INSPECT_SEQ"
                 + "   FROM kyoei.t_product"
                 + "   WHERE SHIP_SEQ = {0}"
                 + "  UNION"
                 + "  SELECT"
                 + "   PRODUCT_SEQ"
                 + "   ,PRODUCT_DATE"
                 + "   ,MACHINE_NAME"
                 + "   ,CUSTOMER"
                 + "   ,GOODS_NAME"
                 + "   ,LOT_NO"
                 + "   ,CONTROL_NUM"
                 + "   ,PRODUCT_COMMENT"
                 + "   ,SHIP_SEQ"
                 + "   ,INSPECT_SEQ"
                 + "   FROM kyoei.t_m_product"
                 + "   WHERE SHIP_SEQ = {0}"
                 + "  UNION"
                 + "  SELECT"
                 + "   PRODUCT_SEQ"
                 + "   ,PRODUCT_DATE"
                 + "   ,MACHINE_NAME"
                 + "   ,CUSTOMER"
                 + "   ,GOODS_NAME"
                 + "   ,LOT_NO"
                 + "   ,CONTROL_NUM"
                 + "   ,PRODUCT_COMMENT"
                 + "   ,SHIP_SEQ"
                 + "   ,INSPECT_SEQ"
                 + "   FROM kyoei.t_t_product"
                 + "   WHERE SHIP_SEQ = {0}"
                 + "   ) p"



                 //+ "  T_PRODUCT p "
                 
                 + " LEFT JOIN kyoei.t_inspect i"
                 + "  ON i.SEQ = p.INSPECT_SEQ"
                 + " LEFT JOIN M_IV iv "
                 + "   ON p.LOT_NO = iv.LOT_NO "
                 + "   AND iv.LGC_DEL = '0'"
                 + "   AND iv.REF_FLG IS NOT NULL"
                 + " LEFT JOIN M_MELTPOINT mp "
                 + "   ON p.LOT_NO = mp.LOT_NO "
                 + "   AND mp.LGC_DEL = '0'"
                 + "   AND mp.REF_FLG IS NOT NULL "
                 + " LEFT JOIN M_BFLOW bf "
                 + "   ON p.LOT_NO = bf.LOT_NO "
                 + "   AND bf.LGC_DEL = '0'"
                 + "   AND bf.REF_FLG IS NOT NULL "
                 + " LEFT JOIN M_COLOR rc "
                 + "   ON p.LOT_NO = rc.LOT_NO "
                 + "   AND rc.LGC_DEL = '0'"
                 + "   AND rc.REF_FLG IS NOT NULL"
                 + " LEFT JOIN M_CCOLOR cc "
                 + "   ON p.LOT_NO = cc.LOT_NO "
                 + "   AND cc.LGC_DEL = '0'"
                 + "   AND cc.REF_FLG IS NOT NULL"
                 + " LEFT JOIN M_MOISTURE m "
                 + "   ON p.LOT_NO = m.LOT_NO "
                 + "   AND m.LGC_DEL = '0'"
                 + "   AND m.REF_FLG IS NOT NULL "
                 + " LEFT JOIN M_ASH_CONTENT ac "
                 + "   ON p.LOT_NO = ac.LOT_NO "
                 + "   AND ac.LGC_DEL = '0' "
                 + " LEFT JOIN M_CONTAMI c "
                 + "   ON p.LOT_NO = c.LOT_NO "
                 + "   AND c.LGC_DEL = '0' "
                 + " LEFT JOIN M_PLT_SIZE ps "
                 + "   ON p.LOT_NO = ps.LOT_NO "
                 + "   AND ps.LGC_DEL = '0' "
                 + " LEFT JOIN M_MFR mi "
                 + "   ON p.LOT_NO = mi.LOT_NO "
                 + "   AND mi.LGC_DEL = '0' "
                 + " LEFT JOIN M_GRAIN100 g1 "
                 + "   ON p.LOT_NO = g1.LOT_NO "
                 + "   AND g1.LGC_DEL = '0' "
                 + " LEFT JOIN M_GRAIN30 g3 "
                 + "   ON p.LOT_NO = g3.LOT_NO "
                 + "   AND g3.LGC_DEL = '0' "
                 + " LEFT JOIN M_GRAIN_NUMBER gn "
                 + "   ON p.LOT_NO = gn.LOT_NO "
                 + "   AND gn.LGC_DEL = '0'"
                 + "   AND gn.REF_FLG IS NOT NULL "
                 + " LEFT JOIN M_IV_RETENTION rtn "
                 + "   ON p.LOT_NO = rtn.LOT_NO "
                 + "   AND rtn.LGC_DEL = '0'"
                 + "   AND rtn.REF_FLG IS NOT NULL "
                 + " LEFT JOIN M_DENSITY dn "
                 + "   ON p.LOT_NO = dn.LOT_NO "
                 + "   AND dn.LGC_DEL = '0' "
                 + "   AND dn.REF_FLG IS NOT NULL"
                 + " LEFT JOIN M_FUSION fu "
                 + "   ON p.LOT_NO = fu.LOT_NO "
                 + "   AND fu.LGC_DEL = '0'"
                 + "   AND fu.REF_FLG IS NOT NULL "
                 + " LEFT JOIN "
                 + " ("
                 + "    SELECT"
                 + "    p.LOT_NO LotNo"
                 + "    ,im1.TANK_NO TNK"
                 + "    ,im1.MATERIAL MTRAL"
                 + "    ,im1.RATIO "
                 + "    ,IFNULL(im1.INPUT_LOT,oi.INPUT_LOT_NO) InLot"
                 + "    FROM kyoei.t_product p"
                 + "    LEFT JOIN kyoei.t_o_input_material im1"
                 + "    ON p.LOT_NO = im1.OUTPUT_LOT"
                 + "    LEFT JOIN kyoei.mj_product jp"
                 + "    ON jp.LOT_NO = im1.INPUT_LOT"
                 + "    LEFT JOIN kyoei.m_j_voucher jv"
                 + "    ON jp.VOUCHER_SEQ = jv.SEQ"
                 + "    LEFT JOIN kyoei.mj_u_product jpu"
                 + "    ON jpu.LOT_NO = im1.INPUT_LOT"
                 + "    LEFT JOIN kyoei.mj_voucher vu"
                 + "    ON vu.SHIPPING_SEQ = jpu.IN_VOUCHER_NO"
                 + "    LEFT JOIN kyoei.t_o_input oi"
                 + "    ON oi.OUTPUT_LOT_NO = p.LOT_NO"
                 + "    WHERE "
                 + "    p.LOT_NO IN "
                 + "    ("
                 + "    SELECT "
                 + "    p1.LOT_NO"
                 + "    FROM kyoei.t_product p1"
                 + "    WHERE p1.SHIP_SEQ = {0}"
                 + "    )"
                 + " ) tmp2 ON tmp2.LotNo = p.LOT_NO"
                 + " WHERE"
                 + "  p.SHIP_SEQ = {0}"
                 + "  OR"
                 + "  p.LOT_NO IN "
                 + "  ("
                 + "    SELECT"
                 + "    tmp.InLot"
                 + "    FROM"
                 + "    ("
                 + "     SELECT"
                 + "    IFNULL(im1.INPUT_LOT,oi.INPUT_LOT_NO) InLot"
                 + "    FROM kyoei.t_product p"
                 + "    LEFT JOIN kyoei.t_o_input_material im1"
                 + "    ON p.LOT_NO = im1.OUTPUT_LOT"
                 + "    LEFT JOIN kyoei.mj_product jp"
                 + "    ON jp.LOT_NO = im1.INPUT_LOT"
                 + "    LEFT JOIN kyoei.m_j_voucher jv"
                 + "    ON jp.VOUCHER_SEQ = jv.SEQ"
                 + "    LEFT JOIN kyoei.mj_u_product jpu"
                 + "    ON jpu.LOT_NO = im1.INPUT_LOT"
                 + "    LEFT JOIN kyoei.mj_voucher vu"
                 + "    ON vu.SHIPPING_SEQ = jpu.IN_VOUCHER_NO"
                 + "    LEFT JOIN kyoei.t_o_input oi"
                 + "    ON oi.OUTPUT_LOT_NO = p.LOT_NO"
                 + "    WHERE "
                 + "    p.LOT_NO IN "
                 + "    ("
                 + "    SELECT "
                 + "    p1.LOT_NO"
                 + "    FROM kyoei.t_product p1"
                 + "    WHERE p1.SHIP_SEQ = {0}"
                 + "    )"
                 + "    ORDER BY"
                 + "    p.PRODUCT_DATE DESC"
                 + "    ,p.MACHINE_NAME"
                 + "    ,CAST(SUBSTRING_INDEX(p.LOT_NO, '-',-1) AS UNSIGNED)"
                 + "    ) tmp"
                 + "  )"
                 + " GROUP BY"
                 + "  p.LOT_NO "
                 + " ORDER BY"
                 + "  p.PRODUCT_DATE DESC"
                 + "  ,p.MACHINE_NAME"
                 + "  ,CAST(SUBSTRING_INDEX(p.LOT_NO,'-',-1) AS UNSIGNED)"
                 + " ;"
                ,sShip);
            return s;
        }

        private void GetData(DataGridView dgv, BindingSource bs, string sSel)
        {
            dgv.Visible = false;
            try
            {
                // dgvの書式設定全般
                fn.SetDGV(dgv, true, 20, true, 9, 10, 50, true, 40, DEF_CON.BLUE, DEF_CON.YELLOW);
                //dgv.MultiSelect = true;

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
                for(int i = 0; i< dgv.Columns.Count; i++)
                {
                    bool bH = true;
                    for(int r = 0; r < dgv.Rows.Count; r++)
                    {
                        if (dgv.Rows[r].Cells[i].Value.ToString().Length > 0)
                        {
                            bH = false;
                            break;
                        }
                    }
                    if (bH) dgv.Columns[i].Visible = false;
                }


                icol = new int[] { 0, 6, 7 };
                iw = new int[] { -1, -1, 1 };
                //fn.setDgvAlign(dgv, icol, iw);
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

        private void button3_Click(object sender, EventArgs e)
        {
            if (dgv0.SelectedRows.Count == 0)
            {
                string[] seT = { "一覧を選択する必要があります。", "false" };
                string[] rcvT = promag_frm.F05_YN.ShowMiniForm(seT);
                return;
            }
            string sLot = dgv0.CurrentRow.Cells[4].Value.ToString();
            sLot = sLot.Substring(0, sLot.IndexOf("-", 7));
            string sDate = dgv0.CurrentRow.Cells[0].Value.ToString();
            sDate = sDate.Replace("/", "-");
            // 

            string sURL = string.Format("http://10.100.10.13:3000/dashboard/385?lotno={0}&fromdate={1}", sLot,sDate);
            System.Diagnostics.Process.Start(sURL);
        }

        private void btn_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            if (dgv0.Rows.Count == 0) return;
            if(label8.Text.Length == 0)
            {
                string[] sSnd = { "承認依頼されていません。\r\n出荷登録画面で承認依頼を実施して下さい。", "false" };
                _ = promag_frm.F05_YN.ShowMiniForm(sSnd);
                return;
            }
            // GYOMU_PERMIT HINKAN_PERMIT SHIP_PERMIT
            if(!radioButton1.Checked && !radioButton2.Checked && !radioButton3.Checked)
            {
                string[] sSnd = { "業務承認、品管承認、製造承認、何れかのラジオボタンをクリックして下さい。", "false" };
                _ = promag_frm.F05_YN.ShowMiniForm(sSnd);
                return;
            }
            string sPermit = string.Empty;
            string sPermit_ID = string.Empty;
            string sPermit_DT = string.Empty;
            if (radioButton1.Checked)
            {
                if (!fn.EnableFunction("GYOMU_PERMIT", usr.id))
                {
                    string[] sSnd = { "承認行為は制限されています。業務資材部にご相談下さい。", "false" };
                    _ = promag_frm.F05_YN.ShowMiniForm(sSnd);
                    return;
                }
                sPermit = "PERMISSION";
                sPermit_ID = "PERMIT_ID";
                sPermit_DT = "PERMIT_DATE";
            }
            else if (radioButton2.Checked)
            {
                if (!fn.EnableFunction("HINKAN_PERMIT", usr.id))
                {
                    string[] sSnd = { "承認行為は制限されています。品質管理室にご相談下さい。", "false" };
                    _ = promag_frm.F05_YN.ShowMiniForm(sSnd);
                    return;
                }
                sPermit = "I_PERMIT"; 
                sPermit_ID = "I_PERMIT_ID";
                sPermit_DT = "I_PERMIT_DATE";
            }
            else if (radioButton3.Checked)
            {
                if (!fn.EnableFunction("SHIP_PERMIT", usr.id))
                {
                    string[] sSnd = { "承認行為は制限されています。工場長にご相談下さい。", "false" };
                    _ = promag_frm.F05_YN.ShowMiniForm(sSnd);
                    return;
                }
                sPermit = "APPROVAL";
                sPermit_ID = "APPROVE_ID";
                sPermit_DT = "APPROVE_DATE";
            }
            else return;

            string sPerm = "0"; // 承認
            if (btn.Name == "button1") // 否認
            {
                sPerm = "1";
            }

            mydb.kyDb con = new mydb.kyDb();
            
            
            string sSql = string.Format(
                "UPDATE t_ship_permission SET {3} = '{0}'"
                + ", {4} = '{1}', {5} = NOW() WHERE S_SEQ = {2};"
                ,sPerm, usr.id, argVals[0],sPermit, sPermit_ID, sPermit_DT);

            string[] Snd = { con.ExecSql(true, DEF_CON.Constr(), sSql), "false" };
            _ = promag_frm.F05_YN.ShowMiniForm(Snd);
            // 承認結果表示
            PermitLabelReflesh();
        }

        private void textBox4_Enter(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;

            #region 申送り事項を入力
            string sTle = "確認コメント入力";
            string[] SndTxt = { sTle, "" };
            string[] RcvT = promag_frm.F08_BIKOU.ShowMiniForm(this, SndTxt);
            if (RcvT[0].Length == 0) return;
            tb.Text = RcvT[0];
            #endregion

            #region 4.テーブルに書込む
            mydb.kyDb con = new mydb.kyDb();

            string sSql = string.Empty;
            // t_tlock,TBL_NAME vachar20,FLG CHAR1
            // ロックテーブルに書込む
            sSql = "SELECT COUNT(*) FROM t_tlock WHERE TBL_NAME = 't_memo';";
            if (con.iGetCount(sSql, DEF_CON.Constr()) > 0)
            {
                string[] Snd = { "現在他の方が書込み中です。時間をおいて再登録して下さい。", "false" };
                _ = promag_frm.F05_YN.ShowMiniForm(Snd);
                return;
            }
            else
            {
                sSql = "INSERT INTO t_tlock (TBL_NAME, FLG) VALUES ('t_memo', '1');";
                con.ExecSql(false, DEF_CON.Constr(), sSql);
            }
            try
            {
                // メモの最大SEQを調べる
                sSql = "SELECT MAX(SEQ) FROM t_memo;";
                int iSEQ = con.iGetCount(sSql, DEF_CON.Constr()); iSEQ++;

                sSql = string.Empty;
                #region DataGridView に入れる場合
                /*
                foreach (DataGridViewRow r in dgv0.SelectedRows)
                {
                    string sLSeq = dgv0.Rows[r.Index].Cells[5].Value.ToString();
                    string s = string.Format(
                        "SELECT COUNT(*) FROM t_take_over2 WHERE LOT_NO = '{0}'"
                        , sLSeq);
                    string[] sSubSeq = {string.Format(
                        "SELECT IFNULL(TAKE_OVER,'NULL') FROM t_take_over2 WHERE LOT_NO = '{0}'"
                        , sLSeq) };

                    if (con.iGetCount(s, DEF_CON.Constr()) > 0)  // 更新
                    {
                        sSql += string.Format(
                            "UPDATE t_take_over2 SET TAKE_OVER = {0}, UPD_ID = '{2}', UPD_DATE = NOW() WHERE LOT_NO = '{1}';"
                            , iSEQ + ir, sLSeq, usr.id);
                        sSubSeq = con.sGetVal(sSubSeq[0], DEF_CON.Constr());
                    }
                    else                                         // 新規
                    {
                        sSql += string.Format(
                            "INSERT INTO t_take_over2 (SEQ, TAKE_OVER, LOT_NO, REG_ID"
                            + ", REG_DATE, UPD_ID, UPD_DATE, LGC_DEL) "
                            + "SELECT IFNULL(MAX(SEQ),0) + 1,{0}, '{1}','{2}',NOW()"
                            + ",'{2}',NOW(),'0' FROM kyoei.t_take_over2;"
                            , iSEQ + ir, sLSeq, usr.id
                            );
                        sSubSeq[0] = "NULL";
                    }
                    sSql += string.Format(
                        "INSERT INTO t_memo (SEQ, SUB_SEQ, CONTENT, UPD_ID, UPD_DATE, LGC_DEL) VALUES "
                        + "({0}, {1}, '{2}', '{3}', NOW(), '0');"
                        , iSEQ + ir, sSubSeq[0], RcvT[0], usr.id);
                    ir++;
                }
                */
                #endregion

                #region 単独のレコードに入れる場合
                // 現在のレコード番号を調べる。
                sSql = string.Format(
                    "SELECT IFNULL(BIKOU_SEQ,0) FROM kyoei.t_ship_permission WHERE S_SEQ = {0};"
                    ,argVals[0]);
                int iBikSEQ = con.iGetCount(sSql, DEF_CON.Constr());
                string subS = iBikSEQ.ToString();
                if (iBikSEQ == 0) subS = "NULL";
                sSql = string.Format(
                  "INSERT INTO t_memo (SEQ, SUB_SEQ, CONTENT, UPD_DATE, UPD_ID, LGC_DEL)"
                + " VALUES({0}, {1}, '{2}', NOW(), '{3}', '0');"
                    , iSEQ.ToString(), subS, RcvT[0], usr.id);
                sSql += string.Format(
                    "UPDATE t_ship_permission SET BIKOU_SEQ = {0} WHERE S_SEQ = {1};"
                    ,iSEQ.ToString(), argVals[0]);
                sSql += "DELETE FROM t_tlock WHERE TBL_NAME = 't_memo';";
                #endregion
                if (sSql.Length > 0)
                {
                    string sM = con.ExecSql(true, DEF_CON.Constr(), sSql);
                    if (sM.IndexOf("エラー") >= 0)
                    {
                        string[] Snd = { sM, "false", sTle };
                        _ = promag_frm.F05_YN.ShowMiniForm(Snd);
                        sSql = "DELETE FROM t_tlock WHERE TBL_NAME = 't_memo';";
                        con.ExecSql(false, DEF_CON.Constr(), sSql);
                        return;
                    }
                    else
                    {
                        string sTitle = "申送り事項";
                        string[] Snd = { "登録しました。", "false", sTitle };
                        _ = promag_frm.F05_YN.ShowMiniForm(Snd);
                    }
                }
                else
                {
                    string sTitle = "申送り事項";
                    string[] Snd = { "登録する内容がありませんでした。", "false", sTitle };
                    _ = promag_frm.F05_YN.ShowMiniForm(Snd);
                    sSql = "DELETE FROM t_tlock WHERE TBL_NAME = 't_memo';";
                    con.ExecSql(false, DEF_CON.Constr(), sSql);
                    return;
                }
                #endregion
            }
            catch (Exception ex)
            {
                string[] Snd1 = { "処理中にエラーが発生しました。\r\n" + ex.Message, "false" };
                _ = promag_frm.F05_YN.ShowMiniForm(Snd1);
                sSql = "DELETE FROM t_tlock WHERE TBL_NAME = 't_memo';";
                con.ExecSql(false, DEF_CON.Constr(), sSql);
            }
        }

        private void label8_TextChanged(object sender, EventArgs e)
        {
            Label lbl = (Label)sender;
            if (lbl.Text.IndexOf( "承認") >= 0) lbl.ForeColor = System.Drawing.Color.DarkGreen;
            else if (lbl.Text.IndexOf("承認済") >= 0) lbl.ForeColor = System.Drawing.Color.Blue;
            else if (lbl.Text.IndexOf("否認") >= 0) lbl.ForeColor = System.Drawing.Color.Red;
            else lbl.ForeColor = System.Drawing.Color.Empty;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            fn.CrtUsrIni("2", "011" + argVals[0]);
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

        private void button4_Click(object sender, EventArgs e)
        {
            ExportXls(dgv0);
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
            string fileNm = string.Format("{1}出荷承認待一覧{0}.xlsx", DateTime.Now.ToLongDateString(), argVals[0]);
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


        private void rB_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            if (!rb.Checked) return;
            string s = string.Empty;
            string sM = string.Empty;
            if(rb.Name == "radioButton2")
            {
                s = string.Format(
                       "SELECT COUNT(*)"                            //0
                     + " FROM kyoei.t_ship_permission sp"
                     + " WHERE "
                     + " sp.S_SEQ = {0}"
                     + " AND sp.PERMISSION = '0';"
                    , argVals[0]);
                sM = "業務承認を行ってから実施して下さい。";
            }
            if (rb.Name == "radioButton3")
            {
                s = string.Format(
                       "SELECT COUNT(*)"                            //0
                     + " FROM kyoei.t_ship_permission sp"
                     + " WHERE "
                     + " sp.S_SEQ = {0}"
                     + " AND sp.I_PERMIT= '0';"
                    , argVals[0]);
                sM = "品管承認を行ってから実施して下さい。";
            }
            if(s.Length > 0)
            {
                mydb.kyDb con = new mydb.kyDb();
                int i = con.iGetCount(s, DEF_CON.Constr());
                if(i == 0)
                {
                    string[] Snd = { sM, "false" };
                    _ = promag_frm.F05_YN.ShowMiniForm(Snd);
                    rb.Checked = false;
                }
            }
        }

        // 
    }
}
