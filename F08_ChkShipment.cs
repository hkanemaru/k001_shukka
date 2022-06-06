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
    public partial class F08_ChkShipment : Form
    {
        #region フォーム変数
        private bool bClose = true;
        private string[] argVals;    // 親フォームから受け取る引数
        public string[] ReturnValue; // 親フォームに返す戻り値
        private bool bPHide = true;  // 親フォームを隠す = True
        ToolTip ToolTip1;
        #endregion
        public F08_ChkShipment(params string[] argVals)
        {
            // 親フォームから受け取ったデータをこのインスタンスメンバに格納
            this.argVals = argVals;
            InitializeComponent();

            // ■■■ 親フォームを隠す設定 true => 隠す　default = 隠す。
            //bPHide = false; // 隠さない
            //■■■ 画面の和名
            string sTitle = "確認登録";
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
            F08_ChkShipment f = new F08_ChkShipment(s);
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
            SetTooltip();
            // argVals[0] [1] [2] //SEQ,受注番号,STATUS,LINO
            if (argVals[2] == "7") // 受領書確認登録
            {
                label1.Text = "届いた受領書の受注番号を登録して下さい。";
                label2.Text = "受注番号";
            }
            else // 売上番号登録
            {
                label1.Text = "売上(IVOICE)番号を入力して下さい。";
                label2.Text = "売上番号";
            }
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
            //ToolTip1.SetToolTip(button2, "日付変更");
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059:値の不必要な代入", Justification = "<保留中>")]
        private void button1_Click(object sender, EventArgs e)
        {
            string s = textBox5.Text;
            string sUpd = string.Empty;
            
            if (s.Length == 0)
            {
                string[] sSed = { "番号を入力して下さい", "false" };
                string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSed);
                return;
            }
            if (System.Text.RegularExpressions.Regex.IsMatch(s, @"[^0-9]"))
            {
                string[] sSed = { "番号は数字のみです。", "false" };
                string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSed);
                return;
            }
            mydb.kyDb con = new mydb.kyDb();
            string sUpdInf = string.Format(
                "UPDATE t_shipment_inf SET UPD_DATE = NOW(), UPD_ID = '{0}' WHERE SEQ = {1};"
                ,usr.id, argVals[0]);
            if (argVals[2] == "7") // 受領書確認登録
            {
                s = "00000000" + s;
                s = s.Substring(s.Length - 8);
                // DN_CHK_DATE   
                if (s != argVals[1])
                {
                    string[] sSed = { "受注番号が一致しません。", "false" };
                    string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSed);
                    return;
                }
                sUpd = string.Format(
                    "UPDATE t_shipment_inf SET DN_CHK_DATE = NOW() WHERE SEQ = {0};"
                    , argVals[0]);
            }
            else // 売上番号登録
            {
                s = "00000000" + s;
                s = s.Substring(s.Length - 8);
                // 売上番号を手登録した時にはSTATUS = '2'とする。
                string tmps = string.Format(
                    "SELECT COUNT(*) FROM sc_juchu_ret WHERE JDNNO = '{0}' AND LINNO = '{1}';"
                    , argVals[1], argVals[3]);
                int i = con.iGetCount(tmps, DEF_CON.Constr());
                if (i == 0)
                {
                    sUpd = string.Format(
                        "INSERT INTO sc_juchu_ret ("
                        + "JDNNO,LINNO,STATUS,CM,UPD_DATE,LGC_DEL"
                        + ") VALUES ("
                        + "'{0}','{1}','2','{2}',NOW(),'0'"
                        + ");"
                        ,argVals[1], argVals[3],s);
                }
                else
                {
                    sUpd = string.Format(
                        "UPDATE sc_juchu_ret SET STATUS = '2',CM = '{0}', UPD_DATE = NOW()"
                        + " WHERE JDNNO = '{1}' AND LINNO = '{2}';"
                        , s, argVals[1], argVals[3]);
                }
                if (sUpd.Length > 0) sUpd += sUpdInf;
             }
            if (sUpd.Length > 0)
            {
                string sRet = con.ExecSql(false, DEF_CON.Constr(), sUpd);
                if(sRet.Length > 0)
                {
                    string[] sSnd = { sRet, "false" };
                    string[] sRvd = promag_frm.F05_YN.ShowMiniForm(sSnd);
                    return;
                }
                string[] sSed = { "登録しました", "false" };
                string[] sRcv = promag_frm.F05_YN.ShowMiniForm(sSed);
                this.ReturnValue = new string[] { "" };
                closing();
            }
        }
    }
}
