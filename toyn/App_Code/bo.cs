using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Data;
using mlib;
using mlib.db;
using mlib.tools;

namespace toyn {
  // business object
  public class bo {
    tl_page _page = null;
    public db_provider db_conn { get { return _page.db_conn; } }
    public core core { get { return _page.core; } }
    public config config { get { return _page.core.config; } }
    public int user_id { get { return _page.user.id; } }
    public string user_name { get { return _page.user.name; } }

    public bo() {
      _page = HttpContext.Current.Handler as tl_page;
      if (_page == null) throw new Exception("pagina attiva non valida!");
    }

    // SETTINGS

    public string set_setting(string setting, string val) {
      db_conn.exec(core.parse_query("base.set-setting"
        , new string[,] { { "setting", setting }, { "value", string.IsNullOrEmpty(val) ? "" : val }, { "user_id", user_id.ToString() } }));
      return val;
    }

    // EMPHASIES

    public List<emphasis> load_emphasies() {
      return db_conn.dt_table(core.parse_query("base.emphasies")).Rows.Cast<DataRow>()
        .Select(x => new emphasis() { style = db_provider.str_val(x["style"]), title = db_provider.str_val(x["title"])
          , order = db_provider.int_val(x["order"]) }).ToList();
    }
  }
}