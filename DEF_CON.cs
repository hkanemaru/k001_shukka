using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace k001_shukka
{
    class DEF_CON
    {
        public static string verString = "Ver.0.01 2020/10/17";
        public static string prjName = "MRBC";
        public static string sHistry =
@"
Ver.0.01 2020/10/17 リリース
　　　　MRFからMRBCへの入庫登録
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
