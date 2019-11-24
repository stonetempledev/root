using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using mlib;
using mlib.xml;

namespace toyn {
  public class value : child {
    public string value_content { get; set; }
    public string value_notes { get; set; }

    protected string _val_ref;
    public string value_ref_value { get { return _val_ref; } }
    public string value_ref { get { return get_ref(_val_ref); } set { _val_ref = value; } }
    public bool value_ref_cmd { get { return _val_ref.StartsWith("{@cmdurl='"); } }

    public value(element el) : base(el) { }

    public value(element el, int value_id, string value_content, string value_ref = "", string value_notes = "")
      : base(el, value_id) {
      this.value_content = value_content; this.value_ref = value_ref; this.value_notes = value_notes;
    }

    public override void add_xml_node(int max_level, xml_node el) {
      xml_node nd = el.add_node("value", this.value_content);
      nd.set_attrs(new string[,] { { "ref", this.value_ref_value }, { "notes", this.value_notes }, { "id", this.id.ToString() } });
    }

    public static value load_xml(element el, xml_node nd) {
      value v = new value(el); v.load_xml_node(el, nd); return v;
    }

    public override void load_xml_node(element el, xml_node nd) {
      this.element = el;
      this.id = nd.get_int("id", 0);
      this.value_ref = nd.get_attr("ref");
      this.value_notes = nd.get_attr("notes");
      this.value_content = nd.text;
    }
  }
}
