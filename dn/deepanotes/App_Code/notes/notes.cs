using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using mlib.db;
using dn_lib;

namespace deepanotes {
  public class notes : bo {
    public notes() {
    }

    public List<synch_folder> load_folders(int? folder_id = null, int? synch_folder_id = null) {

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
            , db_provider.int_val_null(dr["parent_id"]), db_provider.str_val(dr["title"]));
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

      // tasks
      foreach (DataRow dr in db_conn.dt_table(core.parse_query("notes.tasks", pars)).Rows) {
        task t = new task(db_provider.int_val(dr["synch_folder_id"]), db_provider.long_val(dr["task_id"])
          , db_provider.long_val_null(dr["file_id"]), db_provider.long_val_null(dr["folder_id"])
          , db_provider.str_val(dr["title"]), db_provider.str_val(dr["user"]), db_provider.str_val(dr["stato"])
          , db_provider.str_val(dr["task_state"]) != "" ? (task.task_state)Enum.Parse(typeof(task.task_state), db_provider.str_val(dr["task_state"])) : task.task_state.none
          , db_provider.dt_val(dr["dt_upd"]));
        if (t.folder_id.HasValue)
          res.First(x => x.id == t.synch_folder_id).get_folder(t.folder_id.Value).task = t;
        else res.First(x => x.id == t.synch_folder_id).get_file(t.file_id.Value).task = t;
      }

      return res;
    }
  }
}