namespace molinafy {
  partial class frm_molinafy {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose (bool disposing) {
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
    private void InitializeComponent () {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frm_molinafy));
      this.wb_main = new System.Windows.Forms.WebBrowser();
      this.lbl_title = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // wb_main
      // 
      this.wb_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.wb_main.Location = new System.Drawing.Point(0, 39);
      this.wb_main.MinimumSize = new System.Drawing.Size(20, 20);
      this.wb_main.Name = "wb_main";
      this.wb_main.ScriptErrorsSuppressed = true;
      this.wb_main.Size = new System.Drawing.Size(559, 314);
      this.wb_main.TabIndex = 0;
      this.wb_main.Navigated += new System.Windows.Forms.WebBrowserNavigatedEventHandler(this.wb_main_Navigated);
      this.wb_main.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.wb_main_Navigating);
      this.wb_main.Validating += new System.ComponentModel.CancelEventHandler(this.wb_main_Validating);
      // 
      // lbl_title
      // 
      this.lbl_title.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lbl_title.BackColor = System.Drawing.Color.SpringGreen;
      this.lbl_title.Cursor = System.Windows.Forms.Cursors.Hand;
      this.lbl_title.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lbl_title.ForeColor = System.Drawing.Color.DimGray;
      this.lbl_title.Image = ((System.Drawing.Image)(resources.GetObject("lbl_title.Image")));
      this.lbl_title.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.lbl_title.Location = new System.Drawing.Point(-5, 0);
      this.lbl_title.Name = "lbl_title";
      this.lbl_title.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
      this.lbl_title.Size = new System.Drawing.Size(564, 36);
      this.lbl_title.TabIndex = 1;
      this.lbl_title.Text = "      MOLINA-FY";
      this.lbl_title.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.lbl_title.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lbl_title_MouseDoubleClick);
      this.lbl_title.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lbl_title_MouseDown);
      this.lbl_title.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lbl_title_MouseMove);
      this.lbl_title.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lbl_title_MouseUp);
      // 
      // frm_molinafy
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(559, 353);
      this.ControlBox = false;
      this.Controls.Add(this.lbl_title);
      this.Controls.Add(this.wb_main);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Name = "frm_molinafy";
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.f_main_FormClosed);
      this.Load += new System.EventHandler(this.f_main_Load);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.WebBrowser wb_main;
    private System.Windows.Forms.Label lbl_title;
  }
}

