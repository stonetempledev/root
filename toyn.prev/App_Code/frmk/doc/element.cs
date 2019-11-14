using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.Security;
using System.Net;
using System.Net.Mail;
using System.Net.Configuration;
using System.Configuration;
using System.IO;
using mlib;
using mlib.tools;
using mlib.db;
using mlib.xml;
using mlib.tiles;

namespace toyn {
  public class element {
    protected List<element_content> _childs;

    public core core { get; set; }
    public int element_level { get; set; }
    public int element_id { get; set; }
    public element_content element_content { get; set; }
    public bool has_element_content { get { return this.element_content != null; } }
    public string element_type { get; set; }
    public string element_code { get; set; }
    public title title { get; set; }
    public bool has_title { get { return this.title != null; } }
    public bool has_child_elements { get; set; }
    public List<element_content> childs { get { return _childs; } }
    public int back_element_id { get; set; }

    public element(core c, int element_level, int element_id, string element_type, string element_code, string element_title
      , string element_title_ref = "", bool has_child_elements = false, int back_element_id = 0) {
      this.core = c;
      this.element_level = element_level;
      this.element_id = element_id;
      this.element_type = element_type; 
      this.element_code = element_code;
      this.title = new toyn.title(this, element_title, element_title_ref);
      this.has_child_elements = has_child_elements;
      this.back_element_id = back_element_id;
      _childs = new List<element_content>();
    }

    #region childs

    public element add_element(int content_id, element e) { _childs.Add(new element_content(this, content_id, e)); return e; }
    public title add_title(int content_id, title t) { _childs.Add(new element_content(this, content_id, t)); return t; }
    public text add_text(int content_id, text t) { _childs.Add(new element_content(this, content_id, t)); return t; }
    public value add_value(int content_id, value v) { _childs.Add(new element_content(this, content_id, v)); return v; }
    public account add_account(int content_id, account a) { _childs.Add(new element_content(this, content_id, a)); return a; }

    public element get_element(int i) { check_type(i, element_content.element_content_type.element); return (element)_childs[i].child; }
    public title get_title(int i) { check_type(i, element_content.element_content_type.title); return (title)_childs[i].child; }
    public text get_text(int i) { check_type(i, element_content.element_content_type.text); return (text)_childs[i].child; }
    public value get_value(int i) { check_type(i, element_content.element_content_type.value); return (value)_childs[i].child; }
    public account get_account(int i) { check_type(i, element_content.element_content_type.account); return (account)_childs[i].child; }
    protected void check_type(int i, element_content.element_content_type tp) {
      element_content.element_content_type tpi = content_type(i);
      if (tpi != tp) throw new Exception("elemento all'indice " + i.ToString() + " di tipo '" + tpi.ToString() + "' non corrispondente con '" + tp.ToString() + "'!");
    }

    public int c_childs { get { return _childs.Count; } }
    public element_content.element_content_type content_type(int i) { return _childs[i].content_type; }

    #endregion
  }
}