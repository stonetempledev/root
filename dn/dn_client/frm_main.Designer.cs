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
      this.btn_close = new System.Windows.Forms.Button();
      this.ntf_main = new System.Windows.Forms.NotifyIcon(this.components);
      this.ms_main = new System.Windows.Forms.MenuStrip();
      this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.mi_exit = new System.Windows.Forms.ToolStripMenuItem();
      this.tmr_state = new System.Windows.Forms.Timer(this.components);
      this.ms_main.SuspendLayout();
      this.SuspendLayout();
      // 
      // lbl_title
      // 
      this.lbl_title.BackColor = System.Drawing.Color.GreenYellow;
      this.lbl_title.Cursor = System.Windows.Forms.Cursors.Hand;
      this.lbl_title.Dock = System.Windows.Forms.DockStyle.Top;
      this.lbl_title.Font = new System.Drawing.Font("Segoe UI Light", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lbl_title.ForeColor = System.Drawing.SystemColors.MenuHighlight;
      this.lbl_title.Location = new System.Drawing.Point(0, 0);
      this.lbl_title.Name = "lbl_title";
      this.lbl_title.Size = new System.Drawing.Size(609, 29);
      this.lbl_title.TabIndex = 0;
      this.lbl_title.Text = "Deepa Notes Client";
      this.lbl_title.DoubleClick += new System.EventHandler(this.lbl_title_DoubleClick);
      this.lbl_title.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lbl_title_MouseDown);
      this.lbl_title.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lbl_title_MouseMove);
      this.lbl_title.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lbl_title_MouseUp);
      // 
      // btn_close
      // 
      this.btn_close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btn_close.BackColor = System.Drawing.Color.GreenYellow;
      this.btn_close.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btn_close.FlatAppearance.BorderSize = 0;
      this.btn_close.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btn_close.Font = new System.Drawing.Font("Comic Sans MS", 15F);
      this.btn_close.ForeColor = System.Drawing.Color.White;
      this.btn_close.Location = new System.Drawing.Point(572, -3);
      this.btn_close.Margin = new System.Windows.Forms.Padding(0);
      this.btn_close.Name = "btn_close";
      this.btn_close.Size = new System.Drawing.Size(39, 32);
      this.btn_close.TabIndex = 2;
      this.btn_close.Text = "X";
      this.btn_close.TextAlign = System.Drawing.ContentAlignment.TopCenter;
      this.btn_close.UseVisualStyleBackColor = false;
      this.btn_close.Click += new System.EventHandler(this.btn_close_Click);
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
      // ms_main
      // 
      this.ms_main.AutoSize = false;
      this.ms_main.BackColor = System.Drawing.Color.DarkGray;
      this.ms_main.Dock = System.Windows.Forms.DockStyle.None;
      this.ms_main.Font = new System.Drawing.Font("Segoe UI Light", 9F);
      this.ms_main.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
      this.ms_main.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
      this.ms_main.Location = new System.Drawing.Point(-1, 29);
      this.ms_main.Name = "ms_main";
      this.ms_main.Size = new System.Drawing.Size(610, 27);
      this.ms_main.TabIndex = 3;
      this.ms_main.Text = "menuStrip1";
      // 
      // fileToolStripMenuItem
      // 
      this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mi_exit});
      this.fileToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI Light", 10F);
      this.fileToolStripMenuItem.ForeColor = System.Drawing.Color.White;
      this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
      this.fileToolStripMenuItem.Size = new System.Drawing.Size(41, 23);
      this.fileToolStripMenuItem.Text = "&File";
      // 
      // mi_exit
      // 
      this.mi_exit.BackColor = System.Drawing.Color.DarkGray;
      this.mi_exit.ForeColor = System.Drawing.Color.White;
      this.mi_exit.Name = "mi_exit";
      this.mi_exit.Size = new System.Drawing.Size(152, 24);
      this.mi_exit.Text = "&Exit...";
      this.mi_exit.Click += new System.EventHandler(this.mi_exit_Click);
      // 
      // tmr_state
      // 
      this.tmr_state.Enabled = true;
      this.tmr_state.Interval = 300000;
      this.tmr_state.Tick += new System.EventHandler(this.tmr_state_Tick);
      // 
      // frm_main
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.ClientSize = new System.Drawing.Size(609, 396);
      this.ControlBox = false;
      this.Controls.Add(this.btn_close);
      this.Controls.Add(this.lbl_title);
      this.Controls.Add(this.ms_main);
      this.Font = new System.Drawing.Font("Segoe UI Light", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MainMenuStrip = this.ms_main;
      this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "frm_main";
      this.Load += new System.EventHandler(this.frm_main_Load);
      this.Resize += new System.EventHandler(this.frm_main_Resize);
      this.ms_main.ResumeLayout(false);
      this.ms_main.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Label lbl_title;
    private System.Windows.Forms.Button btn_close;
    private System.Windows.Forms.NotifyIcon ntf_main;
    private System.Windows.Forms.MenuStrip ms_main;
    private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem mi_exit;
    private System.Windows.Forms.Timer tmr_state;
  }
}

