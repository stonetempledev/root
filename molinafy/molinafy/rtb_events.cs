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

/*
  public class rtb_doc : RichTextBox {

    protected static Dictionary<string, line_style> _fonts = null;

    doc _doc = null;

    public doc doc { get { return _doc; } }

    public rtb_doc (int id_doc) {

      inits();

      if (_fonts["base"].bg_color.HasValue) this.BackColor = _fonts["base"].bg_color.Value;
      if (_fonts["base"].fore_color.HasValue) this.ForeColor = _fonts["base"].fore_color.Value;
      if (_fonts["base"].font != null) this.Font = _fonts["base"].font;

      _doc = new doc(id_doc);

      parse_doc();
    }

    protected static void inits () {
      if (_fonts != null) return;
      _fonts = new Dictionary<string, line_style>();
      _fonts.Add("base", new line_style(new Font(new FontFamily("segoe ui"), 10), Color.Black, Color.White));
      _fonts.Add("title_doc", new line_style(new Font(new FontFamily("segoe ui"), 25), Color.LightSteelBlue));
      _fonts.Add("des_doc", new line_style(new Font(new FontFamily("segoe ui"), 10, FontStyle.Italic), Color.DarkGray));
    }

    #region parse

    protected string _last_style = "";
    protected void parse_doc () {
      add_line(_doc.title, "title_doc");
      add_line(_doc.des, "des_doc");
      add_line("");
      add_line("");
    }

    protected void add_line (string txt, string style = "") {
      this.AppendText((this.Lines.Count() > 0 ? Environment.NewLine : ""));
      style = style == "" ? "base" : style;
      if (_last_style != style) {
        apply_style(style);
        _last_style = style;
      }
      this.AppendText(txt);
    }

    protected void apply_style (string name) {
      if (string.IsNullOrEmpty(name)) return;
      line_style ls = _fonts[name];
      if (ls.font != null) this.SelectionFont = ls.font;
      if (ls.fore_color.HasValue) this.SelectionColor = ls.fore_color.Value;
      if (ls.bg_color.HasValue) this.SelectionBackColor = ls.bg_color.Value;
    }

    #endregion

    #region events

    protected override void OnKeyDown (KeyEventArgs e) {
      base.OnKeyDown(e);
      //e.Handled = true;
      int line = this.GetLineFromCharIndex(this.GetFirstCharIndexOfCurrentLine());
      app._main.add_war("line: " + line.ToString() + " - key code: " + e.KeyCode.ToString());

      // title_doc
      if (line == 0) {
        if (e.KeyCode == Keys.Return) e.Handled = true;
      }

      // des_doc
      if (line == 1) {
        if (e.KeyCode == Keys.Return) e.Handled = true;
      }
    }

    #endregion
  }
*/