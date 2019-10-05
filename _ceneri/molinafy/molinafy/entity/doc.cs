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
    protected List<paragraph> _pars;

    public int id { get { return _id; } }
    public string title { get { return _title; } set { _title = clean_html_txt(value); } }
    public string des { get { return _des; } set { _des = clean_html_txt(value); } }
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

    public void upd_testata (int user) {
      dal.exec_qry(app._conn, "doc.upd-testata"
        , new Dictionary<string, object>() { { "id_doc", _id }, { "title_doc", _title }, { "des_doc", _des }, { "user", user } });
    }

    public string clean_html_txt (string txt) { 
      return string.IsNullOrEmpty(txt) ? "" : string.IsNullOrEmpty(txt) ? "" : txt.Replace("&nbsp;", " ").Replace("<br>", "\r\n").Replace("</br>", "\r\n").Trim();
    }
  }
}
