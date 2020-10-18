using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.IO;
using dlib.db;
using dlib;

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

    public void load_notes(int? folder_id = null, int? synch_folder_id = null, task_filter tf = null) {

      // folders structure
      this.synch_folders = load_folders(folder_id, synch_folder_id);

      // tasks
      this.tasks = load_tasks(folder_id: folder_id, synch_folder_id: synch_folder_id, tf: tf);

      // task -> synch_folders
      foreach (task t in this.tasks) {
        if (t.folder_id.HasValue && !t.file_id.HasValue)
          this.synch_folders.First(x => x.id == t.synch_folder_id).get_folder(t.folder_id.Value).task = t;
        else this.synch_folders.First(x => x.id == t.synch_folder_id).get_file(t.file_id.Value).task = t;
      }

    }

    public List<free_label> load_free_labels() {
      return db_conn.dt_table(core.parse_query("lib-synch.free-labels")).Rows.Cast<DataRow>()
        .Select(r => new free_label(db_provider.str_val(r["free_txt"]), db_provider.str_val(r["stato"])
          , db_provider.str_val(r["priorita"]), db_provider.str_val(r["tipo"]), db_provider.str_val(r["stima"])
          , db_provider.str_val(r["default"]) == "1")).ToList();
    }

    public task load_task(int task_id) { List<task> l = load_tasks(task_id); return l.Count > 0 ? l[0] : null; }

    protected List<task> load_tasks(int? task_id = null, int? folder_id = null, int? synch_folder_id = null, task_filter tf = null) {

      // filtro
      string filters = tf != null ? tf.def : "";
      if (filters != "") {
        int i_filter = filters.IndexOf("{");
        while (i_filter >= 0) {
          int i_end = filters.IndexOf("}", i_filter);
          if (i_end <= 0) throw new Exception("il filtro '" + tf.def + "' non è definito correttamente!");
          string sub = filters.Substring(i_filter, (i_end - i_filter) + 1);

          // vedo se c'è un campo
          string field = "", name_field = "";
          int f_from = sub.IndexOf("[");
          if (f_from >= 0) {
            int f_end = sub.IndexOf("]", f_from);
            if (f_end <= 0) throw new Exception("il filtro '" + tf.def + "' non è definito correttamente!");
            field = sub.Substring(f_from, (f_end - f_from) + 1);
            name_field = field.Substring(1, field.Length - 2).Trim();
          }

          string new_sub = sub.Substring(1, sub.Length - 2).Replace(field, "t3." + name_field);
          filters = filters.Replace(sub, new_sub);

          i_filter = filters.IndexOf("{");
        }
      }

      string[,] pars = new string[,] { { "task_id", task_id.HasValue ? task_id.Value.ToString() : "null" }
        , { "folder_id", folder_id.HasValue ? folder_id.Value.ToString() : "null" } 
        , { "synch_folder_id", synch_folder_id.HasValue ? synch_folder_id.Value.ToString() : "null" }
        , { "filter_sf", !synch_folder_id.HasValue && folder_id.HasValue ? " ft.tp <> 'synch_folder' and " : "" }
        , { "filters", filters != "" ? " and " + filters : "" } };

      return db_conn.dt_table(core.parse_query("lib-notes.tasks", pars)).Rows.Cast<DataRow>()
        .Select(dr => new task(db_provider.int_val(dr["synch_folder_id"]), db_provider.long_val(dr["task_id"])
          , db_provider.long_val_null(dr["file_id"]), db_provider.long_val_null(dr["folder_id"])
          , db_provider.str_val(dr["title"]), db_provider.str_val(dr["user"])
          , db_provider.dt_val(dr["dt_ref"]), db_provider.dt_val(dr["dt_ins"]), db_provider.dt_val(dr["dt_upd"])
          , new task_stato(db_provider.str_val(dr["stato"]), db_provider.int_val(dr["stato_order"], 99)
            , db_provider.str_val(dr["stato_class"]), db_provider.str_val(dr["stato_title_plurale"]), db_provider.str_val(dr["stato_title_singolare"]))
          , new task_priorita(db_provider.str_val(dr["priorita"]), db_provider.int_val(dr["priorita_order"], 99)
            , db_provider.str_val(dr["priorita_class"]), db_provider.str_val(dr["priorita_title_plurale"]), db_provider.str_val(dr["priorita_title_singolare"]))
          , new task_stima(db_provider.str_val(dr["stima"]), db_provider.float_val(dr["stima_days"])
            , db_provider.str_val(dr["stima_class"]), db_provider.str_val(dr["stima_title_plurale"]), db_provider.str_val(dr["stima_title_singolare"]))
          , new task_tipo(db_provider.str_val(dr["tipo"]), db_provider.int_val(dr["tipo_order"], 99)
            , db_provider.str_val(dr["tipo_class"]), db_provider.str_val(dr["tipo_title_plurale"]), db_provider.str_val(dr["tipo_title_singolare"]))
          , db_provider.int_val(dr["task_notes"]) == 1, db_provider.int_val(dr["task_files"]) == 1)).ToList();
    }

    protected List<synch_folder> load_folders(int? folder_id = null, int? synch_folder_id = null) {

      string[,] pars = new string[,] { { "folder_id", folder_id.HasValue ? folder_id.Value.ToString() : "null" } 
        , { "synch_folder_id", synch_folder_id.HasValue ? synch_folder_id.Value.ToString() : "null" } };

      // folders
      List<synch_folder> res = new List<synch_folder>();
      foreach (DataRow dr in db_conn.dt_table(core.parse_query("lib-notes.folders", pars)).Rows) {
        string tp = db_provider.str_val(dr["tp"]);
        int lf = db_provider.int_val(dr["lvl"]);
        if (tp == "synch_folder") {
          res.Add(new synch_folder(db_provider.int_val(dr["synch_folder_id"])
            , db_provider.str_val(dr["title"]), db_provider.str_val(dr["des"]), db_provider.str_val(dr["http_path"])));
        } else if (tp == "folder") {
          folder f = new folder(db_provider.int_val(dr["synch_folder_id"]), db_provider.int_val(dr["folder_id"])
            , db_provider.int_val_null(dr["parent_id"]), db_provider.str_val(dr["title"]), db_provider.str_val(dr["folder_path"])
            , db_provider.int_val(dr["is_task"]) > 0);
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
      foreach (DataRow dr in db_conn.dt_table(core.parse_query("lib-notes.files", pars)).Rows) {
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

    public void remove_task(int task_id) {
      // aggiorno il file/folder
      DataRow r = db_conn.first_row(core.parse_query("lib-notes.task-paths", new string[,] { { "task_id", task_id.ToString() } }));
      if (r == null) throw new Exception("il task " + task_id.ToString() + " non c'è in tabella!");
      string folder_path = db_provider.str_val(r["folder_path"]), name = db_provider.str_val(r["task_name"])
        , src = Path.Combine(folder_path, name);
      if (db_provider.int_val(r["file_id"]) > 0) {
        if (File.Exists(src)) File.Delete(src);
      } else { if (Directory.Exists(src)) Directory.Delete(src, true); }

      // aggiorno il db
      if (db_provider.int_val(r["file_id"]) > 0)
        db_conn.exec(core.parse_query("lib-synch.del-file", new string[,] { { "file_id", r["file_id"].ToString() } }));
      else
        db_conn.exec(core.parse_query("lib-synch.del-folder", new string[,] { { "folder_id", r["folder_id"].ToString() } }));
      db_conn.exec(core.parse_query("lib-notes.del-task", new string[,] { { "task_id", task_id.ToString() } }));
    }

    public void update_task(int task_id, out string rel_path, List<free_label> fl, string title = null, string assegna = null
      , string stato = null, string priorita = null, string stima = null, string tipo = null) {
      rel_path = "";

      // aggiorno il file/folder
      DataRow r = db_conn.first_row(core.parse_query("lib-notes.task-paths", new string[,] { { "task_id", task_id.ToString() } }));
      if (r == null) throw new Exception("il task " + task_id.ToString() + " non c'è in tabella!");
      rel_path = db_provider.str_val(r["rel_path"]);
      string folder_path = db_provider.str_val(r["folder_path"]), name = db_provider.str_val(r["task_name"]), new_name = name;

      // stato
      if (stato != null) {
        free_label l = fl.FirstOrDefault(x => x.stato == stato && x.def);
        if (l == null) throw new Exception("non è stata definita la free label per lo stato '" + stato + "'!");
        if (db_provider.str_val(r["free_state"]) != "")
          new_name = new_name.Replace("." + db_provider.str_val(r["free_state"]) + ".", "." + l.free_txt + ".");
        else
          new_name = new_name.Substring(0, new_name.IndexOf(".task")) + "." + l.free_txt + new_name.Substring(new_name.IndexOf(".task"));
      }

      // assegna
      if (assegna != null) {
        if (db_provider.str_val(r["free_assegna"]) != "")
          new_name = new_name.Replace("." + db_provider.str_val(r["free_assegna"]) + ".", "." + (assegna != "" ? assegna + "." : ""));
        else
          new_name = new_name.Substring(0, new_name.IndexOf(".task")) + (assegna != "" ? "." + assegna : "")
            + new_name.Substring(new_name.IndexOf(".task"));
      }

      // priorita
      if (priorita != null) {
        free_label l = priorita != "" ? fl.FirstOrDefault(x => x.priorita == priorita && x.def) : null;
        if (priorita != "" && l == null) throw new Exception("non è stata definita la free label per la priorita '" + priorita + "'!");

        if (db_provider.str_val(r["free_priorita"]) != "")
          new_name = new_name.Replace("." + db_provider.str_val(r["free_priorita"]) + ".", "." + (priorita != "" ? l.free_txt + "." : ""));
        else
          new_name = new_name.Substring(0, new_name.IndexOf(".task")) + (priorita != "" ? "." + l.free_txt : "")
            + new_name.Substring(new_name.IndexOf(".task"));
      }

      // tipo
      if (tipo != null) {
        free_label l = tipo != "" ? fl.FirstOrDefault(x => x.tipo == tipo && x.def) : null;
        if (tipo != "" && l == null) throw new Exception("non è stata definita la free label per il tipo '" + tipo + "'!");

        if (db_provider.str_val(r["free_tipo"]) != "")
          new_name = new_name.Replace("." + db_provider.str_val(r["free_tipo"]) + ".", "." + (tipo != "" ? l.free_txt + "." : ""));
        else
          new_name = new_name.Substring(0, new_name.IndexOf(".task")) + (tipo != "" ? "." + l.free_txt : "")
            + new_name.Substring(new_name.IndexOf(".task"));
      }

      // stima
      if (stima != null) {
        free_label l = stima != "" ? fl.FirstOrDefault(x => x.stima == stima && x.def) : null;
        if (stima != "" && l == null) throw new Exception("non è stata definita la free label per la stima '" + stima + "'!");

        if (db_provider.str_val(r["free_stima"]) != "")
          new_name = new_name.Replace("." + db_provider.str_val(r["free_stima"]) + ".", "." + (stima != "" ? l.free_txt + "." : ""));
        else
          new_name = new_name.Substring(0, new_name.IndexOf(".task")) + (stima != "" ? "." + l.free_txt : "") + new_name.Substring(new_name.IndexOf(".task"));
      }

      // title
      if (!string.IsNullOrEmpty(title)) new_name = new_name.Replace(db_provider.str_val(r["title"]) + ".", title + ".");

      string src = Path.Combine(folder_path, name);
      if (db_provider.int_val(r["file_id"]) > 0) {
        if (!File.Exists(src)) throw new Exception("il file non c'è, non è possibile aggiornare l'attività!");
        File.Move(src, Path.Combine(folder_path, new_name));
      } else {
        if (!Directory.Exists(src)) throw new Exception("la cartella non c'è, non è possibile aggiornare l'attività!");
        Directory.Move(src, Path.Combine(folder_path, new_name));
      }

      // stato 
      if (stato != null)
        db_conn.exec(core.parse_query("lib-notes.set-task-state"
          , new string[,] { { "stato", stato }, { "task_id", task_id.ToString() } }));

      // assegna
      if (assegna != null)
        db_conn.exec(core.parse_query("lib-notes.set-task-user"
          , new string[,] { { "user", assegna }, { "task_id", task_id.ToString() } }));

      // priorita 
      if (priorita != null)
        db_conn.exec(core.parse_query("lib-notes.set-task-priorita"
          , new string[,] { { "priorita", priorita }, { "task_id", task_id.ToString() } }));

      // stima 
      if (stima != null)
        db_conn.exec(core.parse_query("lib-notes.set-task-stima"
          , new string[,] { { "stima", stima }, { "task_id", task_id.ToString() } }));

      // tipo
      if (tipo != null)
        db_conn.exec(core.parse_query("lib-notes.set-task-tipo"
          , new string[,] { { "tipo", tipo }, { "task_id", task_id.ToString() } }));

      // title
      if (!string.IsNullOrEmpty(title))
        db_conn.exec(core.parse_query("lib-notes.set-task-title"
          , new string[,] { { "title", title }, { "task_id", task_id.ToString() } }));

      // aggiorno il file nel db
      if (db_provider.int_val(r["file_id"]) > 0)
        db_conn.exec(core.parse_query("lib-synch.set-file-name", new string[,] { { "file_id", r["file_id"].ToString() }, { "file_name", new_name } }));
      else
        db_conn.exec(core.parse_query("lib-synch.set-folder-name", new string[,] { { "folder_id", r["folder_id"].ToString() }, { "folder_name", new_name } }));
    }

    public string get_task_notes(int task_id) {
      DataTable dt = db_conn.dt_table(core.parse_query("lib-notes.get-task-notes", new string[,] { { "task_id", task_id.ToString() } }), true);
      return dt.Rows.Count > 0 ? db_provider.str_val(dt.Rows[0]["task_notes"]) : "";
    }

    public DataTable get_task_allegati(int task_id) {
      return db_conn.dt_table(core.parse_query("lib-notes.get-task-allegati", new string[,] { { "task_id", task_id.ToString() } }));
    }

    public void save_task_notes(int task_id, string notes) {
      DataTable dt = db_conn.dt_table(core.parse_query("lib-notes.info-task-notes", new string[,] { { "task_id", task_id.ToString() } }));
      if (dt.Rows.Count == 0) throw new Exception("l'attività " + task_id.ToString() + " non è registrata correttamente!");

      DataRow dr = dt.Rows[0];
      // aggiornamento del file
      if (db_provider.str_val(dr["file_id_notes"]) != "") {
        string file_path = db_provider.str_val(dr["file_path_notes"]) != "" ? 
          db_provider.str_val(dr["file_path_notes"]) : db_provider.str_val(dr["file_path"]);
        System.Text.Encoding e = dlib.tools.encoding_type.GetType(file_path);
        // file sorgente
        if (db_provider.str_val(dr["type_content"]) == "source") {
          string all = File.ReadAllText(file_path, e), oc = db_provider.str_val(dr["open_comment"])
            , cc = db_provider.str_val(dr["close_comment"]), key_from = "###FROM_NOTES###", key_to = "###TO_NOTES###";
          // cerco i commenti 
          int from = all.IndexOf(key_from), to = all.IndexOf(key_to, from + 1);
          string src = "";
          if (from >= 0 && to > 0) 
            src = all.Substring(0, from + key_from.Length) + "\r\n" + notes + "\r\n" + all.Substring(to);
          else
            src = oc + " " + key_from + "\r\n" + notes + "\r\n" + key_to + cc + "\n\n" + all;
          File.WriteAllText(file_path, src, e);
          db_conn.exec(core.parse_query("lib-notes.init-notes", new string[,] { { "task_id", task_id.ToString() } })
            , pars: new System.Data.Common.DbParameter[] { 
                new System.Data.SqlClient.SqlParameter("@content", System.Data.SqlDbType.VarChar) { Value = notes.Trim(new char[] { ' ', '\n', '\r' }) } });
        }
          // file info
        else {
          File.WriteAllText(file_path, notes, e);
          db_conn.exec(core.parse_query("lib-notes.init-notes", new string[,] { { "task_id", task_id.ToString() } })
            , pars: new System.Data.Common.DbParameter[] { 
                new System.Data.SqlClient.SqlParameter("@content", System.Data.SqlDbType.VarChar) { Value = notes.Trim(new char[] { ' ', '\n', '\r' }) } });
        }
      } // nuovo commento
      else {
        // file
        if (db_provider.str_val(dr["file_id"]) != "") {
          string file_path = db_provider.str_val(dr["file_path"]);
          System.Text.Encoding e = dlib.tools.encoding_type.GetType(file_path);
          // sorgente
          if (db_provider.str_val(dr["type_content"]) == "source") {
            string all = File.ReadAllText(file_path, e), oc = db_provider.str_val(dr["open_comment"])
              , cc = db_provider.str_val(dr["close_comment"]), key_from = "###FROM_NOTES###", key_to = "###TO_NOTES###";
            string src = oc + " " + key_from + "\r\n" + notes + "\r\n" + key_to + cc + "\n\n" + all;
            File.WriteAllText(file_path, src, e);
            db_conn.exec(core.parse_query("lib-notes.init-notes", new string[,] { { "task_id", task_id.ToString() } })
              , pars: new System.Data.Common.DbParameter[] { 
                new System.Data.SqlClient.SqlParameter("@content", System.Data.SqlDbType.VarChar) { Value = notes.Trim(new char[] { ' ', '\n', '\r' }) } });
          } else { 
            // creo una cartella con questo nome di task e aggiungo le note
            string title = db_provider.str_val(dr["title"]);
            string parent = Path.GetDirectoryName(file_path);
            string new_folder = Path.Combine(parent, Path.GetFileNameWithoutExtension(file_path));
            Directory.CreateDirectory(new_folder);
            File.Move(file_path, Path.Combine(new_folder, title + Path.GetExtension(file_path)));
            File.WriteAllText(Path.Combine(new_folder, "i.txt"), notes, System.Text.Encoding.UTF8);
            db_conn.exec(core.parse_query("lib-notes.init-notes", new string[,] { { "task_id", task_id.ToString() } })
              , pars: new System.Data.Common.DbParameter[] { 
              new System.Data.SqlClient.SqlParameter("@content", System.Data.SqlDbType.VarChar) { Value = notes.Trim(new char[] { ' ', '\n', '\r' }) } });
          }
        }
          // cartella
        else {
          File.WriteAllText(db_provider.str_val(dr["folder_path"]) + "i.txt", notes, System.Text.Encoding.UTF8);
          db_conn.exec(core.parse_query("lib-notes.init-notes", new string[,] { { "task_id", task_id.ToString() } })
            , pars: new System.Data.Common.DbParameter[] { 
              new System.Data.SqlClient.SqlParameter("@content", System.Data.SqlDbType.VarChar) { Value = notes.Trim(new char[] { ' ', '\n', '\r' }) } });
        }
      }
    }

    public void add_task(int synch_folder_id, int folder_id, string stato
      , string title, string assegna, string priorita, string tipo, string stima) {
      if (string.IsNullOrEmpty(title)) throw new Exception("il titolo non è stato specificato!");

      string folder_path = folder_id > 0 ? db_conn.first_row(core.parse_query("folder-local-path"
        , new string[,] { { "folder_id", folder_id.ToString() } }))[0].ToString() :
        db_conn.first_row(core.parse_query("synch-folder-local-path"
        , new string[,] { { "synch_folder_id", synch_folder_id.ToString() } }))[0].ToString();

      List<free_label> fl = load_free_labels();

      string name = DateTime.Now.ToString("yyyy").Substring(2, 2) + DateTime.Now.ToString("MMdd") + "." + title;
      if (!string.IsNullOrEmpty(assegna)) name += "." + assegna;
      if (!string.IsNullOrEmpty(stato)) {
        free_label l = fl.FirstOrDefault(x => x.stato == stato && x.def);
        if (l == null) throw new Exception("non è stata definita l'etichetta per lo stato '" + stato + "'!");
        name += "." + l.free_txt;
      }
      if (!string.IsNullOrEmpty(priorita)) {
        free_label l = fl.FirstOrDefault(x => x.priorita == priorita && x.def);
        if (l == null) throw new Exception("non è stata definita l'etichetta per la priorità '" + priorita + "'!");
        name += "." + l.free_txt;
      }
      if (!string.IsNullOrEmpty(tipo)) {
        free_label l = fl.FirstOrDefault(x => x.tipo == tipo && x.def);
        if (l == null) throw new Exception("non è stata definita l'etichetta per il tipo '" + tipo + "'!");
        name += "." + l.free_txt;
      }
      if (!string.IsNullOrEmpty(stima)) {
        free_label l = fl.FirstOrDefault(x => x.stima == stima && x.def);
        if (l == null) throw new Exception("non è stata definita l'etichetta per la stima '" + stima + "'!");
        name += "." + l.free_txt;
      }
      name += ".task";

      // creo la cartella fisica
      Directory.CreateDirectory(Path.Combine(folder_path, name));

      // aggiorno il db
      long new_id = long.Parse(synch_folder_id > 0 ?
        db_conn.exec(core.parse_query("lib-synch.task-ins-into-synch", new string[,] { { "synch_folder_id", synch_folder_id.ToString() }, { "name", name } }), true)
        : db_conn.exec(core.parse_query("lib-synch.task-ins-into-folder", new string[,] { { "folder_id", folder_id.ToString() }, { "name", name } }), true));

      db_conn.exec(core.parse_query("lib-notes.ins-task", new string[,] { { "folder_id", new_id.ToString() }
        , { "title", title }, { "user", assegna }, { "stato", !string.IsNullOrEmpty(stato) ? stato : "da_fare" }
        , { "priorita", priorita }, { "tipo", tipo }, { "stima", stima }}), true);
    }
  }
}