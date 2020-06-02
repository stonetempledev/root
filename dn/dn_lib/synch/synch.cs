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

    public List<synch_folder> list_synch_folders(string pc_name) {
      return db_conn.dt_table(core.parse_query("synch.folders", new string[,] { { "pc_name", pc_name } })).Rows.Cast<DataRow>()
        .Select(x => new synch_folder(db_provider.int_val(x["synch_folder_id"]), db_provider.str_val(x["pc_name"])
          , db_provider.str_val(x["title"]), db_provider.str_val(x["des"])
          , db_provider.str_val(x["local_path"]), db_provider.str_val(x["http_path"])
          , db_provider.str_val(x["user"]), db_provider.str_val(x["password"]))).ToList();
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
        , { "folder_id", t.folder_id.HasValue ? t.folder_id.Value : 0 }, { "file_id", t.file_id.HasValue ? t.file_id.Value : 0 }
        , { "dt_upd", t.dt_upd.HasValue ? t.dt_upd.Value.ToString("yyyy-mm-dd") : "" }}), true, true);
      tp = res.Split(new char[] { ';' })[1];
      return res.Split(new char[] { ';' })[0] != "" ? long.Parse(res.Split(new char[] { ';' })[0]) : -1;
    }

    public synch_machine get_synch_machine(string pc_name) {
      DataRow r = db_conn.first_row(core.parse_query("synch.start-synch-machine", new string[,] { { "pc_name", pc_name } }));
      return r != null ? new synch_machine(db_provider.int_val(r["synch_machine_id"]), db_provider.str_val(r["pc_name"])
        , db_provider.str_val(r["pc_des"]), db_provider.int_val(r["seconds"]), db_provider.int_val(r["active"]) == 1
        , db_provider.str_val(r["state"])) : null;
    }

    public void start_machine(int id, string ip_address) {
      db_conn.exec(core.parse_query("synch.start-machine", new string[,] { { "id", id.ToString() }
        , { "ip_address", ip_address } }));
    }

    public void stop_machine(int id) {
      db_conn.exec(core.parse_query("synch.stop-machine", new string[,] { { "id", id.ToString() } }));
    }

    public void last_synch_machine(int id, int folders, int files, int deleted, int seconds) {
      db_conn.exec(core.parse_query("synch.last-synch-machine", new string[,] { { "id", id.ToString() }
        , { "folders", folders.ToString() }, { "files", files.ToString() }
        , { "deleted", deleted.ToString() }, { "seconds", seconds.ToString() } }));
    }

    public List<synch_machine> list_synch_machine() {
      return db_conn.dt_table(core.parse_query("synch.synch-machines")).Rows.Cast<DataRow>()
        .Select(r => new synch_machine(db_provider.int_val(r["synch_machine_id"]), db_provider.str_val(r["pc_name"])
         , db_provider.str_val(r["pc_des"]), db_provider.int_val(r["seconds"]), db_provider.int_val(r["active"]) == 1
         , db_provider.str_val(r["state"]), db_provider.str_val(r["ip_address"])
         , db_provider.dt_val(r["dt_start"]), db_provider.dt_val(r["dt_stop"]), db_provider.dt_val(r["dt_lastsynch"])
         , db_provider.int_val(r["c_folders"]), db_provider.int_val(r["c_files"]), db_provider.int_val(r["c_deleted"])
         , db_provider.int_val(r["s_synch"]))).ToList();
    }

  }
}