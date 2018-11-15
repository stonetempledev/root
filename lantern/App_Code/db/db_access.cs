using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.OleDb;
using System.Xml;
using deeper.frmwrk;
using deeper.lib;

namespace deeper.db
{
  public class db_access : db_ole
  {
    public db_access(string name, string connString, string provName, string group, int timeout, string language, string cur_ver, string des
      , string dateFormatToQuery, string dateFormatToQuery2, string pathSchema = "", string pathMeta = "", List<db_script> scripts = null)
      : base(name, connString, provName, group, timeout, language, cur_ver, des, dateFormatToQuery, dateFormatToQuery2, pathSchema, pathMeta, scripts) {
      //
      // TODO: Add constructor logic here
      //
    }

    #region base functions

    public override List<string> store_procedures(string likeName = "") { return new List<string>(); }
    public override List<string> functions(string likeName = "") { return new List<string>(); }

    public override System.Data.Common.DbParameter get_par(schema_field col) {
      OleDbParameter result = null;
      if (col.OriginalType == "datetime")
        result = new OleDbParameter("@" + col.Name.ToUpper(), OleDbType.DBDate);
      else if (col.OriginalType == "int" || col.OriginalType == "smallint")
        result = new OleDbParameter("@" + col.Name.ToUpper(), OleDbType.Integer);
      else if (col.OriginalType == "bigint" || col.OriginalType == "long")
        result = new OleDbParameter("@" + col.Name.ToUpper(), OleDbType.BigInt);
      else if (col.OriginalType == "char" || col.OriginalType == "varchar" || col.OriginalType == "text"
          || col.OriginalType == "nchar" || col.OriginalType == "nvarchar" || col.OriginalType == "ntext")
        result = new OleDbParameter("@" + col.Name.ToUpper(), OleDbType.VarChar, col.MaxLength.Value);
      else if (col.OriginalType == "bit")
        result = new OleDbParameter("@" + col.Name.ToUpper(), OleDbType.Boolean);
      else if (col.OriginalType == "float" || col.OriginalType == "real" || col.OriginalType == "decimal" || col.OriginalType == "money"
          || col.OriginalType == "smallmoney" || col.OriginalType == "numeric")
        result = new OleDbParameter("@" + col.Name.ToUpper(), OleDbType.Double);
      else
        throw new Exception("tipo di dati '" + col.OriginalType + "' non supportato per l'importazione dei dati");

      return result;
    }

    public override System.Data.Common.DbCommand dt_command(string sql) {
      OleDbCommand result = null;

      bool opened = false;
      try {
        if (open_conn())
          opened = true;

        logSql(sql);

        result = new OleDbCommand(sql, ((OleDbConnection)_conn));

        //if (_timeout > 0)
        //    result.CommandTimeout = _timeout;

      }
      catch (Exception ex) { logErr(ex); throw ex; }
      finally {
        if (opened)
          close_conn();
      }

      return result;
    }

    #endregion

    #region base special functions

    public override void add_field(schema_field tblField, string tableName) {
      exec("ALTER TABLE [" + tableName + "] ADD "
          + tblField.getFieldAccess());
    }

    public override bool exist_table(string tableName) {
      return schemaTables(tableName).Rows.Count > 0;
    }

    public override string val_toqry(string value, fieldType coltype, dbType type, string nullstr = null, bool add_operator = false, bool tostring = false) {
      if (value == null || (nullstr != null && value == nullstr))
        return (add_operator ? " IS " : "") + "NULL";

      string op = add_operator ? " = " : "", ap = !tostring ? "'" : "";
      if (coltype == fieldType.DATETIME)
        return op + "Format(#" + DateTime.Parse(value).ToString("yyyy/MM/dd HH:mm:ss") + "#, \"yyyy/mm/dd hh:nn:ss\")";
      else if (coltype == fieldType.INTEGER || coltype == fieldType.LONG
          || coltype == fieldType.SMALLINT)
        return op + value;
      else if (coltype == fieldType.CHAR || coltype == fieldType.VARCHAR
          || coltype == fieldType.TEXT)
        return op + ap + value.Replace("'", "''") + ap;
      else if (coltype == fieldType.BOOL)
        return op + (bool.Parse(value) ? "True" : "False");
      else if (coltype == fieldType.DOUBLE || coltype == fieldType.SINGLE
          || coltype == fieldType.DECIMAL || coltype == fieldType.MONEY)
        return op + value.Replace(",", ".");
      else
        throw new Exception("tipo di dati '" + coltype.ToString() + "' non supportato per l'importazione dei dati");
    }

    #endregion

    #region special functions

    protected DataTable schemaTables(string tableName = "") {
      bool opened = false;
      try {
        if (open_conn()) opened = true;

        object[] restr = tableName == "" ? new object[] { null, null, null, "TABLE" }
            : new object[] { null, null, tableName, "TABLE" };

        return oleconn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, restr);
      }
      catch (Exception ex) { logErr(ex); throw ex; }
      finally { if (opened) close_conn(); }
    }

    protected DataTable schemaColumns(string tableName = "", string colName = "") {
      bool opened = false;
      try {
        if (open_conn()) opened = true;

        object[] restr = tableName == "" && colName == "" ? new object[] { null, null, null, null }
          : tableName != "" && colName == "" ? new object[] { null, null, tableName, null }
          : tableName != "" && colName != "" ? new object[] { null, null, tableName, colName } : null;

        DataTable dt = oleconn.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, restr);
        dt.DefaultView.Sort = "ORDINAL_POSITION";
        return dt.DefaultView.ToTable();
      }
      catch (Exception ex) { logErr(ex); throw ex; }
      finally { if (opened) close_conn(); }
    }

    protected bool isAutoIncrement(string tableName, string colName) {
      bool opened = false;
      try {
        if (open_conn()) opened = true;

        OleDbDataAdapter da = new OleDbDataAdapter("SELECT " + colName + " FROM [" + tableName + "] WHERE 1 = 0", oleconn);
        OleDbCommandBuilder cmd = new OleDbCommandBuilder(da);

        DataSet ds = new DataSet(tableName);
        da.FillSchema(ds, SchemaType.Source, tableName);
        da.Fill(ds, tableName);

        return ds.Tables[0].Columns[colName].AutoIncrement;
      }
      catch (Exception ex) { logErr(ex); throw ex; }
      finally { if (opened) close_conn(); }
    }

    protected DataTable schemaForeignKeys(string pkTable = "", string fkTable = "") {
      bool opened = false;
      try {
        if (open_conn()) opened = true;

        DataTable dt = oleconn.GetOleDbSchemaTable(OleDbSchemaGuid.Foreign_Keys, null);

        if (pkTable != "" || fkTable != "") {
          string filter = (pkTable != "" ? "PK_TABLE_NAME = '" + pkTable + "'" : "")
            + (fkTable != "" ? (pkTable != "" ? " AND " : "") + "FK_TABLE_NAME = '" + fkTable + "'" : "");

          DataTable dt2 = dt.Clone();
          dt2.Clear();
          foreach (DataRow row in dt.Select(filter))
            dt2.LoadDataRow(row.ItemArray, LoadOption.OverwriteChanges);
          dt = dt2;
        }
        return dt;
      }
      catch (Exception ex) { logErr(ex); throw ex; }
      finally { if (opened) close_conn(); }
    }

    protected DataTable schemaIndexes(string tableName = "") {
      bool opened = false;
      try {
        if (open_conn()) opened = true;

        string[] restr = new string[5];
        if (tableName != "") restr[4] = tableName;

        return oleconn.GetOleDbSchemaTable(OleDbSchemaGuid.Indexes, restr);
      }
      catch (Exception ex) { logErr(ex); throw ex; }
      finally { if (opened) close_conn(); }
    }

    protected string constraintType(string tableName, string constraintName) {
      bool opened = false;
      try {
        if (open_conn()) opened = true;

        string result = "";

        DataTable dt = oleconn.GetOleDbSchemaTable(OleDbSchemaGuid.Table_Constraints, new object[] { null, null, constraintName, null, null, tableName });
        foreach (DataRow row in dt.Rows) {
          //string table = row["TABLE_NAME"].ToString();
          //string idx = row["CONSTRAINT_NAME"].ToString();
          string type = row["CONSTRAINT_TYPE"].ToString();

          result = type;

          break;
        }

        return result;
      }
      catch (Exception ex) { logErr(ex); throw ex; }
      finally { if (opened) close_conn(); }
    }

    public override List<string> tables(string likeName = "", string list = "") {
      List<string> result = new List<string>();

      foreach (DataRow row in schemaTables().Rows) {
        string tableName = row[2].ToString();
        if (tableName.Substring(0, 1) == "~")
          continue;

        if (likeName != "" && !like(tableName, likeName))
          continue;

        result.Add(tableName);
      }

      return result;
    }

    public override void truncate_table(string table) {
      exec("DELETE FROM [" + table + "]");
    }

    // tableFields
    public override List<schema_field> table_fields(string table, string nameField = "") {
      List<schema_field> result = new List<schema_field>();

      bool alreadyFindAutoNumber = false; int i = 0;
      foreach (DataRow row in schemaColumns(table, nameField).Rows) {
        System.Data.OleDb.OleDbType type = (System.Data.OleDb.OleDbType)Enum.Parse(typeof(System.Data.OleDb.OleDbType), row["DATA_TYPE"].ToString());
        bool autonumber = false;
        if (!alreadyFindAutoNumber) {
          if (type == System.Data.OleDb.OleDbType.BigInt || type == System.Data.OleDb.OleDbType.Integer ||
              type == System.Data.OleDb.OleDbType.SmallInt || type == System.Data.OleDb.OleDbType.TinyInt ||
              type == System.Data.OleDb.OleDbType.UnsignedBigInt || type == System.Data.OleDb.OleDbType.UnsignedInt ||
              type == System.Data.OleDb.OleDbType.UnsignedSmallInt || type == System.Data.OleDb.OleDbType.UnsignedTinyInt)
            autonumber = isAutoIncrement(table, row["COLUMN_NAME"].ToString());
          if (autonumber) alreadyFindAutoNumber = true;
        }

        result.Add(new schema_field(_dbType, row["COLUMN_NAME"].ToString(), type.ToString(), row["IS_NULLABLE"].ToString().ToUpper() == "FALSE" ? false : true
          , row["CHARACTER_MAXIMUM_LENGTH"] != DBNull.Value ? int.Parse(row["CHARACTER_MAXIMUM_LENGTH"].ToString()) : (int?)null
          , row["NUMERIC_PRECISION"] != DBNull.Value ? int.Parse(row["NUMERIC_PRECISION"].ToString()) : (int?)null
          , row["NUMERIC_SCALE"] != DBNull.Value ? int.Parse(row["NUMERIC_SCALE"].ToString()) : (int?)null
          , row["COLUMN_DEFAULT"] != DBNull.Value ? row["COLUMN_DEFAULT"].ToString() : "", autonumber, false, i++));
      }

      return result;
    }

    public override void drop_field(schema_field field, string tableName) {
      exec("ALTER TABLE [" + tableName + "] DROP COLUMN [" + field.Name + "]");
    }

    public override void drop_index(string indexName, string tableName) {
      exec("DROP INDEX [" + indexName + "] ON [" + tableName + "]");
    }

    public override void drop_foreign(string foreignName, string tableName) {
      exec("ALTER TABLE [" + tableName + "] DROP CONSTRAINT [" + foreignName + "]");
    }

    public override void alter_field(schema_field tblfield, string tableName) {
      exec("ALTER TABLE [" + tableName + "] ALTER COLUMN "
          + tblfield.getFieldAccess());
    }

    public override void create_table(XmlNode tableNode, bool createIndexes = true, string nameCreated = "", List<string> flds_null = null) {
      string sql = "";
      foreach (XmlNode col in tableNode.SelectNodes("cols/col"))
        sql += (sql != "" ? ", " : "") + schema_field.getFieldAccess(col.Attributes["name"].Value
          , schema_field.originalToType(_dbType, col.Attributes["type"].Value)
          , xmlDoc.node_val(col, "numprec"), xmlDoc.node_val(col, "numscale"), xmlDoc.node_val(col, "maxlength")
          , xmlDoc.node_bool(col, "nullable", true), xmlDoc.node_val(col, "default"), xmlDoc.node_bool(col, "autonumber", false));
      exec("CREATE TABLE [" + tableNode.Attributes["name"].Value + "](" + sql + ")");

      // indici
      if (createIndexes)
        foreach (idx_table index in schema_doc.table_indexes(tableNode))
          create_index(index);
    }

    public override idx_table create_index(idx_table index) {

      logInfo("creazione indice '" + index.Name + "'");

      //string clusteredSql = "CLUSTERED";
      //if (!index.Clustered) clusteredSql = "NONCLUSTERED";

      string sql = "";
      foreach (idx_field iField in index.Fields) 
        sql += (sql != "" ? ", " : "") + "[" + iField.Name + "]" + (iField.Ascending ? " ASC" : " DESC");

      exec("CREATE " + (index.Unique ? "UNIQUE" : "") + " INDEX [" + index.Name + "] ON [" + index.TableName + "] ("
        + sql + ")" + (index.Primary ? " WITH PRIMARY" : ""));

      return index;
    }

    public override List<idx_table> table_idxs(string table, bool? uniques = null, string index_name = "") {

      List<idx_table> indexes = new List<idx_table>();

      foreach (DataRow row in schemaIndexes(table).Rows) {
        string nm = row["index_name"].ToString();
        bool unique = bool.Parse(row["unique"].ToString());
        bool primary = bool.Parse(row["primary_key"].ToString());

        if (index_name != "" && index_name.ToLower() != nm.ToLower()
          || constraintType(table, nm) == "FOREIGN KEY" || !idx_table.filter_unique(uniques, unique, primary))
          continue;

        idx_table index = indexes.FirstOrDefault(x => x.Name == nm);
        if (index == null)
          indexes.Add(index = new idx_table(table, nm, bool.Parse(row["clustered"].ToString()), unique, primary));

        index.Fields.Add(new idx_field(row["column_name"].ToString(), int.Parse(row["collation"].ToString()) == 1, index.Fields.Count));
      }

      return indexes;
    }

    public override void repairAutoIncrements() {
      logInfo("aggiornamento campi auto numbers delle tabelle");

      // foreign keys da ripristinare
      string active_table = "";
      try {
        // ciclo tabelle
        foreach (string table in tables()) {
          // individuo il campo autonumerico (se cè)
          string fldAuto = "";
          foreach (schema_field field in table_fields(table)) 
            if (field.AutoNumber) { fldAuto = field.Name; break; }

          // cè il contatore
          if (fldAuto != "") {
            long count = 0;
            DataSet ds = dt_set("SELECT MAX(" + fldAuto + ") AS CONTEGGIO FROM [" + table + "]");
            if (ds.Tables[0].Rows[0]["CONTEGGIO"] != DBNull.Value)
              count = long.Parse(ds.Tables[0].Rows[0]["CONTEGGIO"].ToString());
            count++;

            // aggiorno la tabella
            logInfo("aggiornamento campo contatore '" + fldAuto + "' della tabella '" + table + "'");
            exec("ALTER TABLE [" + table + "] ALTER COLUMN [" + fldAuto + "] COUNTER(" + count.ToString() + ", 1)");
          }

        }
      }
      catch (Exception ex) {
        if (active_table != "") logErr(ex.Message + " (tabella: " + active_table + ")");
        else logErr(ex);

        throw ex;
      }
    }

    #endregion
  }
}