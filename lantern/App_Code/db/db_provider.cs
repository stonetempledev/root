using System;
using System.Data;
using System.Collections.Generic;
using System.Web;
using System.Data.Common;
using System.Xml;
using System.Linq;
using deeper.frmwrk;

//Here is the once-per-application setup information
//[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace deeper.db
{
  public enum dbType
  { none, oledb, odbc, sqlserver, mysql, access, xml }

  public enum fieldType
  { LONG, BINARY, BOOL, MONEY, DATETIME, DATETIME2, GUID, DOUBLE, SINGLE, SMALLINT, INTEGER, DECIMAL, VARCHAR, CHAR, XML, TEXT }

  public enum catField
  { NUMERIC, TEXT, DATE }

  public class db_provider
  {
    protected string _nameConn = "", _groupConn = "", _connString = "", _provName = "", _language = ""
      , _des = "", _dateFormatToQuery, _dateFormatToQuery2;
    protected dbType _dbType = dbType.none;
    protected int _timeout = -1;
    protected DbConnection _conn = null;
    protected DbTransaction _trans = null;
    protected Dictionary<string, string> _keys = null;
    static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    public db_provider(string name, string connString, string provName, int timeout = -1, string group = "", string language = "", string des = ""
      , string dateFormatToQuery = "", string dateFormatToQuery2 = "") {
      _dbType = db_provider.type_from_provider(connString, provName); _nameConn = name; _groupConn = group;
      _timeout = timeout; _connString = connString; _provName = provName; _language = language; _des = des;
      _dateFormatToQuery = dateFormatToQuery != "" ? dateFormatToQuery : "yyyy-MM-dd HH:mm:ss";
      _dateFormatToQuery2 = dateFormatToQuery2 != "" ? dateFormatToQuery2 : "yyyy-MM-dd HH:mm:ss.fffffff";
      _keys = new Dictionary<string, string>();
      foreach (string value in _connString.Split(';')) {
        int iUguale = value.IndexOf('=');
        if (iUguale > 0) _keys.Add(value.Substring(0, iUguale).ToLower(),
          value.Substring(iUguale + 1, value.Length - iUguale - 1));
      }
    }

    static public dbType type_from_provider(string conn_string, string provider_name) {
      return provider_name == "System.Data.SqlClient" ? dbType.sqlserver : 
        (provider_name == "MySql.Data.MySqlClient" ? dbType.mysql : 
        (provider_name == "System.Data.OleDb" ? (conn_string.ToLower().IndexOf("provider=microsoft.jet.oledb.4.0") >= 0 ? dbType.access : dbType.oledb) : 
        (provider_name == "System.Data.Odbc" ? dbType.odbc :
        (provider_name == "deeper.xml" ? dbType.xml : dbType.none))));
    }

    public void logInfo(string text) { _log.Info(text); }
    public void logErr(string text) { _log.Error(text); }
    public void logErr(Exception ex) { _log.Error("error", ex); }
    public void logErr(Exception ex, string sql) { _log.Error("sql: " + sql, ex); }
    public void logSql(string text) { _log.Debug(text); }
    public void logWarning(string text) { _log.Warn(text); }

    public string dateFormatToQuery { get { return _dateFormatToQuery; } }
    public string dateFormatToQuery2 { get { return _dateFormatToQuery2; } }
    public dbType type { get { return _dbType; } }
    public string des { get { return _des; } }
    public string conn { get { return _connString; } }
    public string name { get { return _nameConn; } }
    public string group { get { return _groupConn; } }
    public Dictionary<string, string> conn_keys { get { return _keys; } }

    static public catField cat_field(fieldType tp) {
      return tp == fieldType.VARCHAR || tp == fieldType.CHAR || tp == fieldType.XML || tp == fieldType.TEXT ? catField.TEXT :
        tp == fieldType.DATETIME ? catField.DATE : catField.NUMERIC;
    }

    #region base functions

    public void begin_trans() {
      try {
        if (!is_opened()) throw new Exception("begin trans - la connessione non è aperta");
        if (_trans != null) throw new Exception("begin transaction annullata la transazione è già aperta!");

        logSql("begin transaction");
        _trans = _conn.BeginTransaction();
      } catch (Exception ex) { logErr(ex); throw ex; }
    }

    public void commit() {
      try {
        if (!is_opened()) throw new Exception("commit annullata la connessione non è aperta!");
        if (_trans == null) throw new Exception("commit annullata la transazione non è aperta!");

        logSql("commit transaction");
        _trans.Commit();
        _trans = null;
      } catch (Exception ex) { logErr(ex); throw ex; }
    }

    public void rollback() {
      try {
        if (!is_opened()) throw new Exception("rollback annullata la connessione non è aperta!");
        if (_trans == null) throw new Exception("rollback annullata la transazione non è aperta!");

        logSql("rollback transaction");
        _trans.Rollback();
        _trans = null;
      } catch (Exception ex) { logErr(ex); }
    }

    public bool open_conn() { return open_conn(false); }

    public bool open_conn_trans() { return open_conn(true); }

    protected bool open_conn(bool withBeginTrans) {
      if (_conn != null)
        return false;

      logSql("open conn '" + _connString + "'");

      _conn = create_conn(_provName, _connString);

      _conn.Open();

      if (_dbType == dbType.sqlserver && _language != "")
        exec("SET LANGUAGE " + _language);

      if (withBeginTrans && !is_trans())
        begin_trans();

      return true;
    }

    protected static DbConnection create_conn(string provider, string conn_string) {
      if (string.IsNullOrEmpty(conn_string)) throw new Exception("non è stata specificata la stringa di connessione!");

      DbProviderFactory factory = DbProviderFactories.GetFactory(provider);
      DbConnection connection = factory.CreateConnection();
      connection.ConnectionString = conn_string;
      return connection;
    }

    public bool is_opened() { return _conn != null; }

    public bool is_trans() { return _trans != null; }

    public void close_conn() { closeConn(true); }

    public void close_conn_roll() { closeConn(false); }

    protected virtual void closeConn(bool cmt) {
      if (_conn == null) return;

      if (is_trans()) {
        if (cmt) commit();
        else rollback();
      }

      logSql("close connection");

      _conn.Close();
      _conn = null;
    }

    public virtual DataSet dt_set(string sql, bool throwerr = true) { return data_set(sql, throwerr); }
    public virtual DataSet dt_set(string sql, string table_name, bool throwerr = true) { return data_set(sql, throwerr, table_name); }
    public virtual DataTable dt_table(string sql, bool throwerr = true) { return data_table(sql, "", throwerr); }
    public virtual DataTable dt_table(string sql, string table_name, bool throwerr = true) { return data_table(sql, table_name, throwerr); }

    public virtual System.Data.Common.DbDataReader dt_reader(string sql) {
      throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità dt_reader");
    }

    protected DataSet data_set(string sql, bool throwerr = true, string table_name = "") { return (DataSet)open_set(sql, throwerr, true, table_name); }

    protected DataTable data_table(string sql, string table_name = "", bool throwerr = true) {
      return (DataTable)open_set(sql, throwerr, false, table_name);
    }

    protected object open_set(string sql, bool throwerr = true, bool dataset = true, string table_name = "") {
      bool opened = false;
      try {
        opened = open_conn();

        logSql(sql);

        DbCommand cmd = _conn.CreateCommand();
        if (_trans != null) cmd.Transaction = _trans;
        cmd.CommandText = sql;
        if (_timeout > 0) cmd.CommandTimeout = _timeout;
        cmd.CommandType = CommandType.Text;

        DbDataAdapter ad = DbProviderFactories.GetFactory(_dbType == dbType.oledb || _dbType == dbType.access ? "System.Data.OleDb"
          : _dbType == dbType.odbc ? "System.Data.Odbc" : _dbType == dbType.sqlserver ? "System.Data.SqlClient" 
          : _dbType == dbType.mysql ? "MySql.Data.MySqlClient" : "").CreateDataAdapter();
        ad.SelectCommand = cmd;        

        object ds = dataset ? (object)new DataSet() :
          (table_name != "" ? (object)new DataTable(table_name) : (object)new DataTable());
        if (dataset) ad.Fill((DataSet)ds, table_name != "" ? table_name : "table1");
        else ad.Fill((DataTable)ds);
        return ds;
      } catch (Exception ex) { logErr(sql); logErr(ex); if (throwerr) throw ex; else return null; } finally { if (opened) close_conn(); }
    }

    public long exec(string sql, bool getidentity = false) { return execute(sql, getidentity); }

    protected long execute(string sql, bool getidentity = false) {
      if (getidentity && _dbType != dbType.sqlserver)
        throw new Exception("GET IDENTITY supportato solo in SQLServer");

      bool opened = false;
      try {
        if (open_conn()) opened = true;

        if (getidentity) sql += ";select SCOPE_IDENTITY();";

        logSql(sql);

        DbCommand command = _conn.CreateCommand();
        command.CommandText = sql;
        if (_timeout > 0) command.CommandTimeout = _timeout;
        if (_trans != null) command.Transaction = _trans;

        if (!getidentity)
          return (long)command.ExecuteNonQuery();

        DbDataReader reader = command.ExecuteReader();
        if (reader.HasRows) {
          reader.Read();
          long result = reader[0] != DBNull.Value ? Convert.ToInt32(reader[0]) : -1;

          reader.Close();

          return result;
        }

        return -1;
      } catch (Exception ex) { logErr(ex, sql); throw ex; } finally { if (opened) close_conn(); }
    }

    public string key_conn(string key) { return _keys.ContainsKey(key.ToLower()) ? _keys[key.ToLower()] : ""; }

    public virtual System.Data.Common.DbParameter get_par(schema_field col) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità getParameter"); }

    public virtual System.Data.Common.DbCommand dt_command(string sql) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità openDataCommand"); }

    static public System.Xml.XmlDocument set_todoc(System.Data.DataTable dt) {

      // salvo xml 
      System.Xml.XmlDocument docSet = new System.Xml.XmlDocument();
      System.IO.StringWriter stringWriter = new System.IO.StringWriter();
      DataSet ds = new DataSet(); ds.Tables.Add(dt); 
      ds.WriteXml(new System.Xml.XmlTextWriter(stringWriter), System.Data.XmlWriteMode.WriteSchema);
      docSet.LoadXml(stringWriter.ToString());

      System.Xml.XmlNamespaceManager nm = new System.Xml.XmlNamespaceManager(docSet.NameTable);
      nm.AddNamespace("xs", "http://www.w3.org/2001/XMLSchema");
      System.Xml.XmlNodeList fields = docSet.SelectNodes("/" + ds.DataSetName + "/xs:schema//xs:element[@name='" + ds.Tables[0].TableName
        + "']/xs:complexType/xs:sequence/xs:element", nm);
      if (fields == null || fields.Count == 0)
        return null;

      // costruzione xsl                
      System.Xml.Xsl.XslCompiledTransform xslDoc = new System.Xml.Xsl.XslCompiledTransform();
      string strXsl = "<?xml version='1.0'?>"
        + "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">"
        + "<xsl:template match=\"/\">"
        + " <rows schema='xmlschema.datatable'>"
        + "  <xsl:for-each select=\"/" + ds.DataSetName + "/" + ds.Tables[0].TableName + "\"><row>"
        + string.Concat(fields.Cast<XmlNode>().Select(x =>
          string.Format("  <{0}><xsl:value-of select=\"{0}\"/></{0}>", x.Attributes["name"].Value)))
        + "  </row></xsl:for-each>"
        + " </rows></xsl:template></xsl:stylesheet>";

      System.Xml.XmlReader reader = System.Xml.XmlReader.Create(new System.IO.StringReader(strXsl));
      xslDoc.Load(reader);

      // trasformazione xml                                 
      System.Xml.XmlDocument docResult = new System.Xml.XmlDocument();
      System.IO.StringWriter xmlText = new System.IO.StringWriter();
      xslDoc.Transform(docSet, System.Xml.XmlWriter.Create(xmlText));
      docResult.LoadXml(xmlText.ToString());

      return docResult;
    }

    public bool like(string str, string wildcard) {
      wildcard = wildcard.Replace("%", "*");
      return new System.Text.RegularExpressions.Regex("^" + System.Text.RegularExpressions.Regex.Escape(wildcard).Replace(@"\*", ".*").Replace(@"\?", ".") + "$",
          System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline).IsMatch(str);
    }

    static public string name_field(string field) { return (field.IndexOf('.') >= 0 ? field.Substring(field.IndexOf('.') + 1) : field).Replace("[", "").Replace("]", ""); }

    #endregion

    #region base special functions

    public virtual void add_field(schema_field tblField, string tableName) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità add_field"); }

    public virtual void del_field(string field, string table) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità del_field"); }
    
    public virtual void upd_field(schema_field tblField, string tableName) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità upd_field"); }

    public virtual bool exist_field(string table, string col) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità exist_field"); }

    public virtual bool exist_table(string tableName) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità exist_table"); }

    public virtual List<string> tables(string likeName = "", string list = "") { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità tables"); }

    public virtual List<string> synonims(string likeName) { return new List<string>(); }

    public virtual string module_text(string procName) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità module_text"); }

    public virtual List<string> functions(string likeName = "") { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità functions"); }

    public virtual void drop_function(string func) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità drop_function"); }

    public virtual List<string> store_procedures(string likeName = "") { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità store_procedures"); }

    public virtual bool there_schema(string schema) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità there_schema"); }

    public virtual void create_schema(string schemaName) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità create_schema"); }

    public virtual bool exist_function(string function) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità exist_function"); }

    public virtual bool exist_procedure(string procedure) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità exist_procedure"); }

    public virtual void drop_procedure(string proc) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità drop_procedure"); }

    public virtual void drop_table(string table) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità drop_table"); }

    public virtual void rename_table(string old_name, string new_name) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità rename_table"); }

    public virtual void truncate_table(string table) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità truncate_table"); }

    public virtual void set_identity(string table, bool on) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità set_identity"); }

    // truncate table
    public virtual void clean_table(string table, string where = null) {
      exec("DELETE FROM " + table + ""
          + (where != null && where != "" ? " WHERE " + where : ""));
    }

    public schema_field field_table(string table, string nameField) {
      List<schema_field> list = table_fields(table, nameField);
      return list.Count == 0 ? null : list[0];
    }

    // tableFields
    public virtual List<schema_field> table_fields(string table, string nameField = "") { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità tableFields"); }

    public schema_field findField(List<schema_field> list, string fieldName) {
      for (int i = 0; i < list.Count; i++)
        if (list[i].Name.ToLower() == fieldName.ToLower())
          return list[i];

      return null;
    }

    public idx_table find_index(List<idx_table> list, string indexName, string tableName) {
      for (int i = 0; i < list.Count; i++)
        if (list[i].Name.ToLower() == indexName.ToLower()
            && list[i].TableName.ToLower() == tableName.ToLower())
          return list[i];

      return null;
    }

    public bool remove_field(List<schema_field> list, string fieldName) {
      bool result = false;

      for (int i = 0; i < list.Count; i++) {
        if (list[i].Name.ToLower() == fieldName.ToLower()) {
          list.RemoveAt(i);
          result = true;
          i--;
        }
      }

      return result;
    }

    public virtual void drop_field(schema_field field, string tableName) {
      throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità dropFieldToTable");
    }

    public virtual void drop_index(string indexName, string tableName) {
      throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità dropIndex");
    }

    public virtual void drop_foreign(string foreignName, string tableName) {
      throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità dropForeignKey");
    }

    public virtual void alter_field(schema_field tblfield, string tableName) {
      throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità alterFieldTable");
    }

    public virtual void create_table(XmlNode tableNode, string nameCreated, List<string> flds_null = null) {
      create_table(tableNode, true, nameCreated);
    }

    public virtual void create_table(XmlNode tableNode, bool createIndexes = true, string nameCreated = "", List<string> flds_null = null) {
      throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità createTable");
    }

    public virtual idx_table create_index(idx_table index) {
      throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità createIndex");
    }

    public virtual List<idx_table> table_idxs(string table, bool? uniques = null, string index_name = "") {
      throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità tableIndexes");
    }

    public idx_table table_pk(string table) { return table_idxs(table).FirstOrDefault(x => x.Primary); }

    public virtual void repairAutoIncrements() {
      throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità repairAutoIncrements");
    }

    public string field_value(object val, string null_val = null) {
      return val == DBNull.Value || val == null ? null_val
        : (val.GetType().FullName == "System.DateTime" ? ((DateTime)val).ToString("yyyy-MM-dd HH:mm:ss.fffffff")
          : val.ToString());
    }

    public virtual string val_toqry(string value, fieldType coltype, dbType type, string nullstr = null, bool add_operator = false, bool tostring = false) {
      throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità valueToQuery");
    }

    public virtual string val_toqry(string value, fieldType coltype, string nullstr = null, bool add_operator = false, bool tostring = false) {
      return val_toqry(value, coltype, _dbType, nullstr, add_operator, tostring);
    }

    #endregion

    #region tools

    public string get_list(string sql, string field) { return string.Join(",", dt_table(sql).Rows.Cast<DataRow>().Select(r => r[field].ToString())); }

    public long get_count(string sql) {
      return dt_table(sql).Rows[0][0] != null && dt_table(sql).Rows[0][0] != DBNull.Value
        ? Convert.ToInt64(dt_table(sql).Rows[0][0]) : 0;
    }

    public object get_value(string sql) { return dt_table(sql).Rows[0][0]; }

    public DateTime? get_date(string sql) {
      object val = dt_table(sql).Rows[0][0];
      return val == null || val == DBNull.Value ? (DateTime?)null : DateTime.Parse(val.ToString());
    }

    public string get_string(string sql, string null_val = null) {
      object v = dt_table(sql).Rows[0][0];
      return v != null && v != DBNull.Value ? v.ToString() : null_val;
    }

    public static DateTime? max_date(DateTime? dt_1, DateTime? dt_2) {
      if (dt_1.HasValue && dt_2.HasValue) return dt_1 > dt_2 ? dt_1 : dt_2;
      else return dt_1.HasValue ? dt_1 : dt_2.HasValue ? dt_2 : (DateTime?)null;
    }

    public static string join_row(DataRow row) {
      return string.Join(", ", row.Table.Columns.Cast<DataColumn>().Select(col => col.ColumnName + ": " + (row[col.ColumnName] != DBNull.Value
        ? row[col.ColumnName].ToString() : "")));
    }

    #endregion
  }
}
