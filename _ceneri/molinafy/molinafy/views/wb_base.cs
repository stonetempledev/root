using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace molinafy {
  public class wb_base : WebBrowser {
    public wb_base () { }
    public void load_html (string html) {
      try {
        if (this.Document == null) this.DocumentText = html;
        else {
          this.Navigate("about:blank");
          this.Document.OpenNew(false);
          this.Document.Write(html);
          this.Refresh();
        }
      } catch { }
    }
  }
}
