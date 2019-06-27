using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace molello.classes {
  public class item_group {
    protected int _level, _i_grp;
    protected string _okey;

    public int level { get { return _level; } set { _level = value; } }
    public int grp { get { return _i_grp; } set { _i_grp = value; } }
    public string okey { get { return _okey; } set { _okey = value; } }

    public item_group () { _level = _i_grp = 0; _okey = ""; }

    public void set (int level, int i_grp, string okey) {
      _level = level; _i_grp = grp; _okey = okey;
    }
  }
}
