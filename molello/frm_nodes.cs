using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using mlib;
using mlib.tools;

namespace molello {
  public partial class frm_nodes : frm_base {
    public frm_nodes () {
      InitializeComponent();

    }

    private void lbl_title_MouseDown (object sender, MouseEventArgs e) { mouse_down(e); }
    private void lbl_title_MouseMove (object sender, MouseEventArgs e) { mouse_move(e); }
    private void lbl_title_MouseUp (object sender, MouseEventArgs e) { mouse_up(); }

    private void pb_close_MouseClick (object sender, MouseEventArgs e) { Close(); }

    private void frm_nodes_Load (object sender, EventArgs e) {
      try {
        lbl_title.Text = app._core.config.get_var("vars.title-app").value;

        load_top(@"<li><span onclick='open_node(1)'>ROOT</span><span onclick='to_root()' class='ar'></span></li>
        <li><span onclick='open_node(2)'>PRIMO NODO</span><span onclick='to_root()' class='ar'></span></li>
        <li><span onclick='open_node(3)'>SECONDO NODO</span><span onclick='to_root()' class='ar'></span></li>
        <li><span onclick='open_node(4)'>TERZO NODO</span><span onclick='to_root()' class='ar'></span></li>
        <li><span onclick='open_node(5)'>QUARTO NODO</span><span onclick='to_root()' class='ar'></span></li>
        <li><span onclick='open_node(6)'>QUINTO NODO</span><span onclick='to_root()' class='ar'></span></li>
        <li><span onclick='open_node(7)'>SESTO NODO</span><span onclick='to_root()' class='ar'></span></li>
        <li><span onclick='open_node(8)'>OTTAVO NODO</span><span onclick='to_root()' class='ar'></span></li>
        <li><span onclick='open_node(9)'>NONO NODO</span><span onclick='to_root()' class='ar'></span></li>
        <li><span onclick='open_node(10)'>DECIMO E ULTIMO NODO</span></li>");
        load_menu("<span>MENU</span>");
        load_body("<span>BODY</span>");
      } catch (Exception ex) { frm_popup.show_error(ex.Message); }
    }

    protected void load_top (string html) {
      try {
        wb_toolbar.DocumentText = app._core.parse(System.IO.File.ReadAllText(app._core.config.get_var("vars.page-top").value)
          , new Dictionary<string, object>() { { "html", html } });
      } catch (Exception ex) { frm_popup.show_error(ex.Message); }
    }

    protected void load_menu (string html) {
      try {
        wb_menu.DocumentText = app._core.parse(System.IO.File.ReadAllText(app._core.config.get_var("vars.page-menu").value)
          , new Dictionary<string, object>() { { "html", html } });
      } catch (Exception ex) { frm_popup.show_error(ex.Message); }
    }

    protected void load_body (string html) {
      try {
        wb_body.DocumentText = app._core.parse(System.IO.File.ReadAllText(app._core.config.get_var("vars.page-body").value)
          , new Dictionary<string, object>() { { "html", html } });
      } catch (Exception ex) { frm_popup.show_error(ex.Message); }
    }

    private void lbl_title_DoubleClick (object sender, EventArgs e) {
      if (this.WindowState == FormWindowState.Normal) this.WindowState = FormWindowState.Maximized;
      else this.WindowState = FormWindowState.Normal;
    }
  }
}
