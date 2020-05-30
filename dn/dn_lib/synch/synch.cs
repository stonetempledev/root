using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using mlib;
using mlib.db;
using mlib.tools;


namespace dn_lib {
  public class synch : bo_lib {

    public synch(db_provider conn, core c, config cfg, int user_id = 0, string user_name = "")
      : base(conn, c, cfg, user_id, user_name) {
    }

    public List<synch_folder> list_synch_folders() {
      return db_conn.dt_table(core.parse(config.get_query("synch.folders").text)).Rows.Cast<DataRow>()
        .Select(x => new synch_folder(db_provider.int_val(x["synch_folder_id"]), db_provider.str_val(x["title"]), db_provider.str_val(x["code"]), db_provider.str_val(x["des"])
          , db_provider.str_val(x["synch_path"]), db_provider.str_val(x["client_path"]), db_provider.str_val(x["user"]), db_provider.str_val(x["password"]))).ToList();
    }

    public List<synch_setting> list_settings() {
      return db_conn.dt_table(core.parse(config.get_query("synch.settings").text)).Rows.Cast<DataRow>()
        .Select(x => new synch_setting(db_provider.str_val(x["name"]), db_provider.str_val(x["des"]), db_provider.str_val(x["value"]))).ToList();
    }

    public void clean_readed() { db_conn.exec(core.parse_query("synch.clean-readed")); }

    public int del_unreaded() { return int.Parse(db_conn.exec(core.parse_query("synch.del-unreaded"))); }

    public long ins_folder(int synch_folder_id, long? parent_id, string folder_name, out string tp) {
      tp = "";
      string res = db_conn.exec(core.parse_query("synch.ins-folder", new string[,] { { "synch_folder_id", synch_folder_id.ToString() }
        , { "parent_id", !parent_id.HasValue ? "null" : parent_id.Value.ToString() }
        , { "cmp_p", !parent_id.HasValue ? "is" : "=" }, { "folder_name", folder_name }}), true, true);
      tp = res.Split(new char[] { ';' })[1];
      return res.Split(new char[] { ';' })[0] != "" ? long.Parse(res.Split(new char[] { ';' })[0]) : -1;
    }

    public long ins_file(int synch_folder_id, long? folder_id, string file_name, out string tp) {
      tp = "";
      string res = db_conn.exec(core.parse_query("synch.ins-file", new string[,] { { "synch_folder_id", synch_folder_id.ToString() }
        , { "folder_id", !folder_id.HasValue ? "null" : folder_id.ToString() }, { "cmp_f", !folder_id.HasValue ? "is" : "=" }
        , { "file_name", file_name }}), true, true);
      tp = res.Split(new char[] { ';' })[1];
      return res.Split(new char[] { ';' })[0] != "" ? long.Parse(res.Split(new char[] { ';' })[0]) : -1;
    }

    public long ins_task(task t, out string tp) {
      tp = "";
      if (t == null) return 0;
      string res = db_conn.exec(core.parse_query("synch.ins-task", new Dictionary<string, object>() { { "task", t }
      , { "folder_id", t.folder_id.HasValue ? t.folder_id.Value : 0 }, { "file_id", t.file_id.HasValue ? t.file_id.Value : 0 }}), true, true);
      tp = res.Split(new char[] { ';' })[1];
      return res.Split(new char[] { ';' })[0] != "" ? long.Parse(res.Split(new char[] { ';' })[0]) : -1;
    }

  }
}