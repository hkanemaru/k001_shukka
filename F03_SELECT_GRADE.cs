using System;
using System.Windows.Forms;


namespace k001_shukka
{
    public partial class F03_SELECT_GRADE : Form
    {
        private Boolean bClose = true;
        private string[] argVals; // 親フォームから受け取る引数
        public string[] ReturnValue;            // 親フォームに返す戻り値
        private Boolean bPHide = true;
        private Boolean bdgvCellClk = false;
        private int iCount = 0;                      // かな検索の値
        private string sKana = String.Empty; // かな検索の値
        private String sFilter1 = String.Empty; // dgv1で作ったフィルター
        private Boolean bLoaded = false; //ロードしない間はfalse
        private string sBACC = string.Empty;
        private string sGRD = string.Empty;

        public F03_SELECT_GRADE(params string[] argVals)
        {
            // 親フォームから受け取ったデータをこのインスタンスメンバに格納
            this.argVals = argVals;
            InitializeComponent();

            // ■■■ 親フォームを隠す設定 true => 隠す　default = 隠す。
            bPHide = false; // 隠さない
            //■■■ 画面の和名
            string sTitle = "取引先・製品名選択";
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

            // 一覧表のソースを指定
            dgv1.DataSource = bs1;
            dgv2.DataSource = bs2;
        }
        /// <summary>
        ///  グレードを返す。引数1>> 取引先名, 引数2>> 品名にそれぞれセットされる
        /// </summary>
        /// <param name="frm"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        static public string[] ShowMiniForm(Form frm, params string[] s)
        {
            F03_SELECT_GRADE f = new F03_SELECT_GRADE(s);
            f.ShowDialog(frm);

            string[] receiveText = f.ReturnValue; // -- ①
            f.Dispose();
            return receiveText;
        }

        private void F03_SELECT_GRADE_Load(object sender, EventArgs e)
        {
            // 開いた元フォームを隠す設定
            if (bPHide) this.Owner.Hide();
            GetData(dgv1);
            GetData(dgv2);
            rb1.Checked = true;
            bLoaded = true;
        }
        private void FRM_Shown(object sender, EventArgs e)
        {
            if (argVals[0].Length > 0)
            {
                int i = bs1.Find("取引先", argVals[0]);
                if (i > 0)
                {
                    dgv1.Rows[i].Selected = true;
                    label6.Text = argVals[0];
                }
            }
            if (argVals[1].Length > 0)
            {
                int i = bs2.Find("品名", argVals[1]);
                if (i > 0)
                {
                    dgv2.Rows[i].Selected = true;
                    label5.Text = argVals[1];
                }
            }
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
            this.Close();
        }
        // 1) btnClose  -> 2) -> 3) -> 2) -> 1) -> ShowMiniForm.f.showdialog
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.ReturnValue = new string[] { "0", "0" };
            closing();
        }
        // CLOSE処理の最後
        private void FRM_Closed(object sender, FormClosedEventArgs e)
        {
            if (bPHide) this.Owner.Show();
        }
        #endregion

        private void button1_Click(object sender, EventArgs e)
        {// sGRD = GRADE_ACCOUNT_SEQ label6 得意先 label5 品名
            this.ReturnValue = new string[] { label6.Text, label5.Text, sBACC, sGRD };
            closing();
        }

        private void GetData(DataGridView dgv)
        {
            try
            {

                dgv.Visible = false;

                // dgvの書式設定全般
                int[] hcolor = { 52, 86, 86 };
                int[] rcolor = { 217, 236, 198 };
                fn.SetDGV(dgv, true, 30, true, 9, 10, 40, true, 40, hcolor, rcolor);
                dgv.MultiSelect = true;

                if (dgv.Name == "dgv1") bs1.DataSource = null;
                else bs2.DataSource = null;

                // データ抽出
                mydb.kyDb con = new mydb.kyDb();

                string sSeq = string.Empty;
                if(argVals.Length > 2)
                {
                    for(int i = 2; i < argVals.Length; i++)
                    {
                        if(argVals[i].Length > 0)
                        {
                            sSeq += "," + argVals[i];
                        }
                    }
                    if (sSeq.Length > 0) sSeq = sSeq.Substring(1);
                }
                #region string s
                string s = string.Empty;
                string sWhr = string.Empty;
                if (dgv.Name == "dgv1")
                {
                    s = "SELECT ba.B_AC_SEQ seq, ba.B_ACCOUNT 取引先, ba.B_ACCOUNT_KANA カナ, ba.BA_KBN "+
                        " FROM M_B_ACCOUNT ba WHERE LGC_DEL = '0';";
                    if (sKana.Length == 0 && argVals.Length > 2 && sSeq.Length > 0)
                    {
                        s = string.Format(
                            "SELECT ba.B_AC_SEQ seq, ba.B_ACCOUNT 取引先, ba.B_ACCOUNT_KANA カナ, ba.BA_KBN"
                            + " FROM M_B_ACCOUNT ba"
                            + " LEFT JOIN kyoei.m_grade_account ga"
                            + " ON ga.B_AC_SEQ = ba.B_AC_SEQ"
                            + " WHERE ba.LGC_DEL = '0'"
                            + "  AND ga.SEQ in ({0});"
                            , sSeq);
                    }
                }
                else
                {
                    s = "SELECT ga.B_AC_SEQ seq, g.GRADE 品名, ga.SEQ GSEQ, g.GRADE_KANA カナ"
                        + " FROM M_GRADE_ACCOUNT ga "
                        + " LEFT JOIN M_GRADE g ON ga.GRADE_SEQ = g.GRADE_SEQ AND g.LGC_DEL = '0'"
                        + " WHERE ga.LGC_DEL = '0' AND g.LGC_DEL = '0';";
                    if (sKana.Length == 0 && argVals.Length > 2 && sSeq.Length > 0)
                    {
                        s = string.Format(
                            "SELECT ga.B_AC_SEQ seq, g.GRADE 品名, ga.SEQ GSEQ, g.GRADE_KANA カナ"
                        + " FROM M_GRADE_ACCOUNT ga "
                        + " LEFT JOIN M_GRADE g ON ga.GRADE_SEQ = g.GRADE_SEQ AND g.LGC_DEL = '0'"
                        + " WHERE ga.LGC_DEL = '0' AND g.LGC_DEL = '0'"
                        + " AND ga.SEQ in ({0});"
                        , sSeq);
                    }
                }
                #endregion

                con.GetData(s,DEF_CON.Constr());
                // dgvにデータをバインド
                if (dgv.Name == "dgv1") bs1.DataSource = con.ds.Tables[0];
                else bs2.DataSource = con.ds.Tables[0];

                #region dgv書式設定

                // dgv 列幅設定
                int[] ic = { 0 };
                int[] iw = { 277 };
                //fn.setDgvWidth(dgv, ic, iw);
                dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                // dgv 列非表示設定
                ic = new int[] { 0, 2, 3 };
                fn.setDgvInVisible(dgv, ic);
                
                #endregion

                // フォームにグリッドを表示
                dgv.Visible = true;

                dgv.ClearSelection();

                String sLABEL = String.Format("対象： {0}", dgv.Rows.Count.ToString());
                if (dgv.Name == "dgv1")
                {
                    label1.Text = sLABEL;
                    label1.Left = dgv1.Left + dgv1.Width - label1.Width;
                }
                else
                {
                    label2.Text = sLABEL;
                    label2.Left = dgv2.Left + dgv2.Width - label2.Width;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("表示対象がありません。", ex.GetType().FullName);
            }
        }

        private void dgv_MouseDown(object sender, MouseEventArgs e)
        {
            bdgvCellClk = fn.dgvCellClk(sender, e);
        }

        private void dgv1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            // データ欄以外は何もしない
            if (!bdgvCellClk) return;
            sBACC = dgv1.Rows[e.RowIndex].Cells[0].Value.ToString();
            label6.Text = dgv1.Rows[e.RowIndex].Cells[1].Value.ToString();

            sFilter1 = string.Format("seq IN ({0})", sBACC);
            bs2.Filter = sFilter1;
            // 接続解除
            //con.Close();

            gridCount();
        }

        private void SetFKana(object sender)
        {
            //■■■■■■ この関数で使用するプライベート変数 ■■■■■■
            //　iCount = 0;                      // かな検索の値
            //  sKana 　　　　　　 // かな検索の値
            //textBox1 = 品名の検索文字列表示　textBox2 = 得意先の検索文字列表示
            string sRet = String.Empty;
            string tmpKana = String.Empty; // 検索文字列に追加する文字列
            string tmpMoji = String.Empty;  // 得意先、品名のテキストボックスに表示する文字列
            Button btn = (Button)sender;

            if (btn.Name.Substring(btn.Name.Length - 1) != "A")
            {
                iCount += 1;
            }
            else
            {
                iCount = 0;
            }

            tmpMoji += @"[" + btn.Name.Substring(btn.Name.Length - 1) + @"]";

            #region テンキーボタンでフィルタ文字列生成
            switch (btn.Name.Substring(btn.Name.Length - 1))
            {
                case "A":
                    sKana = "";
                    tmpMoji = "";
                    break;
                case "1":
                    tmpKana = String.Format(
                                "((SUBSTRING(カナ,{0},1) >= 'ア' AND SUBSTRING(カナ,{0},1) <= 'オ') OR (SUBSTRING(カナ,{0},1) >= '!' AND SUBSTRING(カナ,{0},1) <= '@') OR SUBSTRING(カナ,{0},1) = '-' OR SUBSTRING(カナ,{0},1) = '1')"
                                , iCount.ToString());
                    break;
                case "2":
                    tmpKana = String.Format(
                                "((SUBSTRING(カナ,{0},1) >= 'カ' AND SUBSTRING(カナ,{0},1) <= 'ゴ') OR (SUBSTRING(カナ,{0},1) >= 'A' AND SUBSTRING(カナ,{0},1) <= 'c') OR SUBSTRING(カナ,{0},1) = '2')"
                                , iCount.ToString());
                    break;
                case "3":
                    tmpKana = String.Format(
                                "((SUBSTRING(カナ,{0},1) >= 'サ' AND SUBSTRING(カナ,{0},1) <= 'ゾ') OR (SUBSTRING(カナ,{0},1) >= 'D' AND SUBSTRING(カナ,{0},1) <= 'f') OR SUBSTRING(カナ,{0},1) = '3')"
                                , iCount.ToString());
                    break;
                case "4":
                    tmpKana = String.Format(
                                "((SUBSTRING(カナ,{0},1) >= 'タ' AND SUBSTRING(カナ,{0},1) <= 'ド') OR (SUBSTRING(カナ,{0},1) >= 'G' AND SUBSTRING(カナ,{0},1) <= 'i') OR SUBSTRING(カナ,{0},1) = '4')"
                                , iCount.ToString());
                    break;
                case "5":
                    tmpKana = String.Format(
                                "((SUBSTRING(カナ,{0},1) >= 'ナ' AND SUBSTRING(カナ,{0},1) <= 'ノ') OR (SUBSTRING(カナ,{0},1) >= 'J' AND SUBSTRING(カナ,{0},1) <= 'l') OR SUBSTRING(カナ,{0},1) = '5')"
                                , iCount.ToString());
                    break;
                case "6":
                    tmpKana = String.Format(
                                "((SUBSTRING(カナ,{0},1) >= 'ハ' AND SUBSTRING(カナ,{0},1) <= 'ボ') OR (SUBSTRING(カナ,{0},1) >= 'M' AND SUBSTRING(カナ,{0},1) <= 'o') OR SUBSTRING(カナ,{0},1) = '6')"
                                , iCount.ToString());
                    break;
                case "7":
                    tmpKana = String.Format(
                                "((SUBSTRING(カナ,{0},1) >= 'マ' AND SUBSTRING(カナ,{0},1) <= 'モ') OR (SUBSTRING(カナ,{0},1) >= 'P' AND SUBSTRING(カナ,{0},1) <= 's') OR SUBSTRING(カナ,{0},1) = '7')"
                                , iCount.ToString());
                    break;
                case "8":
                    tmpKana = String.Format(
                                "((SUBSTRING(カナ,{0},1) >= 'ヤ' AND SUBSTRING(カナ,{0},1) <= 'ヨ') OR (SUBSTRING(カナ,{0},1) >= 'T' AND SUBSTRING(カナ,{0},1) <= 'v') OR SUBSTRING(カナ,{0},1) = '8')"
                                , iCount.ToString());
                    break;
                case "9":
                    tmpKana = String.Format(
                                "((SUBSTRING(カナ,{0},1) >= 'ラ' AND SUBSTRING(カナ,{0},1) <= 'ロ') OR (SUBSTRING(カナ,{0},1) >= 'W' AND SUBSTRING(カナ,{0},1) <= 'z') OR SUBSTRING(カナ,{0},1) = '9')"
                                , iCount.ToString());
                    break;
                case "0":
                    tmpKana = String.Format(
                                "((SUBSTRING(カナ,{0},1) >= 'ワ' AND SUBSTRING(カナ,{0},1) <= 'ン') OR SUBSTRING(カナ,{0},1) = '0' OR SUBSTRING(カナ,{0},1) = '(')"
                                , iCount.ToString());
                    break;
            }
            #endregion

            if (tmpKana.Length == 0)
            {  // tmpKana の設定がないときは sRet = ""
                sRet = "";
            }
            else
            {  // tmpKana に設定があるとき
                if (sKana.Length == 0) // sKana = 設定済みの検索文字列がないときは
                {
                    sKana = tmpKana; // sKana 　-> sKana は tmpKana
                }
                else                         // skana に設定が既にある場合は、 tmpKanaを ANDでつなぐ
                {
                    sKana += " AND (" + tmpKana + ")";
                }
            }

            if (rb1.Checked)
            {
                textBox2.Text += tmpMoji;
                if (iCount < 1) textBox2.Text = "";
            }
            else
            {
                textBox1.Text += tmpMoji;
                if (iCount < 1) textBox1.Text = "";
            }
        }

        private void onFilter()
        {
            string sFilter = String.Empty;

            //if (sRec.Length != 0)
            //{
            //    if (sKana.Length != 0) sFilter = sRec + " AND " + sKana; else sFilter = sRec;
            //}
            //else sFilter = sKana;

            sFilter = sKana;
            if (rb2.Checked)
            {
                if (sFilter1.Length > 0)
                {
                    if (sFilter.Length > 0) bs2.Filter = sFilter1 + " AND " + sFilter;
                    else bs2.Filter = sFilter1;
                }
                else
                {
                    if (sFilter.Length > 0) bs2.Filter = sFilter;
                    else bs2.Filter = String.Empty;
                }
            }
            else
            {
                if (sFilter.Length > 0) bs1.Filter = sFilter;
                else bs1.Filter = String.Empty;
            }
            gridCount();
        }

        private void btnKANA_Click(object sender, EventArgs e)
        {
            SetFKana(sender);
            onFilter();
        }

        private void gridCount()
        {
            label1.Text = String.Format("対象： {0}", dgv1.Rows.Count.ToString());
            label1.Left = dgv1.Left + dgv1.Width - label1.Width;
            if (dgv1.Rows.Count == 0) label6.Text = "";

            label2.Text = String.Format("対象： {0}", dgv2.Rows.Count.ToString());
            label2.Left = dgv2.Left + dgv2.Width - label2.Width;
            if (dgv2.Rows.Count == 0) label5.Text = "";
        }

        private void dgv2_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            sGRD = dgv2.Rows[e.RowIndex].Cells[2].Value.ToString();
            label5.Text = dgv2.Rows[e.RowIndex].Cells[1].Value.ToString();
        }
        // 全取消
        private void button2_Click(object sender, EventArgs e)
        {
            iCount = 0;
            sKana = string.Empty;
            sFilter1 = string.Empty;
            bs1.Filter = string.Empty;
            bs2.Filter = string.Empty;
            Array.Resize(ref argVals, 2);
            
            GetData(dgv1);
            GetData(dgv2);
            dgv1.ClearSelection();
            dgv2.ClearSelection();
            gridCount();
            textBox2.Text = "";
            textBox1.Text = "";
            label6.Text = "";
            label5.Text = "";
        }

        private void dgv1_CurrCellChange()
        {
            if (!bLoaded) return;

            sBACC = dgv1.CurrentRow.Cells[0].Value.ToString();
            label6.Text = dgv1.CurrentRow.Cells[1].Value.ToString();
            // ユーザーの権限により編集画面見せる
            sFilter1 = String.Format("seq = {0}", sBACC);
            bs2.Filter = sFilter1;
            gridCount();
        }
        private void dgv1_CurrentCellChanged(object sender, EventArgs e)
        {
            if (!bLoaded) return;

            if (dgv1.CurrentRow != null)
            {
                String sSEQ = dgv1.CurrentRow.Cells[0].Value.ToString();
                label6.Text = dgv1.CurrentRow.Cells[1].Value.ToString();
                // ユーザーの権限により編集画面見せる
                sFilter1 = String.Format("seq = {0}", sSEQ);
                bs2.Filter = sFilter1;
                gridCount();
            }
        }
    }
}
