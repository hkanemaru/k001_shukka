using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.IO;

namespace k001_shukka
{
    class DEF_CON
    {
        public static string verString = "Ver.0.24 21/02/24";
        public static string prjName = "gyom";
        public static string sHistry =
@"
Ver.0.24 21/02/24 ステータス登録方法変更
Ver.0.23 21/02/19   ステータスロジック変更
Ver.0.21 2021/02/08 出荷票特殊対応
Ver.0.20 2021/02/02 エラー結果表示
Ver.0.17 2021/02/02 受領書確認、売上番号登録機能付与
Ver.0.16 2021/01/25 Lot蘇り対応
Ver.0.15 2021/01/21 納品書1行の時におかしいのを修正
Ver.0.12 2021/01/19 出荷票でひゅーびす対応(海外向け）
Ver.0.11 2021/12/23 出荷票で伝票番号のない未連携のロット印刷時に受注伝票情報を見ないで印刷するよう変更
Ver.0.10 2020/12/23 納品書対応
Ver.0.09 2020/12/23 サンビックの特殊出荷票に対応
Ver.0.08 2020/12/22 納品書得意先宛名サポート
Ver.0.01 2020/12/03 リリース
カクテル連携出荷業務
";

        /*
         string[] sendText = { "登録に失敗しました。名前、SEQ、タイトルバーのFRM番号を控えてシステム管理者に伝えてください。"
                                , "false"
                                , "リスト表示不可"};
                            string[] receiveText = FRM_YesNo.ShowMiniForm(sendText);
                            return;
        */
        public static string Constr()
        {
            string s = string.Empty;
            //if (usr.iDB == 0) s = "server=192.168.254.13;user id=kyoei;Password=password;persist security info=True;database=KYOEI;Allow Zero Datetime=true";
            //if (usr.iDB == 0) s = "server=192.168.254.12;user id=ky;Password=password;persist security info=True;database=KYOEI;Allow Zero Datetime=true"; old syntax=yes
            if (usr.iDB == 0) s = "server=10.100.10.11;user id=kyoei;Password=password;persist security info=True;database=KYOEI;Allow Zero Datetime=true";
            if (usr.iDB == 1) s = "server=10.100.10.12;user id=kyoei;Password=password;persist security info=True;database=KYOEI;Allow Zero Datetime=true";
            if (usr.iDB == 2) s = "server=localhost;user id=kyoei;Password=password;persist security info=True;database=kyoei;Allow Zero Datetime=true";
            return s;
        }

        public static string DbSvrBk = @"\\10.100.10.12\";

        public static string FLSvrSub = @"\\10.100.10.20\share\tetra\";

        public static string appInf = @"C:\tetra\appInf.ini";


        public static int[] BLUE = { 0, 98, 150 };
        public static int[] LIGHT_BLUE = { 220, 240, 255 };

        public static int[] DBLUE = { 0, 37, 115 };
        public static int[] LBLUE = { 225, 240, 255 };

        public static int[] DGreen = { 52, 86, 86 };
        public static int[] LGreen = { 217, 236, 198 };

        public static int[] DGray = { 24, 24, 24 };
        public static int[] LGray = { 216, 216, 216 };

        public static int[] ORANGE = { 255, 170, 37 };
        public static int[] YELLOW = { 255, 255, 142 };

        public static int[] AZURE = { 240, 255, 255 };

        public static int[] DRED2 = { 220, 20, 60 };

        public static int[] LPINK = { 255, 236, 255 };
        public static int[] NAVY2 = { 0, 0, 128 };

        public static string[] ShipVoteRePrintable
          = {"k0111", "k0119", "k0148", "k0149", "k0193", "k0210", "k0234", "k0269"
                , "k0252", "k0245" };
        public static string SvrExcelDir = @"\\192.168.254.4\www\kyoei\xls\";
        /*
         try
         {
         }
        catch (Exception ex)
            {
                Debug.WriteLine("-- -\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                MessageBox.Show(ex.Message, "エラー");
            }

         */
        ///// <summary>
        ///// 生年月日から年齢を計算する
        ///// </summary>
        ///// <param name="birthDate">生年月日</param>
        ///// <param name="today">現在の日付</param>
        ///// <returns>年齢</returns>
        //public static int GetAge(DateTime birthDate, DateTime today, int i)
        //{
        //    int age = today.Year - birthDate.Year;
        //    //誕生日がまだ来ていなければ、1引く
        //    if (today.Month < birthDate.Month ||
        //        (today.Month == birthDate.Month &&
        //        today.Day < birthDate.Day))
        //    {
        //        if (i == 0) age--;
        //    }

        //    return age;
        //}
    }
}
