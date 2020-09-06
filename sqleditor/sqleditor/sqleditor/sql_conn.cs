using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

namespace sqleditor {
  public class sql_conn {

    public string conn_string { get; set; }
    public string provider { get; protected set; }
    public DbConnection conn { get; protected set; }

    public sql_conn() { }

    public void open_conn(string provider, string conn_string) {
      this.conn = create_conn(provider, conn_string);
      this.conn.Open();
      this.provider = provider;
    }

    public void close_conn() { if (this.conn != null) this.conn.Close(); }

    protected static DbConnection create_conn(string provider, string conn_string) {
      if (string.IsNullOrEmpty(conn_string)) throw new Exception("non è stata specificata la stringa di connessione!");

      DbProviderFactory factory = DbProviderFactories.GetFactory(provider);
      DbConnection connection = factory.CreateConnection();
      connection.ConnectionString = conn_string;
      return connection;
    }

    public DataTable open_set(string sql, int? timeout = null) {
      if (this.conn == null) throw new Exception("connection closed!");

      DbCommand cmd = this.conn.CreateCommand();
      //if (_trans != null) cmd.Transaction = _trans;
      cmd.CommandText = sql;
      if (timeout.HasValue) cmd.CommandTimeout = timeout.Value;
      cmd.CommandType = CommandType.Text;

      DbProviderFactory factory = DbProviderFactories.GetFactory(this.provider);
      DbDataAdapter ad = factory.CreateDataAdapter();
      ad.SelectCommand = cmd;
      DataTable dt = new DataTable();
      ad.Fill(dt);
      return dt;
    }

    public int execute(string sql, int? timeout = null, DbParameter[] pars = null) {
      if (this.conn == null) throw new Exception("connection closed!");
      DbCommand cmd = this.conn.CreateCommand();
      if (timeout.HasValue) cmd.CommandTimeout = timeout.Value;
      //if (_trans != null) cmd.Transaction = _trans;
      if (pars != null) { foreach (DbParameter p in pars) cmd.Parameters.Add(p); }
      cmd.CommandText = sql;
      return cmd.ExecuteNonQuery();
    }
  }
}
