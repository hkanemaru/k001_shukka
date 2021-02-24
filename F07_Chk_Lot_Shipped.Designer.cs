namespace k001_shukka
{
    partial class F07_Chk_Lot_Shipped
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(F07_Chk_Lot_Shipped));
            this.lblRCaption = new System.Windows.Forms.Label();
            this.lblCaption = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.dgv0 = new System.Windows.Forms.DataGridView();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.bs0 = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dgv0)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bs0)).BeginInit();
            this.SuspendLayout();
            // 
            // lblRCaption
            // 
            this.lblRCaption.AutoSize = true;
            this.lblRCaption.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblRCaption.Font = new System.Drawing.Font("BIZ UDゴシック", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblRCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblRCaption.Location = new System.Drawing.Point(484, 0);
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
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("BIZ UDゴシック", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label2.Location = new System.Drawing.Point(424, 96);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 16);
            this.label2.TabIndex = 13;
            this.label2.Text = "Count";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // dgv0
            // 
            this.dgv0.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dgv0.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv0.Location = new System.Drawing.Point(12, 115);
            this.dgv0.Name = "dgv0";
            this.dgv0.RowTemplate.Height = 21;
            this.dgv0.Size = new System.Drawing.Size(460, 458);
            this.dgv0.TabIndex = 12;
            // 
            // textBox5
            // 
            this.textBox5.Font = new System.Drawing.Font("Meiryo UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.textBox5.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox5.Location = new System.Drawing.Point(187, 58);
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new System.Drawing.Size(140, 28);
            this.textBox5.TabIndex = 58;
            this.textBox5.TextChanged += new System.EventHandler(this.textBox5_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("BIZ UDゴシック", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label1.Location = new System.Drawing.Point(120, 64);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 16);
            this.label1.TabIndex = 59;
            this.label1.Text = "LotNo.";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.button1.FlatAppearance.BorderColor = System.Drawing.Color.LightGray;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("Meiryo UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.button1.Image = global::k001_shukka.Properties.Resources.Revive;
            this.button1.Location = new System.Drawing.Point(414, 22);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(58, 58);
            this.button1.TabIndex = 60;
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
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
            this.btnClose.TabIndex = 8;
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // F07_Chk_Lot_Shipped
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 585);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox5);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.dgv0);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.lblRCaption);
            this.Controls.Add(this.lblCaption);
            this.Font = new System.Drawing.Font("Meiryo UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "F07_Chk_Lot_Shipped";
            this.Text = "F07_Chk_Lot_Shipped";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FRM_Closing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FRM_Closed);
            this.Load += new System.EventHandler(this.Fxx_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgv0)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bs0)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblRCaption;
        private System.Windows.Forms.Label lblCaption;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DataGridView dgv0;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.BindingSource bs0;
        private System.Windows.Forms.Button button1;
    }
}