using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace toyn {
  public class section {

    public enum type_section { notes_free }

    protected List<section_attr> _attributes;

    public int id { get; set; }
    public int id_ms { get; set; }
    public type_section type { get; set; }
    public string title { get; set; }
    public string notes { get; set; }
    public int order { get; set; }
    public DateTime? dt_ins { get; set; }
    public DateTime? dt_upd { get; set; }
    public DateTime? last_dt { get { return dt_upd.HasValue ? dt_upd : dt_ins; } }
    public string emphasis { get; set; }
    public int cols { get; set; }

    // attributes
    public List<section_attr> attributes { get { return _attributes; } }
    public bool has_attribute(string code) { return _attributes.FirstOrDefault(a => a.code == code) != null; }
    public object attribute_value(string code, bool required = false) {
      return !required ? (has_attribute(code) ? get_attribute(code).value : null) : get_attribute(code).value;
    }
    public section_attr get_attribute(string code) { return _attributes.FirstOrDefault(a => a.code == code); }
    public string get_attribute_string(string code, string def = "") 
    { 
      section_attr a = _attributes.FirstOrDefault(x => x.code == code); 
      return a != null && !string.IsNullOrEmpty(a.get_str) ? a.get_str : def; 
    }
    public bool get_attribute_bool(string code) {
      section_attr a = _attributes.FirstOrDefault(x => x.code == code);
      return a != null ? a.get_bool : false;
    }
    public int get_attribute_int(string code, int def = 0) {
      section_attr a = _attributes.FirstOrDefault(x => x.code == code);
      return a != null && a.get_str != "" ? a.get_int : def;
    }
    public void set_attribute_flag(string code, bool value) { set_attribute(code, section_attr.section_attr_type.flag, value); }
    public void set_attribute_int(string code, int value) { set_attribute(code, section_attr.section_attr_type.integer, value); }
    public void set_attribute_datetime(string code, DateTime value) { set_attribute(code, section_attr.section_attr_type.datetime, value); }
    public void set_attribute_string(string code, string value) { set_attribute(code, section_attr.section_attr_type.varchar, value); }
    public void set_attribute_text(string code, string value) { set_attribute(code, section_attr.section_attr_type.text, value); }
    public void set_attribute(string code, object value) {
      section_attr a = _attributes.FirstOrDefault(x => x.code == code);
      if (a == null)
        throw new Exception("l'attributo '" + code + "' non è stato caricato per l'elemento '" + type.ToString() + "'");
      a.value = value.ToString();
    }
    public void set_attribute(string code, section_attr.section_attr_type type, object value) {
      section_attr a = _attributes.FirstOrDefault(x => x.code == code);
      if (a == null) {
        a = new section_attr(code, type, value);
        _attributes.Add(a);
      } else {
        if (a.type != type) throw new Exception("l'attributo '" + code + "' non è di tipo '" + type.ToString() + "'");
        a.value = value;
      }
    }
    protected void set_attribute(string code, string value, List<section_attr> attrs) {
      if (string.IsNullOrEmpty(value)) return;
      section_attr a = _attributes.FirstOrDefault(x => x.code == code);
      if (a == null) {
        section_attr at = attrs.FirstOrDefault(x => x.code == code);
        if (at == null)
          throw new Exception("non è stato trovato l'attributo '" + code + "' per la sezione '" + this.type.ToString() + "' fra quelli configurati!");
        a = new section_attr(code, at.type, value);
        _attributes.Add(a);
      } else a.value = value;
    }

    public section() { _attributes = new List<section_attr>(); }
  }
}