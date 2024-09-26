using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace k001_shukka
{
    class clsBC
    {
        public static void Save128Image(string sCode, string fileName, ImageFormat imgFormat)
        {
            // バーコード描画 (IBarCode インタフェイス使用)
            //Pao.BarCode.IBarCode bar = null;
            Pao.BarCode.IBarCode bar;

            GraphicsUnit unit = GraphicsUnit.Millimeter;

            float dpi = 300f;
            float width = 60f;

            float height = 15f;
            try
            {
                bar = new Pao.BarCode.Code128(fileName, imgFormat)
                {
                    ImgDpi = dpi,
                    ImgDrawUnit = unit,

                    TextWrite = false // 添え字
                };

                bar.Draw(sCode, 0, 0, width, height, fileName); // 幅ピッタリ 縮小描画
                //bar.DrawDirect(sCode, 0, 0, width, height, fileName); // 幅調整 直接描画
                //bar.DrawDelicate(sCode, 0, 0, width, height, fileName); // 最小幅指定 直接描画
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "コード128出力エラー");
                return;
            }
        }

        public static void Save39Image(string sCode, string fileName, ImageFormat imgFormat)
        {
            // バーコード描画 (IBarCode インタフェイス使用)
            Pao.BarCode.IBarCode bar; // Pao.BarCode.IBarCode bar = null;

            GraphicsUnit unit = GraphicsUnit.Millimeter;

            float dpi = 300f;
            float width = 100f;

            float height = 15f;
            try
            {
                bar = new Pao.BarCode.Code39(fileName, imgFormat)
                {
                    ImgDpi = dpi,
                    ImgDrawUnit = unit,

                    TextWrite = false // 添え字
                };

                bar.Draw(sCode, 0, 0, width, height, fileName); // 幅ピッタリ 縮小描画
                //bar.DrawDirect(sCode, 0, 0, width, height, fileName); // 幅調整 直接描画
                //bar.DrawDelicate(sCode, 0, 0, width, height, fileName); // 最小幅指定 直接描画
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "コード128出力エラー");
                return;
            }
        }

        public static void SaveQRImage(string sCode, string fileName, ImageFormat imgFormat)
        {

            GraphicsUnit unit;
            float dpi = float.Parse("300");
            unit = GraphicsUnit.Point; // 他 Millimeter, Inch, Display, Document, Pixel

            float fwidth = float.Parse("120");
            // float ftxtLineWidht; // float ftxtLineWidht = float.Parse("5");
            try
            {
                Pao.BarCode.QRCode qr = new Pao.BarCode.QRCode(fileName, imgFormat)
                {
                    ErrorCorrect = "M", // 他 L, Q, H エラー訂正レベル
                    EncodeMode = "Z", // 他　N>>数字 , A>>大文字英数字, Z >> 8ビットバイトデータ感じ含む
                    Version = 20, // 1 >> 40 まで　規定値 5
                    ImgDpi = dpi,
                    ImgDrawUnit = unit,
                    StringEncoding = "shift-jis" // utf-8
                };

                qr.Draw(sCode, 0, 0, fwidth, fwidth, fileName); // 幅ピッタリ縮小描画
                //qr.DrawDirect(sCode, 0, 0, fwidth, fwidth, fileName); // 幅ピッタリ縮小描画
                //qr.DrawDelicate(sCode, 0, 0, ftxtLineWidht, fileName); // 幅ピッタリ縮小描画
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "QRコード出力エラー");
                return;
            }
        }
    }
}
