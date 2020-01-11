using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using mlib;
using mlib.xml;

namespace toyn {
  public class title : child {
    public bool has_title_id { get { return this.id > 0; } }
    public string text { get; set; }

    protected string _title_ref;
    public string title_ref_value { get { return _title_ref; } }
    public string title_ref { get { return get_ref(_title_ref); } set { _title_ref = value; } }
    public bool title_ref_cmd { get { return _title_ref.StartsWith("{@cmdurl='"); } }

    public title(element el, int title_id, string text, string title_ref)
      : base(el, content_type.title, title_id) {
      this.text = text; this.title_ref = title_ref;
    }

    public title(element el, string text, string title_ref)
      : base(el, content_type.title, -1) {
      this.text = text; this.title_ref = title_ref;
    }

    public title(element el) : base(el, content_type.title) { }

    public override void add_xml_node(xml_node el) {
      if (!this.has_title_id) throw new Exception("titolo non accessibile dal documento xml!");
      xml_node nd = el.add_node("title", this.text);
      nd.set_attrs(new string[,] { { "ref", this.title_ref_value }, { "id", this.id.ToString() } });
    }

    public static title load_xml(element el, xml_node nd) {
      title t = new title(el); t.load_xml_node(el, nd); return t;
    }

    public override void load_xml_node(element el, xml_node nd) {
      this.element = el;
      this.id = nd.get_int("id", 0);
      this.title_ref = nd.get_attr("ref");
      this.text = nd.text;
    }
  }
}
