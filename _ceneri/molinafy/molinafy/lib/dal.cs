using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using mlib;

namespace molinafy {
  public class dal {
    public static string str_val (object val) { return db_provider.str_val(val); }
    public static int int_val (object val, int def = 0) { return db_provider.int_val(val, def); }
    public static int? int_n_val (object val) { return db_provider.int_n_val(val); }
    public static DateTime date_val (object val) { return db_provider.date_val(val); }
    public static DateTime? date_n_val (object val) { return db_provider.date_n_val(val); }
    public static DataTable dt_qry (db_provider db, string qry_name, Dictionary<string, object> flds = null) {
      try {
        return db.dt_qry(app.cfg.get_query(qry_name), app._core, flds);
      } catch (Exception ex) { throw new Exception("esecuzione query: " + qry_name, ex); }
    }
    public static DataRow first_row (db_provider db, string qry_name, Dictionary<string, object> flds = null) {
      DataTable dt = dt_qry(db, qry_name, flds);
      return dt != null && dt.Rows.Count > 0 ? dt.Rows[0] : null;
    }
    public static string exec_qry (db_provider db, string qry_name, Dictionary<string, object> flds = null, bool getidentity = false) {
      try {
      return db.exec_qry(app.cfg.get_query(qry_name), app._core, flds, getidentity);
      } catch (Exception ex) { throw new Exception("esecuzione query: " + qry_name, ex); }
    }
  }
}
