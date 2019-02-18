namespace molello.forms {
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
      this.wb_toolbar = new System.Windows.Forms.WebBrowser();
      this.tbl_layout = new System.Windows.Forms.TableLayoutPanel();
      this.wb_body = new System.Windows.Forms.WebBrowser();
      this.wb_menu = new System.Windows.Forms.WebBrowser();
      this.wb_title = new System.Windows.Forms.WebBrowser();
      this.tbl_layout.SuspendLayout();
      this.SuspendLayout();
      // 
      // wb_toolbar
      // 
      this.wb_toolbar.AllowNavigation = false;
      this.wb_toolbar.AllowWebBrowserDrop = false;
      this.wb_toolbar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.wb_toolbar.IsWebBrowserContextMenuEnabled = false;
      this.wb_toolbar.Location = new System.Drawing.Point(0, 42);
      this.wb_toolbar.MinimumSize = new System.Drawing.Size(20, 20);
      this.wb_toolbar.Name = "wb_toolbar";
      this.wb_toolbar.ScriptErrorsSuppressed = true;
      this.wb_toolbar.ScrollBarsEnabled = false;
      this.wb_toolbar.Size = new System.Drawing.Size(905, 27);
      this.wb_toolbar.TabIndex = 2;
      this.wb_toolbar.WebBrowserShortcutsEnabled = false;
      // 
      // tbl_layout
      // 
      this.tbl_layout.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tbl_layout.ColumnCount = 2;
      this.tbl_layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 34.21927F));
      this.tbl_layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 65.78073F));
      this.tbl_layout.Controls.Add(this.wb_body, 0, 0);
      this.tbl_layout.Controls.Add(this.wb_menu, 0, 0);
      this.tbl_layout.Location = new System.Drawing.Point(1, 70);
      this.tbl_layout.Margin = new System.Windows.Forms.Padding(0);
      this.tbl_layout.Name = "tbl_layout";
      this.tbl_layout.RowCount = 1;
      this.tbl_layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tbl_layout.Size = new System.Drawing.Size(903, 390);
      this.tbl_layout.TabIndex = 7;
      // 
      // wb_body
      // 
      this.wb_body.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.wb_body.IsWebBrowserContextMenuEnabled = false;
      this.wb_body.Location = new System.Drawing.Point(311, 3);
      this.wb_body.MinimumSize = new System.Drawing.Size(20, 20);
      this.wb_body.Name = "wb_body";
      this.wb_body.ScriptErrorsSuppressed = true;
      this.wb_body.ScrollBarsEnabled = false;
      this.wb_body.Size = new System.Drawing.Size(589, 384);
      this.wb_body.TabIndex = 7;
      // 
      // wb_menu
      // 
      this.wb_menu.AllowNavigation = false;
      this.wb_menu.AllowWebBrowserDrop = false;
      this.wb_menu.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.wb_menu.IsWebBrowserContextMenuEnabled = false;
      this.wb_menu.Location = new System.Drawing.Point(3, 3);
      this.wb_menu.MinimumSize = new System.Drawing.Size(20, 20);
      this.wb_menu.Name = "wb_menu";
      this.wb_menu.ScriptErrorsSuppressed = true;
      this.wb_menu.ScrollBarsEnabled = false;
      this.wb_menu.Size = new System.Drawing.Size(302, 384);
      this.wb_menu.TabIndex = 6;
      this.wb_menu.WebBrowserShortcutsEnabled = false;
      // 
      // wb_title
      // 
      this.wb_title.AllowNavigation = false;
      this.wb_title.AllowWebBrowserDrop = false;
      this.wb_title.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.wb_title.IsWebBrowserContextMenuEnabled = false;
      this.wb_title.Location = new System.Drawing.Point(1, -1);
      this.wb_title.MinimumSize = new System.Drawing.Size(20, 20);
      this.wb_title.Name = "wb_title";
      this.wb_title.ScriptErrorsSuppressed = true;
      this.wb_title.ScrollBarsEnabled = false;
      this.wb_title.Size = new System.Drawing.Size(902, 41);
      this.wb_title.TabIndex = 8;
      this.wb_title.WebBrowserShortcutsEnabled = false;
      // 
      // frm_nodes
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.ClientSize = new System.Drawing.Size(905, 461);
      this.ControlBox = false;
      this.Controls.Add(this.wb_title);
      this.Controls.Add(this.tbl_layout);
      this.Controls.Add(this.wb_toolbar);
      this.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Margin = new System.Windows.Forms.Padding(4);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "frm_nodes";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Load += new System.EventHandler(this.frm_nodes_Load);
      this.tbl_layout.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.WebBrowser wb_toolbar;
    private System.Windows.Forms.TableLayoutPanel tbl_layout;
    private System.Windows.Forms.WebBrowser wb_body;
    private System.Windows.Forms.WebBrowser wb_menu;
    private System.Windows.Forms.WebBrowser wb_title;
  }
}