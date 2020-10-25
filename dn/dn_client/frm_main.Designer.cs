namespace dn_client {
  partial class frm_main {
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
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frm_main));
      this.lbl_title = new System.Windows.Forms.Label();
      this.ntf_main = new System.Windows.Forms.NotifyIcon(this.components);
      this.tmr_state = new System.Windows.Forms.Timer(this.components);
      this.btn_close = new System.Windows.Forms.Button();
      this.wb_main = new System.Windows.Forms.WebBrowser();
      this.SuspendLayout();
      // 
      // lbl_title
      // 
      this.lbl_title.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lbl_title.BackColor = System.Drawing.Color.Honeydew;
      this.lbl_title.Cursor = System.Windows.Forms.Cursors.Hand;
      this.lbl_title.Font = new System.Drawing.Font("Segoe UI Light", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lbl_title.ForeColor = System.Drawing.SystemColors.MenuHighlight;
      this.lbl_title.Location = new System.Drawing.Point(0, 0);
      this.lbl_title.Name = "lbl_title";
      this.lbl_title.Size = new System.Drawing.Size(647, 28);
      this.lbl_title.TabIndex = 0;
      this.lbl_title.Text = "Deepa Notes Client";
      this.lbl_title.DoubleClick += new System.EventHandler(this.lbl_title_DoubleClick);
      this.lbl_title.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lbl_title_MouseDown);
      this.lbl_title.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lbl_title_MouseMove);
      this.lbl_title.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lbl_title_MouseUp);
      // 
      // ntf_main
      // 
      this.ntf_main.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
      this.ntf_main.BalloonTipText = "Apri il pannello di controllo";
      this.ntf_main.BalloonTipTitle = "Deepa Notes Client";
      this.ntf_main.Icon = ((System.Drawing.Icon)(resources.GetObject("ntf_main.Icon")));
      this.ntf_main.Text = "deepa notes client";
      this.ntf_main.DoubleClick += new System.EventHandler(this.ntf_main_DoubleClick);
      // 
      // tmr_state
      // 
      this.tmr_state.Enabled = true;
      this.tmr_state.Interval = 300000;
      this.tmr_state.Tick += new System.EventHandler(this.tmr_state_Tick);
      // 
      // btn_close
      // 
      this.btn_close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btn_close.BackColor = System.Drawing.Color.Honeydew;
      this.btn_close.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btn_close.FlatAppearance.BorderSize = 0;
      this.btn_close.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btn_close.Image = ((System.Drawing.Image)(resources.GetObject("btn_close.Image")));
      this.btn_close.Location = new System.Drawing.Point(616, 0);
      this.btn_close.Name = "btn_close";
      this.btn_close.Size = new System.Drawing.Size(29, 26);
      this.btn_close.TabIndex = 4;
      this.btn_close.UseVisualStyleBackColor = false;
      this.btn_close.Click += new System.EventHandler(this.btn_close_Click);
      // 
      // wb_main
      // 
      this.wb_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.wb_main.Location = new System.Drawing.Point(-1, 29);
      this.wb_main.MinimumSize = new System.Drawing.Size(20, 20);
      this.wb_main.Name = "wb_main";
      this.wb_main.ScriptErrorsSuppressed = true;
      this.wb_main.Size = new System.Drawing.Size(647, 435);
      this.wb_main.TabIndex = 5;
      // 
      // frm_main
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.ClientSize = new System.Drawing.Size(645, 465);
      this.ControlBox = false;
      this.Controls.Add(this.wb_main);
      this.Controls.Add(this.btn_close);
      this.Controls.Add(this.lbl_title);
      this.Font = new System.Drawing.Font("Segoe UI Light", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "frm_main";
      this.Load += new System.EventHandler(this.frm_main_Load);
      this.Resize += new System.EventHandler(this.frm_main_Resize);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Label lbl_title;
    private System.Windows.Forms.NotifyIcon ntf_main;
    private System.Windows.Forms.Timer tmr_state;
    private System.Windows.Forms.Button btn_close;
    private System.Windows.Forms.WebBrowser wb_main;
  }
}

