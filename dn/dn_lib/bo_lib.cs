using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using mlib;
using mlib.db;
using mlib.tools;

namespace dn_lib {
  // business object
  public class bo_lib {
    public db_provider db_conn { get; protected set; }
    public core core { get; protected set; }
    public config config { get; protected set; }
    public int user_id { get; protected set; }
    public string user_name { get; protected set; }

    public bo_lib(db_provider conn, core c, config cfg, int user_id = 0, string user_name = "") {
      this.db_conn = conn; this.core = c; this.config = cfg; this.user_id = user_id; this.user_name = user_name;
    }

    // SETTINGS

    public string set_setting(string setting, string val) {
      db_conn.exec(core.parse_query("base.set-setting"
        , new string[,] { { "setting", setting }, { "value", string.IsNullOrEmpty(val) ? "" : val }, { "user_id", user_id.ToString() } }));
      return val;
    }

  }
}
