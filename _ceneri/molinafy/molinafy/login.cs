using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace molinafy {
  public partial class login : frm_base {
    protected user _user = null;
    public user user { get { return _user; } }

    public static user enter_login () {
      try {
        login f = new login();
        return f.ShowDialog() == DialogResult.OK ? f.user : null;
      } catch (Exception ex) { return null; }
    }

    public login () {
      InitializeComponent();
      this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
    }

    private void pb_close_Click (object sender, EventArgs e) { this.Close(); }

    private void btn_login_Click (object sender, EventArgs e) {
      try {
        if (txt_user.Text == "") throw new Exception("Utente non valido!");
        if (txt_password.Text == "") throw new Exception("Password non valida!");

        _user = user.login(txt_user.Text, txt_password.Text, chk_week.Checked ? (int?)7 : null);
        if (_user == null) throw new Exception("Si è verificato un errore nell'autenticazione!");
        view_info("Benvenuto nel molinafy!", "OK");
      } catch (Exception ex) { view_err(ex.Message); }
    }

    protected void view_info (string txt, object tag = null) {
      lbl_err.ForeColor = Color.DarkBlue;
      lbl_err.Text = txt;
      lbl_err.Visible = true;
      Application.DoEvents();
      tmr_err.Interval = 4000;
      tmr_err.Tag = tag;
      tmr_err.Start();
    }

    protected void view_err (string txt, object tag = null) {
      lbl_err.ForeColor = Color.Red;
      lbl_err.Text = txt;
      lbl_err.Visible = true;
      Application.DoEvents();
      tmr_err.Interval = 4000;
      tmr_err.Tag = tag;
      tmr_err.Start();
    }

    private void tmr_err_Tick (object sender, EventArgs e) {
      lbl_err.Visible = false;
      tmr_err.Stop();
      if (tmr_err.Tag != null && tmr_err.Tag.ToString() == "OK") {
        this.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.Close();
      }
    }

    private void txt_user_KeyPress (object sender, KeyPressEventArgs e) {
      if (e.KeyChar == '\r') btn_login.PerformClick();
    }

    private void txt_password_KeyPress (object sender, KeyPressEventArgs e) {
      if (e.KeyChar == '\r') btn_login.PerformClick();
    }

    private void login_Shown (object sender, EventArgs e) {
      txt_user.Focus();
    }

    private void txt_user_Enter (object sender, EventArgs e) {
      txt_user.SelectAll();
    }

    private void txt_user_Click (object sender, EventArgs e) {
      txt_user.SelectAll();
    }

    private void txt_password_Enter (object sender, EventArgs e) {
      txt_password.SelectAll();
    }

    private void txt_password_Click (object sender, EventArgs e) {
      txt_password.SelectAll();
    }

  }
}
