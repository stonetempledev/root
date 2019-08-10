using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace molinafy {
  public class doc {

    protected int _id;
    protected string _title, _des;
    protected DateTime _created;

    public int id { get { return _id; } }
    public string title { get { return _title; } }
    public string des { get { return _des; } }
    public DateTime created { get { return _created; } }

    public doc (int id_doc) {
      _id = id_doc;
      reload_doc();
    }

    protected void reload_doc () {
      DataRow dr = dal.first_row(app._conn, "doc.testata"
        , new Dictionary<string, object>() { { "id_doc", _id } });
      _title = dal.str_val(dr["title_doc"]);
      _des = dal.str_val(dr["des_doc"]);
      _created = dal.date_val(dr["dt_ins"]);
    }
  }
}
