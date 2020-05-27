using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace deepanotes {
  public class user {
    public enum type_user { none, admin, normal }

    protected type_user _tp_user;
    protected string _user = null, _email = null;
    protected int _id_user;
    protected bool _activated;
    protected DateTime? _dt_activate, _dt_upd;

    public string name { get { return _user; } set { _user = value; } }
    public string email { get { return _email; } set { _email = value; } }
    public int id { get { return _id_user; } set { _id_user = value; } }
    public string id_str { get { return _id_user.ToString(); } }
    public type_user type { get { return _tp_user; } set { _tp_user = value; } }
    public bool activated { get { return _activated; } set { _activated = value; } }
    public bool disactive { get { return !_activated; } }
    public DateTime? dt_activate { get { return _dt_activate; } set { _dt_activate = value; } }
    public DateTime? dt_upd { get { return _dt_upd; } set { _dt_upd = value; } }

    public user(int id, string user, string email, type_user tp, bool activated = true
      , DateTime? dt_activate = null, DateTime? dt_upd = null) {
      _id_user = id; _user = user; _email = email; _tp_user = tp; _activated = activated;
      _dt_activate = dt_activate; _dt_upd = dt_upd;
    }

  }
}