using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace toyn {
  public class attribute {
    public enum attribute_type { text, varchar, link, datetime, integer, real, flag }

    public int id { get; set; }
    public element.type_element e_type { get; set; }
    public attribute_type type { get; set; }
    public string code { get; set; }
    public object value { get; set; }
    public bool content_txt_xml { get; set; }
    public bool static_value { get; set; }
    public bool data_content { get; set; }
    public bool get_bool { get { return this.value != null ? Convert.ToBoolean(this.value) : false; } set { this.value = value; } }
    public double get_real { get { return this.value != null ? Convert.ToDouble(this.value) : 0; } set { this.value = value; } }
    public int get_int { get { return this.value != null ? Convert.ToInt32(this.value) : 0; } set { this.value = value; } }
    public string get_str { get { return this.value != null ? Convert.ToString(this.value) : ""; } set { this.value = value; } }
    public DateTime get_datetime { get { return this.value != null ? Convert.ToDateTime(this.value) : DateTime.MinValue; } set { this.value = value; } }

    public attribute(string code, attribute_type type, object value, bool content_txt_xml, bool static_value) {
      this.code = code; this.type = type; this.value = value; this.content_txt_xml = content_txt_xml; this.static_value = static_value;
    }

    public attribute(element.type_element et, int id, string code, attribute_type type, bool content_txt_xml, bool static_value, bool data_content) {
      this.e_type = et; this.id = id; this.code = code; this.type = type;
      this.content_txt_xml = content_txt_xml; this.static_value = static_value; this.data_content = data_content;
    }
  }
}