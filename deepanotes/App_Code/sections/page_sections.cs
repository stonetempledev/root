using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace toyn {
  public class page_sections {
    public string title { get; set; }
    public string sub_title { get; set; }

    protected List<macro_section> _ms = null;
    public List<macro_section> macro_sections { get { return _ms; } set { _ms = value; } }

    public page_sections() { _ms = new List<macro_section>(); }

  }
}