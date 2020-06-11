using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using mlib.db;
using dn_lib;

namespace deepanotes {
  public class notes : bo {

    public List<synch_folder> synch_folders { get; protected set; }
    public List<task> tasks { get; protected set; }

    public notes() {
    }

    public folder find_folder(long folder_id) {
      foreach (synch_folder sf in this.synch_folders) {
        folder res = sf.get_folder(folder_id);
        if (res != null) return res;
      }
      return null;
    }

    public synch_folder find_synch_folder(long synch_folder_id) {
      return this.synch_folders.FirstOrDefault(x => x.id == synch_folder_id);
    }

    public void load_notes(int? folder_id = null, int? synch_folder_id = null) {

      // folders structure
      this.synch_folders = load_folders(folder_id, synch_folder_id);

      // tasks
      string[,] pars = new string[,] { { "folder_id", folder_id.HasValue ? folder_id.Value.ToString() : "null" } 
        , { "synch_folder_id", synch_folder_id.HasValue ? synch_folder_id.Value.ToString() : "null" }
        , { "filter_sf", !synch_folder_id.HasValue && folder_id.HasValue ? " ft.tp <> 'synch_folder' and " : "" } };

      this.tasks = new List<task>();
      foreach (DataRow dr in db_conn.dt_table(core.parse_query("notes.tasks", pars)).Rows) {
        task t = new task(db_provider.int_val(dr["synch_folder_id"]), db_provider.long_val(dr["task_id"])
          , db_provider.long_val_null(dr["file_id"]), db_provider.long_val_null(dr["folder_id"])
          , db_provider.str_val(dr["title"]), db_provider.str_val(dr["user"]), db_provider.str_val(dr["stato"])
          , db_provider.str_val(dr["task_state"]) != "" ? (task.task_state)Enum.Parse(typeof(task.task_state), db_provider.str_val(dr["task_state"])) : task.task_state.none
          , db_provider.dt_val(dr["dt_upd"]), db_provider.int_val(dr["order"]), db_provider.str_val(dr["class"])
          , db_provider.str_val(dr["title_singolare"]), db_provider.str_val(dr["title_plurale"]));
        this.tasks.Add(t);
      }

      // task -> synch_folders
      foreach (task t in this.tasks) {
        if (t.folder_id.HasValue && !t.file_id.HasValue)
          this.synch_folders.First(x => x.id == t.synch_folder_id).get_folder(t.folder_id.Value).task = t;
        else this.synch_folders.First(x => x.id == t.synch_folder_id).get_file(t.file_id.Value).task = t;
      }

    }

    protected List<synch_folder> load_folders(int? folder_id = null, int? synch_folder_id = null) {

      string[,] pars = new string[,] { { "folder_id", folder_id.HasValue ? folder_id.Value.ToString() : "null" } 
        , { "synch_folder_id", synch_folder_id.HasValue ? synch_folder_id.Value.ToString() : "null" } };

      // folders
      List<synch_folder> res = new List<synch_folder>();
      foreach (DataRow dr in db_conn.dt_table(core.parse_query("notes.folders", pars)).Rows) {
        string tp = db_provider.str_val(dr["tp"]);
        int lf = db_provider.int_val(dr["lvl"]);
        if (tp == "synch_folder") {
          res.Add(new synch_folder(db_provider.int_val(dr["synch_folder_id"])
            , db_provider.str_val(dr["title"]), db_provider.str_val(dr["des"]), db_provider.str_val(dr["http_path"])));
        } else if (tp == "folder") {
          folder f = new folder(db_provider.int_val(dr["synch_folder_id"]), db_provider.int_val(dr["folder_id"])
            , db_provider.int_val_null(dr["parent_id"]), db_provider.str_val(dr["title"]), db_provider.str_val(dr["folder_path"]));
          if (lf == 1) {
            synch_folder p = res.FirstOrDefault(x => x.id == f.synch_folder_id);
            if (p == null) throw new Exception("il synch_folder con id " + f.synch_folder_id.ToString() + " non è stato trovato!");
            p.add_folder(f);
          } else {
            folder p = null;
            foreach (synch_folder s in res) {
              p = s.get_folder(f.parent_id.Value);
              if (p != null) break;
            }
            if (p == null) throw new Exception("il folder con id " + f.parent_id.Value.ToString() + " non è stato trovato!");
            p.add_folder(f);
          }
        } else throw new Exception("tp row '" + tp + "' non supportato!");
      }

      // files
      foreach (DataRow dr in db_conn.dt_table(core.parse_query("notes.files", pars)).Rows) {
        int sfi = db_provider.int_val(dr["synch_folder_id"]);
        long fi = db_provider.long_val(dr["folder_id"]);
        file f = new file(db_provider.int_val(dr["synch_folder_id"]), db_provider.long_val(dr["folder_id"])
            , db_provider.long_val(dr["file_id"]), db_provider.str_val(dr["file_name"])
            , db_provider.dt_val(dr["dt_ins"]).Value);
        if (fi > 0) res.First(x => x.id == sfi).get_folder(fi).add_file(f);
        else res.First(x => x.id == sfi).add_file(f);
      }

      return res;
    }
  }
}