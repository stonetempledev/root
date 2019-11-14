using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using mlib;
using mlib.xml;

namespace toyn {
  public class child {

    public element element { get; set; }
    public int child_id { get; set; }
    public element_content element_content { get; set; }

    protected string get_ref(string ref_url) {
      return ref_url.StartsWith("{@cmdurl='") ? this.element.core.config.var_value_par("vars.router-cmd"
        , ref_url.Substring(10, ref_url.Length - 12)) : ref_url;
    }

    public child(element el, int child_id) {
      this.element = el; this.child_id = child_id;
    }
  }
}
