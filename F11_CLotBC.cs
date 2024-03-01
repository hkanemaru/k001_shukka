using System;
using System.Windows.Forms;
using iTextSharp.text.pdf;
using iTextSharp.text;
//iTextSharp.text.FontクラスがSystem.Drawing.Fontクラスと
//混在するためiFontという別名を設定
using iFont = iTextSharp.text.Font;
using System.IO;

namespace k001_shukka
{
    public partial class F11_CLotBC : Form
    {
        #region フォーム変数
        private Boolean bClose = true;
        private string[] argVals; // 親フォームから受け取る引数
        public string[] ReturnValue;            // 親フォームに返す戻り値
        private Boolean bPHide = true;  // 親フォームを隠す = True
        //private Boolean bdgvCellClk = false; // dgvでクリックする場合には必須
        DateTime loadTime; // formloadの時間
        //private bool bDirty = false; // 編集が行われたらtrue
        string pLineCD = string.Empty;
        #endregion 

        public F11_CLotBC(params string[] argVals)
        {
            // 親フォームから受け取ったデータをこのインスタンスメンバに格納
            this.argVals = argVals;
            InitializeComponent();

            // ■■■ 親フォームを隠す設定 true => 隠す　default = 隠す。
            //bPHide = false; // 隠さない
            //■■■ 画面の和名
            string sTitle = "BarCode作成";
            #region 画面の状態を設定
            // 画面サイズ変更の禁止
            this.MaximizeBox = false;
            string s = string.Empty;
            if (usr.iDB == 1) s += " TestDB: ";
            s += DateTime.Now.ToString("yy/MM/dd HH:mm");
            s += " " + usr.name;
            // タイトルバー表示設定
            this.Text = string.Format("【{0}】 {1}  {2} {3}"
                , this.Name
                , sTitle
                , DateTime.Now.ToShortDateString()
                , DateTime.Now.ToShortTimeString());
            #endregion
        }

        static public string[] ShowMiniForm(Form frm, params string[] s)
        {
            F11_CLotBC f = new F11_CLotBC(s);
            f.ShowDialog(frm);

            string[] receiveText = f.ReturnValue; // -- ①
            f.Dispose();
            return receiveText;
        }

        private void FRM_Load(object sender, EventArgs e)
        {
            // 開いた元フォームを隠す設定
            if (bPHide) this.Owner.Hide();
            #region EnterKeyで移動
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FRM_KeyDown);
            this.KeyPreview = true;
            #endregion
            loadTime = DateTime.Now;
            fn.showCenter(this);
            SetTag();

            button1.Text = "編集";
            button1.Image = global::k001_shukka.Properties.Resources.unlock;

            textBox5.Text = loadTime.Year.ToString().Substring(2);
            textBox5.Text += loadTime.Month.ToString("00") + "-";

            lblTitle.Text = "バーコードのご案内作成";
            lblTitle.Left = (this.Width - lblTitle.Width) / 2;

            if (argVals[0].Length > 0)
            {
                Db2Frm();
                enableCtrl(false);
            }
            else
            {
                string s1 = argVals[1];
                if (s1.IndexOf("広島") >= 0) s1 = "東洋製罐株式会社　広島工場";
                if (s1.IndexOf("埼玉") >= 0) s1 = "東洋製罐株式会社　埼玉工場";
                if (s1.IndexOf("横浜") >= 0) s1 = "東洋製罐株式会社　横浜工場";
                if (s1.IndexOf("榛名") >= 0) s1 = "サントリープロダクツ株式会社　榛名工場";
                if (s1.IndexOf("白州工場") >= 0) s1 = "サントリープロダクツ株式会社　白州工場";
                textBox3.Text = s1;
                textBox1.Text = argVals[2];
                textBox4.Text = argVals[4];
                textBox2.Text = argVals[3];

                textBox1.Text = loadTime.ToShortDateString();
                button1.Text = "登録";
                button1.Image = k001_shukka.Properties.Resources.decide;
                button1.Image = k001_shukka.Properties.Resources.unlock;
            }

            #region 作業フォルダの作成
            string path = @"c:\tetra";
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            // C:\Label\QRcode.png
            path = @"c:\Label";
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            #endregion
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
            if (this.ReturnValue == null)
            {
                this.ReturnValue = new string[] { textBox5.Text };
            }
            this.Close();
        }
        // 1) btnClose  -> 2) -> 3) -> 2) -> 1) -> ShowMiniForm.f.showdialog
        private void btnClose_Click(object sender, EventArgs e)
        {
            if (this.ReturnValue == null)
            {
                this.ReturnValue = new string[] { "" };
            }
            
            closing();
        }
        // CLOSE処理の最後
        private void FRM_Closed(object sender, FormClosedEventArgs e)
        {
            if (bPHide) this.Owner.Show();
        }
        #endregion

        private void SetTag()
        {
            lblSEQ.Tag = "2SEQ";

            textBox1.Tag = "3DUE_DATE";
            textBox3.Tag = "1DELIVERY";
            textBox4.Tag = "1GRADE";
            textBox5.Tag = "1LOT";
            textBox2.Tag = "2WEIGHT";

            lblREG_PSN.Tag = "1REG_PSN";
            lblREG_DATE.Tag = "3REG_DATE";
            lblUPD_PSN.Tag = "1UPD_PSN";
            lblUPD_DATE.Tag = "3UPD_DATE";
        }

        private void enableCtrl(bool b)
        {
            Control[] all = fn.GetAllControls(this);
            foreach (Control c in all)
            {
                if (c.GetType().Equals(typeof(TextBox)) || c.GetType().Equals(typeof(Label)))
                {
                    c.Enabled = b;
                }
            }
        }

        private void Db2Frm()
        {
            // データ抽出
            mydb.kyDb con = new mydb.kyDb();
            
            string sLot = argVals[0];
            if (argVals[0].Length == 0) sLot = textBox5.Text;
            if (sLot.Length == 0) return;
            con.GetData(sLotGet(sLot),DEF_CON.Constr());
            //if (k001_shukka.ds.Tables[0].Rows[0][22].ToString() == "1") argVals[0] = "del";
            string sColNM = string.Empty;
            #region ロジックの説明
            /* フォーム内の全てのコントロールについて、
            // タグを調べていく
            // タグがある場合、　　（タグの値 = Tableカラム名）
            // テーブルのカラム名を全て拾い、
            // カラム名 = タグ値　となれば
            // コンボボックスの場合 カラム値をSelectedValueと一致させる ==> 数値で登録の場合
            // コンボボックスでDISPMEMBERを登録している場合は他に同じ
            // それ以外は.textに代入*/
            #endregion
            foreach (Control c in this.Controls)
            {  // タグがヌルでないばあい
                if (c.Tag != null)
                {                        // データセットの全てのカラム名を調べて合致すれば値を代入
                    for (int i = 0; i < con.ds.Tables[0].Columns.Count; i++)
                    {
                        sColNM = con.ds.Tables[0].Columns[i].ColumnName;
                        // dsのカラム名をsColNMとしtagの値と一致をみる
                        if (c.Tag.ToString().Substring(1) == sColNM)
                        {
                            c.Text = con.ds.Tables[0].Rows[0][i].ToString();
                            if (c.Tag.ToString().Substring(0, 1) == "3") c.Text = c.Text.Substring(0, 10);
                            if (c.Tag.ToString().Substring(0, 1) == "3") c.Text = DateTime.Parse(c.Text).ToShortDateString();

                        }
                    } // for (int i = 0; i < k001_shukka.ds.Tables[0].Columns.Count; i++)
                } // if (c.Tag != null) 
            } // foreach (Control c in this.Controls)

        }

        private string sLotGet(string sLot)
        {
            string s = "T_CAN_BARCODE";

            return string.Format(
                "SELECT * "
             + " FROM {1}"
             + " WHERE LGC_DEL = '0' AND LOT LIKE '{0}%';"
            , sLot, s);
        }

        //登録ボタン
        private void button1_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            if (btn.Text == "編集")
            {
                enableCtrl(true);
                btn.Text = "登録";
                btn.Image = global::k001_shukka.Properties.Resources.decide;
            }
            else
            {
                if (!chkValidate()) return;
                if (argVals[0].Length == 0) // 新規登録
                {
                    string sCnt = string.Format(
                        "SELECT COUNT(*) FROM T_CAN_BARCODE WHERE LOT = '{0}';"
                        , textBox5.Text);
                    mydb.kyDb conn = new mydb.kyDb();
                    if (conn.iGetCount(sCnt,DEF_CON.Constr()) > 0)
                    {
                        MessageBox.Show("LOTが重複しています。");
                        return;
                    }
                }
                // 登録後再抽出してから表示
                mydb.kyDb con = new mydb.kyDb();
                string sMsg = "内容を新規登録しますか？";
                if (argVals[0].Length > 0) sMsg = "編集内容で更新登録しますか?";
                if (true)
                {
                    string[] Snd = { sMsg, "", "登録の確認" };
                    string[] Rcv = promag_frm.F05_YN.ShowMiniForm(Snd);
                    if (Rcv[0].Length == 0) return;
                }
                sMsg = con.ExecSql(true, DEF_CON.Constr(), crtSql());
                if(sMsg.IndexOf("エラー")>= 0)
                {
                    string[] Snd = { sMsg, "false" };
                    _ = promag_frm.F05_YN.ShowMiniForm(Snd);
                    return;
                }
                else
                {
                    string sgetSEQ = string.Format(
                        "SELECT SEQ FROM T_CAN_BARCODE WHERE LOT = '{0}';"
                        , textBox5.Text);
                    //if (argVals[0].Length == 0) argVals[0] = con.iGetCount(sgetSEQ, DEF_CON.Constr()).ToString();
                    Db2Frm();
                    if (argVals[0].Length == 0)
                    {
                        sgetSEQ = string.Format(
                            "UPDATE t_can_barcode SET SHIP_SEQ = {0} WHERE LOT = '{1}';"
                            , argVals[5], textBox5.Text);
                        con.ExecSql(false, DEF_CON.Constr(), sgetSEQ);
                    }
                    
                    enableCtrl(false);
                    btn.Text = "編集";
                    btn.Image = k001_shukka.Properties.Resources.unlock;
                    this.ReturnValue = new string[] { textBox5.Text };
                }
            }
        }

        private bool chkValidate()
        {
            bool b = true;
            Control[] all = fn.GetAllControls(this);

            string sCol = string.Empty;
            string sVal = string.Empty;
            string sValue = string.Empty;
            foreach (Control c in all)
            {
                if (c.GetType().Equals(typeof(TextBox)))
                {
                    switch (c.Name)
                    {
                        case "textBox5":
                        case "textBox1":
                        case "textBox2":
                        case "textBox3":
                        case "textBox4":
                            if (string.IsNullOrWhiteSpace(c.Text))
                            {
                                b = false;
                                MessageBox.Show("必須項目で入力のない項目があります。", c.Tag.ToString());
                            }
                            break;
                    }
                    switch (c.Name)
                    {
                        case "textBox1":
                            if (c.Text.Length > 0)
                            {
                                if (!fn.IsDatetime(c.Text))
                                {
                                    b = false;
                                    MessageBox.Show("日付形式に誤りがあります。", c.Tag.ToString());
                                }
                            }
                            break;
                        case "textBox5":
                            if (c.Text.Length < 6)
                            {
                                b = false;
                                MessageBox.Show("LOTの桁数が足りません", c.Tag.ToString());
                            }
                            break;
                    }
                    if (c.Name == "textBox2")
                    {
                        if (!fn.IsInt(c.Text))
                        {
                            b = false;
                            MessageBox.Show("数値形式で入力して下さい", c.Tag.ToString());
                        }
                    }
                }
            }
            return b;
        }

        private string crtSql()
        {
            Control[] all = fn.GetAllControls(this);
            #region SQL生成
            string sCol = string.Empty;
            string sInsCol = string.Empty;
            string sInsVal = string.Empty;
            string sValue = string.Empty;
            string sTbl = "T_CAN_BARCODE";

            foreach (Control c in all)
            {
                // Tagのないコントロールは除外
                if (c.Tag != null)
                {
                    #region 除外項目
                    if (c.Tag.ToString() == "2SEQ") continue;
                    if (c.Tag.ToString() == "1UPD_PSN") continue;
                    if (c.Tag.ToString() == "1REG_PSN") continue;
                    if (c.Tag.ToString() == "3REG_DATE") continue;
                    if (c.Tag.ToString() == "3UPD_DATE") continue;
                    #endregion
                    #region 更新登録の場合

                    // 除外
                    string sTmp = string.Empty;
                    sInsCol += ", " + c.Tag.ToString().Substring(1);
                    if (c.Tag.ToString().Substring(0, 1) != "2")
                    {
                        if (string.IsNullOrEmpty(c.Text))
                        {
                            sTmp = string.Format(
                            ", {0} = null"
                            , c.Tag.ToString().Substring(1));
                            sInsVal += ", null";
                        }
                        else
                        {
                            sTmp = string.Format(
                                ", {0} = '{1}'"
                                , c.Tag.ToString().Substring(1), c.Text);
                            sInsVal += string.Format(", '{0}'", c.Text);
                        }
                    }
                    if (c.Tag.ToString().Substring(0, 1) == "2")
                    {
                        if (string.IsNullOrEmpty(c.Text))
                        {
                            sTmp = string.Format(
                            ", {0} = null"
                            , c.Tag.ToString().Substring(1));
                            sInsVal += ", null";
                        }
                        else
                        {
                            sTmp = string.Format(
                            ", {0} = {1}"
                            , c.Tag.ToString().Substring(1), c.Text);
                            sInsVal += string.Format(", '{0}'", c.Text);
                        }
                    }
                    sCol += sTmp;
                    #endregion
                }
            }

            sCol += string.Format(
                ", UPD_PSN = '{0}', UPD_DATE = NOW()"
                , usr.name);
            sInsCol = sInsCol.Substring(1);
            sInsCol += ", UPD_PSN, UPD_DATE, REG_PSN, REG_DATE, LGC_DEL";
            sInsVal = sInsVal.Substring(1);
            sInsVal += string.Format(
                ", '{0}', NOW(), '{0}', NOW(), '0'"
                , usr.name);
            if (argVals[0].Length > 0)
            {
                sValue = string.Format(
                    "UPDATE {2} SET {0} WHERE LOT = '{1}'"
                    , sCol.Substring(1), argVals[0], sTbl);
            }
            else
            {
                sValue = string.Format(
                    "INSERT INTO {2} ({0}) VALUES ({1});"
                    , sInsCol, sInsVal, sTbl);
            }
            sValue = sValue.Replace("\\", "\\\\");

            #endregion
            return sValue;
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

        private void textBox_Enter(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            string sdate = string.Empty;
            string sTitle = "納入先";
            string sTitleSu = "";
            string sSql =
                "SELECT DISTINCT 0, DELIVERY FROM T_CAN_BARCODE WHERE LGC_DEL = '0';";

            string[] sendText = { sTitle, sTitleSu, sSql };

            //// FRMxxxxから送られてきた値を受け取る
            string[] reT = promag_frm.F04List.ShowMiniForm(this, sendText);
            if (reT[0].Length > 0) tb.Text = reT[0];
        }

        private void DateBox_Enter(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            string sdate = string.Empty;
            if (tb.Text.Length > 0)
            {
                DateTime dt;
                // if (int.TryParse(dgv0.Rows[i].Cells[14].Value.ToString(), out iTest))
                if (DateTime.TryParse(tb.Text, out dt)) sdate = dt.ToShortDateString();
            }
            else
            {
                sdate = loadTime.ToShortDateString();
            }
            string[] sendText = { sdate, "", "button3" };

            //// FRMxxxxから送られてきた値を受け取る
            string[] reT = promag_frm.F06_SelDate.ShowMiniForm(this, sendText);
            if (reT[0].Length > 0) tb.Text = reT[0];
            //argVals[2] == "button3"
        }


        private void labelPrint()
        {

            // 印刷するかの確認
            //  -- シリアル番号を生成
            //  -- シリアル番号の登録
            // PDF生成                                  --3
            string sDir = @"C:\APPS\";
            if (!Directory.Exists(sDir)) Directory.CreateDirectory(sDir);

            string TORIHIKISAKI = textBox3.Text;
            string DELIVERY_DATE = DateTime.Parse(textBox1.Text).Month.ToString() + "月";
            DELIVERY_DATE += DateTime.Parse(textBox1.Text).Day.ToString() + "日";
            DELIVERY_DATE += "(" + DateTime.Parse(textBox1.Text).ToString("ddd") + ")";
            string B_HINMEI = textBox4.Text;
            string B_LOT = textBox5.Text;
            int B_WEIGHT = 0;
            if (textBox2.Text.Length > 0) B_WEIGHT = int.Parse(textBox2.Text);
            // string strWeek1 = dt.ToString("ddd");
            // num.ToString("#,0")


            string GetSNM =
                "SELECT"
                 + " st.SIPPING_GRADE"
                 + " FROM kyoei.t_can_barcode cb"
                 + " LEFT JOIN kyoei.t_shipment_inf si ON si.SEQ = cb.SHIP_SEQ AND IFNULL(cb.LOCATION,0) = 0"
                 + " LEFT JOIN kyoei.m_j_shipment   js ON js.SEQ = cb.SHIP_SEQ AND IFNULL(cb.LOCATION,0) = 54"
                 + " LEFT JOIN ("
                 + " SELECT"
                 + " p.SHIP_SEQ"
                 + " ,p.GRADE_AC_SEQ gaSEQ"
                 + " FROM kyoei.t_product p"
                 + " GROUP BY p.SHIP_SEQ"
                 + " UNION"
                 + " SELECT"
                 + " p.SHIP_SEQ"
                 + " ,p.GRADE_AC_SEQ gaSEQ"
                 + " FROM kyoei.t_m_product p"
                 + " GROUP BY p.SHIP_SEQ"
                 + " UNION"
                 + " SELECT"
                 + " p.SHIP_SEQ + 100000000"
                 + " ,p.GRADE_AC_SEQ gaSEQ"
                 + " FROM kyoei.kj_p_product p"
                 + " GROUP BY p.SHIP_SEQ"
                 + " ) p           ON p.SHIP_SEQ = IFNULL(si.SEQ,js.SEQ + 100000000)"
                 + " LEFT JOIN kyoei.m_inspect_std  st    ON st.GRADE_AC_SEQ = si.GA_SEQ"
                 + $" WHERE cb.LOT = '{B_LOT}';";
            mydb.kyDb con = new mydb.kyDb();
            string ShipNM = con.sColVal(GetSNM, DEF_CON.Constr());
            if (ShipNM == "err") ShipNM = "NA-BT7906";



            System.Diagnostics.Process p;

            #region ページ設定
            //iTextSharp.text.Rectangle new_Pagesize = new iTextSharp.text.Rectangle(420, 284);//(横,縦)
            // Document doc = new Document(PageSize.A4);
            //ドキュメントを作成
            Document doc = new Document(PageSize.A4, 70f, 70f, 70f, 70f); //(ページサイズ, 左マージン, 右マージン, 上マージン, 下マージン);
                                                        
            //ファイルの出力先を設定
            string sFileDir = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal).ToString();

            sFileDir += "\\";
            string fileName = B_HINMEI + "_" + TORIHIKISAKI + "_" + B_LOT + "_" + (B_WEIGHT / 1000).ToString() + ".pdf";
            System.Text.StringBuilder sb = new System.Text.StringBuilder(fileName);
            sb.Replace("株式会社", "").Replace("東洋製罐", "").Replace("（株", "").Replace("　", "").Replace(" ", "").Replace("工場", "");
            fileName = sb.ToString();

            fileName.Trim();

            string newFile = @"C:\APPS\barcode.pdf";
            newFile = sFileDir + fileName;

            #endregion

            #region ファイル出力用のストリームを取得
            try
            {
                PdfWriter.GetInstance(doc, new FileStream(newFile, FileMode.Create));
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "エラー");
                MessageBox.Show("PDFファイルを使用中です。閉じてから作業して下さい。");
                return;
            }
            #endregion

            #region print
            try
            {
                #region 本文用のフォント(MS 明朝) 設定  
                Font fnt14 = new Font(BaseFont.CreateFont
                    (@"c:\windows\fonts\msmincho.ttc,1", BaseFont.IDENTITY_H, true), 14, iTextSharp.text.Font.NORMAL);
                Font fnt11 = new Font(BaseFont.CreateFont
                    (@"c:\windows\fonts\msmincho.ttc,0", BaseFont.IDENTITY_H, true), 12, iTextSharp.text.Font.NORMAL);
                Font fnt9ui = new Font(BaseFont.CreateFont
                    (@"c:\windows\fonts\msmincho.ttc,1", BaseFont.IDENTITY_H, true), 9, iTextSharp.text.Font.UNDERLINE | iTextSharp.text.Font.ITALIC);
                Font fnt12 = new Font(BaseFont.CreateFont
                    (@"c:\windows\fonts\msmincho.ttc,1", BaseFont.IDENTITY_H, true), 12, iTextSharp.text.Font.NORMAL);
                #endregion

                //文章の出力を開始します。
                doc.Open();
                #region 文字列の追加
                Paragraph para = new Paragraph(loadTime.ToShortDateString(), fnt11);
                para.Alignment = Element.ALIGN_RIGHT;
                doc.Add(para);
                doc.Add(new Paragraph(" "));

                para = new Paragraph(TORIHIKISAKI + " 御中", fnt11);
                para.Alignment = Element.ALIGN_LEFT;
                doc.Add(para);

                string s = "協栄産業株式会社\r\n小山工場\r\n業務・資材部";
                para = new Paragraph(s, fnt11);
                para.Alignment = Element.ALIGN_RIGHT;
                doc.Add(para);

                doc.Add(new Paragraph(" "));
                doc.Add(new Paragraph(" "));

                s = "バーコードのご案内";
                para = new Paragraph(s, fnt14);
                para.Alignment = Element.ALIGN_CENTER;
                doc.Add(para);

                doc.Add(new Paragraph(" "));

                s = "納入日：　" + DELIVERY_DATE;
                para = new Paragraph(s, fnt11);
                para.Alignment = Element.ALIGN_LEFT;
                doc.Add(para);

                s = "品名　：　" + B_HINMEI;
                para = new Paragraph(s, fnt11);
                para.Alignment = Element.ALIGN_LEFT;
                doc.Add(para);

                s = "ＬＯＴ：　" + B_LOT;
                para = new Paragraph(s, fnt11);
                para.Alignment = Element.ALIGN_LEFT;
                doc.Add(para);

                s = "数量　：　" + B_WEIGHT.ToString("#,0") + "kg";
                para = new Paragraph(s, fnt11);
                para.Alignment = Element.ALIGN_LEFT;
                doc.Add(para);
                #endregion

                doc.Add(new Paragraph(" "));
                doc.Add(new Paragraph(" "));

                ShipNM += "          "; // B_HINMEI
                ShipNM = ShipNM.Substring(0, 10);
                B_LOT += "          ";
                B_LOT = B_LOT.Substring(0, 10);
                string s_WEIGHT = B_WEIGHT.ToString();
                s_WEIGHT += "     ";
                s_WEIGHT = s_WEIGHT.Substring(0, 5);
                s = ShipNM + B_LOT + s_WEIGHT;

                #region ここでバーコードを作成しクリップボードにコピーする
                // 1024x768サイズのImageオブジェクトを作成する
                System.Drawing.Graphics g = this.pic.CreateGraphics();
                pic.Size = new System.Drawing.Size(460, 70);

                //ImageオブジェクトのGraphicsオブジェクトを作成する
                //pic.Size = new System.Drawing.Size((int)((64 + ((float.Parse("8.5") - 8) * 8) + 6) * (g.DpiX / 300) * 12), (int)((4 + 6) * (g.DpiX / 300) * 12));
                ////pic.Size = new System.Drawing.Size(140, 38);
                pic.Image = new System.Drawing.Bitmap(pic.Width, pic.Height);
                g = System.Drawing.Graphics.FromImage(pic.Image);
                g.FillRectangle(new System.Drawing.SolidBrush(System.Drawing.Color.White), 0, 0, pic.Width, pic.Height);

                pic.Refresh();
                float height = 0, width = 0;
                width = 80;
                height = 15;
                g.PageUnit = System.Drawing.GraphicsUnit.Millimeter;

                try
                {
                    Pao.BarCode.IBarCode bar = null;

                    bar = new Pao.BarCode.Code39(g);

                    if (bar != null)
                    {
                        bar.TextWrite = false; // 添え字はなし
                                               //bar.Draw(sLot, 5, 2, width, height);
                        bar.DrawDirect(s, 0, 0, width, height);
                        //bar.DrawDelicate(sLot, 5, 5, width, height);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "バーコード出力エラー");
                    return;
                }

                //g.DrawImage(pic, 5, 5);
                Clipboard.SetDataObject(pic.Image, true);
                //Clipboard.SetDataObject(pic.Image);
                #endregion


                #region バーコード作成
                System.Drawing.Image img1 = System.Windows.Forms.Clipboard.GetImage();
                g.DrawImage(img1, 0, 0, 240, 40);

                iTextSharp.text.Image image1 = iTextSharp.text.Image.GetInstance(img1, BaseColor.WHITE);
                image1.ScalePercent(60f);

                image1.SetAbsolutePosition(70f, 440f);
                doc.Add(image1);
                #endregion

                doc.Add(new Paragraph(" ")); doc.Add(new Paragraph(" "));

                para = new Paragraph(s, fnt11);
                para.Alignment = Element.ALIGN_LEFT;
                doc.Add(para);

                
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("---\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                System.Windows.Forms.MessageBox.Show(ex.Message, "エラー");
            }
            #endregion
            doc.Close();

            //string spath = @"c:\LABETTA\test.pdf";
            //p = System.Diagnostics.Process.Start(@"C:\Program Files\Tracker Software\PDF Viewer\PDFXCview.exe", " /print " + spath);
            p = System.Diagnostics.Process.Start(newFile);
            //p.WaitForExit();              // プロセスの終了を待つ
            //MessageBox.Show("印刷しました。");
            //p.WaitForExit();              // プロセスの終了を待つ
            //int iExitCode = p.ExitCode;   // 終了コード
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (tb.Text.IndexOf("埼玉") > 0) textBox2.Text = "21000";
            else textBox2.Text = "17000";
        }

        private void textBox_L_Enter(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            string sdate = string.Empty;
            string sTitle = "納入先";
            string sTitleSu = "";
            string sSql =
                "SELECT ADD_VALUE, DISP_NM FROM m_code WHERE SECTION_CD = 104;";

            string[] sendText = { sTitle, sTitleSu, sSql };

            //// FRMxxxxから送られてきた値を受け取る
            string[] reT = promag_frm.F04List.ShowMiniForm(this, sendText);
            if (reT[0].Length > 0)
            {
                tb.Text = reT[0];
                pLineCD = reT[1];
            }
        }
        // QR印刷
        private void button2_Click(object sender, EventArgs e)
        {
            // 印刷するかの確認
            //  -- シリアル番号を生成
            //  -- シリアル番号の登録
            // PDF生成                                  --3
            string sDir = @"C:\APPS\";
            if (!Directory.Exists(sDir)) Directory.CreateDirectory(sDir);

            string TORIHIKISAKI = textBox3.Text;
            if (TORIHIKISAKI.IndexOf("榛名") >= 0) TORIHIKISAKI = "サントリープロダクツ株式会社 榛名工場";
            if (TORIHIKISAKI.IndexOf("白州") >= 0) TORIHIKISAKI = "サントリープロダクツ株式会社 白州工場";
            string DELIVERY_DATE = DateTime.Parse(textBox1.Text).Month.ToString() + "月";
            DELIVERY_DATE += DateTime.Parse(textBox1.Text).Day.ToString() + "日";
            DELIVERY_DATE += "(" + DateTime.Parse(textBox1.Text).ToString("ddd") + ")";
            string B_HINMEI = textBox4.Text;
            B_HINMEI = "NABT7906RSNH"; // ===================================== サントリー専用7906の品名　特殊処理 20200929
            string B_LOT = textBox5.Text;
            int B_WEIGHT = 0;
            if (textBox2.Text.Length > 0) B_WEIGHT = int.Parse(textBox2.Text);
            // string strWeek1 = dt.ToString("ddd");
            // num.ToString("#,0")


            bool bStop = false;

            if (textBox6.Text.Length == 0) bStop = true;
            if (textBox7.Text.Length == 0) bStop = true;
            if (bStop)
            {
                string[] Snd = { "QR必須項目が入力されていません。", "false", "QR生成不可" };
                _ = promag_frm.F05_YN.ShowMiniForm(Snd);
                return;
            }

            //System.Diagnostics.Process p;

            #region ページ設定
            //iTextSharp.text.Rectangle new_Pagesize = new iTextSharp.text.Rectangle(420, 284);//(横,縦)
            // Document doc = new Document(PageSize.A4);
            //ドキュメントを作成
            Document doc = new Document(PageSize.A4, 70f, 70f, 70f, 70f); //(ページサイズ, 左マージン, 右マージン, 上マージン, 下マージン);
                                                                          //Document doc = new Document(PageSize.A4);
                                                                          //ファイルの出力先を設定

            string sFileDir = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal).ToString();



            sFileDir += "\\";
            string fileName = "Qr_" + B_HINMEI + "_" + TORIHIKISAKI + "_" + B_LOT + "_" + (B_WEIGHT / 1000).ToString() + ".pdf";
            System.Text.StringBuilder sb = new System.Text.StringBuilder(fileName);
            sb.Replace("株式会社", "").Replace("東洋製罐", "").Replace("（株", "").Replace("　", "").Replace(" ", "").Replace("工場", "");
            fileName = sb.ToString();

            fileName.Trim();

            string newFile = @"C:\APPS\barcode.pdf";
            newFile = sFileDir + fileName;

            #endregion

            #region ファイル出力用のストリームを取得
            try
            {
                PdfWriter.GetInstance(doc, new FileStream(newFile, FileMode.Create));
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "エラー");
                MessageBox.Show("PDFファイルを使用中です。閉じてから作業して下さい。");
                return;
            }
            #endregion

            #region print
            try
            {
                #region 本文用のフォント(MS 明朝) 設定  
                Font fnt14 = new Font(BaseFont.CreateFont
                    (@"c:\windows\fonts\msmincho.ttc,1", BaseFont.IDENTITY_H, true), 14, iTextSharp.text.Font.NORMAL);
                Font fnt11 = new Font(BaseFont.CreateFont
                    (@"c:\windows\fonts\msmincho.ttc,0", BaseFont.IDENTITY_H, true), 12, iTextSharp.text.Font.NORMAL);
                Font fnt9ui = new Font(BaseFont.CreateFont
                    (@"c:\windows\fonts\msmincho.ttc,1", BaseFont.IDENTITY_H, true), 9, iTextSharp.text.Font.UNDERLINE | iTextSharp.text.Font.ITALIC);
                Font fnt12 = new Font(BaseFont.CreateFont
                    (@"c:\windows\fonts\msmincho.ttc,1", BaseFont.IDENTITY_H, true), 12, iTextSharp.text.Font.NORMAL);
                #endregion

                //文章の出力を開始します。
                doc.Open();
                #region 文字列の追加
                Paragraph para = new Paragraph(loadTime.ToShortDateString(), fnt11);
                para.Alignment = Element.ALIGN_RIGHT;
                doc.Add(para);
                doc.Add(new Paragraph(" "));

                para = new Paragraph(TORIHIKISAKI + " 御中", fnt11);
                para.Alignment = Element.ALIGN_LEFT;
                doc.Add(para);

                string s = "協栄産業株式会社\r\n小山工場\r\n業務・資材部";
                para = new Paragraph(s, fnt11);
                para.Alignment = Element.ALIGN_RIGHT;
                doc.Add(para);

                doc.Add(new Paragraph(" "));
                doc.Add(new Paragraph(" "));

                s = "バーコードのご案内";
                para = new Paragraph(s, fnt14);
                para.Alignment = Element.ALIGN_CENTER;
                doc.Add(para);

                doc.Add(new Paragraph(" "));

                s = "納入日：　" + DELIVERY_DATE;
                para = new Paragraph(s, fnt11);
                para.Alignment = Element.ALIGN_LEFT;
                doc.Add(para);

                s = "品名　：　" + B_HINMEI;
                para = new Paragraph(s, fnt11);
                para.Alignment = Element.ALIGN_LEFT;
                doc.Add(para);

                s = "ＬＯＴ：　" + B_LOT;
                para = new Paragraph(s, fnt11);
                para.Alignment = Element.ALIGN_LEFT;
                doc.Add(para);

                s = "数量　：　" + B_WEIGHT.ToString("#,0") + "kg";
                para = new Paragraph(s, fnt11);
                para.Alignment = Element.ALIGN_LEFT;
                doc.Add(para);
                #endregion

                doc.Add(new Paragraph(" "));
                doc.Add(new Paragraph(" "));
                // 
                B_HINMEI += "          ";  // Qrの時NABT7906RSNH が10文字を超えるので変更 >> 20200929
                B_HINMEI = B_HINMEI.Substring(0, 13);
                B_LOT += "          ";
                B_LOT = B_LOT.Substring(0, 10);
                string s_WEIGHT = B_WEIGHT.ToString();
                s_WEIGHT += "     ";
                s_WEIGHT = s_WEIGHT.Substring(0, 5);
                s = B_HINMEI + B_LOT + s_WEIGHT;


                #region バーコード追加
                //string fname = @"C:\Label\code128.png";
                string sFACTRY_CD = textBox5.Text.Substring(textBox5.Text.Length - 1);
                string sSeizoDATE = textBox7.Text.Substring(0, 4);
                sSeizoDATE += textBox7.Text.Substring(5, 2);
                sSeizoDATE += textBox7.Text.Substring(8, 2);
                string sFACTRY = string.Empty;
                if (sFACTRY_CD == "O") sFACTRY = "ＯＹＡＭＡ";
                else if (sFACTRY_CD == "M") sFACTRY = "ＭＲＦ";
                sFACTRY += "＿＿＿＿＿＿＿＿＿＿＿＿";
                sFACTRY = sFACTRY.Substring(0, 10);
                pLineCD += "              ";
                pLineCD = pLineCD.Substring(0, 10);

                string sGrd = "NABT7906RSNH"; // 20200928
                sGrd += "                            ";
                sGrd = sGrd.Substring(0, 20);

                string sWeight = textBox2.Text;
                sWeight += "               ";
                sWeight = sWeight.Substring(0, 11);

                string sSeizoRenBan = textBox5.Text;
                sSeizoRenBan += "                  ";
                sSeizoRenBan = sSeizoRenBan.Substring(0, 12);

                string sGrd2 = "NABT7906RSNH";
                sGrd2 += "                        ";
                sGrd2 = sGrd2.Substring(0, 20);

                string sCRD = DateTime.Now.ToString("yyyyMMdd");
                string sTime = DateTime.Now.ToString("HHmmss");

                string sqr = "A77444       ";       // 取引先コード
                sqr += "ＫＹＯＥＩ＿＿＿＿＿";        // 取引先名称

                sqr += string.Format("{0}    ", sFACTRY_CD);     // 取引先工場コード

                sqr += sFACTRY;        // 取引先工場名
                sqr += pLineCD;        // 取引先ラインコード

                sqr += sGrd;      // 包材品目＋細区分
                sqr += sWeight;       // 数量
                sqr += "000000";        // ケース入数・パレット積数
                sqr += "000";       // パレット積函数
                sqr += "kg";        // 梱包単位
                sqr += sSeizoDATE;      // 製造年月日
                sqr += "000000";        // 製造時分秒
                sqr += sSeizoRenBan;      // 製造連番 ■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■
                sqr += "1";     // メーカコード区分
                sqr += "0";     // 汎用品・特注品区分
                sqr += "0";     // 再検査実施有無
                sqr += "0";          // 端数区分
                sqr += sGrd2;        // 取引先品目コード
                sqr += "               ";       // 受注番号
                sqr += "               ";       // 管理番号
                sqr += "ＸＸＸＸＸＸＸＸＸＸＸＸＸＸＸＸＸＸＸＸＸＸＸＸＸＸＸＸＸＸＸＸＸＸＸＸＸＸＸＸ";      // 特記事項
                sqr += "     ";     // 工場コード
                sqr += "                         ";     // 共通予備エリア
                sqr += "     ";     // 端末ＩＤ
                sqr += sCRD;      // 作成年月日
                sqr += sTime;        // 作成時刻（時分秒）
                sqr += "                                                                                                    ";		// メーカ自由エリア

                string qrnm = @"C:\Label\QRcode.png";
                try
                {
                    //clsBC.Save128Image(sLot, fname
                    //    , System.Drawing.Imaging.ImageFormat.Png);
                    clsBC.SaveQRImage(sqr, qrnm
                        , System.Drawing.Imaging.ImageFormat.Png);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "バーコード出力エラー");
                    return;
                }

                iTextSharp.text.Image imgQr
                    = iTextSharp.text.Image.GetInstance(new Uri(qrnm));
                float pect = 30f;
                imgQr.ScalePercent(pect);
                imgQr.SetAbsolutePosition(220f, 200f);
                doc.Add(imgQr);

                //iTextSharp.text.Image imgBc128
                //    = iTextSharp.text.Image.GetInstance(new Uri(fname));
                //pect = 18f;
                //imgBc128.ScalePercent(pect);
                //imgBc128.SetAbsolutePosition(18f, 12f);
                //doc.Add(imgBc128);
                #endregion

                doc.Add(new Paragraph(" ")); doc.Add(new Paragraph(" "));
                para = new Paragraph(s, fnt11);
                para.Alignment = Element.ALIGN_LEFT;
                doc.Add(para);

                #region sqlで搬入データを作成

                #endregion
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("---\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                System.Windows.Forms.MessageBox.Show(ex.Message, "エラー");
            }
            #endregion
            doc.Close();


            _ = System.Diagnostics.Process.Start(newFile);

            //p.WaitForExit();              // プロセスの終了を待つ
            //int iExitCode = p.ExitCode;   // 終了コード
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(textBox5.Text.Length > 0)
            {
                mydb.kyDb con = new mydb.kyDb();
                string s = string.Format(
                      "SELECT COUNT(*) FROM t_can_barcode "
                    + "WHERE LOT = '{0}'"
                    ,textBox5.Text);
                if (con.iGetCount(s, DEF_CON.Constr()) > 0) labelPrint();
                else
                {
                    string[] Snd = { "コンテナ情報を登録してから印刷して下さい。", "false" };
                    _ = promag_frm.F05_YN.ShowMiniForm(Snd);
                    return;
                }
            }
            else
            {
                string[] Snd = { "Lotを登録して下さい。", "false" };
                _ = promag_frm.F05_YN.ShowMiniForm(Snd);
            }
        }
    }
}
