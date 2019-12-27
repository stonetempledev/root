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
  public class element : child {
    protected List<element_content> _childs;

    public core core { get; set; }
    public int parent_id { get; set; }
    public int element_level { get; set; }
    public element_content element_content { get; set; }
    public bool has_element_content { get { return this.element_content != null; } }
    public string element_type { get; set; }
    public string element_code { get; set; }
    public title title { get; set; }
    public bool has_title { get { return this.title != null; } }
    public bool has_child_elements { get; set; }
    public bool hide_element { get; set; }
    public List<element_content> childs { get { return _childs; } }
    public int back_element_id { get; set; }

    public element(core c, int element_level, int element_id, int parent_id, string element_type, string element_code, string element_title
      , string element_title_ref = "", bool has_child_elements = false, int back_element_id = 0, bool hide_element = false)
      : base(null, content_type.element, element_id) {
      this.core = c;
      this.element_level = element_level;
      this.parent_id = parent_id;
      this.element_type = element_type;
      this.element_code = element_code;
      this.title = new toyn.title(this, element_title, element_title_ref);
      this.has_child_elements = has_child_elements;
      this.back_element_id = back_element_id;
      this.hide_element = hide_element;
      _childs = new List<element_content>();
    }

    public element(core c) : base(null, content_type.element) { this.core = c; this.title = new toyn.title(this); _childs = new List<element_content>(); }

    #region childs

    public element add_element(element e, int content_id = 0) { _childs.Add(new element_content(e, content_id)); return e; }
    public title add_title(title t, int content_id = 0) { _childs.Add(new element_content(t, content_id)); return t; }
    public text add_text(text t, int content_id = 0) { _childs.Add(new element_content(t, content_id)); return t; }
    public value add_value(value v, int content_id = 0) { _childs.Add(new element_content(v, content_id)); return v; }
    public account add_account(account a, int content_id = 0) { _childs.Add(new element_content(a, content_id)); return a; }

    public element get_element(int i) { check_type(i, content_type.element); return (element)_childs[i].child; }
    public title get_title(int i) { check_type(i, content_type.title); return (title)_childs[i].child; }
    public text get_text(int i) { check_type(i, content_type.text); return (text)_childs[i].child; }
    public value get_value(int i) { check_type(i, content_type.value); return (value)_childs[i].child; }
    public account get_account(int i) { check_type(i, content_type.account); return (account)_childs[i].child; }
    protected void check_type(int i, content_type tp) {
      content_type tpi = child_type(i);
      if (tpi != tp) throw new Exception("elemento all'indice " + i.ToString() + " di tipo '" + tpi.ToString() + "' non corrispondente con '" + tp.ToString() + "'!");
    }

    public int c_childs { get { return _childs.Count; } }
    public content_type child_type(int i) { return _childs[i].child.type; }

    public int max_level() {
      int l = this.element_level;
      foreach (element ec in childs.Where(c => c.child is element && !((element)c.child).hide_element).Select(c => c.element_child))
        l = ec.max_level();
      return l;
    }

    #endregion

    #region xml

    public xml_doc get_xml(int max_level) {
      xml_doc doc = new xml_doc() { xml = "<element/>" };
      add_xml_node(max_level, doc.root_node);
      return doc;
    }

    public override void add_xml_node(int max_level, xml_node nd) {
      nd.set_attrs(new Dictionary<string, string>() { { "title", this.title.text}, { "ref", this.title.title_ref_value}
        , { "code", this.element_code}, { "type", this.element_type}, { "id", this.id.ToString() } });

      if (!this.hide_element) {
        foreach (element_content ec in this.childs.OrderBy(x => x.content_id))
          ec.child.add_xml_node(max_level, ec.child.type == content_type.element ? nd.add_node(!ec.element_child.hide_element ? "element" : "hide_element") : nd);
      }
    }

    public static List<element> load_xml(core c, string xml) {
      xml_doc d = new xml_doc() { xml = "<root>" + xml + "</root>" };
      List<element> res = new List<element>();
      foreach (xml_node ne in d.nodes("/root/element")) {
        element e = new element(c);
        e.load_xml_node(null, ne);
        foreach (xml_node nd in ne.childs())
          e.add_child_node(nd);
        res.Add(e);
      }
      return res;
    }

    protected void add_child_node(xml_node nd) {
      string name = nd.name;
      if (xml_elements.account.ToString() == name)
        this.add_account(account.load_xml(this, nd));
      else if (xml_elements.element.ToString() == name)
        this.add_element(element.load_xml(this, nd));
      else if (xml_elements.hide_element.ToString() == name)
        this.add_element(element.load_xml(this, nd));
      else if (xml_elements.text.ToString() == name)
        this.add_text(text.load_xml(this, nd));
      else if (xml_elements.title.ToString() == name)
        this.add_title(title.load_xml(this, nd));
      else if (xml_elements.value.ToString() == name)
        this.add_value(value.load_xml(this, nd));
      else throw new Exception("l'elemento '" + name + "' non viene ancora gestito!");
    }

    public static element load_xml(element e, xml_node nd) {
      element el = new element(e.core); el.load_xml_node(e, nd);
      foreach (xml_node ndc in nd.childs())
        el.add_child_node(ndc);
      return el;
    }

    public override void load_xml_node(element el, xml_node nd) {
      this.id = nd.get_int("id", 0);
      this.element = el;
      this.title.text = nd.get_attr("title");
      this.title.title_ref = nd.get_attr("ref");
      this.element_code = nd.get_attr("code");
      this.element_type = nd.get_attr("type");
      this.hide_element = nd.name == xml_elements.hide_element.ToString();
    }

    #endregion
  }
}