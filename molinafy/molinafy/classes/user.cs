using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using mlib;

namespace molinafy {
  public class user {
    int _id_user; string _user_name; DateTime _dt_logged;

    public int id_user { get { return _id_user; } }
    public string user_name { get { return _user_name; } }
    public DateTime dt_logged { get { return _dt_logged; } }

    public user (int id_user, string user_name, DateTime dt_logged) {
      _id_user = id_user;
      _user_name = user_name;
      _dt_logged = dt_logged;
    }

    public static user active_user () {
      DataRow dr = dal.first_row(app._conn, "user.check-user"
        , new Dictionary<string, object>() { { "address_ip", sys.local_ip() } });
      return dr != null ? new user(dal.int_val(dr["id_user"]), dal.str_val(dr["user_name"])
        , dal.date_val(dr["dt_ins"])) : null;
    }

    public static void logout () {
      dal.exec_qry(app._conn, "user.logout-user"
        , new Dictionary<string, object>() { { "address_ip", sys.local_ip() } });
    }

    public static user login (string user_name, string password, int? days = null) {
      DataRow dr = dal.first_row(app._conn, "user.login-user"
        , new Dictionary<string, object>() { { "address_ip", sys.local_ip() }
          , { "user_name", user_name }, { "password", password }
          , { "days", days.HasValue ? (object)days.Value : null } });
      return dr != null ? new user(dal.int_val(dr["id_user"]), dal.str_val(dr["user_name"])
        , dal.date_val(dr["dt_ins"])) : null;
    }
  }
}
