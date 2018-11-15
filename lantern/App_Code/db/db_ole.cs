using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using deeper.frmwrk;

namespace deeper.db
{
  public class db_ole : db_schema
  {
    public db_ole(string name, string connString, string provName, string group, int timeout, string language, string cur_ver, string des
      , string dateFormatToQuery, string dateFormatToQuery2, string pathSchema = "", string pathMeta = "", List<db_script> scripts = null)
      : base(name, group, timeout, provName, connString, language, des, dateFormatToQuery, dateFormatToQuery2, cur_ver, pathSchema, pathMeta, scripts) { }

    public OleDbConnection oleconn { get { return (OleDbConnection)_conn; } }

    #region base functions

    public override void drop_table(string table) { exec("DROP TABLE " + table); }

    public override System.Data.Common.DbDataReader dt_reader(string sql) {
      bool opened = false;
      try {
        if (open_conn()) opened = true;

        logSql(sql);

        OleDbCommand command = ((OleDbConnection)_conn).CreateCommand();
        command.CommandText = sql;
        if (_timeout > 0) command.CommandTimeout = _timeout;
        command.CommandType = CommandType.Text;

        //if (_timeout > 0) command.CommandTimeout = _timeout;

        return command.ExecuteReader();
      }
      catch (Exception ex) { logErr(ex); throw ex; }
      finally { if (opened) close_conn(); }
    }

    #endregion
  }
}