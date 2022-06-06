namespace k001_shukka
{
    partial class F14_SHIP_PERMIT_LIST
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(F14_SHIP_PERMIT_LIST));
            this.lblRCaption = new System.Windows.Forms.Label();
            this.lblCaption = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.dgv0 = new System.Windows.Forms.DataGridView();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.bs0 = new System.Windows.Forms.BindingSource(this.components);
            this.dgvF = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgv0)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bs0)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvF)).BeginInit();
            this.SuspendLayout();
            // 
            // lblRCaption
            // 
            this.lblRCaption.AutoSize = true;
            this.lblRCaption.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblRCaption.Font = new System.Drawing.Font("BIZ UDゴシック", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblRCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblRCaption.Location = new System.Drawing.Point(945, 0);
            this.lblRCaption.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblRCaption.Name = "lblRCaption";
            this.lblRCaption.Size = new System.Drawing.Size(0, 16);
            this.lblRCaption.TabIndex = 7;
            this.lblRCaption.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblCaption
            // 
            this.lblCaption.AutoSize = true;
            this.lblCaption.Font = new System.Drawing.Font("BIZ UDゴシック", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblCaption.Location = new System.Drawing.Point(0, 0);
            this.lblCaption.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCaption.Name = "lblCaption";
            this.lblCaption.Size = new System.Drawing.Size(0, 19);
            this.lblCaption.TabIndex = 6;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("BIZ UDゴシック", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label8.Location = new System.Drawing.Point(885, 106);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(48, 16);
            this.label8.TabIndex = 51;
            this.label8.Text = "Count";
            this.label8.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // dgv0
            // 
            this.dgv0.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dgv0.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv0.Location = new System.Drawing.Point(12, 161);
            this.dgv0.Name = "dgv0";
            this.dgv0.RowTemplate.Height = 21;
            this.dgv0.Size = new System.Drawing.Size(921, 488);
            this.dgv0.TabIndex = 48;
            this.dgv0.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_CellClick);
            this.dgv0.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_CellDoubleClick);
            this.dgv0.ColumnWidthChanged += new System.Windows.Forms.DataGridViewColumnEventHandler(this.dgv0_ColumnWidthChanged);
            this.dgv0.Scroll += new System.Windows.Forms.ScrollEventHandler(this.dgv0_Scroll);
            this.dgv0.MouseDown += new System.Windows.Forms.MouseEventHandler(this.dgv_MouseDown);
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button3.BackColor = System.Drawing.Color.WhiteSmoke;
            this.button3.FlatAppearance.BorderColor = System.Drawing.Color.LightGray;
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button3.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.button3.Image = global::k001_shukka.Properties.Resources.CngCal;
            this.button3.Location = new System.Drawing.Point(774, 28);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(67, 67);
            this.button3.TabIndex = 53;
            this.button3.Text = "期間変更";
            this.button3.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.button3.UseVisualStyleBackColor = false;
            this.button3.Click += new System.EventHandler(this.btn_Click);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.BackColor = System.Drawing.Color.WhiteSmoke;
            this.button2.FlatAppearance.BorderColor = System.Drawing.Color.LightGray;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Font = new System.Drawing.Font("Meiryo UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.button2.Image = global::k001_shukka.Properties.Resources.reflesh;
            this.button2.Location = new System.Drawing.Point(866, 28);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(67, 67);
            this.button2.TabIndex = 52;
            this.button2.Text = "更新";
            this.button2.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnClose.FlatAppearance.BorderColor = System.Drawing.Color.LightGray;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Meiryo UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.btnClose.Image = global::k001_shukka.Properties.Resources.back;
            this.btnClose.Location = new System.Drawing.Point(12, 28);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(58, 58);
            this.btnClose.TabIndex = 1;
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // dgvF
            // 
            this.dgvF.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dgvF.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvF.Location = new System.Drawing.Point(12, 125);
            this.dgvF.Name = "dgvF";
            this.dgvF.RowTemplate.Height = 21;
            this.dgvF.Size = new System.Drawing.Size(921, 30);
            this.dgvF.TabIndex = 66;
            this.dgvF.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvF_CellValueChanged);
            this.dgvF.Scroll += new System.Windows.Forms.ScrollEventHandler(this.dgvF_Scroll);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("BIZ UDゴシック", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label1.Location = new System.Drawing.Point(12, 106);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 16);
            this.label1.TabIndex = 67;
            this.label1.Text = "期間";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // checkBox1
            // 
            this.checkBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(553, 96);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(114, 23);
            this.checkBox1.TabIndex = 68;
            this.checkBox1.Text = "依頼済を表示";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.cB_CheckedChanged);
            // 
            // checkBox2
            // 
            this.checkBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(682, 96);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(112, 23);
            this.checkBox2.TabIndex = 69;
            this.checkBox2.Text = "承認済を隠す";
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.CheckedChanged += new System.EventHandler(this.cB_CheckedChanged);
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.button1.FlatAppearance.BorderColor = System.Drawing.Color.LightGray;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.button1.Image = ((System.Drawing.Image)(resources.GetObject("button1.Image")));
            this.button1.Location = new System.Drawing.Point(663, 28);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(67, 67);
            this.button1.TabIndex = 70;
            this.button1.Text = "出力";
            this.button1.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // F14_SHIP_PERMIT_LIST
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(945, 661);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dgvF);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.dgv0);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.lblRCaption);
            this.Controls.Add(this.lblCaption);
            this.Font = new System.Drawing.Font("Meiryo UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "F14_SHIP_PERMIT_LIST";
            this.Text = "F14_SHIP_PERMIT_LIST";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FRM_Closing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FRM_Closed);
            this.Load += new System.EventHandler(this.FRM_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgv0)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bs0)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvF)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblRCaption;
        private System.Windows.Forms.Label lblCaption;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.DataGridView dgv0;
        private System.Windows.Forms.BindingSource bs0;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.DataGridView dgvF;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.Button button1;
    }
}