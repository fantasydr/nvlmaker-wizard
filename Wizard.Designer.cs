namespace ResConverter
{
    partial class Wizard
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
            this.gbStep1 = new System.Windows.Forms.GroupBox();
            this.txtTemplate = new System.Windows.Forms.TextBox();
            this.lstTemplate = new System.Windows.Forms.ListBox();
            this.gbStep2 = new System.Windows.Forms.GroupBox();
            this.lstScale = new System.Windows.Forms.ListBox();
            this.txtResolution = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cbResolution = new System.Windows.Forms.ComboBox();
            this.numWidth = new System.Windows.Forms.NumericUpDown();
            this.numHeight = new System.Windows.Forms.NumericUpDown();
            this.gbStep3 = new System.Windows.Forms.GroupBox();
            this.checkFolder = new System.Windows.Forms.CheckBox();
            this.txtFolderName = new System.Windows.Forms.TextBox();
            this.txtProjectName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.gbStep4 = new System.Windows.Forms.GroupBox();
            this.txtReport = new System.Windows.Forms.TextBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnPrev = new System.Windows.Forms.Button();
            this.btnNext = new System.Windows.Forms.Button();
            this.gbStep1.SuspendLayout();
            this.gbStep2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numHeight)).BeginInit();
            this.gbStep3.SuspendLayout();
            this.gbStep4.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbStep1
            // 
            this.gbStep1.Controls.Add(this.txtTemplate);
            this.gbStep1.Controls.Add(this.lstTemplate);
            this.gbStep1.Location = new System.Drawing.Point(12, 12);
            this.gbStep1.Name = "gbStep1";
            this.gbStep1.Size = new System.Drawing.Size(370, 216);
            this.gbStep1.TabIndex = 6;
            this.gbStep1.TabStop = false;
            this.gbStep1.Text = "1.选择主题";
            // 
            // txtTemplate
            // 
            this.txtTemplate.Location = new System.Drawing.Point(190, 19);
            this.txtTemplate.Multiline = true;
            this.txtTemplate.Name = "txtTemplate";
            this.txtTemplate.ReadOnly = true;
            this.txtTemplate.Size = new System.Drawing.Size(165, 185);
            this.txtTemplate.TabIndex = 15;
            // 
            // lstTemplate
            // 
            this.lstTemplate.FormattingEnabled = true;
            this.lstTemplate.ItemHeight = 12;
            this.lstTemplate.Location = new System.Drawing.Point(16, 20);
            this.lstTemplate.Name = "lstTemplate";
            this.lstTemplate.Size = new System.Drawing.Size(168, 184);
            this.lstTemplate.TabIndex = 3;
            // 
            // gbStep2
            // 
            this.gbStep2.Controls.Add(this.lstScale);
            this.gbStep2.Controls.Add(this.txtResolution);
            this.gbStep2.Controls.Add(this.label2);
            this.gbStep2.Controls.Add(this.label1);
            this.gbStep2.Controls.Add(this.cbResolution);
            this.gbStep2.Controls.Add(this.numWidth);
            this.gbStep2.Controls.Add(this.numHeight);
            this.gbStep2.Location = new System.Drawing.Point(404, 12);
            this.gbStep2.Name = "gbStep2";
            this.gbStep2.Size = new System.Drawing.Size(370, 216);
            this.gbStep2.TabIndex = 8;
            this.gbStep2.TabStop = false;
            this.gbStep2.Text = "2.设置分辨率";
            // 
            // lstScale
            // 
            this.lstScale.FormattingEnabled = true;
            this.lstScale.ItemHeight = 12;
            this.lstScale.Location = new System.Drawing.Point(223, 19);
            this.lstScale.Name = "lstScale";
            this.lstScale.Size = new System.Drawing.Size(132, 184);
            this.lstScale.TabIndex = 14;
            // 
            // txtResolution
            // 
            this.txtResolution.Location = new System.Drawing.Point(17, 73);
            this.txtResolution.Multiline = true;
            this.txtResolution.Name = "txtResolution";
            this.txtResolution.ReadOnly = true;
            this.txtResolution.Size = new System.Drawing.Size(190, 130);
            this.txtResolution.TabIndex = 13;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(113, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 12);
            this.label2.TabIndex = 10;
            this.label2.Text = "高";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 50);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(17, 12);
            this.label1.TabIndex = 9;
            this.label1.Text = "宽";
            // 
            // cbResolution
            // 
            this.cbResolution.FormattingEnabled = true;
            this.cbResolution.Location = new System.Drawing.Point(17, 20);
            this.cbResolution.Name = "cbResolution";
            this.cbResolution.Size = new System.Drawing.Size(190, 20);
            this.cbResolution.TabIndex = 8;
            // 
            // numWidth
            // 
            this.numWidth.Location = new System.Drawing.Point(37, 46);
            this.numWidth.Name = "numWidth";
            this.numWidth.Size = new System.Drawing.Size(71, 21);
            this.numWidth.TabIndex = 7;
            // 
            // numHeight
            // 
            this.numHeight.Location = new System.Drawing.Point(136, 46);
            this.numHeight.Name = "numHeight";
            this.numHeight.Size = new System.Drawing.Size(71, 21);
            this.numHeight.TabIndex = 6;
            // 
            // gbStep3
            // 
            this.gbStep3.Controls.Add(this.checkFolder);
            this.gbStep3.Controls.Add(this.txtFolderName);
            this.gbStep3.Controls.Add(this.txtProjectName);
            this.gbStep3.Controls.Add(this.label5);
            this.gbStep3.Location = new System.Drawing.Point(12, 282);
            this.gbStep3.Name = "gbStep3";
            this.gbStep3.Size = new System.Drawing.Size(370, 216);
            this.gbStep3.TabIndex = 9;
            this.gbStep3.TabStop = false;
            this.gbStep3.Text = "3.设定名称";
            // 
            // checkFolder
            // 
            this.checkFolder.AutoSize = true;
            this.checkFolder.Location = new System.Drawing.Point(91, 113);
            this.checkFolder.Name = "checkFolder";
            this.checkFolder.Size = new System.Drawing.Size(108, 16);
            this.checkFolder.TabIndex = 15;
            this.checkFolder.Text = "单独设定目录名";
            this.checkFolder.UseVisualStyleBackColor = true;
            // 
            // txtFolderName
            // 
            this.txtFolderName.Location = new System.Drawing.Point(92, 135);
            this.txtFolderName.MaxLength = 64;
            this.txtFolderName.Name = "txtFolderName";
            this.txtFolderName.ReadOnly = true;
            this.txtFolderName.Size = new System.Drawing.Size(190, 21);
            this.txtFolderName.TabIndex = 14;
            // 
            // txtProjectName
            // 
            this.txtProjectName.Location = new System.Drawing.Point(91, 74);
            this.txtProjectName.MaxLength = 64;
            this.txtProjectName.Name = "txtProjectName";
            this.txtProjectName.ReadOnly = true;
            this.txtProjectName.Size = new System.Drawing.Size(190, 21);
            this.txtProjectName.TabIndex = 13;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(89, 59);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 12;
            this.label5.Text = "项目名称";
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(302, 234);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(80, 32);
            this.btnOK.TabIndex = 10;
            this.btnOK.Text = "完成";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // gbStep4
            // 
            this.gbStep4.Controls.Add(this.txtReport);
            this.gbStep4.Location = new System.Drawing.Point(404, 282);
            this.gbStep4.Name = "gbStep4";
            this.gbStep4.Size = new System.Drawing.Size(370, 216);
            this.gbStep4.TabIndex = 11;
            this.gbStep4.TabStop = false;
            this.gbStep4.Text = "4.生成项目";
            // 
            // txtReport
            // 
            this.txtReport.Location = new System.Drawing.Point(17, 19);
            this.txtReport.Multiline = true;
            this.txtReport.Name = "txtReport";
            this.txtReport.ReadOnly = true;
            this.txtReport.Size = new System.Drawing.Size(338, 185);
            this.txtReport.TabIndex = 15;
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(12, 234);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 32);
            this.btnCancel.TabIndex = 16;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnPrev
            // 
            this.btnPrev.Location = new System.Drawing.Point(216, 234);
            this.btnPrev.Name = "btnPrev";
            this.btnPrev.Size = new System.Drawing.Size(80, 32);
            this.btnPrev.TabIndex = 15;
            this.btnPrev.Text = "上一步";
            this.btnPrev.UseVisualStyleBackColor = true;
            this.btnPrev.Click += new System.EventHandler(this.btnPrev_Click);
            // 
            // btnNext
            // 
            this.btnNext.Location = new System.Drawing.Point(302, 234);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(80, 32);
            this.btnNext.TabIndex = 17;
            this.btnNext.Text = "下一步";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // Wizard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(791, 509);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnPrev);
            this.Controls.Add(this.btnNext);
            this.Controls.Add(this.gbStep3);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.gbStep4);
            this.Controls.Add(this.gbStep2);
            this.Controls.Add(this.gbStep1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Wizard";
            this.Text = "项目新建向导";
            this.gbStep1.ResumeLayout(false);
            this.gbStep1.PerformLayout();
            this.gbStep2.ResumeLayout(false);
            this.gbStep2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numHeight)).EndInit();
            this.gbStep3.ResumeLayout(false);
            this.gbStep3.PerformLayout();
            this.gbStep4.ResumeLayout(false);
            this.gbStep4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbStep1;
        private System.Windows.Forms.ListBox lstTemplate;
        private System.Windows.Forms.GroupBox gbStep2;
        private System.Windows.Forms.TextBox txtResolution;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbResolution;
        private System.Windows.Forms.NumericUpDown numWidth;
        private System.Windows.Forms.NumericUpDown numHeight;
        private System.Windows.Forms.GroupBox gbStep3;
        private System.Windows.Forms.TextBox txtProjectName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox checkFolder;
        private System.Windows.Forms.TextBox txtFolderName;
        private System.Windows.Forms.TextBox txtTemplate;
        private System.Windows.Forms.ListBox lstScale;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.GroupBox gbStep4;
        private System.Windows.Forms.TextBox txtReport;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnPrev;
        private System.Windows.Forms.Button btnNext;
    }
}

