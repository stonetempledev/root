using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace molello.forms {
  public partial class frm_popup : frm_base {
    protected string _title, _msg; Color _clr_title;
    protected int _ms = 0;
    protected Timer _tm = null;
    protected bool _yn = false, _yes = false;

    public frm_popup (string title, string msg, Color clr_title, int ms = 0, bool yn = false) {
      InitializeComponent();
      _title = title; _msg = msg; _clr_title = clr_title; _ms = ms; _yn = yn;
    }

    public static void show_msg (string title, string msg, int ms = 0) { (new frm_popup(title, msg, Color.LightBlue, ms) { StartPosition = FormStartPosition.CenterParent }).ShowDialog(); }
    public static void show_error (string title, string msg, int ms = 0) { (new frm_popup(title, msg, Color.Tomato, ms) { StartPosition = FormStartPosition.CenterParent }).ShowDialog(); }
    public static void show_error (string msg, int ms = 0) { (new frm_popup("ERRORE!", msg, Color.Tomato, ms) { StartPosition = FormStartPosition.CenterParent }).ShowDialog(); }
    public static bool show_yesno (string title, string msg) {
      frm_popup f = new frm_popup(title, msg, Color.LightBlue, yn: true) { StartPosition = FormStartPosition.CenterParent };
      f.ShowDialog(); return f._yes;
    }

    private void pb_close_Click (object sender, EventArgs e) { Close(); }

    private void lbl_title_MouseDown (object sender, MouseEventArgs e) { mouse_down(e); }
    private void lbl_title_MouseMove (object sender, MouseEventArgs e) { mouse_move(e); }
    private void lbl_title_MouseUp (object sender, MouseEventArgs e) { mouse_up(); }

    private void frm_popup_MouseDown (object sender, MouseEventArgs e) { mouse_down(e); }
    private void frm_popup_MouseMove (object sender, MouseEventArgs e) { mouse_move(e); }
    private void frm_popup_MouseUp (object sender, MouseEventArgs e) { mouse_up(); }

    private void btn_ok_Click (object sender, EventArgs e) { _yes = true; Close(); }

    private void frm_popup_Load (object sender, EventArgs e) {
      lbl_title.Text = _title; lbl_msg.Text = _msg;
      lbl_title.BackColor = _clr_title; pb_close.BackColor = _clr_title;
      if (_yn) { btn_no.Visible = true; btn_ok.Text = "&SI"; }
      if (_ms > 0) {
        lbl_msg.Height += btn_ok.Height; this.Height -= btn_ok.Height;
        btn_ok.Visible = false;
      }
    }

    private void frm_popup_Shown (object sender, EventArgs e) {
      if (_ms > 0) {
        _tm = new Timer() { Interval = _ms, Enabled = true };
        _tm.Tick += new EventHandler(_tm_Tick);
      }
    }

    void _tm_Tick (object sender, EventArgs e) { this.Close(); }

    private void btn_no_Click (object sender, EventArgs e) { this.Close(); }
  }
}
