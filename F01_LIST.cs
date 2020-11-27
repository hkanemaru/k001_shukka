using ClosedXML.Excel;
using mydb;
using System;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Windows.Forms;

namespace k001_shukka
{
    public partial class F01_LIST : Form
    {
        #region フォーム変数
        private bool bClose = true;
        private string[] argVals; // 親フォームから受け取る引数
        public string[] ReturnValue;            // 親フォームに返す戻り値
        private bool bPHide = true;  // 親フォームを隠す = True
        DateTime loadTime; // formloadの時間
        ToolTip ToolTip1;
        private string SDate = string.Empty;
        private string EDate = string.Empty;
        string sFilter = string.Empty;
        private bool bdgvCellClk = false; // dgvでクリックする場合には必須
        private int pDisp = 0;
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

            lblCaption.Text = sTitle;
            string s = string.Empty; ;
            if (usr.iDB == 1) s += " TestDB: ";
            s += DateTime.Now.ToString("yy/MM/dd HH:mm");
            s += " " + usr.name;
            lblRCaption.Text = s;
            // タイトルバー表示設定
            this.Text = string.Format("【{0}】 {1}"
                , this.Name
                , DEF_CON.prjName + " " + DEF_CON.verString);
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

            SDate = DateTime.Today.AddDays(-1).ToShortDateString();
            EDate = DateTime.Today.ToShortDateString();

            string sTerm = string.Empty;
            double Interval = (DateTime.Parse(EDate) - DateTime.Parse(SDate)).TotalDays;
            if (Interval == 1) sTerm = SDate;
            else sTerm = string.Format("{0} - {1}", SDate, EDate);
            lblCaption.Text += " " + sTerm;

            GetData(dgv0, bs0, GetOrder());
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
            ToolTip1.SetToolTip(button3, "入庫登録");
            ToolTip1.SetToolTip(button4, "表示切替");
            //ToolTip1.SetToolTip(button5, "ロット一覧印刷");
            //ToolTip1.SetToolTip(button6, "ロット単票印刷");
            //ToolTip1.SetToolTip(button7, "表示切替");

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

        private string GetOrder()
        {
            string sMN = string.Empty;
            string sTerm = string.Empty;
            sTerm = string.Format("BETWEEN '{0}' AND '{1}'", SDate, EDate);
            string s = string.Empty;
            if (pDisp == 0)
            {
                s = string.Format(
                    "SELECT"
                     + " p.PRODUCT_SEQ SEQ"  //0
                     + " ,p.GOODS_NAME 品名"  //1
                     + " , CONCAT( "  //2
                     + "   LEFT (p.MACHINE_NAME, 1)"
                     + "   , '-'"
                     + "   , DATE_FORMAT(p.PRODUCT_DATE, '%m')"
                     + "   , '-'"
                     + "   , p.CONTROL_NUM"
                     + " ) 外袋No"
                     + " , CONCAT('-', SUBSTRING_INDEX(p.LOT_NO, '-', - 1)) Lot枝番" // 3
                     + ", p.WEIGHT" // 4
                     + ", p.LOT_NO LotNO" // 5
                     + " FROM"
                     + " kyoei.t_m_product p "
                     + " LEFT JOIN kyoei.t_b_warehouse bw"
                     + " ON bw.LOT_SEQ = p.PRODUCT_SEQ"
                     + " WHERE"
                     + "  p.PRODUCT_DATE {1}"
                     + "  AND bw.LOT_SEQ IS NULL"
                     + "  {0}"
                     + " ORDER BY p.MACHINE_NAME, p.PRODUCT_DATE, p.LOT_NO"
                     + ";"
                    , sMN, sTerm);
            }
            if (pDisp == 1)
            {
                s = string.Format(
                    "SELECT"
                + " DATE_FORMAT(p.PRODUCT_DATE,'%Y/%m/%d') 生産日"  //0
                + " ,p.MACHINE_NAME ライン"  //1
                + " ,p.LOT_NO LotNo"  //2
                + " ,bw.LOT_NO 管理No"  //3
                + " ,'' 備考"  //4
                + " ,p.WEIGHT 在庫数"  //5
                + " ,DATE_FORMAT(bw.STATUS0,'%Y/%m/%d') 倉入日"  //6
                + " FROM kyoei.t_b_warehouse bw"
                + " LEFT JOIN kyoei.t_m_product p"
                + "  ON p.PRODUCT_SEQ = bw.LOT_SEQ"
                + " WHERE bw.STATUS0 {0}"
                + " AND bw.LOT_SEQ IS NOT NULL"
                + " {1}"
                + " ORDER BY p.PRODUCT_DATE,p.MACHINE_NAME,p.LOT_NO;"
                    , sTerm, sMN);
            }
            if (pDisp == 2)
            {
                s = string.Format(
                    "SELECT"
                 + " '' 伝票"  //0
                 + " ,'MR物流センタ-'"  //1
                 + " ,DATE_FORMAT(bw.STATUS0,'%m/%d') 入庫日"  //2
                 + " ,'' 得コード"  //3
                 + " ,'MRファクトリー' 得意先名"  //4
                 + " ,'PET' 種類"  //5
                 + " ,'' 品名CD"  //6
                 + " ,CONCAT(p.GOODS_NAME"
                 + " ,' ','MRF ',p.MACHINE_NAME) 品名"  //7
                 + " ,'' 備考"  //8
                 + " ,COUNT(*) 数量"  //9
                 + " ,'F/B' 単位1"  //10
                 + " ,SUM(p.WEIGHT) 重量"  //11
                 + " ,'kg' 単位2"  //12
                 + " FROM kyoei.t_b_warehouse bw"
                 + " LEFT JOIN kyoei.t_m_product p"
                 + "  ON p.PRODUCT_SEQ = bw.LOT_SEQ"
                 + " WHERE bw.STATUS0 {0}"
                 + " AND bw.LOT_SEQ IS NOT NULL"
                 + " {1}"
                 + " GROUP BY bw.STATUS0,p.PRODUCT_DATE,p.MACHINE_NAME,p.GOODS_NAME"
                 + " ORDER BY bw.STATUS0,p.PRODUCT_DATE,p.MACHINE_NAME,p.GOODS_NAME;"
                    , sTerm, sMN);
            }
            return s;
        }

        private void GetData(DataGridView dgv, BindingSource bs, string sSel)
        {
            dgv.Visible = false;
            try
            {
                // dgvの書式設定全般
                fn.SetDGV(dgv, true, 20, true, 9, 10, 50, true, 40, DEF_CON.DBLUE, DEF_CON.YELLOW);
                dgv.MultiSelect = true;

                //if(bs.DataSource != null) bs.DataSource = null;

                mydb.kyDb con = new mydb.kyDb();

                con.GetData(sSel, DEF_CON.Constr());
                bs0.Filter = "";
                bs.DataSource = con.ds.Tables[0];
                if (pDisp == 2) dgv.Columns[11].DefaultCellStyle.Format = "#,0";
                //dgv.Columns[5].DefaultCellStyle.Format = "#,0";
                //dgv.Columns[4].DefaultCellStyle.Format = "yyyy";

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
                //clsFunc.setDgvWidth(dgv, icol, iw);
                #endregion
                if (pDisp == 0)
                {
                    icol = new int[] { 0, 4 };
                    fn.setDgvInVisible(dgv, icol);
                    icol = new int[] { 0, 3 };
                    iw = new int[] { -1, -1 };
                    fn.setDgvAlign(dgv, icol, iw);
                }

                dgv.ClearSelection();
                ChkFilter();
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
            }
            if (btn.Name == "button1") // 決定
            {
                ExportXls(dgv0);
            }
            if (btn.Name == "button3")
            {
                if (pDisp == 0) RegistRecipt();
                else
                {
                    string[] sSet = { "この表示の時には登録をサポートしません", "false" };
                    string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSet);
                    return;
                }
            }
            if (btn.Name == "button4")
            {
                pDisp++;
                if (pDisp == 3) pDisp = 0;

                // 表示に応じたタイトルの付与
                string sTerm = string.Empty;
                double Interval = (DateTime.Parse(EDate) - DateTime.Parse(SDate)).TotalDays;
                if (Interval == 1) sTerm = SDate;
                else sTerm = string.Format("{0} - {1}", SDate, EDate);
                switch (pDisp)
                {
                    case 0: lblCaption.Text = "入庫登録 " + sTerm; break;
                    case 1: lblCaption.Text = "生産日別入庫済みLot一覧 " + sTerm; break;
                    case 2: lblCaption.Text = "入庫日・拠点・グレード別重量一覧 " + sTerm; break;
                }

                GetData(dgv0, bs0, GetOrder());
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
            string fileNm = string.Format("{1}入庫確認{0}.xlsx", DateTime.Now.ToLongDateString(), argVals[0]);
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

        private void RegistRecipt()
        {
            #region 登録前確認
            if (dgv0.SelectedRows.Count == 0)
            {
                string[] sSet = { "一覧から対象を選択して下さい。", "false" };
                string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSet);
                return;
            }
            else
            {

                string sComment = "選択されているLotを登録します。";
                string[] sSet = { sComment, "" };
                string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSet);
                if (sRcv[0].Length == 0) return;
            }
            #endregion
            string sINS = string.Empty;
            string sINSVAL = string.Empty;
            // 予定SEQと製造元を抽出
            sINS = "INSERT INTO t_b_warehouse (";
            sINS += "LOT_NO,LOT_SEQ,STATUS0,LOCATION,UPD_ID,UPD_DATE,LGC_DEL";
            sINS += ") VALUES ";
            for (int i = 0; i < dgv0.Rows.Count; i++)
            {
                if (dgv0.Rows[i].Selected)
                {
                    string sLot = dgv0.Rows[i].Cells[2].Value.ToString();
                    string sSEQ = dgv0.Rows[i].Cells[0].Value.ToString();

                    sINSVAL += string.Format(
                            ",('{0}', {1}, NOW(), 3, '{2}', NOW(), '0')"
                            , sLot, sSEQ, usr.id);
                }
            }
            sINS += sINSVAL.Substring(1) + ";";


            kyDb con = new kyDb();
            con.ExecSql(false, DEF_CON.Constr(), sINS);

            GetData(dgv0, bs0, GetOrder());
        }

        private void CalcWT()
        {
            int ic = 4;
            if (pDisp == 1) ic = 5;
            if (pDisp == 2) ic = 11;
            double dWt = fn.lColumnSum(dgv0, ic);
            int iWt = (int)dWt;
            label3.Text = dWt.ToString("#,0");
            //if (dgv0.Rows.Count > 0)
            //{
            //    int iw = 0;
            //    for (int i = 0; i < 6; i++)
            //    {
            //        iw += dgv0.Columns[i].Width;
            //    }
            //    label3.Left = dgv0.Left + iw;

            //}
        }

        private void ChkFilter()
        {
            //if (dgv0.Rows.Count <= 0) return;
            sFilter = string.Empty;

            //if (textBox1.Text.Length > 0) sFilter += string.Format(
            //     " AND 生産工場 LIKE '%{0}%'", textBox1.Text);
            //if (textBox2.Text.Length > 0) sFilter += string.Format(
            //     " AND 予定No = {0}", textBox2.Text);
            //if (textBox3.Text.Length > 0) sFilter += string.Format(
            //     " AND 向け先 LIKE '%{0}%'", textBox3.Text);
            try
            {
                if (sFilter.Length > 4) sFilter = sFilter.Substring(4);
                bs0.Filter = sFilter;

                FillDgvCount(dgv0, label1);
                if (dgv0.Rows.Count > 0)
                {
                    int ileft = 0;
                    for (int i = 0; i < 2; i++)
                    {
                        ileft += dgv0.Columns[i].Width;
                    }
                    textBox1.Left = dgv0.Left + ileft;
                    textBox1.Width = dgv0.Columns[2].Width;
                    //textBox2.Left = dgv0.Left;
                    //textBox2.Width = dgv0.Columns[0].Width;
                    //textBox3.Left = textBox1.Left + textBox1.Width + 3;
                    //textBox3.Width = dgv0.Columns[3].Width;
                }
                CalcWT();
            }
            catch (Exception ex)
            {
                MessageBox.Show("検索出来ない文字列を含んでいます。", ex.GetType().FullName);
            }
        }

        private void ctrl_Changed(object sender, EventArgs e)
        {
            // チェックボックスの動きを制御
            Control c = (Control)sender;

            ChkFilter();
        }

        private void SendMail(string sAtesaki, string SEQs, string BIKOU)
        {
            //MailMessageの作成
            System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();
            //送信者（メールアドレスが"sender@xxx.xxx"、名前が"鈴木"の場合）
            msg.From = new System.Net.Mail.MailAddress("tetra_admin@kyoei-rg.co.jp", "Tetra");
            //宛先（メールアドレスが"recipient@xxx.xxx"、名前が"加藤"の場合）
            msg.To.Add(new System.Net.Mail.MailAddress(sAtesaki, sAtesaki)); // 第2引数"テトラ"
            //件名
            string sSub = string.Empty;
            string sBody = string.Empty;
            string sFooter = "\r\n\r\n\r\n\r\n\r\n"
              + "===================================\r\n"
              + "本メールは送信専用のメールアドレスからお送りしています。\r\n"
              + "返信いただいても、送信元には届きませんのであらかじめご了承下さい。";
            if (usr.iDB == 1) sSub = "■■■テストメール■■■";
            if (argVals[0] == "2")
            {
                sSub += "<小山工場>出荷予定否認連絡";
            }
            if (argVals[0] == "1")
            {
                sSub += "<協栄物流>出荷予定配車■■不可■■連絡";
            }
            if (argVals[0] == "3")
            {
                sSub += "<MRBC>出荷予定否認連絡";
            }
            msg.Subject = sSub;
            //本文
            sBody = string.Format("予定No.[{0}]について、\r\n", SEQs);
            if (argVals[0] == "1") sBody += "配車不可 と回答されました。\r\n";
            else sBody += "受入不可 と回答されました。\r\n";
            if (argVals[0] == "1") sBody += "物流本社と、調整をお願いします。\r\n";
            else sBody += "受入れ先と、日程又は数量の調整をお願いします。\r\n";
            if (argVals[0] == "1") sBody += string.Format("\r\n配車不可理由：{0}", BIKOU);
            else sBody += string.Format("\r\n受入れ不可理由：{0}", BIKOU);
            sBody += sFooter;
            msg.Body = sBody;


            System.Net.Mail.SmtpClient sc = new System.Net.Mail.SmtpClient();
            //SMTPサーバーなどを設定する
            sc.Host = "153.149.193.142";
            sc.Port = 587;
            sc.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
            //メッセージを送信する
            sc.Send(msg);

            //後始末
            msg.Dispose();
            //後始末（.NET Framework 4.0以降）
            sc.Dispose();
        }
        private void SendOKMail(string sAtesaki, string SEQs)
        {
            //MailMessageの作成
            System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();
            //送信者（メールアドレスが"sender@xxx.xxx"、名前が"鈴木"の場合）
            msg.From = new System.Net.Mail.MailAddress("tetra_admin@kyoei-rg.co.jp", "Tetra");
            //宛先（メールアドレスが"recipient@xxx.xxx"、名前が"加藤"の場合）
            msg.To.Add(new System.Net.Mail.MailAddress(sAtesaki, sAtesaki)); // 第2引数"テトラ"
            //件名
            string sSub = string.Empty;
            string sBody = string.Empty;
            string sFooter = "\r\n\r\n\r\n\r\n\r\n"
              + "===================================\r\n"
              + "本メールは送信専用のメールアドレスからお送りしています。\r\n"
              + "返信いただいても、送信元には届きませんのであらかじめご了承下さい。";
            if (usr.iDB == 1) sSub = "■■■テストメール■■■";

            sSub += "<協栄物流>出荷予定配車「受付」連絡";
            msg.Subject = sSub;
            //本文
            sBody = string.Format("予定No.[{0}]について、\r\n", SEQs);
            sBody += "予定を確認しました。\r\n";
            sBody += "別途調整が発生する場合もございますので予めご了承下さい。\r\n";
            sBody += sFooter;
            msg.Body = sBody;


            System.Net.Mail.SmtpClient sc = new System.Net.Mail.SmtpClient();
            //SMTPサーバーなどを設定する
            sc.Host = "153.149.193.142";
            sc.Port = 587;
            sc.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
            //メッセージを送信する
            sc.Send(msg);

            //後始末
            msg.Dispose();
            //後始末（.NET Framework 4.0以降）
            sc.Dispose();
        }

        #region dgvクリック関連
        private void dgv0_MouseDown(object sender, MouseEventArgs e)
        {
            bdgvCellClk = fn.dgvCellClk(sender, e);
        }

        private void dgv0_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            // データ欄以外は何もしない
            if (!bdgvCellClk) return;
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
            //string[] sendText = { "edit", s0 };
            //string[] receiveText = F02_EditOrder.ShowMiniForm(this, sendText);

            //GetData(dgv0, bs0, GetOrder());
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
