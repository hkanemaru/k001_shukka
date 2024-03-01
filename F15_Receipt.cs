using ClosedXML.Excel;
using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace k001_shukka
{
    public partial class F15_Receipt : Form
    {
        #region フォーム変数
        private bool bClose = true;
        private string[] argVals;    // 親フォームから受け取る引数
        public string[] ReturnValue; // 親フォームに返す戻り値
        ToolTip ToolTip1;
        private Boolean bdgvCellClk = false; // dgvがクリックされた際にデータ欄だとtrue
        private string _inLcn;
        private string _lcn;
        private string _ShipSeq = string.Empty;
        #endregion

        public F15_Receipt(params string[] argVals)
        {
            // 親フォームから受け取ったデータをこのインスタンスメンバに格納
            this.argVals = argVals;
            InitializeComponent();
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
            F15_Receipt f = new F15_Receipt(s);
            f.ShowDialog(frm);

            string[] receiveText = f.ReturnValue; // -- ①
            f.Dispose();
            return receiveText;
        }

        private void FRM_Load(object sender, EventArgs e)
        {
            if(argVals.Length == 2)
            {
                _ShipSeq = argVals[0];
                _inLcn = argVals[1];
                if (_ShipSeq.Length == 0) _ShipSeq = "NULL";
                if (_inLcn.Length == 0) _inLcn = "NULL";
            }
            
            _lcn = "2";
            // 登録用
            SetTooltip();

            //■■■ 画面の和名
            string sTitle = "受入確認";

            #region 画面のキャプション設定
            lblCaption.Text = fn.frmTxt(sTitle);

            DateTime bldTime = DEF_CON.GetBuildDateTime(DEF_CON.exepath);
            this.Text = $"【{this.Name}】{DEF_CON.prjName} {bldTime:yyyy'/'MM'/'dd}";
            string scap = string.Empty;
            if (usr.iDB == 1) scap += " TestDB: ";
            scap += DateTime.Now.ToString("yy/MM/dd HH:mm");
            scap += " " + usr.name;
            lblRCaption.Text = scap;
            #endregion


            dgv0IniSet(); // 一覧更新

            button1.Left = this.Width - button1.Width - 20;
            lblRCaption.Left = button1.Left - lblRCaption.Width;
            if(argVals.Length == 2)
            {
                string s1 = $" = {_ShipSeq}";
                string s2 = $" = {_inLcn}";
                if (_ShipSeq == "NULL") s1 = " IS NULL";
                if (_inLcn == "NULL") s2 = " IS NULL";
                string s = "SELECT  r.LOT_NO ,r.GRADE"
                         + " FROM kyoei.t_receipt r"
                         + $" WHERE r.SHIP_SEQ{s1} AND r.iLOCATION{s2}; ";
                mydb.kyDb con = new mydb.kyDb();
                con.GetData(s, DEF_CON.Constr());
                if (con.ds.Tables[0].Rows.Count == 0) return;
                for (int i = 0; i < con.ds.Tables[0].Rows.Count; i++)
                {
                    dgv0.Rows.Add();// 240204-5-31
                    dgv0.Rows[i].Cells[0].Value = $"{con.ds.Tables[0].Rows[i][0]}";
                    dgv0.Rows[i].Cells[1].Value = $"{con.ds.Tables[0].Rows[i][1]}";
                }
                lblFillVal(label8, dgv0);
            }
        }

        private void dgv0IniSet()
        {
            fn.SetDGV(dgv0, false, 500, false, 12, 12, 35, false, 40, DEF_CON.DBLUE, DEF_CON.LIGHT_BLUE);
            string[] sHeader = { "LotNo", "GRADE" };  // { "LotNo", "伝票No(出荷No)" };

            dgv0.ColumnCount = sHeader.Length;
            dgv0.Columns[0].Width = 140;
            dgv0.AllowUserToAddRows = true;
            dgv0.Columns[1].Visible = false;
            dgv0.RowCount = 1;
            for (int i = 0; i < sHeader.Length; i++)
            {
                dgv0.Columns[i].HeaderText = sHeader[i];
            }
            dgv0.Width = dgv0.Columns[0].Width  + 30;
        }

        


        private void SetTooltip()
        {
            //ToolTipを作成する
            //ToolTip1 = new ToolTip(this.components);

            //フォームにcomponentsがない場合
            ToolTip1 = new ToolTip();

            #region ToolTipの設定を行う
            //ToolTipが表示されるまでの時間
            ToolTip1.InitialDelay = 200;
            //ToolTipが表示されている時に、別のToolTipを表示するまでの時間
            ToolTip1.ReshowDelay = 500;
            //ToolTipを表示する時間
            ToolTip1.AutoPopDelay = 10000;
            //フォームがアクティブでない時でもToolTipを表示する
            ToolTip1.ShowAlways = true;
            #endregion

            //Button1とButton2にToolTipが表示されるようにする
            ToolTip1.SetToolTip(btnClose, "戾る");

            ToolTip1.SetToolTip(button2, "表示更新");
            ToolTip1.SetToolTip(button3, "期間変更");
            //ToolTip1.SetToolTip(button4, "納品書出力");
        }

        private void lblFillVal(Label lbl, DataGridView dgv)
        {
            int ir = 0;
            for(int i = 0; i < dgv.Rows.Count; i++)
            {
                if ($"{dgv.Rows[i].Cells[0].Value}".Length > 0) ir++;
            }
            lbl.Text = $"{ir}件";
            lbl.Left = dgv.Left + dgv.Width - lbl.Width;
        }

        private void btn_Click(object sender, EventArgs e)
        {
            // button 6
            Button btn = (Button)sender;
            int bnum = int.Parse(btn.Name.Substring(6));
            switch (bnum)
            {
                case 1: openURL(); break; // Manual
                case 2: delAllRows(); break; // 更新
                case 3: regist(); break;
                case 4: delRow(); break;
                case 5: moveList(); break;
                case 6: importXls(); break;
            }
        }

        private void importXls()
        {
            if (true)
            {
                string[] snd = {"J&T環境から送付されたエクセルファイルを取込みます。"
                + "\r\n「製品原料紐付一覧...」を指定して下さい。",""};
                string[] rcv = promag_frm2.F05_YN.ShowMiniForm(snd);
                if (rcv[0].Length == 0) return;
            }
            //OpenFileDialogクラスのインスタンスを作成
            OpenFileDialog ofd = new OpenFileDialog();
            #region oFDの設定
            //はじめのファイル名を指定する
            //はじめに「ファイル名」で表示される文字列を指定する
            ofd.FileName = "";
            //はじめに表示されるフォルダを指定する
            //指定しない（空の文字列）の時は、現在のディレクトリが表示される
            ofd.InitialDirectory = @"C:\";
            ofd.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            //[ファイルの種類]に表示される選択肢を指定する
            //指定しないとすべてのファイルが表示される
            ofd.Filter = "EXCELファイル(*.xlsx)|*.xlsx;*.xls";
            //[ファイルの種類]ではじめに選択されるものを指定する
            //2番目の「すべてのファイル」が選択されているようにする
            ofd.FilterIndex = 1;
            //タイトルを設定する
            ofd.Title = "開くファイルを選択してください";
            //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
            ofd.RestoreDirectory = true;
            //存在しないファイルの名前が指定されたとき警告を表示する
            //デフォルトでTrueなので指定する必要はない
            ofd.CheckFileExists = true;
            //存在しないパスが指定されたとき警告を表示する
            //デフォルトでTrueなので指定する必要はない
            ofd.CheckPathExists = true;
            #endregion
            //ダイアログを表示する
            if (ofd.ShowDialog() == DialogResult.Cancel) return;
            // ワークブックの読み込み
            using (var wb = new ClosedXML.Excel.XLWorkbook(ofd.FileName))
            {
                try
                {
                    this.Refresh();
                    // シートを確認
                    int iMatch = 0;
                    foreach(var ws in wb.Worksheets)
                    {
                        if (ws.Name == "Measure") iMatch++;
                        if (ws.Name == "Inspect") iMatch++;
                        if (ws.Name == "Himoduke") iMatch++;
                    }
                    if(iMatch < 3)
                    {
                        string[] snd = { "ファイルの種類が違います。", "false" };
                        _ = promag_frm2.F05_YN.ShowMiniForm(snd);
                        return;
                    }
                    // すべてのシートについてループ
                    foreach (var ws in wb.Worksheets)
                    {
                        // ws.Nameが小山,MRF,出荷ロット一覧表 の時に処理に入る RET1 Measure Inspect Himoduke
                        // まとめて書き込むSQLを定義
                        string sSql = string.Empty; // 全体
                        string sIns = string.Empty; // Insertの頭
                        string sVal = string.Empty; // value部
                        if (ws.Name == "Measure")
                        {
                            int irow = 0;
                            // テーブル作成
                            var table = ws.RangeUsed().AsTable();

                            #region 参考：セル指定方法／行数と列数の取得
                            // ws.Cell(ir, ic + 1).Value = 1
                            // ws.Cell("D4").Value = "Range:D4"; 
                            // 参考：行数と列数の取得
                            // var ir = ws.RangeUsed().RowCount();
                            // var ic = ws.RangeUsed().ColumnCount();
                            #endregion

                            // Measure の列は4
                            // ITEM_SEQ	LOT_NO	MVALUE	INSPECT_SEQ
                            #region Excel操作 DB登録用変数定義
                            var ic = ws.RangeUsed().ColumnCount();
                            // 書込み済みフラグを記入する列番号
                            var ir = ws.RangeUsed().RowCount();
                            
                            sSql = string.Empty; sIns = string.Empty; sVal = string.Empty;
                            #endregion

                            sIns = "INSERT INTO t_f_measured_value ("
                                    + "LOT_NO, M_VAL, ITEM_SEQ, INSPECT_SEQ, LOCATION, UPD_ID, UPD_DATE, LGC_DEL) VALUES ";

                            mydb.kyDb con = new mydb.kyDb();

                            // データを行単位で取得
                            foreach (var dataRow in table.DataRange.Rows())
                            {
                                // 行ごとの処理
                                if (irow == 0) { irow++; continue; }
                                #region 変数定義
                                int itemseq = int.Parse($"{dataRow.Cell(1).Value}");
                                string LotNo = dataRow.Cell(2).Value.ToString();
                                string val = dataRow.Cell(3).Value.ToString();
                                string insSeq = dataRow.Cell(4).Value.ToString();
                                #endregion
                                #region 検査項目番号の変換 JTK => JPT
                                switch (itemseq)
                                {
                                    case 1: itemseq = 15; break;
                                    case 2: itemseq = 15; break;
                                    case 3: itemseq = 7; break;
                                    case 4: itemseq = 8; break;
                                    case 5: itemseq = 8; break;
                                    
                                    case 6: itemseq = 9; break;
                                    case 7: itemseq = 11; break;
                                    case 8: itemseq = 13; break;
                                    case 9: itemseq = 8; break;
                                    case 10: itemseq = 2; break;
                                    case 11: itemseq = 5; break;
                                    case 12: itemseq = 1; break;
                                }
                                #endregion

                                if (val.Length == 0) val = "NULL";
                                if (insSeq.Length == 0) insSeq = "NULL";

                                /*
                                SEQ	SEQ	int(10) unsigned auto_increment
                                LOT_NO	LOT_NO	varchar(20)
                                測定値	M_VAL	double
                                測定日	MEASURING_DATE	datetime
                                項目ID	ITEM_SEQ	smallint(5) unsigned
                                採用FLG	ADOPTION	decimal(1,0)
                                検査SEQ	INSPECT_SEQ	int(10) unsigned
                                拠点番号	LOCATION	tinyint(3) unsigned          jtk = 24
                                合否	SUCCESS	char(1)
                                備考	BIKOU_SEQ	int(10) unsigned
                                更新者ID	UPD_ID	varchar(5)
                                更新日	UPD_DATE	datetime
                                論理削除	LGC_DEL	char(1)                                 */
                                if(LotNo.Length > 0)
                                {
                                    sVal += $",('{LotNo}',{val},{itemseq},{insSeq},24,'{usr.id}',NOW(), '0')";
                                }
                            }
                            #region foreachが完了し残りをInsする
                            if (sVal.Length > 0)
                            {
                                sSql = sIns + sVal.Substring(1) + ";";
                                sSql += "DELETE FROM t_f_measured_value "
                                    + " WHERE SEQ NOT IN ("
                                    + "SELECT mxSEQ from (SELECT MAX(SEQ) mxSEQ FROM t_f_measured_value m GROUP BY m.ITEM_SEQ,m.LOT_NO,m.INSPECT_SEQ) tmp)";
                                string[] sql = sSql.Split(';');
                                con.ExecSql(false, DEF_CON.Constr(), sql);
                            }
                            #endregion
                        }
                        else if (ws.Name == "Inspect")
                        {
                            int irow = 0;
                            // テーブル作成
                            var table = ws.RangeUsed().AsTable();
                            sSql = string.Empty; sIns = string.Empty; sVal = string.Empty;
                            #region 参考：セル指定方法／行数と列数の取得
                            // ws.Cell(ir, ic + 1).Value = 1
                            // ws.Cell("D4").Value = "Range:D4"; 
                            // 参考：行数と列数の取得
                            // var ir = ws.RangeUsed().RowCount();
                            // var ic = ws.RangeUsed().ColumnCount();
                            #endregion

                            // SEQ	INSPECT_WEIGHT	RET  // Inspect の列は3
                            #region Excel操作 DB登録用変数定義
                            var ic = ws.RangeUsed().ColumnCount();
                            // 書込み済みフラグを記入する列番号
                            var ir = ws.RangeUsed().RowCount();
                            
                            #endregion

                            sIns = "INSERT INTO t_f_inspect ("
                                    + "INSPECT_WEIGHT, RET, ADD_SEQ, LOCATION,REG_ID,REG_DATE, UPD_ID, UPD_DATE, LGC_DEL) VALUES ";

                            // 行数カウント
                            mydb.kyDb con = new mydb.kyDb();

                            // データを行単位で取得
                            foreach (var dataRow in table.DataRange.Rows())
                            {
                                // 行ごとの処理
                                if (irow == 0) { irow++; continue; }
                                #region 変数定義
                                int iSeq = int.Parse($"{dataRow.Cell(1).Value}");
                                string iWeight = dataRow.Cell(2).Value.ToString();
                                string ret = dataRow.Cell(3).Value.ToString();
                                
                                #endregion

                                if (iWeight.Length == 0) iWeight = "NULL";
                                if (ret.Length == 0) ret = "NULL";

                                /*
                                SEQ	SEQ	int(10) unsigned auto_increment
                                検査重量	INSPECT_WEIGHT	double
                                色目判定	COLOR_RET	char(1)
                                判定結果	RET	tinyint(3) unsigned
                                ランク	_RANK	char(1)
                                備考SEQ	BIKOU_SEQ	int(10) unsigned
                                追加SEQ	ADD_SEQ	int(10) unsigned
                                拠点番号	LOCATION	smallint(5) unsigned
                                登録者ID	REG_ID	char(5)
                                登録日時	REG_DATE	datetime
                                更新者ID	UPD_ID	char(5)
                                更新日時	UPD_DATE	datetime
                                削除フラグ	LGC_DEL	char(1)                          */
                                if (iSeq > 0)
                                {
                                    sVal += $",({iWeight},{ret},{iSeq},24,'{usr.id}',NOW(),'{usr.id}',NOW(), '0')";
                                }
                            }
                            #region foreachが完了し残りをInsする
                            if (sVal.Length > 0)
                            {
                                sSql = sIns + sVal.Substring(1) + ";";
                                sSql += "DELETE FROM t_f_inspect "
                                    + " WHERE SEQ NOT IN ("
                                    + "SELECT mxSEQ from ("
                                    + "SELECT MAX(SEQ) mxSEQ FROM t_f_inspect m GROUP BY m.INSPECT_WEIGHT,m.ADD_SEQ"
                                    + ") tmp)";
                                string[] sql = sSql.Split(';');
                                con.ExecSql(false, DEF_CON.Constr(), sql);
                            }
                            #endregion
                        }
                        else if (ws.Name == "Himoduke")
                        {
                            int irow = 0;
                            // テーブル作成
                            var table = ws.RangeUsed().AsTable();
                            sSql = string.Empty; sIns = string.Empty; sVal = string.Empty;
                            #region 参考：セル指定方法／行数と列数の取得
                            // ws.Cell(ir, ic + 1).Value = 1
                            // ws.Cell("D4").Value = "Range:D4"; 
                            // 参考：行数と列数の取得
                            // var ir = ws.RangeUsed().RowCount();
                            // var ic = ws.RangeUsed().ColumnCount();
                            #endregion

                            #region Excel操作 DB登録用変数定義
                            var ic = ws.RangeUsed().ColumnCount();
                            // 書込み済みフラグを記入する列番号
                            var ir = ws.RangeUsed().RowCount();

                            #endregion

                            sIns = "INSERT INTO t_material_source ("
                                 + "LOT_NO, BALE_LOT_NO, oWT, iWT,LOADING_DATE,REPRNM,TYPERNM,SHAPERNM,UPD_ID,UPD_DATE,LGC_DEL) VALUES ";

                            /*
                            SEQ	        SEQ	int(10) unsigned auto_increment
                            LotNo	    LOT_NO	varchar(20)
                            BLotNo	    BALE_LOT_NO	varchar(20)
                            充填重量	oWT	smallint(5) unsigned
                            投入重量	iWT	smallint(5) unsigned
                            搬入日	    LOADING_DATE	datetime
                            回収元	    REPRNM	varchar(40)
                            回収区分	TYPERNM	varchar(20)
                            形状	    SHAPERNM	varchar(20)
                            更新者ID	UPD_ID	char(5)
                            更新日時	UPD_DATE	datetime
                            削除フラグ	LGC_DEL	char(1)                         */

                            mydb.kyDb con = new mydb.kyDb();

                            // データを行単位で取得
                            // LOT_NO	BALE_LOT_NO	oWT	iWT	LOADING_DATE	REPRNM	TYPERNM	SHAPERNM   // Himoduke の列は8
                            foreach (var dataRow in table.DataRange.Rows())
                            {
                                // 行ごとの処理
                                if (irow == 0) { irow++; continue; }
                                #region 変数定義
                                string lotNo = dataRow.Cell(1).Value.ToString();
                                string bLotNo = dataRow.Cell(2).Value.ToString();
                                string owt = dataRow.Cell(3).Value.ToString();
                                string iwt = dataRow.Cell(4).Value.ToString();
                                string lodDt = dataRow.Cell(5).Value.ToString();
                                string REPNM = dataRow.Cell(6).Value.ToString();
                                string TYPNM = dataRow.Cell(7).Value.ToString();
                                string SHAPE = dataRow.Cell(8).Value.ToString();
                                #endregion
                                sVal += $",('{lotNo}','{bLotNo}',{owt},{iwt},'{lodDt}','{REPNM}','{TYPNM}','{SHAPE}','{usr.id}',NOW(),'0')";
                            }
                            #region foreachが完了し残りをInsする
                            if (sVal.Length > 0)
                            {
                                sSql = sIns + sVal.Substring(1) + ";";
                                sSql += "DELETE FROM t_material_source "
                                    + " WHERE SEQ NOT IN ("
                                    + "SELECT mxSEQ from ("
                                    + "SELECT MAX(SEQ) mxSEQ FROM t_material_source m GROUP BY m.LOT_NO,m.BALE_LOT_NO,oWT,iWT"
                                    + ") tmp)";
                                string[] sql = sSql.Split(';');
                                con.ExecSql(false, DEF_CON.Constr(), sql);
                            }
                            #endregion
                        }
                        else if (ws.Name == "LotList")
                        {
                            int irow = 0;
                            // テーブル作成
                            var table = ws.RangeUsed().AsTable();
                            sSql = string.Empty; sIns = string.Empty; sVal = string.Empty;
                            #region 参考：セル指定方法／行数と列数の取得
                            // ws.Cell(ir, ic + 1).Value = 1
                            // ws.Cell("D4").Value = "Range:D4"; 
                            // 参考：行数と列数の取得
                            // var ir = ws.RangeUsed().RowCount();
                            // var ic = ws.RangeUsed().ColumnCount();
                            #endregion

                            #region Excel操作 DB登録用変数定義
                            var ic = ws.RangeUsed().ColumnCount();
                            // 書込み済みフラグを記入する列番号
                            var ir = ws.RangeUsed().RowCount();

                            #endregion

                            sIns = "INSERT INTO t_receipt ("
                                 + "LOT_NO, SUPPLIER, GRADE, iLOCATION,LOCATION,SHIP_SEQ,REG_ID,REG_DATE,UPD_ID,UPD_DATE,LGC_DEL) VALUES ";

                         /* SEQ	        SEQ	        int(10) unsigned auto_increment
                            LotNo	    LOT_NO	    varchar(20)
                            仕入先	    SUPPLIER	varchar(40)
                            グレード	GRADE	    varchar(20)
                            入荷拠点	iLOCATION	smallint(5) unsigned
                            荷受拠点	LOCATION	smallint(5) unsigned
                            受入番号	SHIP_SEQ	int(10) unsigned
                            登録者ID	REG_ID	    char(5)
                            登録日時	REG_DATE	datetime
                            更新者ID	UPD_ID	    char(5)
                            更新日時	UPD_DATE	datetime
                            削除フラグ	LGC_DEL	char(1)                      */

                            mydb.kyDb con = new mydb.kyDb();

                            // データを行単位で取得
                            // LOT_NO	GRADE	SHIP_SEQ                            // LotList の列は3
                            foreach (var dataRow in table.DataRange.Rows())
                            {
                                // 行ごとの処理
                                if (irow == 0) { irow++; continue; }
                                #region 変数定義
                                string lotNo = dataRow.Cell(1).Value.ToString();
                                string Grade = dataRow.Cell(2).Value.ToString();
                                string sSeq = dataRow.Cell(3).Value.ToString();

                                string Supp = "Ｊ＆Ｔ環境";
                                string iLoc = "24";
                                string Loc = "2";
                                
                                #endregion
                                sVal += $",('{lotNo}','{Supp}','{Grade}',{iLoc},{Loc},{sSeq},'{usr.id}',NOW(),'{usr.id}',NOW(),'0')";
                            }
                            #region foreachが完了し残りをInsする
                            if (sVal.Length > 0)
                            {
                                sSql = sIns + sVal.Substring(1) + ";";
                                sSql += "DELETE FROM t_receipt "
                                    + " WHERE SEQ NOT IN ("
                                    + "SELECT mxSEQ from ("
                                    + "SELECT MAX(SEQ) mxSEQ FROM t_receipt m GROUP BY m.LOT_NO"
                                    + ") tmp)";
                                string[] sql = sSql.Split(';');
                                con.ExecSql(false, DEF_CON.Constr(), sql);
                            }
                            #endregion
                        }
                        else continue;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("エラーが発生しました。登録が完全ではありません。\r\n"
                        + "エクセルファイルで確認して下さい。" + ex.Message);
                }
                finally
                {
                    wb.Save();
                    string[] snd = { "取込終了", "false" };
                    _ = promag_frm2.F05_YN.ShowMiniForm(snd);
                }
            }
        }

        private void moveList()
        {
            string lot = textBox1.Text;
            string lcn = textBox2.Text;
            string grade = textBox3.Text;
            if(lcn.Length == 0 || lot.Length == 0 || grade.Length == 0)
            {
                string[] snd = { "協栄G以外の製品の場合は、LotNo、搬入元、グレードの記入が必須です。", "false" };
                _ = promag_frm2.F05_YN.ShowMiniForm(snd);
                return;
            }
            dgv0.Rows.Add();// 240204-5-31
            for (int i = 0; i < dgv0.Rows.Count; i++)
            {
                if ($"{dgv0.Rows[i].Cells[0].Value}".Length == 0)
                {
                    dgv0.Rows[i].Cells[0].Value = lot;
                    dgv0.Rows[i].Cells[1].Value = grade;
                    
                    break;
                }
            }
        }

        private void delRow()
        {
            DataGridView dgv = dgv0;
            if(dgv.SelectedCells.Count == 0)
            {
                string[] snd = { "一覧から削除する行を選んでください。", "false" };
                _ = promag_frm.F05_YN.ShowMiniForm(snd);return;
            }
            if (true)
            {
                string[] snd = { "選択中のLotを削除します。", "" };
                string[] rcv = promag_frm.F05_YN.ShowMiniForm(snd);
                if (rcv[0].Length == 0) return;
            }
            foreach(DataGridViewCell cl in dgv.SelectedCells)
            {
                dgv.Rows.RemoveAt(cl.RowIndex);
            }
        }
        
        private void delAllRows()
        {
            if (true)
            {
                string[] snd = { "表示されているLotを全て削除します。宜しいですか？", "" };
                string[] rcv = promag_frm.F05_YN.ShowMiniForm(snd);
                if (rcv[0].Length == 0) return;
            }
            dgv0IniSet();
            textBox1.Text = "";textBox2.Text = "";
        }

        private string setLcn(string lcn)
        {
            string s = string.Empty;
            switch (lcn)
            {
                case "東日本": s = "51";break;
                case "西日本": s = "54"; break;
                case "宇都宮": s = "14"; break;
                case "小山":   s = "2";  break;
                case "MRF":    s = "3";  break;
                case "栃木":   s = "1";  break;
                case "TPR":    s = "29"; break;
                case "Ｊ＆Ｔ環境": s = "24"; break;
            }

            return s;
        }

        private void regist()
        {
            /* LotNo	LOT_NO	varchar(20)
                品名	ITEM	varchar(40)           // 不要
                仕入先	SUPPLIER	varchar(40)
                重量	WEIGHT	smallint(5) unsigned // 不要
                伝票番号	DENNO	varchar(20)      // 不要
                入荷拠点	iLOCATION	smallint(5) unsigned
                荷受拠点	LOCATION	smallint(5) unsigned
                登録者ID	REG_ID	char(5)
                登録日時	REG_DATE	datetime
                更新者ID	UPD_ID	char(5)
                更新日時	UPD_DATE	datetime
                削除フラグ	LGC_DEL	char(1) */

            if (true)
            {
                string[] snd = { "表示されているLotを登録します。", "" };
                string[] rcv = promag_frm.F05_YN.ShowMiniForm(snd);
                if (rcv[0].Length == 0) return;
            }
            string lcn = textBox2.Text;
            string item = textBox3.Text;
            if (lcn.Length == 0)
            {
                if (radioButton6.Checked)
                {
                    string[] snd = { "「その他」の場合は搬入元が必須です。", "false" };
                    _ = promag_frm2.F05_YN.ShowMiniForm(snd); return;
                }
                lcn = "NULL";
            }
            else lcn = $"'{lcn}'";

            if(item.Length == 0)
            {
                if (radioButton6.Checked)
                {
                    string[] snd = { "「その他」の場合はグレードが必須です。", "false" };
                    _ = promag_frm2.F05_YN.ShowMiniForm(snd);return;
                }
            }

            mydb.kyDb con = new mydb.kyDb();
            string getCount = "SELECT IFNULL(MAX(r.SHIP_SEQ),100000000) + 1 max FROM kyoei.t_receipt r WHERE r.SHIP_SEQ > 100000000";
            int Count = con.iGetCount(getCount, DEF_CON.Constr());

            if (_ShipSeq.Length == 0) _ShipSeq = $"{Count}";


            string Ins = "INSERT INTO t_receipt ("
                + "LOT_NO, SHIP_SEQ, SUPPLIER, GRADE, iLOCATION, LOCATION, REG_ID, REG_DATE, UPD_ID, UPD_DATE, LGC_DEL,ITEM"
                + ") VALUES ";
            
            if (_inLcn == null || _inLcn.Length == 0) _inLcn = "NULL";
            string cont = string.Empty;
            for(int i = 0; i < dgv0.Rows.Count; i++)
            {
                string lot = $"{dgv0.Rows[i].Cells[0].Value}";
                string grade = $"{dgv0.Rows[i].Cells[1].Value}";
                if (grade.Length == 0) grade = "NULL";
                else grade = $"'{grade}'";
                if (lot.Length == 0) continue;
                lot = $"'{lot}'";
                cont += $",({lot}, {_ShipSeq}, {lcn}, {grade}, {_inLcn}, {_lcn}, '{usr.id}', NOW(), '{usr.id}', NOW(), '0','{item}')";
            }

            if(cont.Length == 0)
            {
                string[] snd = { "登録可能なLotNoがありません。", "false" };
                _ = promag_frm.F05_YN.ShowMiniForm(snd); return;
            }
            else
            {
                Ins += cont.Substring(1) + ";";
                
                if (con.ExecSql(true, DEF_CON.Constr(), Ins).IndexOf("エラー") >= 0)
                {
                    string[] snd = { "登録に失敗しました。", "false" };
                    _ = promag_frm.F05_YN.ShowMiniForm(snd); return;
                }
                else
                {
                    string del = "DELETE FROM t_receipt WHERE SEQ NOT IN (SELECT min_SEQ from (SELECT MIN(SEQ) min_SEQ FROM t_receipt GROUP BY LOT_NO, SUPPLIER) tmp);";
                    con.ExecSql(false, DEF_CON.Constr(), del);
                    string[] snd = { "登録しました。", "false" };
                    _ = promag_frm.F05_YN.ShowMiniForm(snd);
                    _ShipSeq = "";
                    _inLcn = "";
                    if(!radioButton5.Checked || !radioButton6.Checked)
                    textBox1.Text = "";
                    textBox2.Text = "";
                    dgv0IniSet();
                }
            }
        }
        

        private void openURL()
        {
            string s = @"\\10.100.10.20\share\www\manual\cont\";
            s += $"{DEF_CON.prjName}-{this.Name.Substring(0, 4)}";
                        
            string fl = s + ".html";
            bool b = System.IO.File.Exists(fl);
            if (!b) fl = s + ".pdf";
            b = System.IO.File.Exists(fl);
            if (!b) return;

            System.Diagnostics.Process ps = new System.Diagnostics.Process();
            ps.StartInfo.FileName = fl;
            ps.Start();
        }

        #region dgvクリック
        private void dgv_MouseDown(object sender, MouseEventArgs e)
        {
            bdgvCellClk = fn.dgvCellClk(sender, e);
        }

        private void dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (!bdgvCellClk) return;
            DataGridView dgv = (DataGridView)sender;
            int ir = e.RowIndex; int ic = e.ColumnIndex;
            try
            {
                if (dgv.Rows[ir].Cells[ic].Value == null) return;
                string s = dgv.Rows[ir].Cells[ic].Value.ToString();

                //アプリケーション終了後、クリップボードからデータは削除される
                Clipboard.SetDataObject(s);
                //フォーム上の座標でマウスポインタの位置を取得する
                //画面座標でマウスポインタの位置を取得する
                System.Drawing.Point sp = System.Windows.Forms.Cursor.Position;
                //画面座標をクライアント座標に変換する
                System.Drawing.Point cp = this.PointToClient(sp);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.GetType().FullName, "一覧クリックエラー");
            }
        }

        private void dgv_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            //DataGridView dgv = (DataGridView)sender;
            //// データ欄以外は何もしない
            //if (!bdgvCellClk) return;
            
        }
        #endregion

        #region CLOSE処理
        // 3) btnClose
        private void FRM_Closing(object sender, FormClosingEventArgs e)
        {
            if (bClose)
            {
                string[] snd = { "「戻る」ボタンで画面を閉じてください。", "false" };
                _ = promag_frm.F05_YN.ShowMiniForm(snd);
                // return;

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
            if (this.ReturnValue == null) this.ReturnValue = new string[] { "" };
            closing();
        }
        // CLOSE処理の最後
        private void FRM_Closed(object sender, FormClosedEventArgs e)
        {

        }

        #endregion

        private void dgv0_Scroll(object sender, ScrollEventArgs e)
        {
            //dgv0.HorizontalScrollingOffset = dgvF.HorizontalScrollingOffset;

        }

        private void dgv0_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {

        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.BackColor = System.Drawing.Color.Yellow;
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.BackColor = System.Drawing.Color.Empty;
        }

        private void addRows(string para)
        {
            mydb.kyDb con = new mydb.kyDb();
            string lcn;
            con.GetData(getLot2(para), DEF_CON.Constr());
            var lots = new List<string>();
            if (con.ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < con.ds.Tables[0].Rows.Count; i++)
                {
                    string lot = $"{con.ds.Tables[0].Rows[i][0]}";
                    if (_ShipSeq.Length > 0 && _ShipSeq != $"{con.ds.Tables[0].Rows[i][2]}")
                    {
                        string[] snd = { "出荷番号が違うLotが混じっています。登録を続けますか？", "" };
                        string[] rcv = promag_frm.F05_YN.ShowMiniForm(snd);
                        if (rcv[0].Length == 0) return;
                    }
                    lots.Add(lot);
                    if (i == 0)
                    {
                        lcn = $"{con.ds.Tables[0].Rows[i][1]}";
                        textBox2.Text = lcn;
                        _inLcn = setLcn(lcn);
                    }
                    _ShipSeq = $"{con.ds.Tables[0].Rows[i][2]}";
                }
            }

            for (int i = 0; i < lots.Count; i++)
            {

                for (int j = 0; j < dgv0.Rows.Count; j++)
                {
                    if ($"{dgv0.Rows[j].Cells[0].Value}".Length == 0)
                    {
                        dgv0.Rows.Add();// 240204-5-31
                        dgv0.Rows[j].Cells[0].Value = lots[i];
                        break;
                    }
                }
            }
        }
            

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // textBox2.Text = "";
            if (radioButton6.Checked) return;
            if (radioButton5.Checked)
            {
                textBox2.Text = "Ｊ＆Ｔ環境";
                _inLcn = "24";
            }
            TextBox tb = (TextBox)sender;
            this.BackColor = System.Drawing.Color.Empty;
            mydb.kyDb con = new mydb.kyDb();
            string lcn;
            if (int.TryParse(tb.Text, out int itb))
            {
                bool bnext = false;
                if (radioButton1.Checked && tb.Text.Length >= 4) bnext = true;
                else if (radioButton5.Checked || radioButton6.Checked) bnext = false;
                else
                {
                    if (radioButton5.Checked)
                    {
                        string[] snd = { "J&T環境のものはLotNoのみ受付けます。", "false" };
                        _ = promag_frm2.F05_YN.ShowMiniForm(snd); return;
                    }
                    if (tb.Text.Length >= 5) bnext = true;
                }
                if(bnext) addRows($"{itb}");
            } // KJ, Ju, JTK, JTH,Tで10桁
            else if ((tb.Text.IndexOf("KJ") >= 0 && tb.Text.Length == 10)
                || (tb.Text.IndexOf("Ju") >= 0 && tb.Text.Length == 10) 
                || (tb.Text.IndexOf("JTH") >= 0 && tb.Text.Length == 11)
                || (tb.Text.IndexOf("JTK") >= 0 && tb.Text.Length == 10) 
                || (tb.Text.IndexOf("T") >= 0 && tb.Text.Length == 10))
            {
                if(tb.Text.IndexOf("JTK") >= 0)
                {
                    string[] snd = { "J&T環境のものはLotNoのみ受付けます。", "false" };
                    _ = promag_frm2.F05_YN.ShowMiniForm(snd);return;
                }
                addRows($"{tb.Text}");
            }
            else if (tb.Text.Length == 11 || tb.Text.Length == 12)
            {
                try
                {
                    con.GetData(getLot(tb.Text), DEF_CON.Constr());
                    if (con.ds.Tables[0].Rows.Count > 0)
                    {
                        string lot = $"{con.ds.Tables[0].Rows[0][0]}";
                        lcn = $"{con.ds.Tables[0].Rows[0][1]}";
                        textBox2.Text = lcn;
                        _inLcn = setLcn(lcn);   // 搬入元からBASE_SEQを変数にセット
                        if (_ShipSeq.Length > 0 && _ShipSeq != $"{con.ds.Tables[0].Rows[0][2]}")
                        {
                            string[] snd = { "出荷番号が違うLotが混じっています。登録を続けますか？", "" };
                            string[] rcv = promag_frm.F05_YN.ShowMiniForm(snd);
                            if (rcv[0].Length == 0) return;
                        }

                        _ShipSeq = $"{con.ds.Tables[0].Rows[0][2]}";
                        dgv0.Rows.Add();// 240204-5-31
                        for (int i = 0; i < dgv0.Rows.Count; i++)
                        {
                            if ($"{dgv0.Rows[i].Cells[0].Value}".Length == 0)
                            {
                                dgv0.Rows[i].Cells[0].Value = lot;
                                textBox1.Text = "";

                                break;
                            }
                        }
                    }
                    else
                    {

                    }
                }
                catch { }
            }
            lblFillVal(label8, dgv0);

        }

        private string getLot(string lot)
        {
            string s;
            s =
                "SELECT DISTINCT"
                 + " tmp.LOT_NO"
                 + " ,tmp.拠点"
                 + " ,tmp.SHIP_SEQ"
                 + " ,tmp.VOUCHER_NO"
                 + " FROM"
                 + " ("
            #region
                 + " SELECT "
                 + " p.LOT_NO"
                 + " ,'小山' 拠点"
                 + " ,p.SHIP_SEQ"
                 + " ,p.SHIP_SEQ VOUCHER_NO"
                 + " FROM kyoei.t_product p"
                 + $" WHERE p.LOT_NO = '{lot}'"
            #endregion
                 + " UNION"
            #region
                 + " SELECT "
                 + " p.LOT_NO"
                 + " ,'MRF' 拠点"
                 + " ,p.SHIP_SEQ"
                 + " ,p.SHIP_SEQ VOUCHER_NO"
                 + " FROM kyoei.t_m_product p"
                 + $" WHERE p.LOT_NO = '{lot}'"
            #endregion
                 + " UNION"
            #region
                 + " SELECT "
                 + " p.LOT_NO"
                 + " ,'栃木' 拠点"
                 + " ,p.SHIP_SEQ"
                 + " ,p.SHIP_SEQ VOUCHER_NO"
                 + " FROM kyoei.t_t_product p"
                 + $" WHERE p.LOT_NO = '{lot}'"
                 + " "
                 + " UNION"
                 + " SELECT "
                 + " p.LOT_NO"
                 + " ,'西日本' 拠点"
                 + " ,p.SHIP_SEQ"
                 + " ,v.VOUCHER_NO"
                 + " FROM kyoei.kj_p_product p"
                 + " LEFT JOIN kyoei.m_j_voucher v ON v.SHIPPING_SEQ = p.SHIP_SEQ"
                 + $" WHERE p.LOT_NO = '{lot}'"
            #endregion
                 + " UNION"
            #region
                 + " SELECT "
                 + " p.LOT_NO"
                 + " ,'宇都宮' 拠点"
                 + " ,CAST(p.IN_VOUCHER_NO AS UNSIGNED) SHIP_SEQ"
                 + " ,v.VOUCHER_NO"
                 + " FROM kyoei.mj_u_product p"
                 + " LEFT JOIN kyoei.mj_voucher v  ON v.SHIPPING_SEQ = CAST(p.IN_VOUCHER_NO AS UNSIGNED)"
                 + $" WHERE p.LOT_NO = '{lot}'"
            #endregion
                 + " UNION"
            #region
                 + " SELECT "
                 + " p.LOT_NO"
                 + " ,CASE WHEN LEFT(p.LOT_NO,1) = 'H' THEN '東日本' ELSE 'TPR' END 拠点"
                 + " ,p.SHIP_SEQ"
                 + " ,v.VOUCHER_NO"
                 + " FROM kyoei.mj_product p"
                 + " LEFT JOIN kyoei.m_j_voucher v ON v.SHIPPING_SEQ = p.SHIP_SEQ"
                 + $" WHERE p.LOT_NO = '{lot}'"
            #endregion
                 + " UNION"
            #region
                 + " SELECT "
                 + " p.LOT_NO"
                 + " ,'西日本' 拠点"
                 + " ,p.SHIP_SEQ"
                 + " ,v.VOUCHER_NO"
                 + " FROM kyoei.kj_t_product p"
                 + " LEFT JOIN kyoei.m_j_voucher v ON v.SHIPPING_SEQ = p.SHIP_SEQ"
                 + $" WHERE p.LOT_NO = '{lot}'"
            #endregion
                 + " ) tmp;";

            return s;
        }

        private string getLot2(string lot)
        {
            string s;
            int shipSeq = -9;
            if(int.TryParse(lot, out int seq))
            {
                shipSeq = seq;
            }

            if (radioButton1.Checked)
            {
                if (shipSeq > 0)
                {
                    s =
                    "SELECT DISTINCT "
                     + " p.LOT_NO"
                     + " , '宇都宮' 拠点"
                     + " , CAST(p.IN_VOUCHER_NO AS UNSIGNED) SHIP_SEQ"
                     + " FROM      kyoei.mj_u_product p "
                     + " WHERE"
                     + $" p.IN_VOUCHER_NO = '{shipSeq}'";
                }
                else
                {
                    s =

                    "SELECT DISTINCT "
                     + " p.LOT_NO"
                     + " , '宇都宮' 拠点"
                     + " , CAST(p.IN_VOUCHER_NO AS UNSIGNED) SHIP_SEQ"
                     + " FROM      kyoei.mj_u_product p "
                     + " LEFT JOIN kyoei.mj_voucher v ON v.SHIPPING_SEQ = CAST(p.IN_VOUCHER_NO AS UNSIGNED) "
                     + " WHERE"
                     + $" v.VOUCHER_NO = '{lot}'";
                }
            }
            else if (radioButton2.Checked || radioButton3.Checked)
            {
                if (shipSeq > 0)
                {
                    s =
                       " SELECT DISTINCT "
                     + " p.LOT_NO"
                     + " , CASE WHEN LEFT (p.LOT_NO, 1) = 'H' THEN '東日本' ELSE 'TPR' END 拠点"
                     + " , p.SHIP_SEQ"
                     + " FROM      kyoei.mj_product p "
                     + " WHERE"
                     + $" p.SHIP_SEQ = {shipSeq}";
                }
                else
                {
                    s =
                       " SELECT DISTINCT "
                     + " p.LOT_NO"
                     + " , CASE WHEN LEFT (p.LOT_NO, 1) = 'H' THEN '東日本' ELSE 'TPR' END 拠点"
                     + " , p.SHIP_SEQ"
                     + " FROM      kyoei.mj_product p "
                     + " LEFT JOIN kyoei.m_j_voucher v ON v.SHIPPING_SEQ = p.SHIP_SEQ "
                     + " WHERE"
                     + $"  v.VOUCHER_NO = '{lot}'";
                }
            }
            else // if (radioButton4.Checked)
            {
                if (shipSeq > 0)
                {
                    s =
                "SELECT DISTINCT *"
                 + " FROM"
                 + " ("
                 + " SELECT "
                 + " p.LOT_NO"
                 + " ,'西日本' 拠点"
                 + " ,p.SHIP_SEQ"
                 + " FROM kyoei.kj_p_product p"
                 + " WHERE"
                 + $" p.SHIP_SEQ = {shipSeq}"
                 + " UNION "
                 + " SELECT "
                 + " p.LOT_NO"
                 + " , '西日本' 拠点"
                 + " , p.SHIP_SEQ"
                 + " FROM   kyoei.kj_t_product p "
                 + " WHERE"
                 + $" p.SHIP_SEQ = {shipSeq}"
                 + ") tmp";
                }
                else
                {
                    s =
                    "SELECT DISTINCT *"
                     + " FROM"
                     + " ("
                     + " SELECT "
                     + " p.LOT_NO"
                     + " ,'西日本' 拠点"
                     + " ,p.SHIP_SEQ"
                     + " FROM kyoei.kj_p_product p"
                     + " LEFT JOIN kyoei.m_j_voucher v ON v.SHIPPING_SEQ = p.SHIP_SEQ"
                     + " WHERE"
                     + $" v.VOUCHER_NO = '{lot}'"
                     + " UNION "
                     + " SELECT "
                     + " p.LOT_NO"
                     + " , '西日本' 拠点"
                     + " , p.SHIP_SEQ"
                     + " FROM   kyoei.kj_t_product p "
                     + " LEFT JOIN kyoei.m_j_voucher v  ON v.SHIPPING_SEQ = p.SHIP_SEQ "
                     + " WHERE"
                     + $" v.VOUCHER_NO = '{lot}'"
                     + ") tmp";
                }
            }

            return s;
        }

        private void rb_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            bool b = false;
            if (rb.Name == "radioButton6") b = true;
            textBox2.Enabled = b; textBox3.Enabled = b;button5.Enabled = b;
        }

        private void tb_Enter(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            DateTime dt = DateTime.Today;
            dt = dt.AddYears(-1);
            if(checkBox1.Checked && tb.Name == "textBox1")
            {
                string[] snd = { "LotNo入力", "" };
                string[] rcv = promag_frm2.F03_KEY_INPUT.ShowMiniForm(this, snd);
                if (rcv[0] != null && rcv[0].Length > 0) tb.Text = rcv[0];
            }
            else if (tb.Name == "textBox1")
            {
                tb.BackColor = System.Drawing.Color.Yellow;
            }
            else if (tb.Name == "textBox2")
            {
                string s =
                            "SELECT"
                     + " c.KEY_CD"
                     + " ,c.DISP_NM 搬入元"
                     + " FROM kyoei.m_code c"
                     + " LEFT JOIN"
                     + " ("
                     + " SELECT"
                     + " ai.SUPPLIER"
                     + " ,COUNT(*) cnt"
                     + " FROM kyoei.m_arrival_inf ai"
                     + $" WHERE ai.SCHEDULED_DATE >= '{dt:yyyy/MM/dd}'"
                     + " GROUP BY ai.SUPPLIER"
                     + " ) ai    ON ai.SUPPLIER = c.DISP_NM"
                     + " WHERE c.SECTION_CD = 23"
                     + " AND c.KEY_CD NOT IN (1,2,3,4,5,6,14,81,90,91,12,21,55,89,84,29)"
                     + " AND c.LGC_DEL = '0'"
                     + " ORDER BY ai.cnt desc;";

                string[] sendT = { "搬入元", s, "0", "", usr.id, "", $"{usr.iDB}" };
                string[] sRcvT = promag_frm2.F02_SEL_LIST.ShowMiniForm(this, sendT);
                if (sRcvT[0].Length > 0)
                {
                    tb.Text = sRcvT[1];
                }
            }
            else if(tb.Name == "textBox3" && checkBox2.Checked)
            {
                string[] snd = { "LotNo入力", "" };
                string[] rcv = promag_frm2.F03_KEY_INPUT.ShowMiniForm(this, snd);
                if (rcv[0] != null && rcv[0].Length > 0) tb.Text = rcv[0];
            }
            else if (tb.Name == "textBox3")
            {
                string s =
                            "SELECT"
                         + " c.KEY_CD"
                         + " ,c.DISP_NM グレード"
                         + " FROM kyoei.m_code c"
                         + " LEFT JOIN"
                         + " ("
                         + " SELECT"
                         + " ai.GRADE"
                         + " ,COUNT(*) cnt"
                         + " FROM kyoei.m_arrival_inf ai"
                         + $" WHERE ai.SCHEDULED_DATE >= '{dt:yyyy/MM/dd}'"
                         + " AND (MID(ai.GRADE,3,1) != '-'"
                         + " AND MID(ai.GRADE,6,1) != '-')"
                         + " GROUP BY ai.GRADE"
                         + " ) ai    ON ai.GRADE = c.DISP_NM"
                         + " WHERE c.SECTION_CD = 24"
                         + " AND (MID(c.DISP_NM,3,1) != '-'"
                         + " AND MID(c.DISP_NM,6,1) != '-')"
                         + " AND c.LGC_DEL = '0'"
                         + " ORDER BY ai.cnt desc;";

                string[] sendT = { "搬入元", s, "0", "", usr.id, "", $"{usr.iDB}" };
                string[] sRcvT = promag_frm2.F02_SEL_LIST.ShowMiniForm(this, sendT);
                if (sRcvT[0].Length > 0)
                {
                    tb.Text = sRcvT[1];
                }
            }
        }
    }
}
