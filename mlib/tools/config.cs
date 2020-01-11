using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using mlib.xml;

namespace mlib.tools {
  public class config {

    #region structure

    public class base_class {
      bool _for_page = false;
      public bool for_page { get { return _for_page; } }

      string _doc_key = "";
      public string doc_key { get { return _doc_key; } }

      public base_class (string doc_key, bool for_pg) { _doc_key = doc_key; _for_page = for_pg; }
    }

    // var
    public class var : base_class {
      string _name, _value;
      public var (string doc_key, string name, string value, bool for_pg = false) : base(doc_key, for_pg) { _name = name; _value = value; }

      public string name { get { return _name; } }
      public string value { get { return _value; } set { _value = value; } }
    }

    // folder
    public class folder : base_class {
      string _name, _path;
      public folder (string doc_key, string name, string path, bool for_pg = false) : base(doc_key, for_pg) { _name = name; _path = path; }

      public string name { get { return _name; } }
      public string path { get { return _path; } set { _path = value; } }
    }

    // conn
    public class conn : base_class {
      string _name, _des, _conn, _provider, _date_format, _sql_key, _key;
      int _timeout = 0;
      public conn (string doc_key, string name, string conn, string provider
        , string des, string date_format, int timeout, string key, string sql_key, bool for_pg = false)
        : base(doc_key, for_pg) {
        _name = name; _conn = conn; _provider = provider;
        _des = des; _date_format = date_format; _timeout = timeout; _key = ""; _sql_key = sql_key;
      }

      public string name { get { return _name; } }
      public string sql_key { get { return _sql_key; } }
      public string key { get { return _key; } }
      public string des { get { return _des; } set { _des = value; } }
      public string conn_string { get { return _conn; } set { _conn = value; } }
      public string provider { get { return _provider; } set { _provider = value; } }
      public string date_format { get { return _date_format; } set { _date_format = value; } }
      public int timeout { get { return _timeout; } set { _timeout = value; } }
    }

    // query
    public class query : base_class {
      public enum tp_query { normal, do_while }
      tp_query _tp = tp_query.normal;
      string _name, _des, _do, _while;
      List<string> _queries = new List<string>();
      public query (string doc_key, string name, string txt, string des = "", bool for_pg = false)
        : base(doc_key, for_pg) {
        _name = name; _des = des; _queries.Add(txt);
      }
      public query (string doc_key, string name, string des = "", bool for_pg = false)
        : base(doc_key, for_pg) {
        _name = name; _des = des;
      }
      public tp_query tp { get { return _tp; } set { _tp = value; } }
      public string text_do { get { return _do; } set { _do = value; } }
      public string text_while { get { return _while; } set { _while = value; } }
      public string des { get { return _des; } }
      public string name { get { return _name; } }
      public string text { get { return _queries[0]; } }
      public void add_query (string txt) { _queries.Add(txt); }
      public List<string> queries { get { return _queries; } }
      public int count { get { return _queries.Count(); } }
    }

    // table
    public class table : base_class {
      string _name; List<string> _cols; List<table_row> _rows;

      public table (string doc_key, string name, xml_node tbl, bool for_pg = false)
        : base(doc_key, for_pg) {
        _name = name; _rows = new List<table_row>();
        _cols = tbl.get_attr("cols").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        foreach (xml_node row in tbl.nodes("rows/row"))
          _rows.Add(new table_row(this, _cols.Select(c => row.get_attr(c)).ToList()));
      }

      public string name { get { return _name; } }
      public int i_col (string name) { return _cols.IndexOf(name); }
      public List<table_row> rows { get { return _rows; } }
      public List<table_row> rows_ordered (string col) { return _rows.OrderBy(r => r.field(col)).ToList(); }
      public List<table_row> rows_ordered (string col, string col2) { return _rows.OrderBy(r => r.field(col) + r.field(col2)).ToList(); }
      public List<table_row> rows_ordered (string col, string col2, string col3) { return _rows.OrderBy(r => r.field(col) + r.field(col2) + r.field(col3)).ToList(); }
      public table_row find_row (Dictionary<string, string> keys) {
        foreach (table_row r in _rows) if (is_row(r, keys)) return r; return null;
      }
      protected bool is_row (table_row tr, Dictionary<string, string> keys) {
        foreach (KeyValuePair<string, string> kv in keys)
          if (tr.field(kv.Key) != kv.Value) return false;
        return true;
      }
    }

    public class table_row {
      table _tbl; List<string> _fields;
      public table_row (table tbl, List<string> vals) { _tbl = tbl; _fields = new List<string>(vals); }
      public string field (string col) { return _fields[_tbl.i_col(col)]; }
      public string this[string col] { get { return field(col); } }
    }

    // html-block
    public class html_block : base_class {
      string _name, _content;
      public string name { get { return _name; } }
      public string content { get { return _content; } }
      public html_block (string doc_key, string name, string content, bool for_pg = false) : base(doc_key, for_pg) { _name = name; _content = content; }
    }

    // level
    public class level : base_class {
      string _name, _color, _title_size;
      public string name { get { return _name; } }
      public string color { get { return _color; } }
      public string title_size { get { return _title_size; } }
      public level (string doc_key, string name, string color, string title_size, bool for_pg = false)
        : base(doc_key, for_pg) {
        _name = name; _color = color; _title_size = title_size;
      }
    }

    #endregion

    core _cr = null;
    Dictionary<string, folder> _folders = new Dictionary<string, folder>();
    Dictionary<string, var> _vars = new Dictionary<string, var>();
    Dictionary<string, conn> _conns = new Dictionary<string, conn>();
    Dictionary<string, table> _tables = new Dictionary<string, table>();
    Dictionary<string, html_block> _blocks = new Dictionary<string, html_block>();
    Dictionary<string, level> _levels = new Dictionary<string, level>();
    Dictionary<string, query> _queries = new Dictionary<string, query>();
    int _max_level = -1;

    public config (core cr) { _cr = cr; }

    public void reset () { _tables.Clear(); _folders.Clear(); _vars.Clear(); _conns.Clear(); _blocks.Clear(); _levels.Clear(); _max_level = -1; }

    public void load_doc (string doc_key, string vars_key, xml_doc doc, bool for_pg = false) {

      string var_key = !string.IsNullOrEmpty(vars_key) ? vars_key + "." : "";

      // aggiungo
      foreach (xml_node vars in doc.nodes("/config/vars")) {
        string machine = vars.get_attr("machine");
        string bname = vars.get_attr("name");
        foreach (xml_node var in vars.nodes("var")) {
          string machine2 = var.get_attr("machine") != "" ? var.get_attr("machine") : machine;
          if (machine2 != "" && core.machine_name().ToLower() != machine2.ToLower()) continue;
          string name = var_key + bname + var.get_attr("name");
          _vars.Add(name, new var(doc_key, name, _cr.parse(var.get_val()), for_pg));
        }
      }

      foreach (xml_node var in doc.nodes("/config//folders/folder")) {
        string name = var_key + var.get_attr("name");
        _folders.Add(name, new folder(doc_key, name, _cr.parse(var.get_val()), for_pg));
      }

      foreach (xml_node var in doc.nodes("/config//conns/conn")) {
        string name = var_key + var.get_attr("name");
        _conns.Add(name, new conn(doc_key, name, var.get_attr("conn-string"), var.get_attr("provider"), var.get_attr("des")
          , var.get_attr("date-format"), var.get_int("timeout", 0), var.get_attr("key"), var.sub_node("sql_key").text, for_pg));
      }

      foreach (xml_node tbl in doc.nodes("/config//tables/table")) {
        string name = var_key + tbl.get_attr("name");
        _tables.Add(name, new table(doc_key, name, tbl, for_pg));
      }

      foreach (xml_node tbl in doc.nodes("/config//blocks/*")) {
        string name = var_key + tbl.name;
        _blocks.Add(name, new html_block(doc_key, name, tbl.text, for_pg));
      }

      foreach (xml_node lev in doc.nodes("/config//style-levels/level")) {
        _levels.Add(lev.get_attr("name"), new level(doc_key, lev.get_attr("name"), lev.get_attr("color"), lev.get_attr("title-size"), for_pg));
      }
      _max_level = max_level();

      foreach (xml_node qry in doc.nodes("/config//queries/*")) {
        if (qry.name == "query") {
          string name = var_key + qry.get_attr("name");
          _queries.Add(name, new query(doc_key, name, qry.text, qry.get_attr("des"), for_pg));
        } else if (qry.name == "query_do") {
          string name = var_key + qry.get_attr("name");
          _queries.Add(name, new query(doc_key, name, qry.get_attr("des"), for_pg) {
            tp = query.tp_query.do_while,
            text_do = qry.sub_node("do").text, text_while = qry.sub_node("while").text
          });
        } else if (qry.name == "queries") {
          string name = var_key + qry.get_attr("name");
          query q = new query(doc_key, name, qry.get_attr("des"), for_pg);
          _queries.Add(name, q);
          foreach (xml_node q2 in qry.nodes("*")) {
            if (q2.name == "query") q.add_query(q2.text);
            else if (q2.name == "exec_query") {
              foreach (string q3 in _queries[q2.get_attr("name")].queries) q.add_query(q3);
            }
          }
        }
      }
    }

    public void remove_for_page () {
      string k = _folders.FirstOrDefault(x => x.Value.for_page == true).Key;
      while (k != null) { _folders.Remove(k); k = _folders.FirstOrDefault(x => x.Value.for_page == true).Key; }

      k = _vars.FirstOrDefault(x => x.Value.for_page == true).Key;
      while (k != null) { _vars.Remove(k); k = _vars.FirstOrDefault(x => x.Value.for_page == true).Key; }

      k = _conns.FirstOrDefault(x => x.Value.for_page == true).Key;
      while (k != null) { _conns.Remove(k); k = _conns.FirstOrDefault(x => x.Value.for_page == true).Key; }

      k = _tables.FirstOrDefault(x => x.Value.for_page == true).Key;
      while (k != null) { _tables.Remove(k); k = _tables.FirstOrDefault(x => x.Value.for_page == true).Key; }

      k = _blocks.FirstOrDefault(x => x.Value.for_page == true).Key;
      while (k != null) { _blocks.Remove(k); k = _blocks.FirstOrDefault(x => x.Value.for_page == true).Key; }

      k = _levels.FirstOrDefault(x => x.Value.for_page == true).Key;
      while (k != null) { _levels.Remove(k); k = _levels.FirstOrDefault(x => x.Value.for_page == true).Key; }
      _max_level = max_level();

      k = _queries.FirstOrDefault(x => x.Value.for_page == true).Key;
      while (k != null) { _queries.Remove(k); k = _queries.FirstOrDefault(x => x.Value.for_page == true).Key; }
    }

    public bool exists_var (string name) { return _vars.ContainsKey(name); }
    public string var_value(string name) { if (!_vars.ContainsKey(name)) throw new Exception("la variabile '" + name + "' non esiste!"); return _vars[name].value; }
    public var get_var(string name) { if (!_vars.ContainsKey(name)) throw new Exception("la variabile '" + name + "' non esiste!"); return _vars[name]; }
    public string var_value_par(string name, string par) { if (!_vars.ContainsKey(name)) throw new Exception("la variabile '" + name + "' non esiste!"); return _vars[name].value.Replace("[@par]", par); }
    public folder get_folder(string name) { if (!_folders.ContainsKey(name)) throw new Exception("il folder '" + name + "' non esiste!"); return _folders[name]; }
    public conn get_conn (string name) { if (!_conns.ContainsKey(name)) throw new Exception("la connessione '" + name + "' non esiste!"); return _conns[name]; }
    public table get_table (string name) { if (!_tables.ContainsKey(name)) throw new Exception("la tabella '" + name + "' non esiste!"); return _tables[name]; }
    public html_block get_block (string name) { if (!_blocks.ContainsKey(name)) throw new Exception("il blocco html '" + name + "' non esiste!"); return _blocks[name]; }
    public query get_query (string name) { if (!_queries.ContainsKey(name)) throw new Exception("la query '" + name + "' non esiste!"); return _queries[name]; }
    public level get_level (int index) { if (!_levels.ContainsKey(index.ToString())) throw new Exception("il livello " + index.ToString() + " non esiste!"); return _levels[index.ToString()]; }
    protected int max_level () {
      int max = -1;
      foreach (level l in _levels.Values) {
        int i = strings.is_int(l.name) ? int.Parse(l.name) : -1;
        max = i > max ? i : max;
      }
      return max;
    }
    public level get_max_level () { if (_max_level < 0 || !_levels.ContainsKey(_max_level.ToString())) throw new Exception("il livello massimo non c'è!"); return _levels[_max_level.ToString()]; }
    public bool exists_level (int index) { return _levels.ContainsKey(index.ToString()); }
  }
}
