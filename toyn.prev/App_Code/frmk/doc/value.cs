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

    public value(element el, int value_id, string value_content, string value_ref = "", string value_notes = "") : base(el, value_id) {
      this.value_content = value_content; this.value_ref = value_ref; this.value_notes = value_notes;
    }
  }
}
