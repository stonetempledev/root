namespace molello {
  partial class frm_nodes {
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frm_nodes));
      this.lbl_title = new System.Windows.Forms.Label();
      this.pb_close = new System.Windows.Forms.PictureBox();
      this.wb_toolbar = new System.Windows.Forms.WebBrowser();
      this.tbl_layout = new System.Windows.Forms.TableLayoutPanel();
      this.wb_body = new System.Windows.Forms.WebBrowser();
      this.wb_menu = new System.Windows.Forms.WebBrowser();
      ((System.ComponentModel.ISupportInitialize)(this.pb_close)).BeginInit();
      this.tbl_layout.SuspendLayout();
      this.SuspendLayout();
      // 
      // lbl_title
      // 
      this.lbl_title.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lbl_title.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
      this.lbl_title.Font = new System.Drawing.Font("Segoe UI", 17F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lbl_title.Location = new System.Drawing.Point(0, -2);
      this.lbl_title.Name = "lbl_title";
      this.lbl_title.Size = new System.Drawing.Size(905, 38);
      this.lbl_title.TabIndex = 0;
      this.lbl_title.Text = "...";
      this.lbl_title.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.lbl_title.DoubleClick += new System.EventHandler(this.lbl_title_DoubleClick);
      this.lbl_title.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lbl_title_MouseDown);
      this.lbl_title.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lbl_title_MouseMove);
      this.lbl_title.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lbl_title_MouseUp);
      // 
      // pb_close
      // 
      this.pb_close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.pb_close.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
      this.pb_close.Cursor = System.Windows.Forms.Cursors.Hand;
      this.pb_close.Image = ((System.Drawing.Image)(resources.GetObject("pb_close.Image")));
      this.pb_close.Location = new System.Drawing.Point(871, 2);
      this.pb_close.Name = "pb_close";
      this.pb_close.Size = new System.Drawing.Size(32, 32);
      this.pb_close.TabIndex = 1;
      this.pb_close.TabStop = false;
      this.pb_close.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pb_close_MouseClick);
      // 
      // wb_toolbar
      // 
      this.wb_toolbar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.wb_toolbar.Location = new System.Drawing.Point(0, 39);
      this.wb_toolbar.MinimumSize = new System.Drawing.Size(20, 20);
      this.wb_toolbar.Name = "wb_toolbar";
      this.wb_toolbar.ScrollBarsEnabled = false;
      this.wb_toolbar.Size = new System.Drawing.Size(905, 27);
      this.wb_toolbar.TabIndex = 2;
      // 
      // tbl_layout
      // 
      this.tbl_layout.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tbl_layout.ColumnCount = 2;
      this.tbl_layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
      this.tbl_layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
      this.tbl_layout.Controls.Add(this.wb_body, 0, 0);
      this.tbl_layout.Controls.Add(this.wb_menu, 0, 0);
      this.tbl_layout.Location = new System.Drawing.Point(1, 66);
      this.tbl_layout.Margin = new System.Windows.Forms.Padding(0);
      this.tbl_layout.Name = "tbl_layout";
      this.tbl_layout.RowCount = 1;
      this.tbl_layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tbl_layout.Size = new System.Drawing.Size(903, 394);
      this.tbl_layout.TabIndex = 7;
      // 
      // wb_body
      // 
      this.wb_body.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.wb_body.Location = new System.Drawing.Point(273, 3);
      this.wb_body.MinimumSize = new System.Drawing.Size(20, 20);
      this.wb_body.Name = "wb_body";
      this.wb_body.ScrollBarsEnabled = false;
      this.wb_body.Size = new System.Drawing.Size(627, 388);
      this.wb_body.TabIndex = 7;
      // 
      // wb_menu
      // 
      this.wb_menu.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.wb_menu.Location = new System.Drawing.Point(3, 3);
      this.wb_menu.MinimumSize = new System.Drawing.Size(20, 20);
      this.wb_menu.Name = "wb_menu";
      this.wb_menu.ScrollBarsEnabled = false;
      this.wb_menu.Size = new System.Drawing.Size(264, 388);
      this.wb_menu.TabIndex = 6;
      // 
      // frm_nodes
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.ClientSize = new System.Drawing.Size(905, 461);
      this.ControlBox = false;
      this.Controls.Add(this.tbl_layout);
      this.Controls.Add(this.wb_toolbar);
      this.Controls.Add(this.pb_close);
      this.Controls.Add(this.lbl_title);
      this.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Margin = new System.Windows.Forms.Padding(4);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "frm_nodes";
      this.Load += new System.EventHandler(this.frm_nodes_Load);
      ((System.ComponentModel.ISupportInitialize)(this.pb_close)).EndInit();
      this.tbl_layout.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Label lbl_title;
    private System.Windows.Forms.PictureBox pb_close;
    private System.Windows.Forms.WebBrowser wb_toolbar;
    private System.Windows.Forms.TableLayoutPanel tbl_layout;
    private System.Windows.Forms.WebBrowser wb_body;
    private System.Windows.Forms.WebBrowser wb_menu;
  }
}