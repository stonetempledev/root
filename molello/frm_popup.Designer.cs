namespace molello {
  partial class frm_popup {
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frm_popup));
      this.lbl_title = new System.Windows.Forms.Label();
      this.pb_close = new System.Windows.Forms.PictureBox();
      this.btn_ok = new System.Windows.Forms.Button();
      this.lbl_msg = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.pb_close)).BeginInit();
      this.SuspendLayout();
      // 
      // lbl_title
      // 
      this.lbl_title.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lbl_title.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
      this.lbl_title.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lbl_title.Location = new System.Drawing.Point(-1, 3);
      this.lbl_title.Name = "lbl_title";
      this.lbl_title.Size = new System.Drawing.Size(543, 33);
      this.lbl_title.TabIndex = 0;
      this.lbl_title.Text = "...";
      this.lbl_title.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.lbl_title.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lbl_title_MouseDown);
      this.lbl_title.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lbl_title_MouseMove);
      this.lbl_title.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lbl_title_MouseUp);
      // 
      // pb_close
      // 
      this.pb_close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.pb_close.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
      this.pb_close.Cursor = System.Windows.Forms.Cursors.Hand;
      this.pb_close.Image = ((System.Drawing.Image)(resources.GetObject("pb_close.Image")));
      this.pb_close.Location = new System.Drawing.Point(510, 7);
      this.pb_close.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
      this.pb_close.Name = "pb_close";
      this.pb_close.Size = new System.Drawing.Size(24, 24);
      this.pb_close.TabIndex = 2;
      this.pb_close.TabStop = false;
      this.pb_close.Click += new System.EventHandler(this.pb_close_Click);
      // 
      // btn_ok
      // 
      this.btn_ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btn_ok.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btn_ok.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.btn_ok.Location = new System.Drawing.Point(437, 158);
      this.btn_ok.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
      this.btn_ok.Name = "btn_ok";
      this.btn_ok.Size = new System.Drawing.Size(98, 37);
      this.btn_ok.TabIndex = 3;
      this.btn_ok.Text = "&OK";
      this.btn_ok.UseVisualStyleBackColor = true;
      this.btn_ok.Click += new System.EventHandler(this.btn_ok_Click);
      // 
      // lbl_msg
      // 
      this.lbl_msg.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lbl_msg.Location = new System.Drawing.Point(6, 47);
      this.lbl_msg.Name = "lbl_msg";
      this.lbl_msg.Size = new System.Drawing.Size(527, 101);
      this.lbl_msg.TabIndex = 4;
      this.lbl_msg.Text = "...";
      // 
      // frm_popup
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.ClientSize = new System.Drawing.Size(539, 200);
      this.ControlBox = false;
      this.Controls.Add(this.lbl_msg);
      this.Controls.Add(this.btn_ok);
      this.Controls.Add(this.pb_close);
      this.Controls.Add(this.lbl_title);
      this.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "frm_popup";
      this.Load += new System.EventHandler(this.frm_popup_Load);
      this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.frm_popup_MouseDown);
      this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.frm_popup_MouseMove);
      this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.frm_popup_MouseUp);
      ((System.ComponentModel.ISupportInitialize)(this.pb_close)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Label lbl_title;
    private System.Windows.Forms.PictureBox pb_close;
    private System.Windows.Forms.Button btn_ok;
    private System.Windows.Forms.Label lbl_msg;
  }
}