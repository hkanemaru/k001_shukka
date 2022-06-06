using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace k001_shukka
{
    public partial class F00 : Form
    {
        #region フォーム変数
        private bool bClose = true;
        string AppID;
        string sFRMID = string.Empty; // この値でどのフォームを開くか決める
        #endregion 
        public F00()
        {
            InitializeComponent();
        }
        private void F00_Load(object sender, EventArgs e)
        {
            string iniFL = @"C:\tetra\usr.ini";
            // 開いてエラーが発生したら戾る
            try
            {
                string sID = string.Empty;
                string sDBID = string.Empty;
                DateTime dDate = DateTime.Today;
                
                using (System.IO.StreamReader sr =
                        new System.IO.StreamReader(iniFL, System.Text.Encoding.GetEncoding("Shift_JIS")))
                {
                    // ストリームの末尾まで繰り返す
                    while (!sr.EndOfStream)
                    {
                        //ファイルから一行読み込む
                        string line = sr.ReadLine();
                        if (line.Substring(line.Length - 1) != ",") line += ",";
                        // 読み込んだ一行をカンマ毎に分けて配列に格納する
                        string[] values = line.Split(',');
                        // 14,k0340,2020/10/02 19:32:34　拠点番号,WkerID, DBID, 日付時間, FRM番号
                        sID = values[1];
                        AppID = values[0];
                        dDate = DateTime.Parse(values[3]);
                        sDBID = values[2];
                        sFRMID = values[4];
                    }
                }
                //if (dDate < DateTime.Today) closing();
                if (sID == "k0000")
                {
                    usr.iDB = 1; usr.author = 9;
                    usr.id = sID; usr.SEI = "sys";
                    usr.name = "system"; usr.pw = "000001";
                }
                else
                {
                    mydb.kyDb con = new mydb.kyDb();
                    usr.iDB = int.Parse(sDBID);

                    string s = string.Format(
                        "SELECT "
                    + "IFNULL(CONCAT(mw.NCNM_SEI, ' ', mw.NCNM_MEI), CONCAT(mw.SEI, ' ', mw.MEI))"
                    + ", mw.AUTH_SEQ, mw.PASSWORD, IFNULL(mw.NCNM_SEI, mw.SEI)"
                    + " FROM M_WORKER mw"
                    + " WHERE mw.WKER_ID = '{0}' AND mw.LGC_DEL = '0'", sID);

                    con.GetData(s, DEF_CON.Constr());
                    usr.author = int.Parse(con.ds.Tables[0].Rows[0][1].ToString());
                    usr.id = sID;
                    usr.name = con.ds.Tables[0].Rows[0][0].ToString();
                    usr.pw = con.ds.Tables[0].Rows[0][2].ToString();
                    usr.SEI = con.ds.Tables[0].Rows[0][3].ToString();
                }
                //if (AppID == "2") 
                OpenFrm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "エラー");
                //closing();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string[] sSnd = { AppID };
            _ = F01_LIST.ShowMiniForm(this, sSnd);
        }
        private void OpenFrm()
        {
            string[] sSnd = { AppID };
            
            // 業務の出荷業務から
            if(AppID == "2" && sFRMID != "F14") _ = F01_LIST.ShowMiniForm(this, sSnd);
            if(AppID == "100")
            {
                sSnd = new string[] { sFRMID };  // sFRMIDは出荷SEQ
                _ = F09_Permit.ShowMiniForm(this, sSnd);
            }
            if(sFRMID == "F14")
            {
                sSnd = new string[] { AppID, sFRMID };  // sFRMIDは出荷SEQ
                _ = F14_SHIP_PERMIT_LIST.ShowMiniForm(this, sSnd);
            }
            //if (sRcv[0].Length == 0) closing();
            closing();
        }
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

        private void button2_Click(object sender, EventArgs e)
        {
            string[] Snd1 = new string[] { AppID, sFRMID };  // sFRMIDは出荷SEQ
            _ = F14_SHIP_PERMIT_LIST.ShowMiniForm(this, Snd1);
        }
    }
}
