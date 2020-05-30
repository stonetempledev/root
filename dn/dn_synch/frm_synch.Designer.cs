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
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frm_synch));
      this.lbl_title = new System.Windows.Forms.Label();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.btn_ok = new System.Windows.Forms.Button();
      this.lw_msg = new System.Windows.Forms.ListView();
      this.first = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.tmr_synch = new System.Windows.Forms.Timer(this.components);
      this.btn_max = new System.Windows.Forms.Button();
      this.btn_min = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // lbl_title
      // 
      this.lbl_title.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lbl_title.BackColor = System.Drawing.Color.PaleTurquoise;
      this.lbl_title.Cursor = System.Windows.Forms.Cursors.Hand;
      this.lbl_title.Font = new System.Drawing.Font("Segoe UI Light", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lbl_title.Location = new System.Drawing.Point(-1, 0);
      this.lbl_title.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.lbl_title.Name = "lbl_title";
      this.lbl_title.Size = new System.Drawing.Size(648, 33);
      this.lbl_title.TabIndex = 0;
      this.lbl_title.Text = "Deepa Synch";
      this.lbl_title.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.lbl_title.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lbl_title_MouseDown);
      this.lbl_title.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lbl_title_MouseMove);
      this.lbl_title.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lbl_title_MouseUp);
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Location = new System.Drawing.Point(-2, 24);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(651, 14);
      this.groupBox1.TabIndex = 2;
      this.groupBox1.TabStop = false;
      // 
      // btn_ok
      // 
      this.btn_ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btn_ok.BackColor = System.Drawing.Color.PaleTurquoise;
      this.btn_ok.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btn_ok.FlatAppearance.BorderSize = 0;
      this.btn_ok.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btn_ok.Font = new System.Drawing.Font("Segoe UI Emoji", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.btn_ok.ForeColor = System.Drawing.Color.RoyalBlue;
      this.btn_ok.Location = new System.Drawing.Point(611, 0);
      this.btn_ok.Name = "btn_ok";
      this.btn_ok.Size = new System.Drawing.Size(36, 33);
      this.btn_ok.TabIndex = 3;
      this.btn_ok.Text = "X";
      this.btn_ok.UseVisualStyleBackColor = false;
      this.btn_ok.Click += new System.EventHandler(this.btn_ok_Click);
      // 
      // lw_msg
      // 
      this.lw_msg.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lw_msg.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.lw_msg.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.first});
      this.lw_msg.Font = new System.Drawing.Font("Courier New", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lw_msg.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
      this.lw_msg.Location = new System.Drawing.Point(-1, 36);
      this.lw_msg.Name = "lw_msg";
      this.lw_msg.Size = new System.Drawing.Size(647, 406);
      this.lw_msg.TabIndex = 4;
      this.lw_msg.UseCompatibleStateImageBehavior = false;
      this.lw_msg.View = System.Windows.Forms.View.Details;
      this.lw_msg.Resize += new System.EventHandler(this.lw_msg_Resize);
      // 
      // first
      // 
      this.first.Text = "Message";
      // 
      // tmr_synch
      // 
      this.tmr_synch.Interval = 10000;
      this.tmr_synch.Tick += new System.EventHandler(this.tmr_synch_Tick);
      // 
      // btn_max
      // 
      this.btn_max.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btn_max.BackColor = System.Drawing.Color.PaleTurquoise;
      this.btn_max.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btn_max.FlatAppearance.BorderSize = 0;
      this.btn_max.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btn_max.Font = new System.Drawing.Font("Segoe UI Emoji", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.btn_max.ForeColor = System.Drawing.Color.RoyalBlue;
      this.btn_max.Location = new System.Drawing.Point(571, 0);
      this.btn_max.Name = "btn_max";
      this.btn_max.Size = new System.Drawing.Size(36, 33);
      this.btn_max.TabIndex = 5;
      this.btn_max.Text = "_";
      this.btn_max.UseVisualStyleBackColor = false;
      this.btn_max.Click += new System.EventHandler(this.btn_min_Click);
      // 
      // btn_min
      // 
      this.btn_min.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btn_min.BackColor = System.Drawing.Color.PaleTurquoise;
      this.btn_min.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btn_min.FlatAppearance.BorderSize = 0;
      this.btn_min.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btn_min.Font = new System.Drawing.Font("Segoe UI Emoji", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.btn_min.ForeColor = System.Drawing.Color.RoyalBlue;
      this.btn_min.Location = new System.Drawing.Point(531, 0);
      this.btn_min.Name = "btn_min";
      this.btn_min.Size = new System.Drawing.Size(36, 33);
      this.btn_min.TabIndex = 6;
      this.btn_min.Text = "_";
      this.btn_min.UseVisualStyleBackColor = false;
      this.btn_min.Click += new System.EventHandler(this.btn_min_Click_1);
      // 
      // frm_synch
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 21F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.ClientSize = new System.Drawing.Size(646, 441);
      this.ControlBox = false;
      this.Controls.Add(this.btn_min);
      this.Controls.Add(this.btn_max);
      this.Controls.Add(this.lw_msg);
      this.Controls.Add(this.btn_ok);
      this.Controls.Add(this.lbl_title);
      this.Controls.Add(this.groupBox1);
      this.Font = new System.Drawing.Font("Segoe UI Light", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "frm_synch";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Load += new System.EventHandler(this.frm_synch_Load);
      this.Shown += new System.EventHandler(this.frm_synch_Shown);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Label lbl_title;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.Button btn_ok;
    private System.Windows.Forms.ListView lw_msg;
    private System.Windows.Forms.ColumnHeader first;
    private System.Windows.Forms.Timer tmr_synch;
    private System.Windows.Forms.Button btn_max;
    private System.Windows.Forms.Button btn_min;
  }
}

