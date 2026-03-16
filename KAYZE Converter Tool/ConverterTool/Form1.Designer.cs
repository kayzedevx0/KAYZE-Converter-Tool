namespace KAYZEConverterTool
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.btnSelectFiles = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.cmbOutputFormat = new System.Windows.Forms.ComboBox();
            this.btnConvert = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
  
            this.btnSelectFiles.Location = new System.Drawing.Point(57, 100);
            this.btnSelectFiles.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnSelectFiles.Name = "btnSelectFiles";
            this.btnSelectFiles.Size = new System.Drawing.Size(457, 75);
            this.btnSelectFiles.TabIndex = 0;
            this.btnSelectFiles.Text = "Select Images (Max 100)";
            this.btnSelectFiles.Click += new System.EventHandler(this.btnSelectFiles_Click);

            this.lblStatus.Location = new System.Drawing.Point(57, 200);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(457, 33);
            this.lblStatus.TabIndex = 1;
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            this.cmbOutputFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbOutputFormat.Location = new System.Drawing.Point(57, 267);
            this.cmbOutputFormat.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmbOutputFormat.Name = "cmbOutputFormat";
            this.cmbOutputFormat.Size = new System.Drawing.Size(141, 33);
            this.cmbOutputFormat.TabIndex = 2;

            this.btnConvert.Location = new System.Drawing.Point(229, 250);
            this.btnConvert.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnConvert.Name = "btnConvert";
            this.btnConvert.Size = new System.Drawing.Size(286, 75);
            this.btnConvert.TabIndex = 3;
            this.btnConvert.Text = "Convert and Save";
            this.btnConvert.Click += new System.EventHandler(this.btnConvert_Click);

            this.progressBar.Location = new System.Drawing.Point(57, 367);
            this.progressBar.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(457, 17);
            this.progressBar.TabIndex = 4;

            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(450, 520);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.btnConvert);
            this.Controls.Add(this.cmbOutputFormat);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnSelectFiles);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "KAYZE Converter Tool";
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Button btnSelectFiles;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.ComboBox cmbOutputFormat;
        private System.Windows.Forms.Button btnConvert;
        private System.Windows.Forms.ProgressBar progressBar;
    }
}
