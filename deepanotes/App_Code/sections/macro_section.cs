using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using mlib.db;

namespace toyn {
  public class macro_section {
    public int id { get; set; }
    public string title { get; set; }
    public string notes { get; set; }
    public int order { get; set; }

    public List<section> sections { get; protected set; }
    public section add_section(section s) { sections.Add(s); return s; }

    public macro_section() { this.sections = new List<section>(); }

  }
}