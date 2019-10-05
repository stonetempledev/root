using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace molinafy {
  public class paragraph {

    protected doc _doc;
    protected int _id;
    protected string _text;

    public int id { get { return _id; } }
    public string text { get { return _text; } set { _text = _doc.clean_html_txt(value); } }
    public doc doc { get { return _doc; } }

    public paragraph (doc d) { _doc = d; _id = -1; _text = ""; }
    public paragraph (doc d, int id_paragraph, string text) { _doc = d; _id = id_paragraph; _text = text; }
  }
}
