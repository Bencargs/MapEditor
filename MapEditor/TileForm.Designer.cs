namespace MapEditor
{
    partial class TileForm
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
            this.terrainCmb = new System.Windows.Forms.ComboBox();
            this.cancel = new System.Windows.Forms.Button();
            this.accept = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // terrainCmb
            // 
            this.terrainCmb.FormattingEnabled = true;
            this.terrainCmb.Location = new System.Drawing.Point(12, 12);
            this.terrainCmb.Name = "terrainCmb";
            this.terrainCmb.Size = new System.Drawing.Size(159, 21);
            this.terrainCmb.TabIndex = 0;
            this.terrainCmb.SelectedIndexChanged += new System.EventHandler(this.ComboBox1_SelectedIndexChanged);
            // 
            // cancel
            // 
            this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancel.Location = new System.Drawing.Point(12, 53);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(75, 23);
            this.cancel.TabIndex = 1;
            this.cancel.Text = "Cancel";
            this.cancel.UseVisualStyleBackColor = true;
            this.cancel.Click += new System.EventHandler(this.Button1_Click);
            // 
            // accept
            // 
            this.accept.Location = new System.Drawing.Point(96, 53);
            this.accept.Name = "accept";
            this.accept.Size = new System.Drawing.Size(75, 23);
            this.accept.TabIndex = 2;
            this.accept.Text = "Accept";
            this.accept.UseVisualStyleBackColor = true;
            this.accept.Click += new System.EventHandler(this.Accept_Click);
            // 
            // TileForm
            // 
            this.AcceptButton = this.accept;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancel;
            this.ClientSize = new System.Drawing.Size(183, 88);
            this.Controls.Add(this.accept);
            this.Controls.Add(this.cancel);
            this.Controls.Add(this.terrainCmb);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "TileForm";
            this.Text = "TileForm";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox terrainCmb;
        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.Button accept;
    }
}