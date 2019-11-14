using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using mlib;
using mlib.xml;

namespace toyn {
  public class title : child {
    public bool has_title_id { get { return this.child_id > 0; } }
    public string text { get; set; }

    protected string _title_ref;
    public string title_ref_value { get { return _title_ref; } }
    public string title_ref { get { return get_ref(_title_ref); } set { _title_ref = value; } }
    public bool title_ref_cmd { get { return _title_ref.StartsWith("{@cmdurl='"); } }

    public title(element el, int title_id, string text, string title_ref)
      : base(el, title_id) {
      this.text = text; this.title_ref = title_ref;
    }

    public title(element el, string text, string title_ref)
      : base(el, -1) {
      this.text = text; this.title_ref = title_ref;
    }
  }
}
