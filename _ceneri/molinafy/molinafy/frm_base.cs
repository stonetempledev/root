using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace molinafy {
  public class frm_base : Form {

    protected Point _dp = Point.Empty;

    public frm_base () : base() { }

    protected bool title_mousedown (MouseEventArgs e) {
      if (e.Button == MouseButtons.Left) { _dp = new Point(e.X, e.Y); return true; }
      return false;
    }

    protected bool title_mousemove (MouseEventArgs e) {
      if (_dp != Point.Empty) {
        Point location = new Point(this.Left + e.X - _dp.X, this.Top + e.Y - _dp.Y);
        this.Location = location;
        return true;
      }
      return false;
    }

    protected bool title_mouseup (MouseEventArgs e) {
      if (e.Button == MouseButtons.Left) { _dp = Point.Empty; return true; }
      return false;
    }

    protected bool title_doubleclick (EventArgs e) {
      if (this.WindowState == FormWindowState.Maximized) {
        this.WindowState = FormWindowState.Normal; return true;
      } else if (this.WindowState == FormWindowState.Normal) {
        this.WindowState = FormWindowState.Maximized; return true;
      }
      return false;
    }

    private void InitializeComponent () {
      this.SuspendLayout();
      // 
      // frm_base
      // 
      this.ClientSize = new System.Drawing.Size(284, 261);
      this.Name = "frm_base";
      this.Load += new System.EventHandler(this.frm_base_Load);
      this.Shown += new System.EventHandler(this.frm_base_Shown);
      this.ResumeLayout(false);

    }

    private void frm_base_Load (object sender, EventArgs e) {

    }

    private void frm_base_Shown (object sender, EventArgs e) {

    }

  }
}
