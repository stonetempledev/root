using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace toyn {
  public class section_attr {
    public enum section_attr_type { text, varchar, datetime, integer, flag }

    public int id { get; set; }
    public section.type_section s_type { get; set; }
    public section_attr_type type { get; set; }
    public string code { get; set; }
    public object value { get; set; }
    public bool get_bool { get { return this.value != null ? Convert.ToBoolean(this.value) : false; } set { this.value = value; } }
    public double get_real { get { return this.value != null ? Convert.ToDouble(this.value) : 0; } set { this.value = value; } }
    public int get_int { get { return this.value != null ? Convert.ToInt32(this.value) : 0; } set { this.value = value; } }
    public string get_str { get { return this.value != null ? Convert.ToString(this.value) : ""; } set { this.value = value; } }
    public DateTime get_datetime { get { return this.value != null ? Convert.ToDateTime(this.value) : DateTime.MinValue; } set { this.value = value; } }

    public section_attr(string code, section_attr_type type, object value) {
      this.code = code; this.type = type; this.value = value; 
    }

    public section_attr(section.type_section st, int id, string code, section_attr_type type) {
      this.s_type = st; this.id = id; this.code = code; this.type = type;
    }
  }
}