namespace fsynch {
  partial class frm_synch {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      this.lbl_title = new System.Windows.Forms.Label();
      this.btn_ok = new System.Windows.Forms.Button();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.SuspendLayout();
      // 
      // lbl_title
      // 
      this.lbl_title.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lbl_title.BackColor = System.Drawing.Color.PaleGreen;
      this.lbl_title.Font = new System.Drawing.Font("Segoe UI Light", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lbl_title.Location = new System.Drawing.Point(-1, 3);
      this.lbl_title.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.lbl_title.Name = "lbl_title";
      this.lbl_title.Size = new System.Drawing.Size(648, 42);
      this.lbl_title.TabIndex = 0;
      this.lbl_title.Text = "Deepa-Synch";
      this.lbl_title.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // btn_ok
      // 
      this.btn_ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btn_ok.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btn_ok.Location = new System.Drawing.Point(539, 393);
      this.btn_ok.Name = "btn_ok";
      this.btn_ok.Size = new System.Drawing.Size(95, 36);
      this.btn_ok.TabIndex = 1;
      this.btn_ok.Text = "OK";
      this.btn_ok.UseVisualStyleBackColor = true;
      this.btn_ok.Click += new System.EventHandler(this.btn_ok_Click);
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Location = new System.Drawing.Point(-2, 40);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(651, 14);
      this.groupBox1.TabIndex = 2;
      this.groupBox1.TabStop = false;
      // 
      // frm_synch
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 21F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.ClientSize = new System.Drawing.Size(646, 441);
      this.ControlBox = false;
      this.Controls.Add(this.lbl_title);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.btn_ok);
      this.Font = new System.Drawing.Font("Segoe UI Light", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "frm_synch";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.TopMost = true;
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Label lbl_title;
    private System.Windows.Forms.Button btn_ok;
    private System.Windows.Forms.GroupBox groupBox1;
  }
}

