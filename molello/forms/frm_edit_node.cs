using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace molello.forms {
  public partial class frm_edit_node : frm_base {

    protected classes.node _node = null;
    protected string _node_title;
    protected bool _ok = false;

    public frm_edit_node(int link_id) {
      InitializeComponent();
      _node = classes.node.dal.get_node(link_id);
    }

    private void pb_close_Click(object sender, EventArgs e) { Close(); }

    private void lbl_title_MouseDown(object sender, MouseEventArgs e) { mouse_down(e); }
    private void lbl_title_MouseMove(object sender, MouseEventArgs e) { mouse_move(e); }
    private void lbl_title_MouseUp(object sender, MouseEventArgs e) { mouse_up(); }

    private void frm_edit_node_MouseDown(object sender, MouseEventArgs e) { mouse_down(e); }
    private void frm_edit_node_MouseMove(object sender, MouseEventArgs e) { mouse_move(e); }
    private void frm_edit_node_MouseUp(object sender, MouseEventArgs e) { mouse_up(); }

    private void btn_ok_Click(object sender, EventArgs e) {
      if (string.IsNullOrEmpty(txt_nome.Text)) { frm_popup.show_error("Devi scrivere il titolo!", 3000); return; }
      // OCIO! check se esiste già con lo stesso nome!
      _ok = true;
      _node.title = txt_nome.Text;
      classes.node.dal.set_node(_node);
      Close();
    }

    private void frm_edit_node_Load(object sender, EventArgs e) {
      txt_nome.Text = _node.title;
      txt_nome.SelectAll();
    }

    private void txt_nome_KeyPress (object sender, KeyPressEventArgs e) {
      if (e.KeyChar == 13) btn_ok.PerformClick();
      else if (e.KeyChar == 27) this.Close();
    }

    public static bool edit_node (int link_id) {
      frm_edit_node f = new frm_edit_node(link_id);
      f.ShowDialog();
      return f._ok;
    }
  }
}
