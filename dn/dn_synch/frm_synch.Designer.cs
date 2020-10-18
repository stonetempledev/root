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
      this.tmr_synch = new System.Windows.Forms.Timer(this.components);
      this.rt_main = new System.Windows.Forms.RichTextBox();
      this.statusStrip1 = new System.Windows.Forms.StatusStrip();
      this.tlt_status = new System.Windows.Forms.ToolStripStatusLabel();
      this.statusStrip1.SuspendLayout();
      this.SuspendLayout();
      // 
      // tmr_synch
      // 
      this.tmr_synch.Interval = 10000;
      this.tmr_synch.Tick += new System.EventHandler(this.tmr_synch_Tick);
      // 
      // rt_main
      // 
      this.rt_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.rt_main.BackColor = System.Drawing.Color.Black;
      this.rt_main.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.rt_main.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rt_main.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
      this.rt_main.Location = new System.Drawing.Point(0, 0);
      this.rt_main.Name = "rt_main";
      this.rt_main.ReadOnly = true;
      this.rt_main.Size = new System.Drawing.Size(646, 415);
      this.rt_main.TabIndex = 0;
      this.rt_main.Text = "";
      // 
      // statusStrip1
      // 
      this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tlt_status});
      this.statusStrip1.Location = new System.Drawing.Point(0, 416);
      this.statusStrip1.Name = "statusStrip1";
      this.statusStrip1.Size = new System.Drawing.Size(646, 25);
      this.statusStrip1.TabIndex = 1;
      this.statusStrip1.Text = "statusStrip1";
      // 
      // tlt_status
      // 
      this.tlt_status.BackColor = System.Drawing.Color.Transparent;
      this.tlt_status.Font = new System.Drawing.Font("Segoe UI", 11F);
      this.tlt_status.ForeColor = System.Drawing.Color.SteelBlue;
      this.tlt_status.Name = "tlt_status";
      this.tlt_status.Size = new System.Drawing.Size(18, 20);
      this.tlt_status.Text = "...";
      // 
      // frm_synch
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 21F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.ClientSize = new System.Drawing.Size(646, 441);
      this.Controls.Add(this.statusStrip1);
      this.Controls.Add(this.rt_main);
      this.Font = new System.Drawing.Font("Segoe UI Light", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
      this.Name = "frm_synch";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "DeepaNotes Synch";
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frm_synch_FormClosing);
      this.Load += new System.EventHandler(this.frm_synch_Load);
      this.Shown += new System.EventHandler(this.frm_synch_Shown);
      this.statusStrip1.ResumeLayout(false);
      this.statusStrip1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Timer tmr_synch;
    private System.Windows.Forms.RichTextBox rt_main;
    private System.Windows.Forms.StatusStrip statusStrip1;
    private System.Windows.Forms.ToolStripStatusLabel tlt_status;
  }
}

