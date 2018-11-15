using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mlib.tiles {
  public class user {
    public enum type_user { none, admin, normal }

    protected type_user _tp_user;
    protected string _user = null, _email = null;
    protected int _id_user;

    public user (int id, string user, string email, type_user tp) {
      _id_user = id; _user = user; _email = email; _tp_user = tp;
    }

    public string name { get { return _user; } set { _user = value; } }
    public string email { get { return _email; } set { _email = value; } }
    public int id { get { return _id_user; } set { _id_user = value; } }
    public type_user type { get { return _tp_user; } set { _tp_user = value; } }
  }
}
