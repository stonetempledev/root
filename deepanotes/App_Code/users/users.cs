using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using mlib.db;

namespace toyn {
  public class users : bo {
    public users() {
    }

    public List<user> list_users() {
      return db_conn.dt_table(core.parse(config.get_query("users.users").text)).Rows.Cast<DataRow>()
        .Select(x => new user(db_provider.int_val(x["user_id"]), db_provider.str_val(x["nome"]), db_provider.str_val(x["email"])
          , user.type_user.normal, db_provider.int_val(x["activated"]) == 1, db_provider.dt_val(x["dt_activate"]), db_provider.dt_val(x["dt_upd"]))).ToList();
    }
  }
}