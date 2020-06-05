using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using mlib.db;

namespace deepanotes
{
  public class notes : bo
  {
    public notes() {
    }

    public List<synch_folder> load_folders() {
      List<synch_folder> res = new List<synch_folder>();
      foreach (DataRow dr in db_conn.dt_table(core.parse_query("notes.folders-tree")).Rows) {
        string tp = db_provider.str_val(dr["tp"]);
        if (tp == "synch_folder") {
          res.Add(new synch_folder(db_provider.int_val(dr["synch_folder_id"])
            , db_provider.str_val(dr["title"]), db_provider.str_val(dr["des"]), db_provider.str_val(dr["http_path"])));
        } else if (tp == "folder") {
          folder f = new folder(db_provider.int_val(dr["synch_folder_id"]), db_provider.int_val(dr["folder_id"])
            , db_provider.int_val_null(dr["parent_id"]), db_provider.str_val(dr["title"]));
          if (!f.parent_id.HasValue) {
            synch_folder p = res.FirstOrDefault(x => x.synch_folder_id == f.synch_folder_id);
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
      return res;
    }
  }
}