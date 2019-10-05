namespace molinafy {
  partial class main {
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
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(main));
      this.cms_main = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.events_item = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
      this.logout_item = new System.Windows.Forms.ToolStripMenuItem();
      this.pb_icon = new System.Windows.Forms.PictureBox();
      this.pb_close = new System.Windows.Forms.PictureBox();
      this.lbl_title = new System.Windows.Forms.Label();
      this.ss = new System.Windows.Forms.StatusStrip();
      this.rtb_path = new molinafy.rtb_nodes();
      this.rtbe = new molinafy.rtb_events();
      this.rtb_cmd = new molinafy.rtb_cmds();
      this.split = new System.Windows.Forms.SplitContainer();
      this.tltp = new System.Windows.Forms.ToolTip(this.components);
      this.ac_cmd = new AutocompleteMenuNS.AutocompleteMenu();
      this.tmr_warning = new System.Windows.Forms.Timer(this.components);
      this.cms_main.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pb_icon)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pb_close)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.split)).BeginInit();
      this.split.Panel1.SuspendLayout();
      this.split.Panel2.SuspendLayout();
      this.split.SuspendLayout();
      this.SuspendLayout();
      // 
      // cms_main
      // 
      this.cms_main.BackColor = System.Drawing.Color.White;
      this.cms_main.Font = new System.Drawing.Font("Segoe UI", 12F);
      this.cms_main.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.events_item,
            this.toolStripSeparator1,
            this.logout_item});
      this.cms_main.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
      this.cms_main.Name = "cms_main";
      this.cms_main.ShowImageMargin = false;
      this.cms_main.ShowItemToolTips = false;
      this.cms_main.Size = new System.Drawing.Size(105, 62);
      // 
      // events_item
      // 
      this.events_item.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
      this.events_item.Name = "events_item";
      this.events_item.Size = new System.Drawing.Size(104, 26);
      this.events_item.Text = "Eventi";
      this.events_item.Click += new System.EventHandler(this.events_item_Click);
      // 
      // toolStripSeparator1
      // 
      this.toolStripSeparator1.Name = "toolStripSeparator1";
      this.toolStripSeparator1.Size = new System.Drawing.Size(101, 6);
      // 
      // logout_item
      // 
      this.logout_item.BackColor = System.Drawing.Color.White;
      this.logout_item.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
      this.logout_item.Name = "logout_item";
      this.logout_item.Size = new System.Drawing.Size(104, 26);
      this.logout_item.Text = "Logout";
      this.logout_item.Click += new System.EventHandler(this.logout_item_Click);
      // 
      // pb_icon
      // 
      this.pb_icon.BackColor = System.Drawing.Color.Azure;
      this.pb_icon.Cursor = System.Windows.Forms.Cursors.Hand;
      this.pb_icon.Image = ((System.Drawing.Image)(resources.GetObject("pb_icon.Image")));
      this.pb_icon.Location = new System.Drawing.Point(2, 0);
      this.pb_icon.Name = "pb_icon";
      this.pb_icon.Size = new System.Drawing.Size(32, 32);
      this.pb_icon.TabIndex = 2;
      this.pb_icon.TabStop = false;
      this.pb_icon.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pb_icon_MouseDown);
      // 
      // pb_close
      // 
      this.pb_close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.pb_close.BackColor = System.Drawing.Color.Azure;
      this.pb_close.Cursor = System.Windows.Forms.Cursors.Hand;
      this.pb_close.Image = ((System.Drawing.Image)(resources.GetObject("pb_close.Image")));
      this.pb_close.Location = new System.Drawing.Point(820, 0);
      this.pb_close.Name = "pb_close";
      this.pb_close.Size = new System.Drawing.Size(32, 32);
      this.pb_close.TabIndex = 1;
      this.pb_close.TabStop = false;
      this.pb_close.Click += new System.EventHandler(this.pb_close_Click);
      // 
      // lbl_title
      // 
      this.lbl_title.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lbl_title.BackColor = System.Drawing.Color.Azure;
      this.lbl_title.Cursor = System.Windows.Forms.Cursors.Hand;
      this.lbl_title.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lbl_title.ForeColor = System.Drawing.Color.MidnightBlue;
      this.lbl_title.Location = new System.Drawing.Point(-1, 0);
      this.lbl_title.Name = "lbl_title";
      this.lbl_title.Padding = new System.Windows.Forms.Padding(36, 0, 0, 0);
      this.lbl_title.Size = new System.Drawing.Size(857, 32);
      this.lbl_title.TabIndex = 0;
      this.lbl_title.Text = "Title";
      this.lbl_title.DoubleClick += new System.EventHandler(this.lbl_title_DoubleClick);
      this.lbl_title.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lbl_title_MouseDown);
      this.lbl_title.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lbl_title_MouseMove);
      this.lbl_title.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lbl_title_MouseUp);
      // 
      // ss
      // 
      this.ss.Location = new System.Drawing.Point(0, 427);
      this.ss.Name = "ss";
      this.ss.Size = new System.Drawing.Size(855, 22);
      this.ss.TabIndex = 3;
      this.ss.Text = "statusStrip1";
      // 
      // rtb_path
      // 
      this.rtb_path.BackColor = System.Drawing.Color.AliceBlue;
      this.rtb_path.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.rtb_path.DetectUrls = false;
      this.rtb_path.Font = new System.Drawing.Font("Courier New", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rtb_path.ForeColor = System.Drawing.Color.SteelBlue;
      this.rtb_path.Location = new System.Drawing.Point(2, 3);
      this.rtb_path.Multiline = false;
      this.rtb_path.Name = "rtb_path";
      this.rtb_path.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
      this.rtb_path.Size = new System.Drawing.Size(846, 18);
      this.rtb_path.TabIndex = 4;
      this.rtb_path.Text = "";
      this.tltp.SetToolTip(this.rtb_path, "current nodes path");
      this.rtb_path.WordWrap = false;
      // 
      // rtbe
      // 
      this.rtbe.BackColor = System.Drawing.Color.White;
      this.rtbe.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.rtbe.ColorErr = System.Drawing.Color.Red;
      this.rtbe.ColorInfo = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
      this.rtbe.ColorWar = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
      this.rtbe.DetectUrls = false;
      this.rtbe.Dock = System.Windows.Forms.DockStyle.Fill;
      this.rtbe.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rtbe.ForeColor = System.Drawing.Color.Black;
      this.rtbe.Location = new System.Drawing.Point(0, 0);
      this.rtbe.Name = "rtbe";
      this.rtbe.ReadOnly = true;
      this.rtbe.Size = new System.Drawing.Size(148, 44);
      this.rtbe.TabIndex = 5;
      this.rtbe.Text = "";
      this.rtbe.TimeFormat = "T";
      this.tltp.SetToolTip(this.rtbe, "molinafy events");
      this.rtbe.WordWrap = false;
      // 
      // rtb_cmd
      // 
      this.rtb_cmd.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.rtb_cmd.BackColor = System.Drawing.Color.WhiteSmoke;
      this.rtb_cmd.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.rtb_cmd.DetectUrls = false;
      this.rtb_cmd.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rtb_cmd.ForeColor = System.Drawing.Color.DarkCyan;
      this.rtb_cmd.Location = new System.Drawing.Point(2, 24);
      this.rtb_cmd.Margin = new System.Windows.Forms.Padding(0);
      this.rtb_cmd.Multiline = false;
      this.rtb_cmd.Name = "rtb_cmd";
      this.rtb_cmd.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
      this.rtb_cmd.Size = new System.Drawing.Size(846, 18);
      this.rtb_cmd.TabIndex = 5;
      this.rtb_cmd.Text = "";
      this.tltp.SetToolTip(this.rtb_cmd, "molinafy commands");
      this.rtb_cmd.WordWrap = false;
      // 
      // split
      // 
      this.split.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.split.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.split.Location = new System.Drawing.Point(2, 35);
      this.split.Name = "split";
      this.split.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // split.Panel1
      // 
      this.split.Panel1.Controls.Add(this.rtb_cmd);
      this.split.Panel1.Controls.Add(this.rtb_path);
      // 
      // split.Panel2
      // 
      this.split.Panel2.Controls.Add(this.rtbe);
      this.split.Panel2Collapsed = true;
      this.split.Size = new System.Drawing.Size(852, 390);
      this.split.SplitterDistance = 265;
      this.split.TabIndex = 5;
      // 
      // ac_cmd
      // 
      this.ac_cmd.AutoPopup = false;
      this.ac_cmd.CaptureFocus = true;
      this.ac_cmd.Colors = ((AutocompleteMenuNS.Colors)(resources.GetObject("ac_cmd.Colors")));
      this.ac_cmd.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
      this.ac_cmd.ImageList = null;
      this.ac_cmd.Items = new string[0];
      this.ac_cmd.TargetControlWrapper = null;
      // 
      // tmr_warning
      // 
      this.tmr_warning.Tick += new System.EventHandler(this.tmr_warning_Tick);
      // 
      // main
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.ClientSize = new System.Drawing.Size(855, 449);
      this.ControlBox = false;
      this.Controls.Add(this.split);
      this.Controls.Add(this.ss);
      this.Controls.Add(this.pb_icon);
      this.Controls.Add(this.pb_close);
      this.Controls.Add(this.lbl_title);
      this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "main";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.main_FormClosing);
      this.Load += new System.EventHandler(this.main_Load);
      this.cms_main.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.pb_icon)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pb_close)).EndInit();
      this.split.Panel1.ResumeLayout(false);
      this.split.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.split)).EndInit();
      this.split.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label lbl_title;
    private System.Windows.Forms.PictureBox pb_close;
    private System.Windows.Forms.PictureBox pb_icon;
    private System.Windows.Forms.ContextMenuStrip cms_main;
    private System.Windows.Forms.ToolStripMenuItem logout_item;
    private System.Windows.Forms.StatusStrip ss;
    private AutocompleteMenuNS.AutocompleteMenu ac_path;
    private rtb_nodes rtb_path;
    private System.Windows.Forms.ToolTip tltp;
    private System.Windows.Forms.SplitContainer split;
    private rtb_events rtbe;
    private rtb_cmds rtb_cmd;
    private AutocompleteMenuNS.AutocompleteMenu ac_cmd;
    private System.Windows.Forms.ToolStripMenuItem events_item;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
    private System.Windows.Forms.Timer tmr_warning;
  }
}

