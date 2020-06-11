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
      this.SuspendLayout();
      // 
      // tmr_synch
      // 
      this.tmr_synch.Interval = 10000;
      this.tmr_synch.Tick += new System.EventHandler(this.tmr_synch_Tick);
      // 
      // rt_main
      // 
      this.rt_main.BackColor = System.Drawing.Color.Black;
      this.rt_main.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.rt_main.Dock = System.Windows.Forms.DockStyle.Fill;
      this.rt_main.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rt_main.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
      this.rt_main.Location = new System.Drawing.Point(0, 0);
      this.rt_main.Name = "rt_main";
      this.rt_main.ReadOnly = true;
      this.rt_main.Size = new System.Drawing.Size(646, 441);
      this.rt_main.TabIndex = 0;
      this.rt_main.Text = "";
      // 
      // frm_synch
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 21F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.ClientSize = new System.Drawing.Size(646, 441);
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
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Timer tmr_synch;
    private System.Windows.Forms.RichTextBox rt_main;
  }
}

