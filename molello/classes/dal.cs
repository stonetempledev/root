using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using mlib.db;

namespace molello.classes {
  public class base_dal {
    public static string str_val (object val) { return db_provider.str_val(val); }
    public static int int_val (object val) { return db_provider.int_val(val); }
    public static DataTable dt_qry (string qry_name, Dictionary<string, object> flds = null) {
      return app._db.dt_qry(app.cfg.get_query(qry_name), app._core, flds);
    }
    public static void exec_qry (string qry_name, Dictionary<string, object> flds = null) {
      app._db.exec_qry(app.cfg.get_query(qry_name), app._core, flds);
    }
  }
}
