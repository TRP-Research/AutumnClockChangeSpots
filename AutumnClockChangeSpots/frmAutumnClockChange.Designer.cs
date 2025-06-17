namespace AutumnClockChangeSpots
{
    partial class frmAutumnClockChange
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.REportDateInfoLabel = new System.Windows.Forms.Label();
            this.btnRunAll = new System.Windows.Forms.Button();
            this.btnRunChannel = new System.Windows.Forms.Button();
            this.lueChannel = new DevExpress.XtraEditors.LookUpEdit();
            this.label2 = new System.Windows.Forms.Label();
            this.dtpYear = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.button1 = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lueChannel.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.REportDateInfoLabel);
            this.panel1.Controls.Add(this.btnRunAll);
            this.panel1.Controls.Add(this.btnRunChannel);
            this.panel1.Controls.Add(this.lueChannel);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.dtpYear);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(800, 133);
            this.panel1.TabIndex = 0;
            // 
            // REportDateInfoLabel
            // 
            this.REportDateInfoLabel.AutoSize = true;
            this.REportDateInfoLabel.Location = new System.Drawing.Point(68, 20);
            this.REportDateInfoLabel.Name = "REportDateInfoLabel";
            this.REportDateInfoLabel.Size = new System.Drawing.Size(58, 13);
            this.REportDateInfoLabel.TabIndex = 6;
            this.REportDateInfoLabel.Text = "select year";
            // 
            // btnRunAll
            // 
            this.btnRunAll.Location = new System.Drawing.Point(626, 53);
            this.btnRunAll.Name = "btnRunAll";
            this.btnRunAll.Size = new System.Drawing.Size(89, 23);
            this.btnRunAll.TabIndex = 5;
            this.btnRunAll.Text = "Run All Channels";
            this.btnRunAll.UseVisualStyleBackColor = true;
            this.btnRunAll.Click += new System.EventHandler(this.btnRunAll_Click);
            // 
            // btnRunChannel
            // 
            this.btnRunChannel.Location = new System.Drawing.Point(508, 53);
            this.btnRunChannel.Name = "btnRunChannel";
            this.btnRunChannel.Size = new System.Drawing.Size(92, 23);
            this.btnRunChannel.TabIndex = 4;
            this.btnRunChannel.Text = "Run Channel";
            this.btnRunChannel.UseVisualStyleBackColor = true;
            this.btnRunChannel.Click += new System.EventHandler(this.btnRunChannel_Click);
            // 
            // lueChannel
            // 
            this.lueChannel.Location = new System.Drawing.Point(299, 54);
            this.lueChannel.Name = "lueChannel";
            this.lueChannel.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.lueChannel.Properties.Columns.AddRange(new DevExpress.XtraEditors.Controls.LookUpColumnInfo[] {
            new DevExpress.XtraEditors.Controls.LookUpColumnInfo("WorkName", "Channel Name"),
            new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Carrier", "Carrier"),
            new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Region", "Region"),
            new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Platform", "Platform")});
            this.lueChannel.Properties.DisplayMember = "WorkName";
            this.lueChannel.Properties.NullText = "";
            this.lueChannel.Size = new System.Drawing.Size(175, 20);
            this.lueChannel.TabIndex = 3;
            this.lueChannel.Closed += new DevExpress.XtraEditors.Controls.ClosedEventHandler(this.lueChannel_Closed);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(217, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "select channel";
            // 
            // dtpYear
            // 
            this.dtpYear.CustomFormat = "\"yyyy\"";
            this.dtpYear.Location = new System.Drawing.Point(100, 54);
            this.dtpYear.Name = "dtpYear";
            this.dtpYear.ShowUpDown = true;
            this.dtpYear.Size = new System.Drawing.Size(91, 20);
            this.dtpYear.TabIndex = 1;
            this.dtpYear.ValueChanged += new System.EventHandler(this.dtpYear_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(35, 58);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "select year";
            // 
            // progressBar1
            // 
            this.progressBar1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressBar1.Location = new System.Drawing.Point(0, 110);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(800, 23);
            this.progressBar1.TabIndex = 1;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(508, 15);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(89, 23);
            this.button1.TabIndex = 7;
            this.button1.Text = "test";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // frmAutumnClockChange
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 133);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.panel1);
            this.Name = "frmAutumnClockChange";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.frmAutumnClockChange_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lueChannel.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private DevExpress.XtraEditors.LookUpEdit lueChannel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker dtpYear;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button btnRunAll;
        private System.Windows.Forms.Button btnRunChannel;
        private System.Windows.Forms.Label REportDateInfoLabel;
        private System.Windows.Forms.Button button1;
    }
}

