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

    public text(element el) : base(el, content_type.text) { }

    public text(element el, int text_id, string text_content, string text_style = "")
      : base(el, content_type.text, text_id) {
      this.text_content = text_content; this.text_style = text_style;
    }

    public override void add_xml_node(xml_node el) {
      xml_node nd = el.add_node("text");
      nd.text = this.text_content;
      nd.set_attrs(new string[,] { { "style", this.text_style }, { "id", this.id.ToString() } });
    }

    public static text load_xml(element el, xml_node nd) {
      text t = new text(el); t.load_xml_node(el, nd); return t;
    }

    public override void load_xml_node(element el, xml_node nd) {
      this.element = el;
      this.id = nd.get_int("id", 0);
      this.text_content = nd.text;
      this.text_style = nd.get_attr("style");
    }

  }
}
