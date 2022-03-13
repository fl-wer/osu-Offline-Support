namespace Offline_Support
{
    partial class ApiKey
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ApiKey));
            this.apiKeyMainPicture = new System.Windows.Forms.PictureBox();
            this.explanationText = new System.Windows.Forms.Label();
            this.explanationLinkText = new System.Windows.Forms.LinkLabel();
            this.keyTextBox = new System.Windows.Forms.TextBox();
            this.keySubmit = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.apiKeyMainPicture)).BeginInit();
            this.SuspendLayout();
            // 
            // apiKeyMainPicture
            // 
            this.apiKeyMainPicture.Image = global::Offline_Support.Properties.Resources.apiKeyPicture;
            this.apiKeyMainPicture.Location = new System.Drawing.Point(92, 12);
            this.apiKeyMainPicture.Name = "apiKeyMainPicture";
            this.apiKeyMainPicture.Size = new System.Drawing.Size(204, 204);
            this.apiKeyMainPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.apiKeyMainPicture.TabIndex = 6;
            this.apiKeyMainPicture.TabStop = false;
            // 
            // explanationText
            // 
            this.explanationText.AutoSize = true;
            this.explanationText.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.explanationText.ForeColor = System.Drawing.Color.White;
            this.explanationText.Location = new System.Drawing.Point(21, 229);
            this.explanationText.Name = "explanationText";
            this.explanationText.Size = new System.Drawing.Size(358, 20);
            this.explanationText.TabIndex = 8;
            this.explanationText.Text = "You will need to provide                      to use this app.";
            this.explanationText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // explanationLinkText
            // 
            this.explanationLinkText.ActiveLinkColor = System.Drawing.Color.CornflowerBlue;
            this.explanationLinkText.AutoSize = true;
            this.explanationLinkText.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.explanationLinkText.LinkColor = System.Drawing.Color.CornflowerBlue;
            this.explanationLinkText.Location = new System.Drawing.Point(187, 229);
            this.explanationLinkText.Name = "explanationLinkText";
            this.explanationLinkText.Size = new System.Drawing.Size(87, 20);
            this.explanationLinkText.TabIndex = 9;
            this.explanationLinkText.TabStop = true;
            this.explanationLinkText.Text = "osu! api key";
            this.explanationLinkText.VisitedLinkColor = System.Drawing.Color.CornflowerBlue;
            this.explanationLinkText.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.keyLinkLab_LinkClicked);
            // 
            // keyTextBox
            // 
            this.keyTextBox.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.keyTextBox.Location = new System.Drawing.Point(60, 261);
            this.keyTextBox.Name = "keyTextBox";
            this.keyTextBox.Size = new System.Drawing.Size(270, 22);
            this.keyTextBox.TabIndex = 10;
            this.keyTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // keySubmit
            // 
            this.keySubmit.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.keySubmit.Location = new System.Drawing.Point(92, 299);
            this.keySubmit.Name = "keySubmit";
            this.keySubmit.Size = new System.Drawing.Size(204, 23);
            this.keySubmit.TabIndex = 11;
            this.keySubmit.Text = "SUBMIT";
            this.keySubmit.UseVisualStyleBackColor = true;
            this.keySubmit.Click += new System.EventHandler(this.keySubmitBtn_Click);
            // 
            // ApiKey
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(18)))), ((int)(((byte)(18)))));
            this.ClientSize = new System.Drawing.Size(406, 344);
            this.Controls.Add(this.keySubmit);
            this.Controls.Add(this.keyTextBox);
            this.Controls.Add(this.explanationLinkText);
            this.Controls.Add(this.explanationText);
            this.Controls.Add(this.apiKeyMainPicture);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ApiKey";
            this.Text = "ApiKey";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ApiKey_FormClosed);
            this.Load += new System.EventHandler(this.ApiKey_Load);
            ((System.ComponentModel.ISupportInitialize)(this.apiKeyMainPicture)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox apiKeyMainPicture;
        private System.Windows.Forms.Label explanationText;
        private System.Windows.Forms.LinkLabel explanationLinkText;
        private System.Windows.Forms.TextBox keyTextBox;
        private System.Windows.Forms.Button keySubmit;
    }
}