using mydb;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace k001_shukka
{
    public partial class F02_EditOrder : Form
    {
        #region フォーム変数
        private bool bClose = true;
        private string[] argVals; // 親フォームから受け取る引数
        public string[] ReturnValue;            // 親フォームに返す戻り値
        private bool bPHide = true;  // 親フォームを隠す = True
        DateTime loadTime; // formloadの時間
        //ToolTip ToolTip1;


        private string sFLG = string.Empty; //DEL_FLG を格納
        private bool bDirty = false; // 編集が行われたらtrue
        private string sNow = string.Empty; // 更新日時
        private string[] preText;
        private bool bdgvCellClk = false; // dgvでクリックする場合には必須
        #endregion 

        public F02_EditOrder(params string[] argVals)
        {
            // 親フォームから受け取ったデータをこのインスタンスメンバに格納
            this.argVals = argVals;
            InitializeComponent();

            // ■■■ 親フォームを隠す設定 true => 隠す　default = 隠す。
            bPHide = false; // 隠さない
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
            this.Text = string.Format("【{0}】 {1}  {2} {3}"
                , this.Name
                , sTitle
                , DateTime.Now.ToShortDateString()
                , DateTime.Now.ToShortTimeString());
            #endregion
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
            if (bPHide) this.Owner.Hide();
            loadTime = DateTime.Now;
            lblRCaption.Left = this.Width - lblRCaption.Width - 20;
            lblRCaption.Top = lblCaption.Top;

            this.KeyPreview = true;
            setIniValue();
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
            if (bDirty) s = "1";
            this.ReturnValue = new string[] { "戻り値です", s };
            closing();
        }
        // CLOSE処理の最後
        private void FRM_Closed(object sender, FormClosedEventArgs e)
        {
            if (bPHide) this.Owner.Show();
        }
        #endregion

        private void setIniValue()
        {
            //uDTP.Value = "";

        }

        private void setTags()
        {
            //checkBox1.Tag = "1MOVE_FLG";
            //textBox7.Tag = "2WEIGHT";
            //textBox9.Tag = "1BIKOU";
            //textBox10.Tag = "1C_REASON";
            //label18.Tag = "2SEQ";
        }

        private void Db2Frm(string sSeq)
        {
            #region string SelSql
            string SelSql = string.Format(
                        "SELECT "
                        + " ai.SEQ"   //0
                        + ", ai.SUPPLIER"   //1
                        + ", ai.DESTINATION"   //2
                        + ", ai.SCHEDULED_DATE"   //3
                        + ", ai.MATERIAL_KBN"   //4
                        + ", ai.GOODS_NAME"   //5
                        + ", ai.GOODS_NAME2"   //6
                        + ", ai.GRADE"   //7
                        + ", ai.WEIGHT"   //8
                        + ", ai.TRANSPORTER"   //9
                        + ", ai.C_REASON"   //10
                        + ", ai.BIKOU"   //11
                        + ", ai.DEL_FLG"   //12
                        + ", ai.UPD_PSN"   //13
                        + ", ai.UPD_ID"   //14
                        + ", ai.UPD_DATE"   //15
                                            //+ ", ARRIVE_CHK"   //16
                                            //+ ", ARRIVE_CHK_TIME"   //17
                                            //+ ", ARRIVE_CHK_PSN"   //18
                        + ", ''"   //16
                        + ", ''"   //17
                        + ", ''"   //18
                        + ", ai.DELI_CHK"   //19
                        + ", ai.DELI_CHK_TIME"   //20
                        + ", ai.DELI_CHK_PSN"   //21
                        + ", ai.LGC_DEL" //22
                        + ", ai.RECEIPT" //    23
                        + ", CONCAT(w.SEI,w.MEI)" //24
                        + ", ai.RECEIPT_DATE" //25
                        + ", ai.DISPATCH" // 26
                        + ", ai.DISPATCH_DATE" // 27
                        + " FROM M_ARRIVAL_INF ai"
                        + " LEFT JOIN m_worker w ON ai.RECEIPT_ID = w.WKER_ID AND w.LGC_DEL = '0'"
                        + " WHERE ai.SEQ = {0}"
                        , sSeq);
            #endregion
            // データ抽出
            kyDb jpt = new kyDb();

            jpt.GetData(SelSql, DEF_CON.Constr());
            if (jpt.ds.Tables[0].Rows[0][22].ToString() == "1") argVals[0] = "del";
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
                    for (int i = 0; i < jpt.ds.Tables[0].Columns.Count; i++)
                    {
                        sColNM = jpt.ds.Tables[0].Columns[i].ColumnName;
                        if (sColNM == "DEL_FLG") sFLG = jpt.ds.Tables[0].Rows[0][i].ToString();
                        // dsのカラム名をsColNMとしtagの値と一致をみる
                        if (c.Tag.ToString().Substring(1) == sColNM)
                        {
                            #region comboBoxのtextを登録している場合
                            if (c.GetType().Equals(typeof(DateTimePicker)))
                            {
                                if (jpt.ds.Tables[0].Rows[0][i] is DBNull)
                                {
                                    //((DateTimePicker)c).Value = null;
                                }
                                else ((DateTimePicker)c).Value = DateTime.Parse(jpt.ds.Tables[0].Rows[0][i].ToString());
                            }
                            else  //(c.GetType().Equals(typeof(TextBox)) || c.GetType().Equals(typeof(yomiTextBox)) || c.GetType().Equals(typeof(Label)))
                            {
                                if (c.GetType().Equals(typeof(CheckBox)))
                                {
                                    if (jpt.ds.Tables[0].Rows[0][i].ToString() == "1") ((CheckBox)c).Checked = true;
                                    else ((CheckBox)c).Checked = false;
                                }
                                else
                                    c.Text = jpt.ds.Tables[0].Rows[0][i].ToString();
                            }
                            #endregion
                        }
                    } // for (int i = 0; i < jpt.ds.Tables[0].Columns.Count; i++)
                } // if (c.Tag != null) 
            } // foreach (Control c in this.Controls)

        }

        private bool preRegChk()
        {

            kyDb con = new kyDb();
            // ■1■更新時間のチェック  編集登録の時
            #region 0. 更新時間を比較し、フォームを開いたより後に更新がかかっていれば更新しない
            if (argVals[0] == "edit")
            {
                if (fn.ErrUpdTime(SqlStr("time"), loadTime))
                {
                    MessageBox.Show("編集中に登録記録が変更されています。開きなおして編集してください。");
                    return false;
                }
            }
            #endregion

            // ■2■二重登録チェック -- 新規の時


            // ■3■　各項目のチェック
            #region 登録内容整合性チェック
            // 全体の エラー無し用 flg エラーありは false
            bool bOK = true;
            // 登録フォームの全項目を foreach メソッドでそれぞれ確認
            Control[] all = fn.GetAllControls(this);
            foreach (Control c in all)
            {
                #region 必須項目チェック
                switch (c.Name)
                {
                    case "comboBox1":
                    case "comboBox2":
                    case "comboBox3":
                    case "comboBox4":
                    case "comboBox5":
                    case "textBox7":
                    case "uDTP":
                        if (c.Text.Length == 0)
                        {
                            MessageBox.Show("必須項目を入力しないと登録出来ません。", "必須項目不足");
                            bOK = false;
                        }
                        break;
                }
                #endregion
                // 空白入力チェックは今回除外
                #region 数値入力チェック
                if (c.Name == "textBox7")
                {
                    decimal d;
                    if (!decimal.TryParse(c.Text, out d))
                    {
                        MessageBox.Show("重量に数値が入力されていません。", "書式エラー");
                        bOK = false;
                    }
                }
                #endregion
            }
            if (!bOK) return false;
            #endregion

            // ■4■登録の確認
            string sMessage = string.Empty;
            if (argVals[0] == "new") sMessage = "入荷情報を新規登録しますか？";
            else
            {
                //if (textBox10.Text.Length == 0)
                //{
                //    MessageBox.Show("情報を変更する際は変更理由を記入してください。");
                //    return false;
                //}
                //sMessage = "この入荷情報を変更しますか。";
            }


            string[] sSET = { sMessage, "" };
            string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSET);
            if (sRcv[0].Length == 0) return false;
            else return true;
        }

        private string SqlStr(string s)
        {
            string str = string.Empty;
            string sw = string.Empty;
            //if (textBox7.Text.Length == 0) sw = "null";
            //else sw = textBox7.Text;
            //switch (s)
            //{
            //    case "time":
            //        str = string.Format(
            //            "SELECT UPD_DATE FROM M_ARRIVAL_INF WHERE LGC_DEL = '0' AND SEQ = {0}"
            //            , label18.Text);
            //        break;
            //    case "dbl":
            //        break;
            //}
            return str;
        }

        private string crtSql()
        {
            Control[] all = fn.GetAllControls(this);
            #region SQL生成
            string sCol = string.Empty;
            string sVal = string.Empty;
            string sValue = string.Empty;
            foreach (Control c in all)
            {
                // Tagのないコントロールは除外
                if (c.Tag != null)
                {
                    // 除外項目
                    if (c.Tag.ToString() == "1MOVE_FLG") continue;
                    #region 新規登録の場合
                    if (argVals[0] == "new")
                    {
                        if (c.Text.Length == 0) continue;
                        if (c.Tag.ToString().Substring(1) == "SEQ") continue;
                        if (c.Tag.ToString().Substring(0, 1) != "2")
                        {
                            sCol += ", " + c.Tag.ToString().Substring(1);
                            if (String.IsNullOrEmpty(c.Text)) sVal += ", null";
                            else sVal += ", '" + c.Text + "'";
                        }
                        if (c.Tag.ToString().Substring(0, 1) == "2")
                        {
                            sCol += ", " + c.Tag.ToString().Substring(1);
                            if (c.GetType().Equals(typeof(ComboBox)))
                            {
                                if (String.IsNullOrEmpty(c.Text)) sVal += ", null";
                                else sVal += ", " + ((ComboBox)c).SelectedValue.ToString();
                            }
                            else
                            {
                                if (String.IsNullOrEmpty(c.Text)) sVal += ", null";
                                else sVal += ", " + c.Text;
                            }
                        }
                    }
                    #endregion
                    #region 更新登録の場合
                    if (argVals[0] == "edit")
                    {
                        // 除外
                        //if (c.Tag.ToString() == "2GRAD_AC_SEQ") continue;
                        string sTmp = string.Empty;
                        if (c.Tag.ToString().Substring(0, 1) != "2")
                        {
                            if (String.IsNullOrEmpty(c.Text))
                            {
                                sTmp = string.Format(
                                ", {0} = null"
                                , c.Tag.ToString().Substring(1));
                            }
                            else
                            {
                                sTmp = string.Format(
                                ", {0} = '{1}'"
                                , c.Tag.ToString().Substring(1), c.Text);
                            }
                        }
                        if (c.Tag.ToString().Substring(0, 1) == "2")
                        {
                            if (c.GetType().Equals(typeof(ComboBox)))
                            {
                                if (String.IsNullOrEmpty(c.Text))
                                {
                                    sTmp = string.Format(
                                            ", {0} = null"
                                            , c.Tag.ToString().Substring(1));
                                }
                                else
                                {
                                    sTmp = string.Format(
                                                        ", {0} = {1}"
                                                        , c.Tag.ToString().Substring(1)
                                                        , ((ComboBox)c).SelectedValue.ToString());
                                }
                            }
                            else
                            {
                                if (String.IsNullOrEmpty(c.Text))
                                {
                                    sTmp = string.Format(
                                    ", {0} = null"
                                    , c.Tag.ToString().Substring(1));
                                }
                                else
                                {
                                    sTmp = string.Format(
                                    ", {0} = {1}"
                                    , c.Tag.ToString().Substring(1), c.Text);
                                }
                            }
                        }
                        sCol += sTmp;
                    }
                    #endregion
                }
            }
            #region 更新した箇所が基本情報かどうかを判定する。 B_INF_UPD_DATE
            // 前段で基本情報が変更した場合当日の日付をセットする
            // preText 納入先　入荷予定日　原料区分　品名　グレード　重量
            // 特に納入先、入荷予定日、グレードが変更した場合、同一グレードの当日の
            // 入荷がゼロになるので、その場合は記録を残す。(FLG = bRetZero)-->> INSERT SELECT
            string sUpddate = string.Empty;
            bool bRetZero = false;

            //if (preText[5] != textBox7.Text) sUpddate = DateTime.Today.ToShortDateString();
            if (sUpddate.Length > 0) sUpddate = string.Format("'{0}'", sUpddate);
            else sUpddate = "NULL";
            #endregion
            string s = string.Empty; // MOVE_FLGの値
            //if (checkBox1.Checked) s = "1";
            //else s = "0";
            sNow = DateTime.Now.ToString();
            #region 新規登録の場合
            if (argVals[0] == "new")
            {
                sCol += ", UPD_PSN, UPD_ID, UPD_DATE, LGC_DEL, DEL_FLG, MOVE_FLG";
                sCol += ", REG_PSN, REG_ID, REG_DATE, B_INF_UPD_DATE";
                sVal += string.Format(", '{0}', '{1}', '{4}', '0', '1', '{2}', '{0}', '{1}', '{4}', {3}"
                    , usr.name, usr.id, s, sUpddate, sNow);

                sValue = string.Format(
                    "INSERT INTO M_ARRIVAL_INF ({0}) VALUES ({1});"
                    , sCol.Substring(1), sVal.Substring(1));
            }
            #endregion
            #region 更新登録の場合
            if (argVals[0] == "edit")
            {
                string sColumn = string.Empty;
                if (sUpddate == "NULL")
                {
                    sUpddate = "";
                }
                else
                {
                    sColumn = ", B_INF_UPD_DATE = ";
                }
                sCol += string.Format(
                    ", RECEIPT = NULL, DISPATCH = NULL, UPD_PSN = '{0}', UPD_ID = '{1}', UPD_DATE = '{4}', MOVE_FLG = '{2}'{5}{3}"
                    , usr.name, usr.id, s, sUpddate, sNow, sColumn);

                sValue = string.Format(
                    "UPDATE M_ARRIVAL_INF SET {0} WHERE SEQ = {1};"
                    , sCol.Substring(1), argVals[1]);
            }
            #endregion
            sValue = sValue.Replace("\\", "\\\\");

            string sInsDbl = string.Empty;
            #region 当日の入荷が結果0になる場合
            //sInsDbl = string.Format(
            //    "INSERT INTO m_arrival_inf"
            // + " SELECT NULL AS SEQ"
            // + ",SUPPLIER"
            // + ",DESTINATION"
            // + ",MANUFACTURER"
            // + ",SCHEDULED_DATE"
            // + ",MATERIAL_KBN"
            // + ",GOODS_NAME"
            // + ",GOODS_NAME2"
            // + ",GRADE"
            // + ",0 AS WEIGHT"
            // + ",TRANSPORTER"
            // + ",'{3}' AS C_REASON"
            // + ",BIKOU"
            // + ",DEL_FLG"
            // //+ ",UPD_FLG"
            // //+ ",ARRIVE_CHK"
            // //+ ",ARRIVE_CHK_TIME"
            // //+ ",ARRIVE_CHK_PSN"
            // + ",DELI_CHK"
            // + ",DELI_CHK_TIME"
            // + ",DELI_CHK_PSN"
            // + ", RECEIPT"
            // + ", RECEIPT_DATE"
            // + ", RECEIPT_ID"
            // + ", DISPATCH"
            // + ", DISPATCH_DATE"
            // + ", DISPATCH_ID"
            // + ", '{1}' AS UPD_PSN"
            // + ", '{2}' AS UPD_ID"
            // + ", NOW() AS UPD_DATE"
            // + ",REG_PSN"
            // + ",REG_ID"
            // + ",REG_DATE"
            // + ",MOVE_FLG"
            // + ", CURDATE() AS B_INF_UPD_DATE"
            // + ",LGC_DEL"
            // + " FROM m_arrival_inf WHERE SEQ = {0};"
            //    , label18.Text, usr.name, usr.id, textBox10.Text);
            if (bRetZero && argVals[0] == "edit")
            {
                sValue = sInsDbl + sValue;
            }
            #endregion

            #endregion
            return sValue;
        }

        private void enableCtrl(string sMode, bool b)
        {
            Control[] all = fn.GetAllControls(this);
            foreach (Control c in all)
            {
                if (c.GetType().Equals(typeof(TextBox)) || c.GetType().Equals(typeof(ComboBox))
                    || c.GetType().Equals(typeof(Button)) || c.GetType().Equals(typeof(DateTimePicker))
                    || c.GetType().Equals(typeof(CheckBox)))
                {
                    if (c.Name != "btnClose") c.Enabled = b;
                }
                // 新規に開いたとき
                //    if (sMode == "new")
                //    {
                //        button2.Enabled = true;


                //        button1.Enabled = false;
                //        button14.Enabled = false;
                //    }
                //    if (sMode == "edit") button1.Enabled = true;
                //    if (sMode == "reg")
                //    {
                //        button1.Enabled = true;


                //    }
                //    if (sMode == "copy" || sMode == "change")
                //    {
                //        button1.Enabled = false;
                //        textBox10.Text = "";
                //    }
            }
            //chk1_Checked(checkBox1);
        }

        private void FRM302_KeyDown(object sender, KeyEventArgs e)
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
        //登録ボタン
        private void button2_Click(object sender, EventArgs e)
        {

        }
        // 編集
        private void button1_Click(object sender, EventArgs e)
        {
            enableCtrl("change", true);
        }
        // 取消ボタン
        private void button14_Click(object sender, EventArgs e)
        {

        }
        private void chk1_Checked(CheckBox chk)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            //chk1_Checked(checkBox1);
        }
    }
}
