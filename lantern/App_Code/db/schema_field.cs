using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace deeper.db
{
  public class schema_field
  {
    protected dbType _typeDb = dbType.none;
    protected string _name = "", _attrName = "", _originaltype = "", _default = "";
    protected bool _request = false, _autoNumber = false, _primary = false;
    protected int? _maxLength = null, _numPrecision = null, _numScale = null;
    protected int _index = 0;

    public schema_field(dbType typeDb, string name, string originaltype, bool nullable, int? maxLength = null, int? numPrecision = null, int? numScale = null
        , string defaultval = "", bool autoNumber = false, bool primary = false, int index = 0, string attr_name = "") {
      _typeDb = typeDb; _name = name; _originaltype = originaltype; _request = !nullable;
      _maxLength = maxLength; _numPrecision = numPrecision; _numScale = numScale;
      _default = defaultval; _autoNumber = autoNumber; _primary = primary; _index = index; _attrName = attr_name;
    }

    public dbType TypeDb { get { return _typeDb; } }
    public string Name { get { return _name; } }
    public string AttrName { get { return _attrName; } set { _attrName = value; } }
    public string OriginalType { get { return _originaltype; } }
    public string Default { get { return _default; } }
    public bool Request { get { return _request; } }
    public bool Nullable { get { return !_request; } set { _request = !value; } }
    public bool AutoNumber { get { return _autoNumber; } }
    public int? MaxLength { get { return _maxLength; } }
    public int? NumPrecision { get { return _numPrecision; } }
    public int? NumScale { get { return _numScale; } }
    public bool Primary { get { return _primary; } }
    public int Index { get { return _index; } }

    public static fieldType originalToType(dbType typeDb, string type) {
      string tp = type.ToLower();

      if (typeDb == dbType.access) {
        if (tp == "binary" || tp == "varbinary" || tp == "longvarbinary") return fieldType.BINARY;
        else if (tp == "boolean") return fieldType.BOOL;
        else if (tp == "currency") return fieldType.MONEY;
        else if (tp == "date" || tp == "dbtime" || tp == "dbtimestamp"
            || tp == "filetime" || tp == "dbdate") return fieldType.DATETIME;
        else if (tp == "guid") return fieldType.GUID;
        else if (tp == "double") return fieldType.DOUBLE;
        else if (tp == "single") return fieldType.SINGLE;
        else if (tp == "smallint" || tp == "unsignedsmallint" || tp == "tinyint" || tp == "unsignedtinyint")
          return fieldType.SMALLINT;
        else if (tp == "integer" || tp == "bigint"
            || tp == "unsignedbigint" || tp == "unsignedint") return fieldType.INTEGER;
        else if (tp == "varnumeric" || tp == "decimal" || tp == "numeric") return fieldType.DECIMAL;
        else if (tp == "bstr" || tp == "varchar" || tp == "longvarchar" || tp == "varwchar" || tp == "longvarwchar")
          return fieldType.VARCHAR;
        else if (tp == "char" || tp == "wchar") return fieldType.CHAR;
      }
      else if (typeDb == dbType.sqlserver) {
        if (tp == "datetime") return fieldType.DATETIME;
        else if (tp == "datetime2") return fieldType.DATETIME2;
        else if (tp == "tinyint" || tp == "smallint") return fieldType.SMALLINT;
        else if (tp == "int") return fieldType.INTEGER;
        else if (tp == "bigint") return fieldType.LONG;
        else if (tp == "bit") return fieldType.BOOL;
        else if (tp == "real" || tp == "float" || tp == "bit") return fieldType.DOUBLE;
        else if (tp == "money" || tp == "smallmoney") return fieldType.MONEY;
        else if (tp == "numeric" || tp == "decimal") return fieldType.DECIMAL;
        else if (tp == "xml") return fieldType.XML;
        else if (tp == "text" || tp == "ntext") return fieldType.TEXT;
        else if (tp == "varchar" || tp == "nvarchar") return fieldType.VARCHAR;
        else if (tp == "varbinary") return fieldType.BINARY;
        else if (tp == "char" || tp == "nchar") return fieldType.CHAR;
      }

      throw new Exception("type field '" + type + "' not supported for '" + typeDb.ToString() + "'");
    }

    public void setOriginalType(dbType typeDb, fieldType type) { _originaltype = typeToOriginal(typeDb, type); }

    public static string typeToOriginal(dbType typeDb, fieldType type) {

      if (typeDb == dbType.access) {
        if (type == fieldType.BINARY) return "binary";
        else if (type == fieldType.BOOL) return "boolean";
        else if (type == fieldType.MONEY) return "currency";
        else if (type == fieldType.DATETIME) return "date";
        else if (type == fieldType.GUID) return "guid";
        else if (type == fieldType.DOUBLE) return "double";
        else if (type == fieldType.SINGLE) return "single";
        else if (type == fieldType.SMALLINT) return "smallint";
        else if (type == fieldType.INTEGER) return "integer";
        else if (type == fieldType.DECIMAL) return "decimal";
        else if (type == fieldType.VARCHAR) return "varchar";
        else if (type == fieldType.CHAR) return "char";
      }
      else if (typeDb == dbType.sqlserver) {
        if (type == fieldType.BINARY) return "varbinary";
        else if (type == fieldType.BOOL) return "bit";
        else if (type == fieldType.MONEY) return "money";
        else if (type == fieldType.DATETIME) return "datetime";
        else if (type == fieldType.DATETIME2) return "datetime2";
        else if (type == fieldType.DOUBLE) return "float";
        else if (type == fieldType.SMALLINT) return "smallint";
        else if (type == fieldType.INTEGER) return "int";
        else if (type == fieldType.LONG) return "bigint";
        else if (type == fieldType.DECIMAL) return "decimal";
        else if (type == fieldType.VARCHAR) return "varchar";
        else if (type == fieldType.CHAR) return "char";
        else if (type == fieldType.TEXT) return "text";
        else if (type == fieldType.XML) return "xml";
      }

      throw new Exception("type field '" + type.ToString() + "' not supported for '" + typeDb.ToString() + "'");
    }

    public fieldType TypeField { get { return originalToType(_typeDb, _originaltype); } }

    static public schema_field find(List<schema_field> list, string fieldName) {
      for (int i = 0; i < list.Count; i++)
        if (list[i].Name.ToLower() == fieldName.ToLower())
          return list[i];

      return null;
    }

    static public bool removeField(List<schema_field> list, string fieldName) {
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

    public string getFieldSqlServer() {
      return getFieldSqlServer(Name, _originaltype, NumPrecision.HasValue ? NumPrecision.ToString() : ""
          , NumScale.HasValue ? NumScale.ToString() : "", MaxLength.HasValue ? MaxLength.ToString() : "", Nullable, Default, _autoNumber);
    }

    public string getFieldAccess() {
      return getFieldAccess(Name, TypeField, NumPrecision.HasValue ? NumPrecision.ToString() : ""
          , NumScale.HasValue ? NumScale.ToString() : "", MaxLength.HasValue ? MaxLength.ToString() : ""
          , !Request, Default, AutoNumber);
    }

    static public string getFieldAccess(string name, fieldType type, string numprec, string numscale, string maxlength, bool nullable, string defaultval, bool autonumber) {
      string sql = "[" + name + "]";

      if (autonumber) sql += " AUTOINCREMENT";
      else if (type == fieldType.BINARY) sql += " BINARY";
      else if (type == fieldType.BOOL) sql += " BIT";
      else if (type == fieldType.SMALLINT) sql += " SMALLINT";
      else if (type == fieldType.MONEY) sql += " MONEY";
      else if (type == fieldType.DATETIME) sql += " DATETIME";
      else if (type == fieldType.GUID) sql += " UNIQUEIDENTIFIER";
      else if (type == fieldType.DOUBLE) sql += " DOUBLE";
      else if (type == fieldType.SINGLE) sql += " SINGLE";
      else if (type == fieldType.INTEGER) sql += " INTEGER";
      else if (type == fieldType.LONG) sql += " LONG";
      else if (type == fieldType.DECIMAL) sql += " DECIMAL(" + numprec + ", " + numscale + ")";
      else if (type == fieldType.VARCHAR || type == fieldType.CHAR) {
        if (maxlength == "0") sql += " MEMO";
        else sql += " VARCHAR(" + (maxlength == "-1" ? "255" : maxlength) + ")";
      }
      else
        throw new Exception("il campo '" + type.ToString() + "' di access non viene gestito per l'aggiornamento struttura tabelle");

      if (defaultval != "") sql += " DEFAULT " + defaultval;
      if (!nullable) sql += " NOT NULL";

      return sql;
    }

    public static bool isFieldNumericSqlServer(string type) { return (type.ToLower() == "tinyint" || type.ToLower() == "int" || type.ToLower() == "bigint" || type.ToLower() == "smallint") ? true : false; }

    static public string getFieldSqlServer(string name, string type, string numprec, string numscale, string maxlength, bool nullable, string defaultval, bool autonumber) {
      string sql = "[" + name + "]";

      if (type == "datetime" || type == "datetime2" || type == "text" || type == "ntext" || type == "xml"
          || type == "tinyint" || type == "int" || type == "bigint" || type == "smallint" || type == "real"
          || type == "float" || type == "bit" || type == "money" || type == "smallmoney")
        sql += " [" + type + "]";
      else if (type == "numeric" || type == "decimal")
        sql += " [" + type + "](" + numprec + ", " + numscale + ")";
      else if (type == "char" || type == "varchar" || type == "nchar" || type == "nvarchar" || type == "varbinary")
        sql += " [" + type + "](" + (maxlength == "-1" ? "max" : maxlength) + ")";
      else throw new Exception("il campo '" + type + "' di sql server non viene gestito per l'aggiornamento struttura tabelle");

      sql += autonumber ? " IDENTITY(1, 1)" : (nullable ? " NULL" : " NOT NULL" + (defaultval != "" ? " DEFAULT " + defaultval : ""));

      return sql;
    }
  }
}