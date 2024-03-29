﻿using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.IO;

namespace k001_shukka
{
    class DEF_CON
    {
        //public static string verString = "Ver.0.30 21/08/23";
        
        public static string prjName = "k001_shukka";
        public static string exepath = "k001_shukka.exe";
        public static string sHistry =
@"
1.00 F01ロット明細でコンテナ対応
0.96 F01出荷情報一覧の抽出条件を大幅に見直し
0.95 F02出荷情報登録で品目を選ぶ際テトラの品名にしない場合は、変更していても受注伝票の品名にする。
0.94 F01出荷情報一覧 でstatus 3出荷待ちの状態でも出荷登録が出来るようにする。
Ver.0.91 F02 でコンテナロット生成にLOT紐付けの制限を追加
Ver.0.30 21/08/23 F01List  で小山小山以外のフィルターの見直し
Ver.0.30 21/07/12 F02_Edit グレード選択しても出荷品名を連携させないことを可能とする
Ver.0.29 21/07/05 グレード変更
Ver.0.28 21/06/07 受注連携
Ver.0.281 21/06/04 バグフィクス
Ver.0.28 21/05/18 備考欄追加
Ver.0.27 21/05/18 日付変更、連携結果の文字処理
Ver.0.25 21/05/08 0要確認対応時の対応方法変更
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

        // usr.ini
        public static string usr_dir = @"C:\tetra";
        public static string usr_fl = @"\usr.ini";

        public static string DbSvrBk = @"\\10.100.10.12\";

        public static string FLSvrSub = @"\\10.100.10.20\share\tetra\";

        public static string appInf = @"C:\tetra\appInf.ini";
        public static string FLSvrXls = @"\\10.100.10.20\share\tetra\xls\";


        public static int[] BLUE = { 0, 98, 150 };
        public static int[] LIGHT_BLUE = { 220, 240, 255 };

        public static int[] MNBLUE = { 35, 42, 70 };
        public static int[] LLPINK = { 246, 218, 222 };

        public static int[] DBLUE = { 0, 37, 115 };
        public static int[] STLBLUE = { 70, 130, 180 };

        public static int[] LBLUE = { 225, 240, 255 };


        public static int[] DGreen = { 52, 86, 86 };
        public static int[] LGreen = { 217, 236, 198 };

        public static int[] DGray = { 24, 24, 24 };
        public static int[] LGray = { 216, 216, 216 };
        public static int[] LLGray = { 240, 240, 240 };

        public static int[] ORANGE = { 255, 170, 37 };
        public static int[] YELLOW = { 255, 255, 142 };

        public static int[] AZURE = { 240, 255, 255 };

        public static int[] DRED2 = { 220, 20, 60 };

        public static int[] LPINK = { 255, 236, 255 };
        public static int[] NAVY2 = { 0, 0, 128 };

        public static int[] Miziro = { 41, 200, 255 };
        public static int[] Midori = { 135, 191, 97 };
        public static int[] DLimeG = { 112, 173, 71 };
        public static int[] Orange = { 255, 176, 16 };

        public static string[] ShipVoteRePrintable
          = {"k0111", "k0119", "k0148", "k0149", "k0193", "k0210", "k0234", "k0269"
                , "k0252", "k0245" };
        public static string SvrExcelDir = @"\\192.168.254.4\www\kyoei\xls\";


        public static string GetVersion()
        {
            if (!ApplicationDeployment.IsNetworkDeployed) return string.Empty;

            var version = ApplicationDeployment.CurrentDeployment.CurrentVersion;
            // 自動インクリメントのみ実行するので実際はRevisionのみしか増えない
            // Major, Minor,Build が設定されている場合はそちらを優先する
            string Bld = version.Revision.ToString();
            Bld = Bld.Substring(Bld.Length - 1);
            string Min = version.Revision.ToString();
            if (Min.Length < 2) Min = "0";
            else
            {
                Min = Min.Substring(Min.Length - 2, 1);
            }
            string Maj = version.Revision.ToString();
            if (Maj.Length < 3) Maj = "0";
            else Maj = Maj.Substring(0, Maj.Length - 2);

            return (
                "Ver." + Maj + "." + Min + Bld
            //version.Major.ToString() + "." +
            //version.Minor.ToString() + "." +
            //version.Build.ToString() + "." +
            //version.Revision.ToString()
            );
        }

        /// <summary>
        /// アセンブリファイルのビルド日時を取得する。
        /// </summary>
        /// <param name="asmPath">exeやdll等のアセンブリファイルのパス。
        /// <returns>取得したビルド日時。</returns>
        public static DateTime GetBuildDateTime(string asmPath)
        {
            // ファイルオープン
            using (FileStream fs = new FileStream(asmPath, FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(fs))
            {
                // まずはシグネチャを探す
                byte[] signature = { 0x50, 0x45, 0x00, 0x00 };// "PE\0\0"
                List<byte> bytes = new List<byte>();
                while (true)
                {
                    bytes.Add(br.ReadByte());

                    if (bytes.Count < signature.Length)
                    {
                        continue;
                    }

                    while (signature.Length < bytes.Count)
                    {
                        bytes.RemoveAt(0);
                    }

                    bool isMatch = true;
                    for (int i = 0; i < signature.Length; i++)
                    {
                        if (signature[i] != bytes[i])
                        {
                            isMatch = false;
                            break;
                        }
                    }
                    if (isMatch)
                    {
                        break;
                    }
                }

                // COFFファイルヘッダを読み取る
                var coff = new
                {
                    Machine = br.ReadBytes(2),
                    NumberOfSections = br.ReadBytes(2),
                    TimeDateStamp = br.ReadBytes(4),
                    PointerToSymbolTable = br.ReadBytes(4),
                    NumberOfSymbols = br.ReadBytes(4),
                    SizeOfOptionalHeader = br.ReadBytes(2),
                    Characteristics = br.ReadBytes(2),
                };

                // タイムスタンプをDateTimeに変換
                int timestamp = BitConverter.ToInt32(coff.TimeDateStamp, 0);
                DateTime baseDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                DateTime buildDateTimeUtc = baseDateTime.AddSeconds(timestamp);
                DateTime buildDateTimeLocal = buildDateTimeUtc.ToLocalTime();
                return buildDateTimeLocal;
            }
        }
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
