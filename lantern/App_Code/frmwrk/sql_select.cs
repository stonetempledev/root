using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace deeper.frmwrk {
  // elemento scripts/select
  public class sql_select {
    public enum type_sql { select, script, join };

    // nome della select
    string _name;
    public string name { get { return _name; } }

    // tipo di comando
    type_sql _type = type_sql.select;
    public type_sql type { get { return _type; } }
    public bool is_script { get { return _type == type_sql.script; } }
    public bool is_select { get { return _type == type_sql.select; } }
    public bool is_join { get { return _type == type_sql.join; } }

    // condizione legata alla select
    string _if_cond = "";
    public string if_cond { get { return _if_cond; } }

    // comando sql, id script 
    string _command;
    public string command { get { return _command; } set { _command = value; } }

    // type join
    string _fields, _from, _join;
    public string fields { get { return _fields; } }
    public string from { get { return _from; } }
    public string join { get { return _join; } }

    // data fields
    string _data_fields;
    public string[] data_fields { get { return _data_fields.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries); } }

    public sql_select(sql_select cpy) {
      _name = cpy._name; _command = cpy._command;
      _if_cond = cpy._if_cond; _type = cpy._type;
      _fields = cpy._fields; _from = cpy._from; _join = cpy._join;
    }

    protected sql_select(string name, string command, type_sql type
      , string if_cond, string fields, string from, string join, string data_fields) {
      _name = name; _command = command;
      _if_cond = if_cond; _type = type;
      _fields = fields; _from = from; _join = join;
      _data_fields = data_fields;
    }

    static public sql_select create_select(string name, string cmd_select, string if_cond, string data_fields) {
      return new sql_select(name, cmd_select, type_sql.select, if_cond, "", "", "", data_fields);
    }

    static public sql_select create_script(string name, string script_id, string if_cond) {
      return new sql_select(name, script_id, type_sql.script, if_cond, "", "", "", "");
    }

    static public sql_select create_join(string name, string fields, string from, string join, string if_cond) {
      return new sql_select(name, "", type_sql.join, if_cond, fields, from, join, "");
    }
  }

}