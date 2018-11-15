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
  public class db_script_cmd
  {
    public enum cmd_type
    {
      none, sql, init_tables, init_table, drop_tables, drop_functions,
      drop_procedures, exec_script, for_each_table, set_info
    }
    public enum cmd_sub_type
    { none, remove_idx_ins_on_table, delete_data }

    protected db_script _script = null;
    db_script script { get { return _script; } }

    protected cmd_type _type = cmd_type.none;
    public cmd_type type { get { return _type; } }

    protected cmd_sub_type _sub_type = cmd_sub_type.none;
    public cmd_sub_type sub_type { get { return _sub_type; } }

    protected XmlNode _node;
    public XmlNode node { get { return _node; } }

    protected db_script_cmd _parent;
    public db_script_cmd parent { get { return _parent; } }

    public db_script_cmd(db_script script, XmlNode node) {
      _script = script; _node = node; _type = (cmd_type)Enum.Parse(typeof(cmd_type), _node.Name);
    }
    public db_script_cmd(db_script script, XmlNode node, db_script_cmd parent) {
      _script = script; _node = node; _parent = parent;
      _type = parent.type; _sub_type = (cmd_sub_type)Enum.Parse(typeof(cmd_sub_type), node.Name);
    }

    public string text { get { return _node.InnerText; } }

    public string attr(string name) { return xmlDoc.node_val(_node, name); }
    public bool attr_bool(string name) { return xmlDoc.node_bool(_node, name); }

    public List<db_script_cmd> sub_cmds { get { return _node.SelectNodes("/root/script/*").Cast<XmlNode>().Select(x => new db_script_cmd(_script, x, this)).ToList(); } }
  }

  public class db_script : xmlDoc
  {
    protected string _name;
    public string name { get { return _name; } }

    public db_script(string name, string path) : base(path) { _name = name; }

    public string title { get { return get_value("/root", "title"); } }
    public string des { get { return get_value("/root", "des"); } }

    protected db_script_cmd cmd(string name_cmd) { return cmds(name_cmd)[0]; }
    protected List<db_script_cmd> cmds(string name_cmd = "") {
      return nodes(name_cmd != "" ? (exist("/root/script/group[@name='" + name_cmd + "']/*") ? "/root/script/group[@name='" + name_cmd + "']/*" :
        "/root/script/*[@name='" + name_cmd + "']") : "/root/script/*").Cast<XmlNode>().Select(x => new db_script_cmd(this, x)).ToList();
    }

    protected string qry_text(string name_cmd) { return cmd(name_cmd).text; }

    public DataTable dt_from_cmd(db_schema conn, string name_cmd) { return conn.dt_table(qry_text(name_cmd)); }

    public DataTable dt_from_cmd(db_schema conn, string name_cmd, page_cls pg_parse) {
      return conn.dt_table(pg_parse.page.parse(qry_text(name_cmd)));
    }

    public DataTable dt_from_cmd(db_schema conn, string name_cmd, page_cls pg_parse, Dictionary<string, string> fields) {
      return conn.dt_table(pg_parse.page.parse(qry_text(name_cmd), fields));
    }

    public DataTable dt_from_cmd(db_schema conn, string name_cmd, page_cls pg_parse, DataRow row) {
      return conn.dt_table(pg_parse.page.parse(qry_text(name_cmd), row));
    }

    public string sel_des(string name_cmd) { return cmd(name_cmd).attr("des"); }

    public long exec(db_schema conn, string name_cmd = "", page_cls pg_parse = null, Dictionary<string, string> fields = null, DataRow row = null) {

      long result = 0;

      // ciclo comandi
      if (fields == null) fields = new Dictionary<string, string>();
      foreach (db_script_cmd cmd in cmds(name_cmd)) {
        if (cmd.type == db_script_cmd.cmd_type.sql) {
          string sql = pg_parse == null ? cmd.text : pg_parse.page.parse(cmd.text, "", fields, row);
          if (cmd.attr("setkey") != "") {
            long key = conn.exec(sql, true); result++;
            if (!fields.ContainsKey(cmd.attr("setkey"))) fields.Add(cmd.attr("setkey"), key.ToString());
            else fields[cmd.attr("setkey")] = key.ToString();
          } else result += conn.exec(sql);
        } else if (cmd.type == db_script_cmd.cmd_type.init_tables)
          conn.upgrade_data(new db_xml(new xmlDoc(conn.parse_dbexpression(conn.schema_path, conn.parse_dbexpression(cmd.attr("ver"))))
              , conn.parse_dbexpression(conn.meta_path, conn.parse_dbexpression(cmd.attr("ver")))), false, true, cmd.attr("notes"));
        else if (cmd.type == db_script_cmd.cmd_type.init_table) {
          if (conn.exist_table(cmd.attr("table")))
            conn.drop_table(cmd.attr("table"));
          conn.create_table(conn.schema.table_node(cmd.attr("table")));
        } else if (cmd.type == db_script_cmd.cmd_type.drop_tables) conn.dropTables();
        else if (cmd.type == db_script_cmd.cmd_type.drop_functions) conn.dropFunctions();
        else if (cmd.type == db_script_cmd.cmd_type.drop_procedures) conn.dropProcedures();
        else if (cmd.type == db_script_cmd.cmd_type.exec_script) conn.exec_script(cmd.attr("name"));
        else if (cmd.type == db_script_cmd.cmd_type.for_each_table) for_each_table(conn, cmd);
        else if (cmd.type == db_script_cmd.cmd_type.set_info)
          conn.setInfo(cmd.attr("name"), conn.parse_dbexpression(cmd.attr("value")), 0, cmd.attr("notes"));
      }

      return result;

    }

    protected void for_each_table(db_schema conn, db_script_cmd cmd) {

      // filtri
      string[] filter = cmd.attr("filter_type").Split(new char[] { ',' }
        , StringSplitOptions.RemoveEmptyEntries);
      string[] exclude = cmd.attr("exclude").ToLower().Split(new char[] { ',' }
        , StringSplitOptions.RemoveEmptyEntries);
      bool no_enums = cmd.attr_bool("exclude_enums");

      // ciclo tabelle
      foreach (string tbl in conn.tables()) {
        if (filter == null || (filter != null && (filter.Contains("data") && conn.meta_doc.type_table(tbl) == db.meta_doc.table_type.data
          || filter.Contains("history") && conn.meta_doc.type_table(tbl) == db.meta_doc.table_type.storico))
          && (exclude == null || (exclude != null && !exclude.Contains(tbl.ToLower())))
          && (!no_enums || (no_enums && !conn.meta_doc.enum_tbl(tbl)))) {

          foreach (db_script_cmd sub_cmd in cmd.sub_cmds)
            if (sub_cmd.sub_type == db_script_cmd.cmd_sub_type.remove_idx_ins_on_table)
              conn.table_idxs(tbl, true).ForEach(i => {
                if (i.Fields.Count == 1 && i.Fields[0].Name.ToLower() == conn.meta_doc.field_ins().ToLower()) conn.drop_index(i.Name, tbl);
              });
            else if (sub_cmd.sub_type == db_script_cmd.cmd_sub_type.delete_data) conn.clean_table(tbl);
        }
      }
    }

  }
}

