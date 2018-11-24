using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace molello {
  public partial class frm_add_node : frm_base {

    protected string _node_title;

    public frm_add_node() {
      InitializeComponent();
    }

    private void pb_close_Click(object sender, EventArgs e) { Close(); }

    private void lbl_title_MouseDown(object sender, MouseEventArgs e) { mouse_down(e); }
    private void lbl_title_MouseMove(object sender, MouseEventArgs e) { mouse_move(e); }
    private void lbl_title_MouseUp(object sender, MouseEventArgs e) { mouse_up(); }

    private void frm_add_node_MouseDown(object sender, MouseEventArgs e) { mouse_down(e); }
    private void frm_add_node_MouseMove(object sender, MouseEventArgs e) { mouse_move(e); }
    private void frm_add_node_MouseUp(object sender, MouseEventArgs e) { mouse_up(); }

    private void btn_ok_Click(object sender, EventArgs e) {
      if (string.IsNullOrEmpty(txt_nome.Text)) { frm_popup.show_error("Devi scrivere il titolo!", 3000); return; }
      _node_title = txt_nome.Text;
      Close();
    }

    private void frm_add_node_Load(object sender, EventArgs e) { }

    public static string add_node() {
      frm_add_node frm = new frm_add_node() { StartPosition = FormStartPosition.CenterParent };
      frm.ShowDialog();
      return frm._node_title;
    }

    private void txt_nome_KeyPress (object sender, KeyPressEventArgs e) {
      if (e.KeyChar == 13) btn_ok.PerformClick();
      else if (e.KeyChar == 27) this.Close();
    }
  }
}
