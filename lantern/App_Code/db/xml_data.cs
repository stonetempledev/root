using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace deeper.db
{
  public class xml_data : System.IDisposable
  {
    GotDotNet.XPath.XPathReader _xr = null;
    List<Dictionary<string, string>> _cols = null; // ...colonne cols/col
    List<schema_field> _flds = null;
    List<string> _key_flds = null;
    string _table = null, _path = null;
    db_xml _db_xml = null;
    int _level = 1;

    public xml_data(string path, string table = null, schema_doc sch = null, int? level = null) { 
      _table = table; 
      read_cols(path, sch); 
      _level = level.HasValue ? level.Value : _level; 
    }

    public xml_data(db_xml db, string table, int? level = null) {
      _db_xml = db; _table = table;
      _key_flds = db.meta_doc.indexUnique(table) != null ? new List<string>(db.meta_doc.indexUnique(table).Fields.Select(x => x.Name))
        : new List<string>();
      if (db.exist_data(table)) read_cols(db.data_path(table));
      _level = level.HasValue ? level.Value : _level;
    }

    public int level { get { return _level; } }

    protected void read_cols(string path, schema_doc sch = null) {
      _path = path;

      _cols = new List<Dictionary<string, string>>();
      _flds = new List<schema_field>();

      // carico le colonne di struttura + schema campi
      using (GotDotNet.XPath.XPathReader xr = new GotDotNet.XPath.XPathReader(_path, "/root/cols/col")) {
        while (xr.ReadUntilMatch()) {
          Dictionary<string, string> col = new Dictionary<string, string>();
          while (xr.MoveToNextAttribute())
            col.Add(xr.Name, xr.Value);

          _flds.Add(new schema_field(dbType.xml, col["name"], col["type"], col.ContainsKey("nullable") ? bool.Parse(col["nullable"]) : false
              , col.ContainsKey("maxlength") ? int.Parse(col["maxlength"]) : (int?)null, col.ContainsKey("numprec") ? int.Parse(col["numprec"]) : (int?)null
              , col.ContainsKey("numscale") ? int.Parse(col["numscale"]) : (int?)null, col.ContainsKey("default") ? col["numscale"] : ""
              , col.ContainsKey("autonumber") ? bool.Parse(col["autonumber"]) : false
              , col["level"] == "1" && col.ContainsKey("pkfield") && col["pkfield"] == "1", 0));

          _cols.Add(col);
        }
      }

      // carico i campi
      using (GotDotNet.XPath.XPathReader xr = new GotDotNet.XPath.XPathReader(_path, "/root/data")) {
        if (xr.ReadUntilMatch()) {
          schema_doc sk = _db_xml != null ? _db_xml.schema : sch;
          if (_flds.Count == 0)
            for (int i = 0; i < xr.AttributeCount; i++)
              _flds.Add(sk.new_schema_field(sk.field_node(table, xr.GetAttribute("f" + i.ToString("00")))
                , "f" + i.ToString("00")));
          else
            for (int i = 0; i < xr.AttributeCount; i++)
              _flds.Find(x => x.Name.ToUpper() == xr.GetAttribute("f" + i.ToString("00")).ToUpper()).AttrName = "f" + i.ToString("00");
        }
      }

      to_begin();
    }

    public void to_begin() {
      if (_path != null) {
        if (_xr != null) _xr.Close();
        _xr = new GotDotNet.XPath.XPathReader(_path, "/root/rows/row");
      }
    }

    public int count_defcols { get { return _cols.Count; } }
    public string defcol(int i, string info) { return _cols[i][info]; }
    public string find_defcol(string filter, string info) { return find_defcols(filter, info, true); }
    public string find_defcols(string filter, string info, bool first = false) {
      string result = "";
      foreach (Dictionary<string, string> icol in _cols) {
        bool find = true;
        foreach (string cond in filter.Split(','))
          if (!icol.ContainsKey(cond.Split('=')[0]) || icol[cond.Split('=')[0]].ToUpper() != cond.Split('=')[1].ToUpper()) { find = false; break; }
        if (find) { result += result != "" ? "," : icol[info]; if (first) break; }
      }
      return result;
    }

    public string table { get { return _table; } }

    List<string> key_flds { get { return _key_flds; } }

    public string data_path { get { return _path; } }

    public bool exists { get { return _path != null; } }

    public string val_to_qry(string field, db_schema db_ref) {
      schema_field sfld = get_field(field);
      return db_ref.val_toqry(_xr[sfld.AttrName], sfld.TypeField, db_schema.null_value);
    }

    public string val(string field) {
      schema_field sfld = get_field(field);
      return _xr[sfld.AttrName] != null && _xr[sfld.AttrName] != db_schema.null_value ? _xr[sfld.AttrName] : null;
    }

    public DateTime? date(string field) {
      string vl = val(field);
      return vl != null ? DateTime.Parse(vl) : (DateTime?)null;
    }

    public string val_to_qry(string val, string field, db_schema db_ref) {
      return db_ref.val_toqry(val == null ? db_schema.null_value : val, get_field(field).TypeField, db_schema.null_value);
    }

    public long val_pkid() { return long.Parse(val(_flds.Find(x => x.Primary).Name)); }

    public long? val_id(string field) {
      try {
        string val = _xr[get_field(field).AttrName];
        return string.IsNullOrEmpty(val) || val == db_schema.null_value ? (long?)null : long.Parse(val);
      }
      catch (Exception ex) { throw ex; }
    }

    public string field_val(string field) { return _xr.GetAttribute(get_field(field).AttrName); }

    protected long? n_val(string field) {
      string val = field_val(field);
      return val == null ? (long?)null : long.Parse(val);
    }

    protected schema_field get_field(string field) { return _flds.FirstOrDefault(x => x.Name.ToLower() == field.ToLower()); }

    public bool read_row() { return _path == null ? false : _xr.ReadUntilMatch(); }

    public void Dispose() { close(); }
    public void close() { if (_xr != null) { _xr.Close(); _xr = null; } if (_flds != null) { _flds.Clear(); _flds = null; } }

    public bool find_row_byid(string id_fld, string value) {
      long n = 0;
      if (long.TryParse(value, out n))
        return find_row_byid(id_fld, n);
      return false;
    }

    public bool find_row_byid(string id_fld, long value) {
      if (val_id(id_fld) == value)
        return true;

      to_begin();
      while (read_row())
        if (val_id(id_fld) == value)
          return true;

      return false;
    }
  }
}
