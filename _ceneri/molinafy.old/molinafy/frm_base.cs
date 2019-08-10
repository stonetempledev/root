using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using mlib.tools;

namespace molinafy {
  public class frm_base : Form {

    public frm_base () { }

    protected bool _mdown = false; protected Point _fp;
    protected void mouse_down (MouseEventArgs e) { _fp = e.Location; _mdown = true; }
    protected void mouse_move (MouseEventArgs e) {
      if (_mdown) this.Location = new Point(this.Location.X - (_fp.X - e.Location.X), this.Location.Y - (_fp.Y - e.Location.Y));
    }
    protected void mouse_up () { _mdown = false; }

    protected void navigate (WebBrowser wb, string url) {
      wb.Tag = null;
      wb.Navigate(url);
    }

    protected void load_html (WebBrowser wb, string html) {
      if (wb.Tag == null || wb.Tag != null && wb.Tag.ToString() != "page-blank") {
        wb.Navigate("about:blank");
        wb.Tag = "page-blank";
      }

      if (wb.Document == null) wb.DocumentText = html;
      else {
        wb.Navigate("about:blank");
        wb.Document.OpenNew(false);
        wb.Document.Write(html);
        wb.Refresh();
      }
    }
  }
}
