using System;
using System.Windows.Forms;

namespace k001_shukka
{
    public partial class F12_SHIP_SETTING : Form
    {
        #region フォーム変数
        private Boolean bClose = true;
        private string[] argVals; // 親フォームから受け取る引数
        public string[] ReturnValue;            // 親フォームに返す戻り値
        private Boolean bPHide = true;  // 親フォームを隠す = True
        //private Boolean bdgvCellClk = false; // dgvでクリックする場合には必須
        DateTime loadTime; // formloadの時間
        //private bool bDirty = false; // 編集が行われたらtrue
        //private string sRegID;
        ToolTip ToolTip1;
        #endregion 

        public F12_SHIP_SETTING(params string[] argVals)
        {
            // 親フォームから受け取ったデータをこのインスタンスメンバに格納
            this.argVals = argVals;
            InitializeComponent();

            // ■■■ 親フォームを隠す設定 true => 隠す　default = 隠す。
            //bPHide = false; // 隠さない
            //■■■ 画面の和名
            string sTitle = "出荷票設定";
            #region 画面の状態を設定
            // 画面サイズ変更の禁止
            this.MaximizeBox = false;
            lblDT.Text = fn.frmRTxt();
            lblTitle.Text = fn.frmLTxt(sTitle);
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
            F12_SHIP_SETTING f = new F12_SHIP_SETTING(s);
            f.ShowDialog(frm);

            string[] receiveText = f.ReturnValue; // -- ①
            f.Dispose();
            return receiveText;
        }

        private void FRM_Load(object sender, EventArgs e)
        {
            SetTooltip();
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

            Db2Frm();
            enableCtrl(false);
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
            ToolTip1.SetToolTip(button1, "登録");
        }

        private void SetTag()
        {
            lblSEQ.Tag = "2SEQ";
            textBox1.Tag = "3PRODUCT_DATE";
            textBox10.Tag = "3INPUT_DATE";
            lblUPD_PSN.Tag = "1UPD_ID";
            lblUPD_DATE.Tag = "3UPD_DATE";
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
            //if (this.ReturnValue == null)
            //{
            //    this.ReturnValue = new string[] { "戻り値です" };
            //}
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

        private void enableCtrl(bool b)
        {
            Control[] all = fn.GetAllControls(this);
            foreach (Control c in all)
            {
                if (c.GetType().Equals(typeof(TextBox)) || c.GetType().Equals(typeof(Label))
                    || c.GetType().Equals(typeof(CheckBox)))
                {
                    c.Enabled = b;
                }
            }
        }

        private void Db2Frm()
        {
            label8.Text = argVals[0];
            label7.Text = argVals[3];
            label3.Text = argVals[4];
            // データ抽出
            mydb.kyDb con = new mydb.kyDb();
            con.GetData(sLotGet(), DEF_CON.Constr());
            if(con.ds.Tables[0].Rows.Count > 0)
            {
                textBox1.Text = con.ds.Tables[0].Rows[0][1].ToString();
                textBox10.Text = con.ds.Tables[0].Rows[0][2].ToString();
                lblSEQ.Text = con.ds.Tables[0].Rows[0][0].ToString();
                if (con.ds.Tables[0].Rows[0][3].ToString() == "1") checkBox1.Checked = true;
                if (con.ds.Tables[0].Rows[0][6].ToString() == "1") checkBox2.Checked = true;
                lblUPD_PSN.Text = con.ds.Tables[0].Rows[0][4].ToString();
                lblUPD_PSN.Text = fn.sStaffNAME(lblUPD_PSN.Text);
                lblUPD_DATE.Text = con.ds.Tables[0].Rows[0][5].ToString();
            }
        }

        private string sLotGet()
        {
            return string.Format(
                 "SELECT"
                 + " sn.SEQ"    //0
                 + " ,sn.SHIP_NAME"    //1
                 + " ,sn.DESTINATION"    //2
                 + " ,sn.DEST_FLG"    //3
                 + " ,sn.UPD_ID"    //4
                 + " ,DATE_FORMAT(sn.UPD_DATE, '%Y/%m/%d %H:%i') UPD_DATE"    //5
                 + " ,ATBL "        //6
                 + " FROM kyoei.m_shipment_name sn"
                 + " WHERE "
                 + " sn.GA_SEQ = {0}"
                 + " AND sn.TOKCD = '{1}'"
                 + " AND sn.NHSCD = '{2}'"
                 + ";"
                , argVals[0],argVals[1], argVals[2]);
        }

        //登録ボタン
        private void button1_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            //k001_shukka.Properties.Resources._lock　k001_shukka.Properties.Resources.unlock
            if (btn.Text == "編集")
            {
                enableCtrl(true);
                btn.Text = "登録";
                btn.Image = global::k001_shukka.Properties.Resources.unlock;
            }
            else
            {
                // 登録後再抽出してから表示
                mydb.kyDb con = new mydb.kyDb();
                string sMsg = "内容を新規登録しますか？";
                if (lblSEQ.Text.Length > 0) sMsg = "編集内容で更新登録しますか?";
                if (true)
                {
                    string[] Snd = { sMsg, "", "登録の確認" };
                    string[] Rcv = promag_frm.F05_YN.ShowMiniForm(Snd);
                    if (Rcv[0].Length == 0) return;
                }
                sMsg = con.ExecSql(true, DEF_CON.Constr(), crtSql());
                if (sMsg.IndexOf("エラー") >= 0)
                {
                    string[] Snd = { sMsg, "false", "登録失敗" };
                    _ = promag_frm.F05_YN.ShowMiniForm(Snd);
                    return;
                }
                else this.ReturnValue = new string[] { "OK" };
                enableCtrl(false);
                btn.Text = "編集";
                btn.Image = global::k001_shukka.Properties.Resources.unlock;
            }
        }


        private string crtSql()
        {
            
            #region SQL生成
            string sValue;
            string sHIN = "NULL";
            if (textBox1.Text.Length > 0) sHIN = string.Format("'{0}'", textBox1.Text);
            string sDest = "NULL";
            if (textBox10.Text.Length > 0) sDest = string.Format("'{0}'", textBox10.Text);
            string sALot = "NULL";
            string sATbl = "NULL";
            if (checkBox1.Checked) sALot = "1";
            if (checkBox2.Checked) sATbl = "1";
            if (lblSEQ.Text.Length > 0)
            {
                sValue = string.Format(
                    "UPDATE m_shipment_name SET GA_SEQ = {0}, SHIP_NAME = {1}, DESTINATION = {2}, DEST_FLG = {3}"
                    + ", UPD_ID = '{5}', ATBL = {6}, UPD_DATE = NOW() WHERE SEQ = {4};"
                    , argVals[0], sHIN, sDest, sALot, lblSEQ.Text, usr.id, sATbl);
            }
            else
            {
                sValue = string.Format(
                    "INSERT INTO m_shipment_name ("
                    + "GA_SEQ, SHIP_NAME, TOKCD, NHSCD, DESTINATION, DEST_FLG, UPD_ID, UPD_DATE, REG_DATE, LGC_DEL, ATBL"
                    + ") VALUES ("
                    +"{0}, {1}, '{2}', '{3}', {4}, {5}, '{6}', NOW(), NOW(), '0', {7});"
                    , argVals[0], sHIN,argVals[1], argVals[2], sDest, sALot, usr.id, sATbl);
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

        private void tB_Enter(object sender, EventArgs e)
        {
            
        }
    }
}
