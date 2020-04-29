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

    public enum type_element { element, text, title, list, link, account, value, attivita, code, par }

    protected List<element> _childs;
    protected List<attribute> _attributes;

    public bool has_childs { get; set; }
    public bool has_child_elements { get; set; }
    public core core { get; set; }
    public List<element> childs { get { return _childs; } }

    // base attributes
    public long id { get; set; }
    public long from_id { get; set; }
    public long parent_id { get; set; }
    public long content_id { get; set; }
    public bool sham { get; set; }
    public int level { get; set; }
    public int order_xml { get; set; }
    public int order { get; set; }
    public bool is_root { get; set; }
    public type_element type { get; set; }
    public string code { get { return get_attribute_string("code"); } set { set_attribute_string("code", strings.remove_specials(value.Trim().Replace(" ", "_")), true); } }
    public string title { get { return get_attribute_string("title"); } set { set_attribute_string("title", value, true); } }
    public bool has_title { get { return !string.IsNullOrEmpty(this.title); } }
    public string des { get; set; }
    public string content { get { return get_attribute_string("content"); } }
    public bool has_content { get { return !string.IsNullOrEmpty(this.content); } }
    public string childs_types { get; set; }
    public bool can_have_childs { get { return !string.IsNullOrEmpty(this.childs_types); } }
    public bool all_childs { get { return !string.IsNullOrEmpty(this.childs_types) && this.childs_types == "{all}" ? true : false; } }
    public List<string> list_childs_types {
      get {
        return !string.IsNullOrEmpty(this.childs_types) && this.childs_types != "{all}" ?
          this.childs_types.Split(new char[] { ',' }).ToList() : null;
      }
    }
    public bool can_content_type(string element_type) {
      if (!string.IsNullOrEmpty(this.childs_types) &&
        (this.childs_types == "{all}" || this.childs_types.Split(new char[] { ',' }).Contains(element_type))) return true;
      return false;
    }
    public bool added { get; set; }
    public bool undeleted { get; set; }
    public bool in_list { get; set; }
    public long? back_element_id { get; set; }
    public string key_page { get; set; }
    public DateTime? dt_ins { get; set; }
    public DateTime? dt_upd { get; set; }
    public DateTime? dt_stored { get; set; }
    public bool stored { get { return this.dt_stored.HasValue; } }

    // attributes
    public List<attribute> attributes { get { return _attributes; } }
    public bool has_attribute(string code) { return _attributes.FirstOrDefault(a => a.code == code) != null; }
    public object attribute_value(string code, bool required = false) {
      return !required ? (has_attribute(code) ? get_attribute(code).value : null) : get_attribute(code).value;
    }
    public attribute get_attribute(string code) { return _attributes.FirstOrDefault(a => a.code == code); }
    public string get_attribute_string(string code) { attribute a = _attributes.FirstOrDefault(x => x.code == code); return a != null ? a.get_str : ""; }
    public bool get_attribute_bool(string code) {
      attribute a = _attributes.FirstOrDefault(x => x.code == code);
      return a != null ? a.get_bool : false;
    }
    public int get_attribute_int(string code) {
      attribute a = _attributes.FirstOrDefault(x => x.code == code);
      return a != null ? a.get_int : 0;
    }
    public void set_attribute_flag(string code, bool value) { set_attribute(code, attribute.attribute_type.flag, value); }
    public void set_attribute_real(string code, double value) { set_attribute(code, attribute.attribute_type.real, value); }
    public void set_attribute_int(string code, int value) { set_attribute(code, attribute.attribute_type.integer, value); }
    public void set_attribute_datetime(string code, DateTime value) { set_attribute(code, attribute.attribute_type.datetime, value); }
    public void set_attribute_string(string code, string value) { set_attribute(code, attribute.attribute_type.varchar, value); }
    public void set_attribute_string(string code, string value, bool static_value) { 
      set_attribute(code, attribute.attribute_type.varchar, value, static_value: static_value); }
    public void set_attribute_text(string code, string value) { set_attribute(code, attribute.attribute_type.text, value); }
    public void set_attribute_link(string code, string value) { set_attribute(code, attribute.attribute_type.link, value); }
    public void set_attribute(string code, object value) {
      attribute a = _attributes.FirstOrDefault(x => x.code == code);
      if (a == null) 
        throw new Exception("l'attributo '" + code + "' non è stato caricato per l'elemento '" + type.ToString() + "'");
      a.value = value.ToString();
    }
    public void set_attribute(string code, attribute.attribute_type type, object value, bool context_txt_xml = false, bool static_value = false) {
      attribute a = _attributes.FirstOrDefault(x => x.code == code);
      if (a == null) {
        a = new attribute(code, type, value, context_txt_xml, static_value);
        _attributes.Add(a);
      } else {
        if (a.type != type) throw new Exception("l'attributo '" + code + "' non è di tipo '" + type.ToString() + "'");
        a.value = value;
      }
    }
    protected void set_attribute(string code, string value, List<attribute> attrs) {
      if (string.IsNullOrEmpty(value)) return;
      attribute a = _attributes.FirstOrDefault(x => x.code == code);
      if (a == null) {
        attribute at = attrs.FirstOrDefault(x => x.code == code);
        if (at == null)
          throw new Exception("non è stato trovato l'attributo '" + code + "' per l'elemento '" + this.type.ToString() + "' fra quelli configurati!");
        a = new attribute(code, at.type, value, at.content_txt_xml, at.static_value);
        _attributes.Add(a);
      } else a.value = value;
    }

    public element(core c, type_element type, string code, string title, int level = 0, long id = 0, long parent_id = 0, long content_id = 0
      , bool has_childs = false, bool has_child_elements = false, bool in_list = false, string des = ""
      , DateTime? dt_ins = null, DateTime? dt_upd = null, DateTime? dt_stored = null, int order_xml = 0, int order = 0, bool sham = false) {
      _childs = new List<element>();
      _attributes = new List<attribute>();
      this.id = id;
      this.core = c;
      this.back_element_id = null;
      this.level = level;
      this.des = des;
      this.parent_id = parent_id;
      this.content_id = content_id;
      this.type = type;
      this.title = title;
      this.has_childs = has_childs;
      this.has_child_elements = has_child_elements;
      this.in_list = in_list;
      this.dt_ins = dt_ins;
      this.dt_upd = dt_upd;
      this.dt_stored = dt_stored;
      this.order_xml = order_xml;
      this.order = order;
      this.sham = sham;
      this.code = code;
    }

    public element(core c) {
      this.core = c;
      this.back_element_id = null;
      _childs = new List<element>();
      _attributes = new List<attribute>();
    }

    #region tools

    public element find_element(long id) {
      if (this.id == id) return this;
      foreach (element ec in this.childs) {
        element res = ec.find_element(id);
        if (res != null) return res;
      }
      return null;
    }

    public static int max_level(List<element> els) {
      int lvl = 0;
      foreach (element e in els)
        lvl = e.max_level(lvl);
      return lvl;
    }

    protected int max_level(int from_lvl) {
      int lvl = from_lvl;
      if (this.level >= lvl) lvl = this.level;
      foreach (element e in this.childs)
        lvl = e.max_level(lvl);
      return lvl;
    }

    public static List<element> find_elements(List<element> els, int level) {
      List<element> res = new List<element>();
      foreach (element e in els)
        e.find_element(res, level);
      return res;
    }

    protected void find_element(List<element> res, int level) {
      if (this.level == level && this.type == type_element.element) res.Add(this);
      foreach (element ec in this.childs)
        ec.find_element(res, level);
    }

    public static bool find_stored(List<element> els) {
      List<element> res = new List<element>();
      foreach (element e in els) {
        if (e.find_stored()) return true;
      }
      return false;
    }

    protected bool find_stored() {
      if (this.dt_stored.HasValue) return true;
      foreach (element ec in this.childs) {
        if (ec.find_stored()) return true;
      }
      return false;
    }

    #endregion

    #region childs

    public element add_child(element e, int content_id = 0) { e.content_id = content_id; _childs.Add(e); return e; }

    public element get_element(int i) { return _childs[i]; }

    public int c_childs { get { return _childs.Count; } }

    public void reset_ids() {
      this.id = 0;
      foreach (element c in _childs)
        c.reset_ids();
    }

    #endregion

    #region xml

    public static xml_doc get_doc(List<element> els, List<element> etypes) {
      xml_doc doc = new xml_doc() { xml = "<elements/>" };
      foreach (element el in els) 
        el.set_xml_node(doc.root_node.add_node(el.type.ToString()), etypes);
      return doc;
    }

    public void set_xml_node(xml_node nd, List<element> etypes) {

      element etype = etypes.FirstOrDefault(x => x.type.ToString() == nd.name);
      if (etype == null) throw new Exception("l'elemento '" + nd.name + "' non è stato configurato correttamente!");

      // title
      nd.set_attr("title", this.title);

      // attributi
      bool set_text = false;
      foreach (attribute a in this.attributes.Where(x => x.value != null)) {
        if (a.value != null && a.value.ToString() != "") {
          attribute atype = etype.get_attribute(a.code);
          if (atype == null) throw new Exception("l'attributo '" + a.code + "' per l'elemento '" + etype.type.ToString() + "' id " + this.id + " non è previsto!");

          if (!a.content_txt_xml) nd.set_attr(a.code, a.type == attribute.attribute_type.flag ? a.value.ToString().ToLower() : a.value.ToString());
          else {
            if (atype.data_content) nd.data = a.value.ToString();
            else nd.text = a.value.ToString();
            set_text = true;
          }
        }
      }
      if (!set_text) nd.text = " ";

      // sys attributes
      nd.set_attr("_id", this.id.ToString() + (this.key_page != null ? ":" + this.key_page : ":"));
      if (this.sham) nd.set_attr("_sham", "true");
      if (this.dt_stored.HasValue) nd.set_attr("_stored", "true");

      if (!this.sham) {
        foreach (element ec in this.childs) 
          ec.set_xml_node(nd.add_node(ec.type.ToString()), etypes);
      }
    }

    public static List<element> load_xml(core c, List<element> types, string xml, int? back_element_id = null) {

      xml_doc d = new xml_doc() { xml = "<elements>" + xml + "</elements>" };
      List<element> res = new List<element>(); int order = 0;
      foreach (xml_node ne in d.nodes("/elements/*")) {
        element e = new element(c) { level = 0, order_xml = order };
        e.back_element_id = back_element_id;

        // element type
        element el_type = types.FirstOrDefault(et => et.type.ToString() == ne.name);
        if (el_type == null)
          throw new Exception("attenzione l'elemento '" + ne.name + "' non è configurato correttamente nel database o non esiste!");

        e.load_node(ne, el_type);
        int order_child = 0;
        foreach (xml_node nd in ne.childs()) {
          if (!nd.is_element) continue;

          if (!el_type.can_have_childs)
            throw new Exception("attenzione l'elemento '" + el_type.type + "' non può contenere elementi al suo interno!");

          if (!el_type.can_content_type(nd.name))
            throw new Exception("attenzione l'elemento '" + el_type.type + "' non può contenere l'elemento '" + nd.name + "' al suo interno!");

          // element type
          element elc_type = types.FirstOrDefault(et => et.type.ToString() == nd.name);
          if (elc_type == null)
            throw new Exception("attenzione l'elemento '" + nd.name + "' non è configurato correttamente nel database o non esiste!");

          e.add_child_elements(nd, types, order_child);
          order_child++;
        }
        res.Add(e); order++;
      }
      return res;
    }

    protected element add_child_elements(xml_node nd, List<element> types, int order) {

      // element type
      element el_type = types.FirstOrDefault(et => et.type.ToString() == nd.name);
      if (el_type == null)
        throw new Exception("attenzione l'elemento '" + nd.name + "' non è configurato correttamente nel database o non esiste!");

      element el = new element(this.core) { level = this.level + 1, order_xml = order };
      el.load_node(nd, el_type);
      this.add_child(el);

      int order_child = 0;
      foreach (xml_node ndc in nd.childs()) {
        if (!ndc.is_element) continue;

        if (!el_type.can_have_childs)
          throw new Exception("attenzione l'elemento '" + el_type.type + "' non può contenere elementi al suo interno!");

        if (!el_type.can_content_type(ndc.name))
          throw new Exception("attenzione l'elemento '" + el_type.type + "' non può contenere l'elemento '" + ndc.name + "' al suo interno!");

        // element type
        element elc_type = types.FirstOrDefault(et => et.type.ToString() == ndc.name);
        if (elc_type == null)
          throw new Exception("attenzione l'elemento '" + ndc.name + "' non è configurato correttamente nel database o non esiste!");

        el.add_child_elements(ndc, types, order_child);
        order_child++;
      }

      return el;
    }

    public void load_node(xml_node nd, element el_type) {
      this.type = (type_element)Enum.Parse(typeof(type_element), nd.name);

      // content
      string txt = nd.text;
      attribute ca = el_type.attributes.FirstOrDefault(x => x.content_txt_xml);
      if (ca != null) set_attribute(ca.code, txt, el_type.attributes);
      else if (ca == null && txt != "") throw new Exception("attenzione c'è un nodo con il contenuto non ammesso!");

      // attributes
      foreach (string attr in nd.attrs()) {
        if (attr == "_id") {
          string val = nd.get_val(attr);
          this.id = val != "" ? int.Parse(val.Split(new char[] { ':' })[0]) : 0;
          this.key_page = val != "" ? val.Split(new char[] { ':' })[1] : "";
          continue;
        } else if (attr == "_from_id") {
          string val = nd.get_val(attr);
          this.from_id = val != "" ? (val.Contains(":") ? int.Parse(val.Split(new char[] { ':' })[0]) : int.Parse(val)) : 0;
          continue;
        } else if (attr == "_sham") {
          this.sham = nd.get_bool(attr); continue;
        } else if (attr == "_stored") {
          this.dt_stored = nd.get_bool(attr) ? DateTime.MinValue : (DateTime?)null; continue;
        }
        set_attribute(attr, nd.get_attr(attr), el_type.attributes);
      }
    }

    #endregion
  }
}