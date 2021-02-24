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
    public partial class F05_Dialog : Form
    {
        #region フォーム変数
        private bool bClose = true;
        private string[] argVals;    // 親フォームから受け取る引数
        public string[] ReturnValue; // 親フォームに返す戻り値
        private bool bPHide = true;  // 親フォームを隠す = True
        ToolTip ToolTip1;
        #endregion
        public F05_Dialog(params string[] argVals)
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
                , DEF_CON.prjName + " " + DEF_CON.verString);
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
            F05_Dialog f = new F05_Dialog(s);
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
            // argVals[0] jdnno , 1]linno
            string s = string.Empty;
            if(argVals.Length > 2)
            {
                label2.Text = "";
                label1.Text = argVals[0];
                s = argVals[2];
                textBox1.Width = 200;
                this.Width = 500;
                groupBox1.Visible = false;
            }
            else
            {
                string sSql = string.Format(
                "SELECT LINCM FROM sc_juchu "
                + "WHERE JDNNO = '{0}' AND LINNO = '{1}';"
                , argVals[0], argVals[1]);
                mydb.kyDb con = new mydb.kyDb();
                con.GetData(sSql, DEF_CON.Constr());
                s = con.ds.Tables[0].Rows[0][0].ToString();
            }
            
            textBox1.Text = s;
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
            //ToolTip1.SetToolTip(button1, "登録");
            //ToolTip1.SetToolTip(button2, "日付変更");
            //ToolTip1.SetToolTip(button3, "新規");
            //ToolTip1.SetToolTip(button4, "納品書出力");
            //ToolTip1.SetToolTip(button5, "ロット明細印刷");
            //ToolTip1.SetToolTip(button6, "表示更新");
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
            if (argVals.Length == 2) Reg();
            this.Close();
        }
        // 1) btnClose  -> 2) -> 3) -> 2) -> 1) -> ShowMiniForm.f.showdialog
        private void btnClose_Click(object sender, EventArgs e)
        {
            if (argVals.Length > 2) this.ReturnValue = new string[] { textBox1.Text };
            else
            {
                string s = "2";
                if (rb1.Checked) s = "1";
                this.ReturnValue = new string[] { s };
            }
            
            closing();
        }
        // CLOSE処理の最後
        private void FRM_Closed(object sender, FormClosedEventArgs e)
        {
            if (bPHide) this.Owner.Show();
        }
        #endregion

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (tb.Text.Length > 0) label2.Text = string.Format("{0}文字", tb.Text.Length);
        }

        private void Reg()
        {
            if (textBox1.Text.Length == 0) return;
            string s = string.Empty;
            if (textBox1.Text.Length > 81) s = textBox1.Text.Substring(0, 40);
            string sSql = string.Format(
                "UPDATE sc_juchu SET LINCM = '{2}'"
                + "WHERE JDNNO = '{0}' AND LINNO = '{1}';"
                , argVals[0], argVals[1], textBox1.Text);
            mydb.kyDb con = new mydb.kyDb();
            con.ExecSql(false, DEF_CON.Constr(), sSql);
        }
    }
}
