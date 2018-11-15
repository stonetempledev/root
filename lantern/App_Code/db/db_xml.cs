using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Xml;
using deeper.frmwrk;
using deeper.lib;

namespace deeper.db
{
  public class db_xml : db_schema
  {
    List<xml_data> _datatbls = null;

    public db_xml(xmlDoc schema_doc, string path_meta = "", string des = "")
      : base(schema_doc, path_meta, des) {
      _groupConn = _schema.doc.root_value("group");
      _datatbls = new List<xml_data>();
    }

    public bool is_opened_xmldata(string table, int? level = null) { return get_xmldata(table, level) != null; }
    protected xml_data open_xmldata(string table, int? level = null) {
      xml_data dt = get_xmldata(table, level);
      if (dt == null) {
        dt = new xml_data(this, table, level);
        _datatbls.Add(dt);
      }
      return dt;
    }
    public xml_data get_xmldata(string table, int? level = null) {
      return _datatbls.FirstOrDefault(x => x.table.ToUpper() == table.ToUpper()
        && (!level.HasValue || (level.HasValue && x.level == level)));
    }
    public void close_xmldata(string table, int? level = null) {
      xml_data tbl = _datatbls.FirstOrDefault(x => x.table.ToUpper() == table.ToUpper()
        && (!level.HasValue || (level.HasValue && x.level == level)));
      if (tbl != null) { tbl.close();  _datatbls.Remove(tbl); }
    }
    protected void close_data() { _datatbls.ForEach(x => { x.close(); }); _datatbls.Clear(); }
    protected override void closeConn(bool commit) {
      close_data();
    }

    public override List<string> tables(string likeName = "", string list = "") { return new List<string>(schema.tables_name(likeName)); }
    public bool there_data(string table) { return schema.doc.get_value("/root/tables/table[@nameupper='" + table.ToUpper() + "']", "data") != ""; }
    public bool exist_data(string table) { return System.IO.File.Exists(data_path(table)); }
    public string data_path(string table) {
      return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(schema.doc.path),
          schema.doc.get_value("/root/tables/table[@nameupper='" + table.ToUpper() + "']", "data"));
    }

    public void xmldata_to_table(db_schema db, string table) {
      List<schema_field> cols = db.table_fields(table);

      // cols
      using (GotDotNet.XPath.XPathReader xr = new GotDotNet.XPath.XPathReader(data_path(table), "/root/data")) {
        if (xr.ReadUntilMatch()) {
          while (xr.MoveToNextAttribute()) {
            schema_field field = findField(cols, xr.Value);
            if (field == null) continue;
            field.AttrName = xr.Name;
          }
        }
        else throw new Exception("la struttura xml del file data della tabella '" + table + "' non è corretta");
      }

      // insert rows
      bool identity = db.type == dbType.sqlserver && cols.FirstOrDefault(x => x.AutoNumber) != null;
      if (identity) db.set_identity(table, true);
      try {
        using (GotDotNet.XPath.XPathReader xr = new GotDotNet.XPath.XPathReader(data_path(table), "/root/rows/row")) {
          string header = string.Format("INSERT INTO {0} ({1})", table, string.Join(", ", cols.Select(x => "[" + x.Name + "]")));
          while (xr.ReadUntilMatch())
            db.exec(header + " VALUES (" + string.Join(", ", cols.Select(x => db.val_toqry(xr[x.AttrName], x.TypeField, type, _nullxml))) + ")");
        }
      }
      finally { if (identity) db.set_identity(table, false); }
    }

    public override bool exist_field(string table, string col) { return _schema.existCol(table, col); }

    public override bool exist_table(string tableName) { return _schema.existTable(tableName); }

    public override string module_text(string procName) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità module_text"); }

    public override List<string> functions(string likeName = "") { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità functions"); }

    public override List<string> store_procedures(string likeName = "") { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità store_procedures"); }

    public override bool exist_function(string function) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità exist_function"); }

    public override bool exist_procedure(string procedure) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità exist_procedure"); }

    public override List<schema_field> table_fields(string table, string nameField = "") { return string.IsNullOrEmpty(nameField) ? _schema.table_fields(table) : _schema.table_fields(table, nameField); }

    public override List<idx_table> table_idxs(string table, bool? uniques = null, string index_name = "") {
      return _schema.table_indexes(table, uniques)
        .Where(x => string.IsNullOrEmpty(index_name) || (!string.IsNullOrEmpty(index_name) && string.Compare(x.Name, index_name, false) == 0)).ToList();
    }

    public override DataSet dt_set(string sql, bool throwerr = true) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità openDataSet"); }
    public override DataSet dt_set(string sql, string table_name, bool throwerr = true) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità openDataSet"); }
    public override DataTable dt_table(string sql, bool throwerr = true) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità openDataTable"); }
    public override DataTable dt_table(string sql, string table_name, bool throwerr = true) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità openDataTable"); }

    public override System.Data.Common.DbDataReader dt_reader(string sql) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità dt_reader"); }

    public bool read_row(string table, int? level = null) { return open_xmldata(table, level).read_row(); }

    public void begin_table(string table, int? level = null, bool with_all_close = false) {
      if (with_all_close) { close_data(); open_xmldata(table, level); }
      else{
        if (!is_opened_xmldata(table, level)) open_xmldata(table, level);
        else get_xmldata(table, level).to_begin();
      }
    }

    public bool find_row_byid(string table, string id_fld, string value, int? level = null) {
      long n = 0;
      if (long.TryParse(value, out n))
        return find_row_byid(table, id_fld, n, level);
      return false;
    }

    public bool find_row_byid(string table, string id_fld, long value, int? level = null) {
      return (!is_opened_xmldata(table, level) ? open_xmldata(table, level) : get_xmldata(table, level)).find_row_byid(id_fld, value);
    }
  }
}

