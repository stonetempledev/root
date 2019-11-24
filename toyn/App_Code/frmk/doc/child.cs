using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using mlib;
using mlib.xml;

namespace toyn {
  public class child {

    public enum xml_elements { account, element, text, title, value, hide_element }

    public element element { get; set; }
    public int id { get; set; }
    public element_content element_content { get; set; }

    protected string get_ref(string ref_url) {
      return ref_url.StartsWith("{@cmdurl='") ? this.element.core.config.var_value_par("vars.router-cmd"
        , ref_url.Substring(10, ref_url.Length - 12)) : ref_url;
    }

    public child(element el, int child_id) { this.element = el; this.id = child_id; }

    public child(element el) { this.element = el; this.id = 0; }

    public virtual void add_xml_node(int max_level, xml_node el) { throw new Exception("add_xml_node non implementata!"); }
    public virtual void load_xml_node(element el, xml_node nd) { throw new Exception("load_xml_node non implementata!"); }
  }
}
