using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace molinafy {
  public class rtb_events : RichTextBox {

    public Color ColorInfo { get; set; }
    public Color ColorErr { get; set; }
    public Color ColorWar { get; set; }
    public string TimeFormat { get; set; }

    public rtb_events () { }

    public void add_info (string text) { add_line(text, this.ColorInfo); }
    public void add_err (string text) { add_line(text, this.ColorErr); }
    public void add_err (Exception ex) { add_line(ex.ToString(), this.ColorErr); }
    public void add_war (string text) { add_line(text, this.ColorWar); }

    protected void add_line (string text, Color clr) {
      try {
        if (string.IsNullOrEmpty(text)) return;
        if(clr != Color.Empty) this.SelectionColor = clr;
        this.AppendText(string.Format("{0}{1}\r\n", !string.IsNullOrEmpty(this.TimeFormat) ? DateTime.Now.ToString(this.TimeFormat) + " - " : "", text));
        if (this.Visible) { this.ScrollToCaret(); Application.DoEvents(); }
      } catch { }
    }
  }
}

