using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using dlib.db;

namespace dlib {
  public class setting {

    public int id { get; set; }
    public string name { get; set; }
    public string value { get; set; }

    public setting(int id, string name, string value) {
      this.id = id; this.name = name; this.value = value;
    }
  }

  public class settings {
    public List<setting> list { get; set; }
    public settings(List<setting> l) {
      this.list = l;
    }
    public static settings read_settings(core c, db_provider conn) {
      return new settings(conn.dt_table(c.config.get_query("lib-base.get-settings").text)
          .Rows.Cast<DataRow>().Select(r => new setting((int)r["setting_id"]
            , (string)r["setting_name"], db_provider.str_val(r["setting_var"]))).ToList());
    }
    public string get_value(string setting_name) {
      return this.list.FirstOrDefault(x => x.name == setting_name).value;
    }
    public string set_value(string setting_name, string value) {
      this.list.FirstOrDefault(x => x.name == setting_name).value = value;
      return value;
    }
  }
}
