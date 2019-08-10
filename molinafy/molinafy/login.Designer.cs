namespace molinafy {
  partial class login {
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(login));
      this.tmr_err = new System.Windows.Forms.Timer(this.components);
      this.lbl_err = new System.Windows.Forms.Label();
      this.chk_week = new System.Windows.Forms.CheckBox();
      this.txt_password = new System.Windows.Forms.TextBox();
      this.txt_user = new System.Windows.Forms.TextBox();
      this.btn_login = new System.Windows.Forms.Button();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.pb_icon = new System.Windows.Forms.PictureBox();
      this.pb_close = new System.Windows.Forms.PictureBox();
      this.lbl_title = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.pb_icon)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pb_close)).BeginInit();
      this.SuspendLayout();
      // 
      // tmr_err
      // 
      this.tmr_err.Tick += new System.EventHandler(this.tmr_err_Tick);
      // 
      // lbl_err
      // 
      this.lbl_err.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lbl_err.BackColor = System.Drawing.Color.White;
      this.lbl_err.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lbl_err.ForeColor = System.Drawing.Color.Red;
      this.lbl_err.Location = new System.Drawing.Point(3, 192);
      this.lbl_err.Name = "lbl_err";
      this.lbl_err.Size = new System.Drawing.Size(502, 36);
      this.lbl_err.TabIndex = 12;
      this.lbl_err.Text = "...";
      this.lbl_err.Visible = false;
      // 
      // chk_week
      // 
      this.chk_week.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.chk_week.AutoSize = true;
      this.chk_week.ForeColor = System.Drawing.Color.DimGray;
      this.chk_week.Location = new System.Drawing.Point(12, 258);
      this.chk_week.Name = "chk_week";
      this.chk_week.Size = new System.Drawing.Size(170, 17);
      this.chk_week.TabIndex = 11;
      this.chk_week.Text = "ricordami per una settimana";
      this.chk_week.UseVisualStyleBackColor = true;
      // 
      // txt_password
      // 
      this.txt_password.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.txt_password.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.txt_password.Location = new System.Drawing.Point(125, 131);
      this.txt_password.Name = "txt_password";
      this.txt_password.PasswordChar = '*';
      this.txt_password.Size = new System.Drawing.Size(373, 25);
      this.txt_password.TabIndex = 10;
      this.txt_password.Click += new System.EventHandler(this.txt_password_Click);
      this.txt_password.Enter += new System.EventHandler(this.txt_password_Enter);
      this.txt_password.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txt_password_KeyPress);
      // 
      // txt_user
      // 
      this.txt_user.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.txt_user.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.txt_user.Location = new System.Drawing.Point(125, 69);
      this.txt_user.Name = "txt_user";
      this.txt_user.Size = new System.Drawing.Size(373, 25);
      this.txt_user.TabIndex = 9;
      this.txt_user.Click += new System.EventHandler(this.txt_user_Click);
      this.txt_user.Enter += new System.EventHandler(this.txt_user_Enter);
      this.txt_user.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txt_user_KeyPress);
      // 
      // btn_login
      // 
      this.btn_login.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btn_login.BackColor = System.Drawing.SystemColors.Control;
      this.btn_login.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btn_login.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.btn_login.Location = new System.Drawing.Point(420, 258);
      this.btn_login.Name = "btn_login";
      this.btn_login.Size = new System.Drawing.Size(75, 35);
      this.btn_login.TabIndex = 8;
      this.btn_login.Text = "&Entra";
      this.btn_login.UseVisualStyleBackColor = false;
      this.btn_login.Click += new System.EventHandler(this.btn_login_Click);
      // 
      // label2
      // 
      this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label2.BackColor = System.Drawing.Color.White;
      this.label2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label2.ForeColor = System.Drawing.Color.Black;
      this.label2.Location = new System.Drawing.Point(2, 128);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(502, 31);
      this.label2.TabIndex = 7;
      this.label2.Text = "Password:";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label1
      // 
      this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label1.BackColor = System.Drawing.Color.White;
      this.label1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.ForeColor = System.Drawing.Color.Black;
      this.label1.Location = new System.Drawing.Point(2, 66);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(502, 31);
      this.label1.TabIndex = 6;
      this.label1.Text = "Utente:";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // pb_icon
      // 
      this.pb_icon.BackColor = System.Drawing.Color.Azure;
      this.pb_icon.Cursor = System.Windows.Forms.Cursors.Hand;
      this.pb_icon.Image = ((System.Drawing.Image)(resources.GetObject("pb_icon.Image")));
      this.pb_icon.Location = new System.Drawing.Point(4, 6);
      this.pb_icon.Name = "pb_icon";
      this.pb_icon.Size = new System.Drawing.Size(32, 32);
      this.pb_icon.TabIndex = 5;
      this.pb_icon.TabStop = false;
      // 
      // pb_close
      // 
      this.pb_close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.pb_close.BackColor = System.Drawing.Color.Azure;
      this.pb_close.Cursor = System.Windows.Forms.Cursors.Hand;
      this.pb_close.Image = ((System.Drawing.Image)(resources.GetObject("pb_close.Image")));
      this.pb_close.Location = new System.Drawing.Point(474, 6);
      this.pb_close.Name = "pb_close";
      this.pb_close.Size = new System.Drawing.Size(32, 32);
      this.pb_close.TabIndex = 4;
      this.pb_close.TabStop = false;
      this.pb_close.Click += new System.EventHandler(this.pb_close_Click);
      // 
      // lbl_title
      // 
      this.lbl_title.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lbl_title.BackColor = System.Drawing.Color.Azure;
      this.lbl_title.Cursor = System.Windows.Forms.Cursors.Hand;
      this.lbl_title.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lbl_title.ForeColor = System.Drawing.Color.MidnightBlue;
      this.lbl_title.Location = new System.Drawing.Point(0, 4);
      this.lbl_title.Name = "lbl_title";
      this.lbl_title.Padding = new System.Windows.Forms.Padding(36, 0, 0, 0);
      this.lbl_title.Size = new System.Drawing.Size(509, 36);
      this.lbl_title.TabIndex = 3;
      this.lbl_title.Text = "Autenticazione molinafy";
      // 
      // login
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.ClientSize = new System.Drawing.Size(508, 305);
      this.ControlBox = false;
      this.Controls.Add(this.lbl_err);
      this.Controls.Add(this.chk_week);
      this.Controls.Add(this.txt_password);
      this.Controls.Add(this.txt_user);
      this.Controls.Add(this.btn_login);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.pb_icon);
      this.Controls.Add(this.pb_close);
      this.Controls.Add(this.lbl_title);
      this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "login";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.TopMost = true;
      this.Shown += new System.EventHandler(this.login_Shown);
      ((System.ComponentModel.ISupportInitialize)(this.pb_icon)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pb_close)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.PictureBox pb_icon;
    private System.Windows.Forms.PictureBox pb_close;
    private System.Windows.Forms.Label lbl_title;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Button btn_login;
    private System.Windows.Forms.TextBox txt_user;
    private System.Windows.Forms.TextBox txt_password;
    private System.Windows.Forms.CheckBox chk_week;
    private System.Windows.Forms.Label lbl_err;
    private System.Windows.Forms.Timer tmr_err;

  }
}