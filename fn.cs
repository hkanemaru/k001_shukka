using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
// PresentationCore.dll
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace k001_shukka
{
    class fn
    {
        public static void CrtUsrIni(string sID, string sPara)
        {
            if (!Directory.Exists(DEF_CON.usr_dir))
            {
                Directory.CreateDirectory(DEF_CON.usr_dir);
            }
            string scont = sID;
            scont += "," + usr.id;
            scont += "," + usr.iDB.ToString();
            scont += "," + DateTime.Now.ToString();
            scont += "," + sPara;
            System.IO.StreamWriter sw1 = new System.IO.StreamWriter(
            DEF_CON.usr_dir + DEF_CON.usr_fl,
            false,
            System.Text.Encoding.GetEncoding("shift_jis"));

            //LogFileに書き込む
            sw1.Write(scont);
            //閉じる
            sw1.Close();
        }
        public static void CrtUsrIni(string sID, string sPara, string sAdd1, string sAdd2)
        {
            if (!Directory.Exists(DEF_CON.usr_dir))
            {
                Directory.CreateDirectory(DEF_CON.usr_dir);
            }
            string scont = sID;
            scont += "," + usr.id;
            scont += "," + usr.iDB.ToString();
            scont += "," + DateTime.Now.ToString();
            scont += "," + sPara;
            scont += "," + sAdd1;
            scont += "," + sAdd2;

            System.IO.StreamWriter sw1 = new System.IO.StreamWriter(
            DEF_CON.usr_dir + DEF_CON.usr_fl,
            false,
            System.Text.Encoding.GetEncoding("shift_jis"));

            //LogFileに書き込む
            sw1.Write(scont);
            //閉じる
            sw1.Close();
        }
        public static string frmLTxt(string sT)
        {
            string s = sT;
            //s += " " + "MRBC";

            s += " " + DEF_CON.GetVersion();
            return s;
        }

        public static string frmRTxt()
        {
            string s = string.Empty;
            if (usr.iDB == 1) s = "TDB: ";
            s += usr.name;
            return s;
        }


        static System.Text.Encoding sjisEnc = System.Text.Encoding.GetEncoding("Shift_JIS");

        public static bool isHankaku(string str)
        {
            int num = sjisEnc.GetByteCount(str);
            return num == str.Length;
        }

        public static bool isHankakuEisu(string str)
        {
            if (string.IsNullOrEmpty(str)) return false;

            return System.Text.RegularExpressions.Regex.IsMatch(
                str, "^[a-zA-Z0-9!-/:-@¥[-`{-~]+$");
        }

        /// <summary>
        /// i= 1 false 0 true
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static bool booClose(int i)
        {
            bool b = true;
            if (i > 0) b = false;
            return b;
        }

        /// <summary>
        /// 指定したディレクトリにあるフォルダを新しいもの以外削除する。
        /// </summary>
        /// <param name="sdir">ディレクトリ名</param>
        public static void DelOldDir(string sdir)
        {
            /*
              指定したフォルダ以下にあるすべてのサブフォルダのパスを取得するには、
              Directory.GetDirectoriesメソッド（System.IO名前空間）を使います。
              GetDirectoriesメソッドは3番目のパラメータを省略
              （あるいは、SearchOption.TopDirectoryOnlyを指定）すると、
              指定したフォルダにあるサブフォルダしか取得せず、サブフォルダのサブフォルダは取得しません。
              サブフォルダのサブフォルダも含め、すべてのサブフォルダを取得するには、
              3番目のパラメータにSearchOption.AllDirectoriesを指定します。 
            */
            //"C:\test"以下のサブフォルダをすべて取得する
            //ワイルドカード"*"は、すべてのフォルダを意味する
            try
            {
                string[] subFolders = System.IO.Directory.GetDirectories(
                    sdir, "*", SearchOption.TopDirectoryOnly);
                //ListBox1に結果を表示する
                //ListBox1.Items.AddRange(subFolders);
                DateTime[] updTime = new DateTime[subFolders.Length];
                for (int i = 0; i < subFolders.Length; i++)
                {
                    updTime[i] = Directory.GetLastWriteTime(subFolders[i]);
                }
                if (subFolders.Length > 1)
                {
                    for (int i = 0; i < subFolders.Length - 1; i++)
                    {
                        if (updTime[i] < updTime[i + 1]) Directory.Delete(subFolders[i], true);
                        if (updTime[i] > updTime[i + 1]) Directory.Delete(subFolders[i + 1], true);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("スリム化出来ませんでした。", ex.GetType().FullName);
            }
        }
        public static string frmTxt(string sT)
        {
            string s = sT;
            //s += " " + "MRBC";
            s += " " + DEF_CON.GetVersion();
            return s;
        }

        public static void imageFileCopy(string fls)
        {
            string fld = @"C:\tetra\img";
            // フォルダ
            if (!System.IO.Directory.Exists(fld))
            {
                System.IO.Directory.CreateDirectory(fld);
            }
            // ファイル
            string baseFld = @"\\10.100.10.20\tetra\img";

            string[] files = fls.Split(',');
            for (int i = 0; i < files.Length; i++)
            {
                string flnm = files[i];
                if (flnm.IndexOf(".") < 0) flnm += ".png";
                string currFl = fld + @"\" + flnm;
                string baseFl = baseFld + @"\" + flnm;
                try
                {
                    // || System.IO.File.GetLastWriteTime(currFl) < System.IO.File.GetLastWriteTime(baseFl)
                    if (!System.IO.File.Exists(currFl))
                        System.IO.File.Copy(baseFl, currFl);
                }
                catch (Exception ex)
                {
                    string[] snd = { ex.Message, "false" };
                    _ = promag_frm2.F05_YN.ShowMiniForm(snd);
                    if (usr.id == "k0340")
                    {
                        string base0 = @"D:\document\PNG\png\resize2\";
                        System.IO.File.Copy(base0 + flnm, baseFl);
                        System.IO.File.Copy(base0 + flnm, currFl);
                    }
                }
            }
        }


        /// <summary>
        ///  ユーザーIDを与えると名前を返す関数
        /// </summary>
        /// <param name="sID">WkerID</param>
        /// <returns>SEI + ' ' + MEI</returns>
        public static string sStaffNAME(string sID)
        {
            mydb.kyDb con = new mydb.kyDb();
            string sNM = string.Empty;
            string s = string.Format(
                "SELECT "
            + "IFNULL(CONCAT(mw.NCNM_SEI, ' ', mw.NCNM_MEI), CONCAT(mw.SEI, ' ', mw.MEI))"
            + " FROM M_WORKER mw"
            + " WHERE mw.WKER_ID = '{0}' AND mw.LGC_DEL = '0'", sID);
            try
            {
                con.GetData(s, DEF_CON.Constr());
                sNM = con.ds.Tables[0].Rows[0][0].ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "エラー");
                return "";
            }
            return sNM;
        }

        public static bool bIntOK(string si)
        {
            bool b = false;
            int itest;
            if (int.TryParse(si, out itest)) b = true;
            return b;
        }

        public static bool bDecOK(string sd)
        {
            bool b = false;
            decimal dtest;
            if (decimal.TryParse(sd, out dtest)) b = true;
            return b;
        }
        /// <summary>
        /// dgv_Click時にデータ部分をクリックしていればTRUE 違えばFALSE
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public static bool dgvCellClk(object sender, MouseEventArgs e)
        {
            // dgvをクリックした際にデータ部分をクリックしていれば true 違えば false
            //DataGridView dgv = (DataGridView)sender;
            DataGridView.HitTestInfo info = ((DataGridView)sender).HitTest(e.X, e.Y);
            switch (info.Type)
            {
                case DataGridViewHitTestType.Cell:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// datagridviewの縦計を集計する
        /// </summary>
        /// <param name="dv">datagridview</param>
        /// <param name="icol">列番号</param>
        /// <returns></returns>
        public static double lColumnSum(DataGridView dv, int icol)
        {
            double l = 0;
            for (int i = 0; i < dv.Rows.Count; i++)
            {
                if (dv.Rows[i].Cells[icol].Value.ToString().Length > 0) l += double.Parse(dv.Rows[i].Cells[icol].Value.ToString().Replace(",", ""));
            }
            return l;
        }

        public static long dgvColSum(DataGridView dgv, int icol)
        {
            //int iy = dgv0.CurrentCell.ColumnIndex;
            //string sHeader = dgv0.Columns[iy].HeaderText;
            long l = 0;
            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                if (dgv.Rows[i].Cells[icol].Value.ToString().Length > 0) l += long.Parse(dgv.Rows[i].Cells[icol].Value.ToString().Replace(",", "").Replace(".0", ""));
            }
            return l;
        }


        /// <summary>
        /// 指定された文字列がメールアドレスとして正しい形式か検証する
        /// </summary>
        /// <param name="address">検証する文字列</param>
        /// <returns>正しい時はTrue。正しくない時はFalse。</returns>
        public static bool IsValidMailAddress(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                return false;
            }

            try
            {
                System.Net.Mail.MailAddress a =
                    new System.Net.Mail.MailAddress(address);
            }
            catch (FormatException)
            {
                //FormatExceptionがスローされた時は、正しくない
                return false;
            }

            return true;
        }

        /// <summary>
        /// 生年月日から年齢を計算する
        /// </summary>
        /// <param name="birthDate">生年月日</param>
        /// <param name="today">現在の日付</param>
        /// <param name="i">かぞえ = 0, 満 = 1</param>
        /// <returns>年齢</returns>
        public static int GetAge(DateTime birthDate, DateTime today, int i)
        {
            int age = today.Year - birthDate.Year;
            //誕生日がまだ来ていなければ、1引く
            if (today.Month < birthDate.Month ||
                (today.Month == birthDate.Month &&
                today.Day < birthDate.Day))
            {
                if (i == 0) age--;
            }
            return age;
        }

        /// <summary>
        /// 更新しようとする1つのレコードが直前で変更がないかチェックする
        /// </summary>
        /// <param name="s">レコードの更新日を抽出するQuery</param>
        /// <param name="d">比較するDateTime</param>
        /// <returns>エラーありがTRUE。なしはFALSE</returns>
        public static bool ErrUpdTime(String s, DateTime d)
        {
            mydb.kyDb con = new mydb.kyDb();
            con.GetData(s, DEF_CON.Constr());
            DateTime updTime = DateTime.Parse(con.ds.Tables[0].Rows[0][0].ToString());
            if (d <= updTime)
            {
                MessageBox.Show("編集中にこのレコードを別の人が更新しました。", "更新不可");
                return true;
            }
            return false;
        }

        /// <summary>
        /// 画像ファイルをリサイズして指定フォルダにコピー
        /// </summary>
        /// <param name="saveAddr">保存フォルダ</param>
        /// <param name="dpixel">縮小サイズ</param>
        public static String resizePicture(String saveAddr, Double dpixel)
        { // saveAddr = @"\\192.168.254.4\www\kyoei\pict\";
            string targetFile = string.Empty;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            ofd.Title = "ファイルを選択して下さい";
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                // フォルダ名＋ファイル名のフルパスからフォルダ名を取得
                String FolderPath = System.IO.Path.GetDirectoryName(ofd.FileName);
                // フォルダ名＋ファイル名のフルパスからファイル名を取得
                String FileName = System.IO.Path.GetFileName(ofd.FileName);

                // 画像ファイルじゃない場合はスキップ
                if (!Regex.IsMatch(FileName, "\\.(?:png|jpg|gif|bmp)$", RegexOptions.IgnoreCase)) return "";

                try
                {
                    // DBの写真保存先に選択したファイルをコピーする。
                    // 1.コピー先ファイル名を生成 targetFile
                    targetFile = saveAddr + FileName;
                    // 2.同名のファイルが既にあるか調べる。
                    bool b = System.IO.File.Exists(targetFile);
                    // 3.同名のファイルを上書きするかどうか聞いて処理。
                    if (b)
                    {
                        //メッセージボックスを表示する
                        String sMessage = "選択したファイル名は既に保存先にあり重複しています。\r\n"
                                                   + "上書きした場合、既に登録したデータにこの画像が紐づけられるおそれがあります。\r\n"
                                                   + "ファイルを上書きしてもよろしいですか？？";
                        DialogResult result = MessageBox.Show(sMessage, "確認",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Exclamation,
                            MessageBoxDefaultButton.Button2);
                        //「いいえ」が選択された時
                        if (result == DialogResult.No) return "";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("既に同名のファイルを指定している場合があります。\r\nファイル名を変えて再実行してください。\r\n" + ex.GetType().FullName, "写真コピーエラー");
                    return "";
                }
                // ファイルを開いて Stream オブジェクトを作成
                using (var sourceStream = File.OpenRead(ofd.FileName))
                {
                    // 画像をデコードするための BitmapDecoder オブジェクトを作成する
                    // (ファイルの種類に応じて適切なデコーダーが作成される)
                    var decoder = BitmapDecoder.Create(sourceStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

                    // 画像ファイル内の1フレーム目を取り出す (通常1フレームしかない)
                    var bitmapSource = decoder.Frames[0];

                    var scale = dpixel / bitmapSource.Height; // 拡大率

                    // 拡大・縮小されたビットマップを作成する
                    var scaledBitmapSource = new TransformedBitmap(bitmapSource, new ScaleTransform(scale, scale));

                    // 元のファイルと同じ形式のエンコーダー (BitmapEncoder) を選択・作成する
                    var extension = Path.GetExtension(ofd.FileName);
                    var encoder =
                        extension == ".png" ? new PngBitmapEncoder() :
                        extension == ".jpg" ? new JpegBitmapEncoder() :
                        extension == ".gif" ? new GifBitmapEncoder() :
                        extension == ".bmp" ? new BmpBitmapEncoder() :
                        (BitmapEncoder)(new PngBitmapEncoder());

                    // エンコーダーにフレームを追加する
                    encoder.Frames.Add(BitmapFrame.Create(scaledBitmapSource));

                    // 出力ディレクトリが存在しない場合は、新しく作成する
                    if (!Directory.Exists(saveAddr))
                    {
                        Directory.CreateDirectory(saveAddr);
                    }

                    var dest = saveAddr + Path.GetFileNameWithoutExtension(ofd.FileName) + extension; // 出力ファイル

                    // 出力ファイルのストリームを開く
                    using (var destStream = File.OpenWrite(dest))
                    {
                        encoder.Save(destStream); // 保存
                    }
                }
            } // if (ofd.ShowDialog() == DialogResult.OK)
            return targetFile;
        } // resizePict

        /// <summary>
        /// 画像ファイルをリサイズして指定フォルダにコピー2
        /// </summary>
        /// <param name="saveAddr">保存フォルダ</param>
        /// <param name="dpixel">縮小サイズ</param>
        public static String resizePicture2(String saveAddr, Double dpixel)
        { // saveAddr = @"\\192.168.254.4\www\kyoei\pict\";
            string targetFile = string.Empty;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            ofd.Title = "ファイルを選択して下さい";
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                // フォルダ名＋ファイル名のフルパスからフォルダ名を取得
                String FolderPath = System.IO.Path.GetDirectoryName(ofd.FileName);
                // フォルダ名＋ファイル名のフルパスからファイル名を取得
                String FileName = System.IO.Path.GetFileName(ofd.FileName);

                // 画像ファイルじゃない場合はスキップ
                if (!Regex.IsMatch(FileName, "\\.(?:png|jpg|gif|bmp)$", RegexOptions.IgnoreCase)) return "";

                try
                {
                    // DBの写真保存先に選択したファイルをコピーする。
                    // 1.コピー先ファイル名を生成 targetFile
                    targetFile = saveAddr + FileName;
                    // 2.同名のファイルが既にあるか調べる。
                    bool b = System.IO.File.Exists(targetFile);
                    // 3.同名のファイルを上書きするかどうか聞いて処理。
                    if (b)
                    {
                        //メッセージボックスを表示する
                        String sMessage = "選択したファイル名は既に保存先にあり重複しています。\r\n"
                                                   + "上書きした場合、既に登録したデータにこの画像が紐づけられるおそれがあります。\r\n"
                                                   + "ファイルを上書きしてもよろしいですか？？";
                        DialogResult result = MessageBox.Show(sMessage, "確認",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Exclamation,
                            MessageBoxDefaultButton.Button2);
                        //「いいえ」が選択された時
                        if (result == DialogResult.No) return "";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("既に同名のファイルを指定している場合があります。\r\nファイル名を変えて再実行してください。\r\n" + ex.GetType().FullName, "写真コピーエラー");
                    return "";
                }
                // ファイルを開いて Stream オブジェクトを作成
                using (Bitmap src = new Bitmap(ofd.FileName))
                {

                    // Loads original image.
                    //Bitmap src = new Bitmap("C:\Test.jpg");

                    //// Reduces image to quater.
                    //int w = bitmap.Width / 4;
                    //int h = bitmap.Height / 4;
                    ////Bitmap dst = new Bitmap(w, h);
                    ////Graphics g = Graphics.FromImage(dst);
                    ////g.InterpolationMode = Bicubic;
                    //g.DrawImage(src, 0, 0, w, h);

                    //// Saves result.
                    //dst.Save("C:\TestResult.png", ImageFormat.Png);

                    var scale = dpixel / src.Height; // 拡大率

                    // 高画質縮小の為Graphicsオブジェクトを使う
                    int w = (int)(src.Width * scale);
                    int h = (int)(src.Height * scale);

                    Bitmap dst = new Bitmap(w, h);
                    Graphics g = Graphics.FromImage(dst);
                    g.InterpolationMode = InterpolationMode.Low;
                    g.DrawImage(src, 0, 0, w, h);



                    //// 元のファイルと同じ形式のエンコーダー (BitmapEncoder) を選択・作成する
                    //var extension = Path.GetExtension(ofd.FileName);
                    //var encoder =
                    //    extension == ".png" ? new PngBitmapEncoder() :
                    //    extension == ".jpg" ? new JpegBitmapEncoder() :
                    //    extension == ".gif" ? new GifBitmapEncoder() :
                    //    extension == ".bmp" ? new BmpBitmapEncoder() :
                    //    (BitmapEncoder)(new PngBitmapEncoder());

                    //// エンコーダーにフレームを追加する
                    //encoder.Frames.Add(BitmapFrame.Create(scaledBitmapSource));

                    // 出力ディレクトリが存在しない場合は、新しく作成する
                    if (!Directory.Exists(saveAddr))
                    {
                        Directory.CreateDirectory(saveAddr);
                    }
                    dst.Save(String.Concat(saveAddr, Path.GetFileNameWithoutExtension(ofd.FileName), ".png"), ImageFormat.Png);

                }
            } // if (ofd.ShowDialog() == DialogResult.OK)
            return targetFile;
        } // resizePict

        /// <summary>
        /// ファイルを開くDialogを表示し選択されたファイルを指定先に保存する
        /// </summary>
        /// <param name="saveAddr">ファイルを保存するアドレス</param>
        public static String CopyFileTo(String saveAddr)
        {
            string targetFile = String.Empty;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            ofd.Title = "ファイルを選択して下さい";
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                // フォルダ名＋ファイル名のフルパスからフォルダ名を取得
                String FolderPath = System.IO.Path.GetDirectoryName(ofd.FileName);

                // フォルダ名＋ファイル名のフルパスからファイル名を取得
                String FileName = System.IO.Path.GetFileName(ofd.FileName);

                try
                {
                    // DBの写真保存先に選択したファイルをコピーする。
                    // 1.コピー先ファイル名を生成 targetFile
                    targetFile = saveAddr + FileName;
                    // 2.同名のファイルが既にあるか調べる。
                    bool b = System.IO.File.Exists(targetFile);
                    // 3.同名のファイルを上書きするかどうか聞いて処理。
                    if (b)
                    {
                        //メッセージボックスを表示する
                        DialogResult result = MessageBox.Show("重複しています。",
                            "確認",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Exclamation,
                            MessageBoxDefaultButton.Button2);
                        //何が選択されたか調べる
                        if (result == DialogResult.Yes)
                        {
                            System.IO.File.Copy(ofd.FileName, targetFile, true);
                        }
                        else if (result == DialogResult.No)
                        {
                            //「いいえ」が選択された時
                            return "";
                        }
                    }
                    else
                    {
                        System.IO.File.Copy(ofd.FileName, targetFile, true);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("既に同名のファイルを指定している場合があります。\r\nファイル名を変えて再実行してください。\r\n" + ex.GetType().FullName, "Fileコピーエラー");
                    return "";
                }
            }
            return targetFile;
        }

        /// <summary>
        ///  フルパスのファイルをsaveAddrにそのままコピーする。
        ///  成功true,失敗false
        /// </summary>
        /// <param name="filename">ファイル名（フルパス）</param>
        /// <param name="saveAddr">保存先アドレス（フルパス）</param>
        /// <returns></returns>
        public static Boolean SelectedFileCopyTo(String filename, String saveAddr)
        {
            string targetFile = String.Empty;
            // フォルダ名＋ファイル名のフルパスからファイル名を取得
            String filenm = System.IO.Path.GetFileName(filename);
            try
            {
                // 指定した保存先に選択したファイルをコピーする。
                // 1.コピー先ファイル名を生成 targetFile
                targetFile = saveAddr + filenm;
                // 2.同名のファイルが既にあるか調べる。
                bool b = System.IO.File.Exists(targetFile);
                // 3.同名のファイルを上書きするかどうか聞いて処理。
                if (b)
                {
                    //メッセージボックスを表示する
                    DialogResult result = MessageBox.Show("重複しています。上書きしますか？",
                        "確認",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Exclamation,
                        MessageBoxDefaultButton.Button2);

                    if (result == DialogResult.Yes) // 「はい」が選択された時
                    {
                        System.IO.File.Copy(filename, targetFile, true);
                        return false;
                    }
                    else if (result == DialogResult.No) return false; // 「いいえ」が選択された時
                }
                else
                {
                    System.IO.File.Copy(filename, targetFile, true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("既に同名のファイルを指定している場合があります。\r\nファイル名を変えて再実行してください。\r\n" + ex.GetType().FullName, "Fileコピーエラー");
                return false;
            }
            return true;
        }

     
        public static Control[] GetAllControls(Control top)
        {
            ArrayList buf = new ArrayList();
            foreach (Control c in top.Controls)
            {
                buf.Add(c);
                buf.AddRange(GetAllControls(c));
            }
            return (Control[])buf.ToArray(typeof(Control));
        }
        /// <summary>
        /// ボタンの機能や閲覧制限。trueの時は実行させる。
        /// </summary>
        /// <param name="sKBN">機能制限の種類</param>
        /// <param name="sid">=usr.id</param>
        /// <returns></returns>
        public static Boolean EnableFunction(String sKBN, String sid)
        {
            if (usr.author == 9) return true;
            #region 環境対策課　許認可 KYOKA
            if (sKBN == "KYOKA")
            {
                switch (sid)
                {                           //敬称略
                    case "k0006":  // 本間
                    case "k0135":  // 福田
                    case "k0137":  // 大塚
                    case "k0140":  // 柳
                    case "k0190":  // 中島
                    case "k0197":  // 岡田
                    case "k0268":  // 海老原
                    case "k0149":  // 羽石
                        return true;
                    default:
                        MessageBox.Show("担当部署に限定しています。環境対策課に相談してください。");
                        break;
                }
            }
            #endregion

            #region 業務 GYOMU
            if (sKBN == "GYOMU")
            {
                string s = string.Format(
                        "SELECT "
                    + " CASE WHEN se2.SECT_SEQ IS NOT NULL THEN se2.SECT_SEQ"
                    + "      WHEN se1.SECT_SEQ IS NOT NULL THEN se1.SECT_SEQ"
                    + "  ELSE se0.SECT_SEQ END TOP_SEQ"
                    + " FROM M_SECTION se0"
                    + " LEFT JOIN M_SECTION se1 ON se0.HI_SECT = se1.SECT_SEQ"
                    + " LEFT JOIN M_SECTION se2 ON se1.HI_SECT = se2.SECT_SEQ"
                    + " WHERE se0.SECT_SEQ = {0}", sid);
                mydb.kyDb con = new mydb.kyDb();
                if (con.iGetCount(s, DEF_CON.Constr()) == 9) return true;
                else
                {
                    MessageBox.Show("担当部署に限定しています。業務資材部に相談してください。");
                    return false;
                }
            }
            #endregion
            #region 小山Prt
            if (sKBN == "小山Prt")
            {
                sSectPost(sid);
                mydb.kyDb con = new mydb.kyDb();
                con.GetData(sid, DEF_CON.Constr());
                // 小山2で課長8以上、小山製造7で副主任13以上
                int FstSect = 99;
                if (con.ds.Tables[0].Rows[0][0] != null) FstSect = (int)con.ds.Tables[0].Rows[0][0];
                int SecSect = 99;
                if (con.ds.Tables[0].Rows[0][1] != null) SecSect = (int)con.ds.Tables[0].Rows[0][1];
                int Sect = (int)con.ds.Tables[0].Rows[0][0];
                int pSeq = (int)con.ds.Tables[0].Rows[0][0];
                int bSeq = (int)con.ds.Tables[0].Rows[0][0];
                bool b = false;
                if (bSeq == 2 && pSeq >= 8) b = true;
                if (FstSect == 7 || SecSect == 7 || Sect == 7)
                {
                    if (pSeq >= 13) b = true;
                }
                return b;
            }
            #endregion

            #region 小山主任以上
            if (sKBN == "oyama-seizo")
            {
                switch (sid)
                {                           //敬称略
                    case "k0119":  // 鈴木
                    case "k0148":  // 松岡
                    case "k0149":  // 羽石
                    case "k0193":  // 澤口
                    case "k0210":  // 高山 
                        return true;
                }
            }
            #endregion

            #region ■■■■■　小山出荷承認　GYOMU_PERMIT HINKAN_PERMIT SHIP_PERMIT
            if (sKBN == "SHIP_PERMIT") 
            {
                switch (sid)
                {                           //敬称略
                    case "k0119":  // 鈴木
                    case "k0214":  // 八文字 
                        return true;
                }
            }
            if (sKBN == "GYOMU_PERMIT")
            {
                switch (sid)
                {                           //敬称略
                    case "k0134":  // 中山
                    case "k0194":  //赤坂
                    case "k0214":  // 八文字 
                        return true;
                }
            }
            if (sKBN == "HINKAN_PERMIT")
            {
                switch (sid)
                {
                    case "k0119":  // 鈴木　//敬称略
                    case "k0331":  // 吉原
                    case "k0926":  // 関
                    case "k0214":  // 八文字 
                        return true;
                }
            }
            #endregion


            #region mrf
            if (sKBN == "mrf")
            {
                sGetBaseSeq(sid);
                mydb.kyDb con = new mydb.kyDb();
                int i = con.iGetCount(sGetBaseSeq(sid), DEF_CON.Constr());
                if (i == 3) return true;
            }
            #endregion

            #region mrf_mg
            if (sKBN == "mrf-mg")
            {
                sGetBaseSeq(sid);
                mydb.kyDb con = new mydb.kyDb();
                int i = con.iGetCount(sGetBaseSeq(sid), DEF_CON.Constr());
                int j = con.iGetCount(sGetPostOrder(sid), DEF_CON.Constr());
                if (i == 3 && j < 24) return true;
            }
            #endregion

            #region japantech　事務 JPT-U-J 
            if (sKBN == "JPT-U-J") // JPT-U-J 宇都宮 -J 事務
            {
                switch (sid)
                {                           //敬称略
                    case "k0194":  // 赤坂
                    case "k0215":  // 高橋
                    case "k0150":  // 佐藤K
                    case "k0018":  // 小林B
                    case "k0418":  // 本田
                    case "k0314":  // 渡邉
                    case "k0239":  // 亀山
                    case "k0170":  // 諏訪
                        return true;
                    default:
                        MessageBox.Show("担当部署に限定しています。Sys担当に相談してください。");
                        break;
                }
            }
            #endregion
            #region japantech　製造再印刷 JPT-U-S
            if (sKBN == "JPT-U-S") // JPT-U 宇都宮 -製造
            {
                switch (sid)
                {                           //敬称略
                    case "k0220":  // 熊倉
                    case "k0215":  // 高橋
                    case "k0150":  // 佐藤K
                    case "k0018":  // 小林B
                    case "k0219":  //　長瀧
                    case "k0166":  // 青木
                    case "k0239":  // 亀山
                    case "k0257":  // 戸崎
                    case "k0170":  // 諏訪
                        return true;
                    default:
                        MessageBox.Show("担当部署に限定しています。Sys担当に相談してください。");
                        break;
                }
            }
            #endregion
            #region 宇都宮工場勤務 jpt-u
            if (sKBN == "jpt-u") // JPT-U 宇都宮
            {
                string sSQL = string.Format(
                    "SELECT COUNT(*) FROM M_WORKER w "
                    + "WHERE w.WKER_ID = '{0}' AND w.LGC_DEL = '0' AND w.BASE_SEQ = 14;"
                    , sid);
                mydb.kyDb con = new mydb.kyDb();
                if (con.iGetCount(sSQL, DEF_CON.Constr()) > 0) return true;
                else
                {
                    MessageBox.Show("担当部署に限定しています。Sys担当に相談してください。");
                    return false;
                }
            }
            #endregion

            return false;
        }

        private static string sSectPost(string sid)
        {
            string s = string.Format(
                    "SELECT sn.TwoUp_SEQ, sn.OneUp_SEQ, sn.SECT_SEQ, w.POST_SEQ, w.BASE_SEQ"
                 + " FROM M_WORKER w"
                 + " LEFT JOIN V_SECT_NAME sn ON w.SECT_SEQ = sn.SECT_SEQ"
                 + " WHERE w.WKER_ID = '{0}' AND LGC_DEL = '0';"
                , sid);
            return s;
        }

        private static string sGetBaseSeq(string sid)
        {
            string s = string.Format(
                    "SELECT BASE_SEQ "
                 + " FROM M_WORKER w"
                 + " WHERE w.WKER_ID = '{0}' AND LGC_DEL = '0';"
                , sid);
            return s;
        }

        private static string sGetPostOrder(string sid)
        {
            string s = string.Format(
                    "SELECT po.DISP_ORDER"
                 + " FROM M_WORKER w"
                 + " LEFT JOIN M_POST po w.POST_SEQ = po.POST_SEQ"
                 + " WHERE w.WKER_ID = '{0}' AND LGC_DEL = '0';"
                , sid);
            return s;
        }
        /// <summary>
        /// コントロールのDoubleBufferedプロパティをTrueにする
        /// </summary>
        /// <param name="control">対象のコントロール</param>
        public static void EnableDoubleBuffering(Control control)
        {
            control.GetType().InvokeMember(
               "DoubleBuffered",
               BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
               null,
               control,
               new object[] { true });
        }

        /// <summary>
        /// DataGridViewの書式設定
        /// </summary>
        /// <param name="dgv">対象となるDGV</param>
        /// <param name="bWrap">改行を許容</param>
        /// <param name="iW">列幅の規定値通常50-80</param>
        /// <param name="bRowSelect">行の選択モード</param>
        /// <param name="iHeaderFSize">ヘッダ行のフォントサイズ</param>
        /// <param name="iTempFSize">行のフォントサイズ</param>
        /// <param name="iHedHight">列ヘッダの高さ</param>
        /// <param name="bROnly">編集禁止=true</param>
        /// <param name="iHi">行の高さ=true</param>
        /// <param name="hdColor">ヘッダーの色RGB</param>
        /// <param name="rColor">行の色RGB</param>
        public static void SetDGV(DataGridView dgv, bool bWrap, int iW, bool bRowSelect, int iHeaderFSize
            , int iTempFSize, int iHedHight, bool bROnly, int iHi, int[] hdColor, int[] rColor)
        {
            // ヘッダーに色を付けるためにVisualStyleをオフする。
            //dgv.EnableHeadersVisualStyles = false;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(hdColor[0], hdColor[1], hdColor[2]);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            // 列ヘッダーの高さを任意に変えるためにColumnHeadersHeightSizeModeを設定する
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;

            // 列ヘッダーの折り返しモード => 折り返す= datagridviewtristate.true
            #region 列ヘッダーの折り返しモード
            if (bWrap) dgv.RowHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            #endregion

            #region ++ 行の高さ　と　列の幅
            // 列ヘッダー高さ
            #region 列ヘッダー高さ
            dgv.ColumnHeadersHeight = iHedHight;

            //        dgv.RowTemplate.Height = 22;
            #endregion
            // 列の高さ
            dgv.RowTemplate.Height = iHi;
            #region 列の幅　全体
            dgv.RowHeadersWidth = iW;
            #endregion

            #region　列幅の自動調整
            //dgv.AutoResizeColumn(0);　　// 特定の列
            //dgv.AutoResizeColumns();　　　// 全体を自動調整
            #endregion

            #endregion 行の高さと列の幅

            #region ++ 表示
            //列ヘッダーを非表示にする
            #region 列ヘッダーを非表示
            if (dgv.Name == "dgvM0")
            {　　// 月のシフトの人数欄
                dgv.ColumnHeadersVisible = false;
            }
            #endregion
            //行ヘッダーを非表示にする
            dgv.RowHeadersVisible = false;

            #region 可動行列の固定表示
            //switch (dgv.Name.Substring(0, 4))
            //{
            //    case "dgvM":
            //        dgv.Columns[0].Frozen = true;  //１列目を固定
            //        if (dgv.Name.Substring(0, 5) == "dgvM1")
            //        {
            //            dgv.Columns[1].Frozen = true;  //１列目を固定
            //            dgv.Columns[2].Frozen = true;  //１列目を固定
            //            dgv.Columns[3].Frozen = true;  //１列目を固定
            //            dgv.Columns[4].Frozen = true;  //１列目を固定
            //        }
            //        break;
            //    default:
            //        dgv.Columns[0].Frozen = true;  //１列目を固定
            //        break;
            //}
            //１行目を固定
            //dgv.Rows[0].Frozen = true;
            #endregion

            #endregion

            #region ++ 編集モード
            //DataGridView1の列の幅をユーザーが変更できないようにする
            //dgv.AllowUserToResizeColumns = false;
            dgv.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
            //DataGridView1の行の高さをユーザーが変更できないようにする
            dgv.AllowUserToResizeRows = false;
            //DataGridView1のセルを読み取り専用にする
            dgv.ReadOnly = bROnly;
            //DataGridView1にユーザーが新しい行を追加できないようにする
            dgv.AllowUserToAddRows = false;
            #endregion

            #region ++ 選択モード
            // 選択モードを行単位での選択のみにする
            if (dgv.Name == "dgvM1")
            {
                //グリッド線の色を赤にする
                dgv.GridColor = System.Drawing.Color.White;
            }

            if (bRowSelect) dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            //DataGridView1でセル、行、列が複数選択されないようにする
            dgv.MultiSelect = false;
            #endregion

            #region ++ 書式
            // フォントの色を変更する
            //dgv.DefaultCellStyle.ForeColor = Color.Blue;
            //セルの境界線を一重線にする
            //dgv.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            //グリッド線の色を赤にする
            //dgv.GridColor = Color.FromArgb(96, 96, 96);
            //dgv.GridColor = Color.Red;
            //DataGridViewの境界線を3Dにする
            dgv.BorderStyle = BorderStyle.Fixed3D;

            // フォントを変更する
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("メイリオ", iHeaderFSize, FontStyle.Regular);
            dgv.DefaultCellStyle.Font = new Font("メイリオ", iTempFSize, FontStyle.Regular);
            //switch (dgv.Name.Substring(0, 4))
            //{
            //    case "dgvM":
            //        dgv.ColumnHeadersDefaultCellStyle.Font = new Font("メイリオ", 9, FontStyle.Regular);
            //        dgv.DefaultCellStyle.Font = new Font("メイリオ", 10, FontStyle.Regular);
            //        break;
            //    case "dgvI":
            //        dgv.ColumnHeadersDefaultCellStyle.Font = new Font("メイリオ", 9, FontStyle.Regular);
            //        dgv.DefaultCellStyle.Font = new Font("メイリオ", 10, FontStyle.Regular);
            //        break;
            //    default:
            //        dgv.ColumnHeadersDefaultCellStyle.Font = new Font("メイリオ", 10, FontStyle.Regular);
            //        dgv.DefaultCellStyle.Font = new Font("メイリオ", 11, FontStyle.Regular);
            //        break;
            //}

            //奇数行の背景色にYellowGreenを指定
            //Color.FromArgb(int alpha, int red, int green, int blue)
            //dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.YellowGreen;

            //dgv.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.Honeydew;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(rColor[0], rColor[1], rColor[2]);
            //switch (dgv.Name.Substring(0, 4))
            //{
            //    case "dgvM":
            //        break;
            //    case "dgv0":
            //        break;
            //    default:
            //        dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.Honeydew;
            //        break;
            //}

            //アクティブなセルを強調表示する（背景色：DarkBlue、文字の色:White）
            dgv.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.DarkRed;
            dgv.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.White;

            // テキストの配置--------------------------------------------
            //　ヘッダー全体
            dgv.ColumnHeadersDefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;
            // 2018/04/23 コメントアウト
            //dgv.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            //　特定のヘッダーのみ
            //dgv.Columns["Column1"].HeaderCell.Style.Alignment =
            //    DataGridViewContentAlignment.MiddleCenter;

            #endregion
        }

        /// <summary>
        /// テキスト配置の一括設定　特定列
        /// 2つの配列は同じ長さ
        /// </summary>
        /// <param name="dgv">DataGridView オブジェクト</param>
        /// <param name="icol">列番号</param>
        /// <param name="iAlign">1=左 0=中央 -1=右</param>
        public static void setDgvAlign(DataGridView dgv, int[] icol, int[] iAlign)
        {
            if (icol.Length != iAlign.Length)
            {
                MessageBox.Show("列番号と設定値の数が違います。");
                return;
            }
            for (int i = 0; i < icol.Length; ++i)
            {
                // 正の場合は左揃え
                if (iAlign[i] > 0) dgv.Columns[icol[i]].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                // 0の場合は中央揃え
                if (iAlign[i] == 0) dgv.Columns[icol[i]].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                // 負の場合は右揃え
                if (iAlign[i] < 0) dgv.Columns[icol[i]].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }

        /// <summary>
        /// 列非表示一括設定 = 任意指定
        /// 列番号=icolnum[] を非表示にする　列番号は特定のものだけ、ex. 1列と3列を非表示 -> {0,2}  
        /// </summary>
        /// <param name="dgv"></param>
        /// <param name="icolnum">列番号</param>
        public static void setDgvInVisible(DataGridView dgv, params int[] icolnum)
        {
            for (int i = 0; i < dgv.Columns.Count; ++i)
            {
                dgv.Columns[i].Visible = true;
            }
            for (int i = 0; i < icolnum.Length; ++i)
            {
                dgv.Columns[icolnum[i]].Visible = false;
            }
        }

        /// <summary>
        ///  列幅の一括設定　特定列を指定する
        /// icolNumに列番号 iwdtに幅
        /// 配列の長さは一致させなければエラー
        /// </summary>
        /// <param name="dgv">DataGridView</param>
        /// <param name="icolNum">列番号</param>
        /// <param name="iwdt">幅</param>
        public static void setDgvWidth(DataGridView dgv, int[] icolNum, int[] iwdt)
        {
            if (icolNum.Length != iwdt.Length)
            {
                MessageBox.Show("列番号と幅の指定数が違います。");
                return;
            }
            for (int i = 0; i < iwdt.Length; ++i)
            {
                if (iwdt[i] > 0) dgv.Columns[icolNum[i]].Width = iwdt[i];
            }
        }

        /// <summary>
        /// 携帯のテンキーを押すと絞り込みを行う
        /// ボタンの名前は[ac]=>btnA,[1]=>btn1,
        /// </summary>
        /// <param name="btn">テンキー</param>
        /// <param name="iCharNum"></param>
        /// <param name="sKANA"></param>
        /// <param name="sLABEL"></param>
        /// <returns></returns>
        public static string SerchString(Button btn, int iCharNum, String sKANA, String sLABEL)
        {
            //必要な変数
            //private int iCount = 0;                      // かな検索の値
            //private string sKana = String.Empty; // かな検索の値
            //textBox1 検索文字列を表示するボックス
            string sRet = String.Empty;
            string tmpKana = String.Empty;

            if (btn.Name.Substring(btn.Name.Length - 1) != "A")
            {
                iCharNum += 1;
            }
            else
            {
                iCharNum = 0;
            }
            switch (btn.Name.Substring(btn.Name.Length - 1))
            {
                case "A":
                    sKANA = "";
                    break;
                case "1":
                    tmpKana = String.Format(
                                "((SUBSTRING({1},{0},1) >= 'ア' AND SUBSTRING({1},{0},1) <= 'オ') OR (SUBSTRING({1},{0},1) >= '!' AND SUBSTRING({1},{0},1) <= '@')"
                                + " OR SUBSTRING({1},{0},1) = '-' OR SUBSTRING({1},{0},1) = '1' OR SUBSTRING({1},{0},1) = '(' OR SUBSTRING({1},{0},1) = ')')"
                                , iCharNum.ToString(), sLABEL);
                    break;
                case "2":
                    tmpKana = String.Format(
                                "((SUBSTRING({1},{0},1) >= 'カ' AND SUBSTRING({1},{0},1) <= 'ゴ') OR (SUBSTRING({1},{0},1) >= 'A' AND SUBSTRING({1},{0},1) <= 'c') OR SUBSTRING({1},{0},1) = '2')"
                                , iCharNum.ToString(), sLABEL);
                    break;
                case "3":
                    tmpKana = String.Format(
                                "((SUBSTRING({1},{0},1) >= 'サ' AND SUBSTRING({1},{0},1) <= 'ゾ') OR (SUBSTRING({1},{0},1) >= 'D' AND SUBSTRING({1},{0},1) <= 'f') OR SUBSTRING({1},{0},1) = '3')"
                                , iCharNum.ToString(), sLABEL);
                    break;
                case "4":
                    tmpKana = String.Format(
                                "((SUBSTRING({1},{0},1) >= 'タ' AND SUBSTRING({1},{0},1) <= 'ド') OR (SUBSTRING({1},{0},1) >= 'G' AND SUBSTRING({1},{0},1) <= 'i') OR SUBSTRING({1},{0},1) = '4')"
                                , iCharNum.ToString(), sLABEL);
                    break;
                case "5":
                    tmpKana = String.Format(
                                "((SUBSTRING({1},{0},1) >= 'ナ' AND SUBSTRING({1},{0},1) <= 'ノ') OR (SUBSTRING({1},{0},1) >= 'J' AND SUBSTRING({1},{0},1) <= 'l') OR SUBSTRING({1},{0},1) = '5')"
                                , iCharNum.ToString(), sLABEL);
                    break;
                case "6":
                    tmpKana = String.Format(
                                "((SUBSTRING({1},{0},1) >= 'ハ' AND SUBSTRING({1},{0},1) <= 'ボ') OR (SUBSTRING({1},{0},1) >= 'M' AND SUBSTRING({1},{0},1) <= 'o') OR SUBSTRING({1},{0},1) = '6')"
                                , iCharNum.ToString(), sLABEL);
                    break;
                case "7":
                    tmpKana = String.Format(
                                "((SUBSTRING({1},{0},1) >= 'マ' AND SUBSTRING({1},{0},1) <= 'モ') OR (SUBSTRING({1},{0},1) >= 'P' AND SUBSTRING({1},{0},1) <= 's') OR SUBSTRING({1},{0},1) = '7')"
                                , iCharNum.ToString(), sLABEL);
                    break;
                case "8":
                    tmpKana = String.Format(
                                "((SUBSTRING({1},{0},1) >= 'ヤ' AND SUBSTRING({1},{0},1) <= 'ヨ') OR (SUBSTRING({1},{0},1) >= 'T' AND SUBSTRING({1},{0},1) <= 'v') OR SUBSTRING({1},{0},1) = '8')"
                                , iCharNum.ToString(), sLABEL);
                    break;
                case "9":
                    tmpKana = String.Format(
                                "((SUBSTRING({1},{0},1) >= 'ラ' AND SUBSTRING({1},{0},1) <= 'ロ') OR (SUBSTRING({1},{0},1) >= 'W' AND SUBSTRING({1},{0},1) <= 'z') OR SUBSTRING({1},{0},1) = '9')"
                                , iCharNum.ToString(), sLABEL);
                    break;
                case "0":
                    tmpKana = String.Format(
                                "((SUBSTRING({1},{0},1) >= 'ワ' AND SUBSTRING({1},{0},1) <= 'ン') OR SUBSTRING({1},{0},1) = '0'"
                                + " OR SUBSTRING({1},{0},1) = '(' OR SUBSTRING({1},{0},1) = '-' OR SUBSTRING({1},{0},1) = 'ー' OR SUBSTRING({1},{0},1) = 'ｰ' OR SUBSTRING({1},{0},1) = '－')"
                                , iCharNum.ToString(), sLABEL);
                    break;
            }

            if (tmpKana.Length == 0)
            {
                sRet = "";
            }
            else
            {
                if (sKANA.Length == 0)
                {
                    sKANA = tmpKana;
                }
                else
                {
                    sKANA += " AND (" + tmpKana + ")";
                }
            }
            return sKANA;
            //if (rb1.Checked)
            //{
            //    textBox2.Text += tmpMoji;
            //    if (iCharNum < 1) textBox2.Text = "";
            //}
            //else
            //{
            //    textBox1.Text += tmpMoji;
            //    if (iCharNum < 1) textBox1.Text = "";
            //}
        }

        /// <summary>
        /// LotNoを返す
        /// </summary>
        /// <param name="machine">生産装置</param>
        /// <param name="sDate">日付20xx/xx/xx</param>
        /// <param name="sACCOUNT">取引先名</param>
        /// <param name="sGrade">製品名</param>
        /// <returns></returns>
        public static string LotNo(string machine, string sDate, string sACCOUNT, string sGrade)
        {
            string LotNo = string.Empty;
            string middleNo = string.Empty;
            string middleNo_SHIGA = string.Empty;
            string grade_SHIGA = string.Empty;
            string LastNo = string.Empty;
            try
            {
                if (sDate.Length > 0)
                {
                    LotNo = sDate;
                    LotNo = LotNo.Substring(2, 2) + LotNo.Substring(5, 2) + LotNo.Substring(8, 2);
                }

                if (machine.Length > 0)
                {
                    if (machine.Length == 1)
                    {
                        middleNo = machine;
                    }
                    else
                    {
                        switch (machine.Substring(0, 2))
                        {
                            case "SM":
                                middleNo = "V" + machine.Substring(machine.Length - 1);
                                middleNo_SHIGA = middleNo;
                                break;
                            case "T1":
                                middleNo = "T1";
                                break;
                            case "T3":
                                middleNo = "J";
                                break;
                            case "T4":
                                middleNo = "T";
                                break;
                            case "TS":
                                middleNo = "TS";
                                break;
                            default:
                                middleNo = machine.Substring(machine.Length - 1);
                                middleNo_SHIGA = "0" + middleNo;
                                break;
                        }
                    }
                    middleNo = "-" + middleNo;
                }

                if (sACCOUNT != "東レ滋賀") LotNo = LotNo + middleNo;
                else
                {
                    #region 滋賀のミドルネーム作成
                    switch (sGrade.Substring(0, 4))
                    {
                        case "CEC0":
                            grade_SHIGA = "C01";
                            break;
                        case "CU30":
                            grade_SHIGA = "C30";
                            if (sGrade.Substring(0, 5) == "CU30M") grade_SHIGA = "C30M";
                            break;
                        case "R41-":
                            grade_SHIGA = "41";
                            break;
                        case "MX70":
                            grade_SHIGA = "70";
                            break;
                        case "N71-":
                            grade_SHIGA = "71";
                            break;
                        case "R75-":
                            grade_SHIGA = "75";
                            break;
                        case "R75M":
                            grade_SHIGA = "75M";
                            break;
                        case "RS-1":
                            grade_SHIGA = "10";
                            break;
                        case "T60-":
                            grade_SHIGA = "60";
                            break;
                        case "U30-":
                            grade_SHIGA = "30";
                            break;
                        case "WEC-":
                            grade_SHIGA = "WEC";
                            break;
                        case "MX11":
                            grade_SHIGA = "11";
                            break;
                        case "CQ77G":
                            grade_SHIGA = "WEC";
                            break;
                        default:
                            grade_SHIGA = sGrade.Substring(sGrade.IndexOf("-") - 2, 2);
                            break;
                    }
                    #endregion
                    LotNo = middleNo_SHIGA + "-" + grade_SHIGA + "-" + LotNo;
                    MessageBox.Show("東レ滋賀なのでLOT名が正しいか確認してください", "確認のお願い");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("LotNO生成中にエラーが発生しました。\r\nLOT名は最後の枝番を除いて自分で入力してください。", ex.GetType().FullName);
            }
            return LotNo;
        }

        public static string LotNoMRF(string machine, string sDate, string sACCOUNT, string sGrade)
        {
            string LotNo = string.Empty;
            string middleNo = string.Empty;
            string middleNo_SHIGA = string.Empty;
            string grade_SHIGA = string.Empty;
            string LastNo = string.Empty;
            try
            {
                if (sDate.Length > 0)
                {
                    LotNo = sDate;
                    LotNo = LotNo.Substring(2, 2) + LotNo.Substring(5, 2) + LotNo.Substring(8, 2);
                }

                if (machine.Length > 0)
                {
                    middleNo = machine.Substring(0, 1);
                    middleNo = "-" + middleNo;
                }

                LotNo = LotNo + middleNo;
            }
            catch (Exception ex)
            {
                MessageBox.Show("LotNO生成中にエラーが発生しました。\r\nLOT名は最後の枝番を除いて自分で入力してください。", ex.GetType().FullName);
            }
            return LotNo;
        }


        public static void showCenter(Form frm)
        {
            frm.StartPosition = FormStartPosition.CenterScreen;
            frm.Top = 0;
        }

        public static string sDBVAL(string sSql)
        {
            string s = string.Empty;
            mydb.kyDb con = new mydb.kyDb();
            con.GetData(sSql, DEF_CON.Constr());
            if (con.ds.Tables[0].Rows.Count > 0)
            {
                s = con.ds.Tables[0].Rows[0][0].ToString();
            }
            return s;
        }

        /// <summary>
        /// 複雑になったフレークグレードの詳細を返す
        /// </summary>
        /// <param name="sGrade">詳細を返すグレード名</param>
        /// <returns></returns>
        public static string sGradeDetail(string sGrade)
        {
            string s = string.Empty;
            switch (sGrade)
            {
                case "DR-S":
                    s = "長繊維・薄膜ラベル_店頭";
                    break;
                case "DR-A":
                    s = "ボトル・食品シート_容リ";
                    break;
                case "DR-B":
                    s = "ボトル・食品シート_容リ_長期";
                    break;
                case "DR-C":
                    s = "増量用_容リ_長期";
                    break;
                case "DR-J":
                    s = "増量用_事業系";
                    break;
                case "DR-Z":
                    s = "増量用_事業系_ガラス混";
                    break;
                case "SR-A":
                    s = "短繊維・シート_国内直販_容リ";
                    break;
                case "SR-B":
                    s = "増量用・容リ_長期_ガラス混";
                    break;
                case "SR-J":
                    s = "増量用_事業系";
                    break;
                case "SR-Z":
                    s = "増量用_事業系_ガラス混";
                    break;
                case "C":
                    s = "水洗浄品_直販可能";
                    break;
                case "D":
                    s = "水洗浄品_限定直販可能";
                    break;
                case "Y":
                    s = "水洗浄品_加工用";
                    break;
                case "Z":
                    s = "輸出・販売又は増量";
                    break;
                default:
                    //MessageBox.Show("規定のグレードを選択して下さい。");
                    break;
            }
            return s;
        }

        public static int dgvWidth(DataGridView dgv)
        {
            int i = 0;
            for (int icol = 0; icol < dgv.Columns.Count; icol++)
            {
                if (dgv.Columns[icol].Visible)
                {
                    i += dgv.Columns[icol].Width;
                }
            }
            i += 21;
            return i;
        }

        public static bool IsDatetime(string sd)
        {
            bool b = false;
            DateTime dtest;
            if (DateTime.TryParse(sd, out dtest)) b = true;
            return b;
        }

        public static bool IsInt(string si)
        {
            bool b = false;
            int itest;
            if (int.TryParse(si, out itest)) b = true;
            return b;
        }

        public static bool IsDec(string sd)
        {
            bool b = false;
            decimal dtest;
            if (decimal.TryParse(sd, out dtest)) b = true;
            return b;
        }

        /// <summary>
        /// 指定したディレクトリが無ければ作る
        /// </summary>
        /// <param name="sDir"></param>
        public static void CrtDir(string sDir)
        {
            if (!System.IO.Directory.Exists(sDir))
            {
                System.IO.Directory.CreateDirectory(sDir);
            }
        }
    }
}
