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

    public enum type_element { element, text, title }

    protected List<element> _childs;
    protected List<attribute> _attributes;
    
    public bool has_childs { get; set; }
    public bool has_child_elements { get; set; }
    public core core { get; set; }
    public List<element> childs { get { return _childs; } }

    // base attributes
    public long id { get; set; }
    public long parent_id { get; set; }
    public long back_id { get; set; }
    public long content_id { get; set; }
    public bool sham { get; set; }
    public int level { get; set; }
    public type_element type { get; set; }
    public string title { get; set; }
    public bool has_title { get { return !string.IsNullOrEmpty(this.title); } }
    public string content { get { return get_attribute_string("content"); } }
    public bool has_content { get { return !string.IsNullOrEmpty(this.content); } }

    // attributes
    public List<attribute> attributes { get { return _attributes; } }
    public bool has_attribute(string code) { return _attributes.FirstOrDefault(a => a.code == code) != null; }
    public object attribute_value(string code, bool required = false) {
      return !required ? (has_attribute(code) ? get_attribute(code).value : null) : get_attribute(code).value;
    }
    public attribute get_attribute(string code) { return _attributes.FirstOrDefault(a => a.code == code); }
    public string get_attribute_string(string code) { attribute a = _attributes.FirstOrDefault(x => x.code == code); return a != null ? a.get_str : ""; }
    public void set_attribute_real(string code, double value) { set_attribute(code, attribute.attribute_type.real, value); }
    public void set_attribute_int(string code, int value) { set_attribute(code, attribute.attribute_type.integer, value); }
    public void set_attribute_datetime(string code, DateTime value) { set_attribute(code, attribute.attribute_type.datetime, value); }
    public void set_attribute_string(string code, string value) { set_attribute(code, attribute.attribute_type.varchar, value); }
    public void set_attribute_text(string code, string value) { set_attribute(code, attribute.attribute_type.text, value); }
    public void set_attribute_link(string code, string value) { set_attribute(code, attribute.attribute_type.link, value); }
    public void set_attribute(string code, attribute.attribute_type type, object value, bool context_txt_xml = false) {
      attribute a = _attributes.FirstOrDefault(x => x.code == code);
      if (a == null) {
        a = new attribute(code, type, value, context_txt_xml);
        _attributes.Add(a);
      } else {
        if (a.type != type) throw new Exception("l'attributo non è di tipo '" + type.ToString() + "'");
        a.value = value;
      }
    }
    protected void set_attribute(string code, string value, List<attribute> attrs) {
      if (string.IsNullOrEmpty(value)) return;
      attribute a = _attributes.FirstOrDefault(x => x.code == code);
      if (a == null) {
        attribute at = attrs.FirstOrDefault(x => x.e_type == this.type && x.code == code);
        if (at == null) 
          throw new Exception("non è stato trovato l'attributo '" + code + "' per l'elemento '" + this.type.ToString() + "' fra quelli configurati!");
        a = new attribute(code, at.type, value, at.content_txt_xml);
        _attributes.Add(a);
      } else a.value = value;
    }

    public element(core c, type_element type, string title
      , int level = 0, long id = 0, long parent_id = 0, long content_id = 0
      , bool has_childs = false, bool has_child_elements = false, long back_element_id = 0, bool sham = false) {
      _childs = new List<element>();
      _attributes = new List<attribute>();
      this.id = id;
      this.core = c;
      this.level = level;
      this.parent_id = parent_id;
      this.content_id = content_id;
      this.type = type;
      this.title = title;
      this.has_childs = has_childs;
      this.has_child_elements = has_child_elements;
      this.back_id = back_element_id;
      this.sham = sham;
    }

    public element(core c) {
      this.core = c;
      _childs = new List<element>();
      _attributes = new List<attribute>();
    }

    #region childs

    public element add_child(element e, int content_id = 0) { e.content_id = content_id; _childs.Add(e); return e; }

    public element get_element(int i) { return _childs[i]; }

    public int c_childs { get { return _childs.Count; } }

    #endregion

    #region xml

    public static xml_doc get_doc(List<element> els) {
      xml_doc doc = new xml_doc() { xml = "<elements/>" };
      int level = 0;
      foreach (element el in els.OrderBy(x => x.content_id))
        el.set_xml_node(doc.root_node.add_node(el.type.ToString()));
      return doc;
    }

    public void set_xml_node(xml_node nd) {
      // attributes
      nd.set_attrs(new Dictionary<string, string>() { { "title", this.title}, { "id", this.id.ToString() } });

      if (this.sham) nd.set_attr("sham", "true");

      foreach (attribute a in this.attributes.Where(x => x.value != null)) {
        if (!a.content_txt_xml) nd.set_attr(a.code, a.value.ToString());
        else nd.text = a.value.ToString();
      }

      if (!this.sham) {
        foreach (element ec in this.childs.OrderBy(x => x.content_id))
          ec.set_xml_node(nd.add_node(ec.type.ToString()));
      }
    }

    public static List<element> load_xml(core c, List<attribute> attrs, string xml, int back_element_id = 0) {

      xml_doc d = new xml_doc() { xml = "<elements>" + xml + "</elements>" };
      List<element> res = new List<element>();
      foreach (xml_node ne in d.nodes("/elements/*")) {
        element e = new element(c) { level = 0, back_id = back_element_id };
        e.load_node(ne, attrs);
        foreach (xml_node nd in ne.childs()) {
          if (!nd.is_element) continue;
          e.add_child_elements(nd, attrs);
        }
        res.Add(e);
      }
      return res;
    }

    protected element add_child_elements(xml_node nd, List<attribute> attrs) {

      element el = new element(this.core) { level = this.level + 1 }; 
      el.load_node(nd, attrs);
      this.add_child(el);

      foreach (xml_node ndc in nd.childs()) {
        if (!ndc.is_element) continue;
        el.add_child_elements(ndc, attrs);
      }
      
      return el;
    }

    public void load_node(xml_node nd, List<attribute> attrs) {
      this.type = (type_element)Enum.Parse(typeof(type_element), nd.name);

      attribute ca = attrs.FirstOrDefault(x => x.content_txt_xml);
      if(ca != null) set_attribute(ca.code, nd.text, attrs);

      // attributes
      foreach (string attr in nd.attrs()) {
        if (attr == "id") {
          this.id = nd.get_int(attr, 0); continue;
        } else if (attr == "title") {
          this.title = nd.get_attr(attr); continue;
        } else if (attr == "sham") {
          this.sham = nd.get_bool(attr); continue;
        }
        set_attribute(attr, nd.get_attr(attr), attrs);
      }
    }

    #endregion
  }
}