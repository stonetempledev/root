using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using dlib;
using dlib.db;
using dlib.tools;

namespace dlib {
  public class synch : bo {

    public synch(db_provider conn, core c, config cfg, int user_id = 0, string user_name = "")
      : base(conn, c, cfg, user_id, user_name) {
    }

    public List<synch_folder> list_synch_folders(string pc_name) {
      return db_conn.dt_table(core.parse_query("lib-synch.folders", new string[,] { { "pc_name", pc_name } })).Rows.Cast<DataRow>()
        .Select(x => new synch_folder(db_provider.int_val(x["synch_folder_id"]), db_provider.str_val(x["pc_name"])
          , db_provider.str_val(x["title"]), db_provider.str_val(x["des"])
          , db_provider.str_val(x["local_path"]), db_provider.str_val(x["http_path"])
          , db_provider.str_val(x["user"]), db_provider.str_val(x["password"]))).ToList();
    }

    public void clean_readed() { db_conn.exec(core.parse_query("lib-synch.clean-readed")); }

    public int del_unreaded() { return int.Parse(db_conn.exec(core.parse_query("lib-synch.del-unreaded"))); }

    public long ins_folder(int synch_folder_id, long? parent_id, string folder_name, DateTime dt_ins, DateTime dt_upd, out string tp, out int cc) {
      tp = ""; cc = 0;
      string res = db_conn.exec(core.parse_query("lib-synch.ins-folder"
        , new Dictionary<string, object>() { { "synch_folder_id", synch_folder_id.ToString() }
          , { "dt_ins", dt_ins }, { "dt_upd", dt_upd }
          , { "parent_id", !parent_id.HasValue ? "null" : parent_id.Value.ToString() }
          , { "cmp_p", !parent_id.HasValue ? "is" : "=" }, { "folder_name", folder_name } }), true, true);
      tp = res.Split(new char[] { ';' })[1];
      cc = int.Parse(res.Split(new char[] { ';' })[2]);
      return res.Split(new char[] { ';' })[0] != "" ? long.Parse(res.Split(new char[] { ';' })[0]) : -1;
    }

    public long ins_file(int synch_folder_id, long? folder_id, string file_name, string extension, DateTime dt_ins, DateTime dt_upd, out string tp, out int cc) {
      tp = ""; cc = 0;
      string res = db_conn.exec(core.parse_query("lib-synch.ins-file"
        , new Dictionary<string, object>() { { "synch_folder_id", synch_folder_id.ToString() }
          , { "dt_ins", dt_ins }, { "dt_upd", dt_upd }
          , { "folder_id", !folder_id.HasValue ? "null" : folder_id.ToString() }, { "cmp_f", !folder_id.HasValue ? "is" : "=" }
          , { "file_name", file_name }, { "extension", !string.IsNullOrEmpty(extension) ? extension.ToLower() : "" } }), true, true);
      tp = res.Split(new char[] { ';' })[1];
      cc = int.Parse(res.Split(new char[] { ';' })[2]);
      return res.Split(new char[] { ';' })[0] != "" ? long.Parse(res.Split(new char[] { ';' })[0]) : -1;
    }

    public long ins_task(task t, out string tp, out int cc) {

      tp = ""; cc = 0;
      if (t == null) return 0;
      string res = db_conn.exec(core.parse_query("lib-synch.ins-task", new Dictionary<string, object>() { { "task", t }
        , { "folder_id", t.folder_id.HasValue ? t.folder_id.Value : 0 }, { "file_id", t.file_id.HasValue ? t.file_id.Value : 0 }
        , { "stato", t.stato }, { "priorita", t.priorita }, { "tipo", t.tipo }, { "stima", t.stima } }), true, true);
      
      tp = res.Split(new char[] { ';' })[1];
      cc = int.Parse(res.Split(new char[] { ';' })[2]);
      return res.Split(new char[] { ';' })[0] != "" ? long.Parse(res.Split(new char[] { ';' })[0]) : -1;
    }

    public synch_machine get_synch_machine(string pc_name) {
      DataRow r = db_conn.first_row(core.parse_query("lib-synch.start-synch-machine", new string[,] { { "pc_name", pc_name } }));
      return r != null ? new synch_machine(db_provider.int_val(r["synch_machine_id"]), db_provider.str_val(r["pc_name"])
        , db_provider.str_val(r["pc_des"]), db_provider.int_val(r["seconds"]), db_provider.int_val(r["active"]) == 1
        , db_provider.str_val(r["state"])) : null;
    }

    public void start_machine(int id, string ip_address) {
      db_conn.exec(core.parse_query("lib-synch.start-machine", new string[,] { { "id", id.ToString() }
        , { "ip_address", ip_address } }));
    }

    public void stop_machine(int id) {
      db_conn.exec(core.parse_query("lib-synch.stop-machine", new string[,] { { "id", id.ToString() } }));
    }

    public void last_synch_machine(int id, int folders, int files, int deleted, int seconds) {
      db_conn.exec(core.parse_query("lib-synch.last-synch-machine", new string[,] { { "id", id.ToString() }
        , { "folders", folders.ToString() }, { "files", files.ToString() }
        , { "deleted", deleted.ToString() }, { "seconds", seconds.ToString() } }));
    }

    public List<synch_machine> list_synch_machine() {
      return db_conn.dt_table(core.parse_query("lib-synch.synch-machines")).Rows.Cast<DataRow>()
        .Select(r => new synch_machine(db_provider.int_val(r["synch_machine_id"]), db_provider.str_val(r["pc_name"])
         , db_provider.str_val(r["pc_des"]), db_provider.int_val(r["seconds"]), db_provider.int_val(r["active"]) == 1
         , db_provider.str_val(r["state"]), db_provider.str_val(r["ip_address"])
         , db_provider.dt_val(r["dt_start"]), db_provider.dt_val(r["dt_stop"]), db_provider.dt_val(r["dt_lastsynch"])
         , db_provider.int_val(r["c_folders"]), db_provider.int_val(r["c_files"]), db_provider.int_val(r["c_deleted"])
         , db_provider.int_val(r["s_synch"]))).ToList();
    }

    public List<free_label> load_free_labels() {
      return db_conn.dt_table(core.parse_query("lib-synch.free-labels")).Rows.Cast<DataRow>()
        .Select(r => new free_label(db_provider.str_val(r["free_txt"]), db_provider.str_val(r["stato"])
          , db_provider.str_val(r["priorita"]), db_provider.str_val(r["tipo"]), db_provider.str_val(r["stima"])
          , db_provider.str_val(r["default"]) == "1")).ToList();
    }
  }
}