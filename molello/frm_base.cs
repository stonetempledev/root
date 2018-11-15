using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace molello {
  public class frm_base : Form {
    protected bool _mdown = false; protected Point _fp;

    public frm_base () { }

    protected void mouse_down (MouseEventArgs e) { _fp = e.Location; _mdown = true; }
    protected void mouse_move (MouseEventArgs e) {
      if (_mdown) this.Location = new Point(this.Location.X - (_fp.X - e.Location.X), this.Location.Y - (_fp.Y - e.Location.Y));
    }
    protected void mouse_up () { _mdown = false; }
  }
}
