using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using dlib;
using dlib.db;

namespace dn_client {
  public partial class frm_main : Form {

    public static core _c = null;
    protected frm_sql _sql = null;
    protected Point _dp = Point.Empty;

    public frm_main(core c) {
      _c = c; InitializeComponent();
    }

    private void lbl_title_MouseDown(object sender, MouseEventArgs e) {
      if (e.Button != MouseButtons.Left) return; _dp = new Point(e.X, e.Y);
    }

    private void lbl_title_MouseMove(object sender, MouseEventArgs e) {
      if (_dp == Point.Empty) return;
      Point location = new Point(this.Left + e.X - _dp.X, this.Top + e.Y - _dp.Y);
      this.Location = location;
    }

    private void lbl_title_MouseUp(object sender, MouseEventArgs e) {
      if (e.Button != MouseButtons.Left) return;
      _dp = Point.Empty;
    }

    private void btn_close_Click(object sender, EventArgs e) {
      if (!_c.config.var_bool("client.tray-icon")) {
        this.Close(); return; 
      }

      if (_sql != null) _sql.Hide();
      this.WindowState = FormWindowState.Minimized;
    }

    private void frm_main_Resize(object sender, EventArgs e) {
      if (this.WindowState == FormWindowState.Minimized) {
        Hide(); ntf_main.Visible = true;
      }
    }

    private void ntf_main_DoubleClick(object sender, EventArgs e) {
      Show();
      if (_sql != null && !_sql.closed) { _sql.Show(); _sql.WindowState = FormWindowState.Normal; }
      this.WindowState = FormWindowState.Normal;
      ntf_main.Visible = false;
    }

    private void frm_main_Load(object sender, EventArgs e) {
      try {
        // tray
        ntf_main.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
        ntf_main.ContextMenuStrip.Items.Add("Apri...", null, this.MenuApri_Click);
        ntf_main.ContextMenuStrip.Items.Add("Esci", null, this.MenuExit_Click);
        ntf_main.ContextMenuStrip.BackColor = Color.Gray;
        ntf_main.ContextMenuStrip.ForeColor = Color.White;
        ntf_main.ContextMenuStrip.Font = new Font("segoe ui light", 9, FontStyle.Regular);

        // get mac address
        string externalip = new System.Net.WebClient().DownloadString("http://icanhazip.com");


      } catch { }
    }

    void MenuApri_Click(object sender, EventArgs e) {
      ntf_main_DoubleClick(null, null);
    }

    void MenuExit_Click(object sender, EventArgs e) {
      if (_sql != null) _sql.force_close = true;
      Application.Exit();
    }

    private void lbl_title_DoubleClick(object sender, EventArgs e) {
      if (this.WindowState == FormWindowState.Normal) this.WindowState = FormWindowState.Maximized;
      else if (this.WindowState == FormWindowState.Maximized) this.WindowState = FormWindowState.Normal;
    }

    private void mi_sqleditor_Click(object sender, EventArgs e) {
      try {
        if (_sql == null) {
          _sql = new frm_sql(_c);
          _sql.Show();
        } else { _sql.Show(); _sql.Focus(); Application.DoEvents(); }
      } catch (Exception ex) { msg_error(ex.Message); }
    }

    protected void msg_error(string txt) {
      MessageBox.Show(txt, "Attenzione!", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private void mi_exit_Click(object sender, EventArgs e) {
      MenuExit_Click(sender, e);
    }

    private void tmr_state_Tick(object sender, EventArgs e) {
      try {
        db_provider conn = Program.open_conn();
        conn.exec(_c.parse_query("client.opened"
          , new string[,] { { "ip_machine", dlib.core.machine_ip() } }));
        conn.close_conn(); conn = null;
      } catch { }
    }
  }
}

