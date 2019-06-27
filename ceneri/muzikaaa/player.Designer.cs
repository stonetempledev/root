namespace muzikaaa
{
    partial class player
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(player));
      this.tmr_state = new System.Windows.Forms.Timer(this.components);
      this.tmr_keys = new System.Windows.Forms.Timer(this.components);
      this.SuspendLayout();
      // 
      // tmr_state
      // 
      this.tmr_state.Enabled = true;
      this.tmr_state.Tick += new System.EventHandler(this.tmr_state_Tick);
      // 
      // tmr_keys
      // 
      this.tmr_keys.Enabled = true;
      this.tmr_keys.Interval = 500;
      this.tmr_keys.Tick += new System.EventHandler(this.tmr_keys_Tick);
      // 
      // player
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(503, 207);
      this.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      this.MaximizeBox = false;
      this.Name = "player";
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "MuZiKaaA";
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.player_FormClosed);
      this.Load += new System.EventHandler(this.player_Load);
      this.Resize += new System.EventHandler(this.player_Resize);
      this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer tmr_state;
        private System.Windows.Forms.Timer tmr_keys;

    }
}

