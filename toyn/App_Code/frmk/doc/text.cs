using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using mlib;
using mlib.xml;

namespace toyn {
  public class text : child {
    public enum text_styles { underline, bold }

    public string text_content { get; set; }
    public string text_style { get; set; }
    public bool is_style() { return !string.IsNullOrEmpty(this.text_style); }
    public bool is_style(text_styles ts) { return !string.IsNullOrEmpty(this.text_style) ? this.text_style.Contains("[" + ts.ToString() + "]") : false; }
    public text_styles[] get_styles() {
      if (string.IsNullOrEmpty(this.text_style)) return new text_styles[] { };
      string ts = this.text_style.Replace("][", ",").Replace("[", "").Replace("]", "");
      return ts.Split(new char[] { ',' }).Select(x => (text_styles)Enum.Parse(typeof(text_styles), x)).ToArray();
    }

    public text(element el, int text_id, string text_content, string text_style = "")
      : base(el, text_id) {
      this.text_content = text_content; this.text_style = text_style;
    }
  }
}
