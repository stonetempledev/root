using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using AutocompleteMenuNS;

namespace molinafy {
  public class rtb_nodes : RichTextBox {

    protected List<node> _nodes_path = null;

    public rtb_nodes () {
      _nodes_path = new List<node>();
    }

    protected override void OnKeyPress (KeyPressEventArgs e) {
      base.OnKeyPress(e);

      if (e.KeyChar == '/') {
        e.Handled = true;
        this.AppendText("\\");
      }
    }

    protected override void OnKeyUp (KeyEventArgs e) {
      base.OnKeyUp(e);

      try {
        //// autocomplete nodes
        //if (Char.IsLetterOrDigit((char)e.KeyValue)) {
        //  string last_3 = this.TextLength >= 3 ? this.Text.Substring(this.SelectionStart - 3, 3) : "";
        //  if (last_3.Length > 0) {
        //    if (last_3 == "pop") {
        //      //add_event("pop");
        //      int st = this.SelectionStart;
        //      this.Select(this.SelectionStart - 3, 3);
        //      this.SelectionColor = Color.Blue;
        //      this.SelectionStart = st;
        //      this.SelectionLength = 0;
        //      this.SelectionColor = Color.White;
        //    } 
        //  }
        //}
      } catch (Exception ex) {
      }
    }

  }
}

