using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using dn_lib;
using dn_lib.db;
using dn_lib.tools;

namespace dn_lib
{
  public class synch : bo
  {
    public List<synch_folder> synch_folders { get; set; }

    public synch(db_provider conn, core c, config cfg, int user_id = 0, string user_name = "")
      : base(conn, c, cfg, user_id, user_name)
    {
    }

    #region data access

    public List<synch_folder> list_synch_folders(string pc_name)
    {
      return db_conn.dt_table(core.parse_query("lib-notes.synch-folders", new string[,] { { "pc_name", pc_name } })).Rows.Cast<DataRow>()
        .Select(x => new synch_folder(db_provider.int_val(x["synch_folder_id"]), db_provider.str_val(x["pc_name"])
          , db_provider.str_val(x["title"]), db_provider.str_val(x["des"])
          , db_provider.str_val(x["local_path"]), db_provider.str_val(x["http_path"])
          , db_provider.str_val(x["user"]), db_provider.str_val(x["password"]))).ToList();
    }

    public void clean_readed() { db_conn.exec(core.parse_query("lib-notes.clean-readed")); }

    public int del_unreaded() { return int.Parse(db_conn.exec(core.parse_query("lib-notes.del-unreaded"))); }

    public long ins_folder(int synch_folder_id, long? parent_id, string folder_name, DateTime dt_ins, DateTime dt_upd, out string tp, out int cc)
    {
      tp = ""; cc = 0;
      string res = db_conn.exec(core.parse_query("lib-notes.ins-folder"
        , new Dictionary<string, object>() { { "synch_folder_id", synch_folder_id.ToString() }
          , { "dt_ins", dt_ins }, { "dt_upd", dt_upd }
          , { "parent_id", !parent_id.HasValue ? "null" : parent_id.Value.ToString() }
          , { "cmp_p", !parent_id.HasValue ? "is" : "=" }, { "folder_name", folder_name } }), true, true);
      tp = res.Split(new char[] { ';' })[1];
      cc = int.Parse(res.Split(new char[] { ';' })[2]);
      return res.Split(new char[] { ';' })[0] != "" ? long.Parse(res.Split(new char[] { ';' })[0]) : -1;
    }

    public long ins_file(int synch_folder_id, long? folder_id, string file_name, string extension, DateTime ct, DateTime lwt, out string tp, out int cc, out DateTime? content_lwt)
    {
      tp = ""; cc = 0; content_lwt = null;
      string res = db_conn.exec(core.parse_query("lib-notes.ins-file"
        , new Dictionary<string, object>() { { "synch_folder_id", synch_folder_id.ToString() }, { "ct", ct }, { "lwt", lwt }
          , { "folder_id", !folder_id.HasValue ? "null" : folder_id.ToString() }, { "cmp_f", !folder_id.HasValue ? "is" : "=" }
          , { "file_name", file_name }, { "extension", !string.IsNullOrEmpty(extension) ? extension.ToLower() : "" } }), true, true);
      string[] ress = res.Split(new char[] { ';' });
      tp = ress[1]; cc = int.Parse(ress[2]);
      content_lwt = ress[3] != "" ? Convert.ToDateTime(ress[3]) : (DateTime?)null;
      return ress[0] != "" ? long.Parse(ress[0]) : -1;
    }

    public long ins_task(task t, out string tp, out int cc, out DateTime? notes_lwt)
    {
      tp = ""; cc = 0; notes_lwt = null;
      string res = db_conn.exec(core.parse_query("lib-notes.ins-task", new Dictionary<string, object>() { { "task", t }
        , { "folder_id", t.folder_id.HasValue ? t.folder_id.Value : 0 }, { "file_id", t.file_id.HasValue ? t.file_id.Value : 0 }
        , { "stato", t.stato }, { "priorita", t.priorita }, { "tipo", t.tipo }, { "stima", t.stima } }), true, true);

      string[] ress = res.Split(new char[] { ';' });
      tp = ress[1]; cc = int.Parse(ress[2]);
      notes_lwt = ress[3] != "" ? Convert.ToDateTime(ress[3]) : (DateTime?)null;
      return ress[0] != "" ? long.Parse(ress[0]) : -1;
    }

    public synch_machine get_synch_machine(string pc_name)
    {
      DataRow r = db_conn.first_row(core.parse_query("lib-synch.start-synch-machine", new string[,] { { "pc_name", pc_name } }));
      return r != null ? new synch_machine(db_provider.int_val(r["synch_machine_id"]), db_provider.str_val(r["pc_name"])
        , db_provider.str_val(r["pc_des"]), db_provider.int_val(r["seconds"]), db_provider.int_val(r["active"]) == 1
        , db_provider.str_val(r["state"])) : null;
    }

    public void start_machine(int id, string ip_address)
    {
      db_conn.exec(core.parse_query("lib-synch.start-machine", new string[,] { { "id", id.ToString() }
        , { "ip_address", ip_address } }));
    }

    public void stop_machine(int id)
    {
      db_conn.exec(core.parse_query("lib-synch.stop-machine", new string[,] { { "id", id.ToString() } }));
    }

    public void last_synch_machine(int id, int folders, int files, int deleted, int seconds)
    {
      db_conn.exec(core.parse_query("lib-synch.last-synch-machine", new string[,] { { "id", id.ToString() }
        , { "folders", folders.ToString() }, { "files", files.ToString() }
        , { "deleted", deleted.ToString() }, { "seconds", seconds.ToString() } }));
    }

    public List<synch_machine> list_synch_machine()
    {
      return db_conn.dt_table(core.parse_query("lib-synch.synch-machines")).Rows.Cast<DataRow>()
        .Select(r => new synch_machine(db_provider.int_val(r["synch_machine_id"]), db_provider.str_val(r["pc_name"])
         , db_provider.str_val(r["pc_des"]), db_provider.int_val(r["seconds"]), db_provider.int_val(r["active"]) == 1
         , db_provider.str_val(r["state"]), db_provider.str_val(r["ip_address"])
         , db_provider.dt_val(r["dt_start"]), db_provider.dt_val(r["dt_stop"]), db_provider.dt_val(r["dt_lastsynch"])
         , db_provider.int_val(r["c_folders"]), db_provider.int_val(r["c_files"]), db_provider.int_val(r["c_deleted"])
         , db_provider.int_val(r["s_synch"]))).ToList();
    }

    public List<free_label> load_free_labels()
    {
      return db_conn.dt_table(core.parse_query("lib-notes.free-labels")).Rows.Cast<DataRow>()
        .Select(r => new free_label(db_provider.str_val(r["free_txt"]), db_provider.str_val(r["stato"])
          , db_provider.str_val(r["priorita"]), db_provider.str_val(r["tipo"]), db_provider.str_val(r["stima"])
          , db_provider.str_val(r["default"]) == "1")).ToList();
    }

    public List<file_info> load_file_info()
    {
      return db_conn.dt_table(core.parse_query("lib-notes.file-infos")).Rows.Cast<DataRow>()
        .Select(r => new file_info(db_provider.str_val(r["file_name"]), (file_info.fi_type)Enum.Parse(typeof(file_info.fi_type), db_provider.str_val(r["type_info"])))).ToList();
    }

    public List<file_type> load_file_types()
    {
      return db_conn.dt_table(core.parse_query("lib-notes.file-types")).Rows.Cast<DataRow>()
        .Select(r => new file_type(db_provider.str_val(r["extension"]), db_provider.str_val(r["des_extension"]), db_provider.str_val(r["open_comment"]), db_provider.str_val(r["type_content"])
          , (file_type.ft_type_content)Enum.Parse(typeof(file_type.ft_type_content), db_provider.str_val(r["type_content"])))).ToList();
    }

    #endregion

    #region synch

    public event EventHandler<synch_event_args> synch_event;
    protected void fire_synch_event(string txt, bool init = false)
    {
      log.log_debug(txt);
      EventHandler<synch_event_args> handler = synch_event;
      if(handler != null) {
        handler(this, new synch_event_args() { message = txt, init = init });
      }
    }

    protected bool _loaded_settings = false;
    protected List<string> _users = null;
    protected List<free_label> _labels = null;
    protected List<file_info> _f_infos = null;
    protected List<file_type> _f_types = null;
    protected void reload_settings()
    {
      if(_loaded_settings) return;
      _loaded_settings = true;
      _users = db_conn.dt_table(core.parse_query("lib-notes.task-users")).Rows.Cast<DataRow>()
        .Select(r => db_provider.str_val(r["nome"])).ToList();
      _labels = load_free_labels();
      _f_infos = load_file_info();
      _f_types = load_file_types();
    }

    public file_info is_info_file(string file_name) { reload_settings(); return _f_infos.FirstOrDefault(x => x.file_name.ToLower() == file_name.ToLower()); }

    public file_type is_type_file(string extension) { reload_settings(); return _f_types.FirstOrDefault(x => x.extension.ToLower() == extension.ToLower()); }

    public synch_results reload_folders()
    {
      reload_settings();

      synch_results res = new synch_results();
      try {
        DateTime start = DateTime.Now;
        fire_synch_event("lettura cartelle", true);

        // folders to synch
        this.synch_folders = list_synch_folders(Environment.MachineName);
        foreach(synch_folder f in this.synch_folders)
          fire_synch_event(string.Format("cartella di sincronizzazione '{0}' - {1}, path: {2}"
            , f.title, f.des, f.local_path), true);

        // leggo le cartelle
        clean_readed();
        foreach(synch_folder f in this.synch_folders) {
          fire_synch_event(string.Format("leggo la cartella {0}", f.local_path), true);
          res = init_synch_folder(f.id, f.local_path, res: res);
        }
        res.deleted = del_unreaded();
        if(res.deleted > 0) fire_synch_event("cancellati " + res.deleted.ToString() + " files/folders/tasks");
        res.seconds = (int)(DateTime.Now - start).TotalSeconds;
      } catch(Exception ex) { res.err = ex.Message; log.log_err(ex.Message); } finally { }

      return res;
    }

    public void set_file_content(int file_id, string extension, string content, DateTime ct, DateTime lwt)
    {
      db_conn.exec(core.parse_query("lib-notes.set-content", new string[,] { { "file_id", file_id.ToString() }, { "extension", extension.ToLower() }
        , { "ct", ct.ToString("yyyy-MM-dd HH:mm:ss") }, { "lwt", lwt.ToString("yyyy-MM-dd HH:mm:ss") } })
        , pars: new System.Data.Common.DbParameter[] { new SqlParameter("@content", System.Data.SqlDbType.VarChar) { Value = content } });
    }

    protected synch_results init_synch_folder(int synch_folder_id, string path
      , long? parent_id = null, task parent_task = null, synch_results res = null)
    {
      if(res == null) res = new synch_results();
      try {

        // folders
        foreach(string fp in Directory.EnumerateDirectories(path)) {
          DirectoryInfo di = new DirectoryInfo(fp);
          DateTime ct = sys.without_ms(di.CreationTime), lwt = sys.without_ms(di.LastWriteTime);

          // folder
          string tp; int cc = 0;
          long folder_id = ins_folder(synch_folder_id, parent_id, di.Name, ct, lwt, out tp, out cc);
          if(tp == "insert") fire_synch_event("added folder: " + fp);
          else if(tp == "update" && cc > 0) fire_synch_event("updated folder: " + fp);

          // task
          task t = null;
          if(parent_task == null) {
            t = task.parse_task(synch_folder_id, fp, ct, lwt, _users, _labels, folder_id: folder_id);
            if(t != null) {
              t.id = ins_task(t, out tp, out cc, out DateTime? notes_lwt);
              if(tp == "insert") fire_synch_event("added task: " + Path.Combine(path, t.title));
              else if(tp == "update" && cc > 0) fire_synch_event("updated task: " + Path.Combine(path, t.title));
            }
          }
          res.folders++;
          if(parent_task != null) parent_task.level_folder++;
          res = init_synch_folder(synch_folder_id, Path.Combine(path, di.Name), folder_id, t != null ? t : parent_task, res);
        }

        // files
        foreach(string fn in Directory.EnumerateFiles(path)) {
          FileInfo fi = new FileInfo(fn);
          DateTime ct = sys.without_ms(fi.CreationTime), lwt = sys.without_ms(fi.LastWriteTime);

          // web.config
          if(Path.GetFileName(fn).ToLower() == "web.config" && !parent_id.HasValue)
            continue;

          // file          
          long file_id = ins_file(synch_folder_id, parent_id, fi.Name, fi.Extension, ct, lwt
            , out string tp, out int cc, out DateTime? content_lwt);
          if(tp == "insert") fire_synch_event("added file " + file_id.ToString() + ": " + fn);
          else if(tp == "update" && cc > 0) fire_synch_event("updated file " + file_id.ToString() + ": " + fn);

          // file content
          string new_content = "";
          file_info info = is_info_file(fi.Name);
          file_type ftp = is_type_file(fi.Extension);
          if(info != null || ftp != null) {
            if(tp == "insert" || (tp == "update" && (!content_lwt.HasValue || (content_lwt.HasValue && lwt > content_lwt.Value)))) {
              new_content = File.ReadAllText(fn);
              set_file_content((int)file_id, Path.GetExtension(fn).ToLower(), new_content, ct, lwt);

              if(parent_task != null && info != null)
                set_task_notes(parent_task.id, file_id, new_content, file_type.ft_type_content.info, ct, lwt);

              fire_synch_event("letto contenuto file '" + fn + "'");
            }
          }

          // task
          if(parent_task == null) {
            task t = task.parse_task(synch_folder_id, fn, ct, lwt, _users, _labels, file_id: file_id);
            if(t != null) {
              long task_id = ins_task(t, out tp, out cc, out DateTime? notes_lwt);
              if(tp == "insert") fire_synch_event("added task: " + Path.Combine(path, t.title));
              else if(tp == "update" && cc > 0) fire_synch_event("updated task: " + Path.Combine(path, t.title));

              // task notes
              if(ftp != null && (tp == "insert" || (tp == "update" && (!notes_lwt.HasValue || (notes_lwt.HasValue && lwt > notes_lwt.Value)))))
                set_task_notes(task_id, file_id, new_content, ftp.type_content, ct, lwt);
            }
          }

          res.files++;
        }

        // task folder - dt_upd
        if(parent_task != null) {
          string cc = db_conn.exec(core.parse_query("lib-notes.upd-task-date", new string[,] { { "task_id", parent_task.id.ToString() } }));
          if(cc != "0") fire_synch_event("updated task: " + Path.Combine(Path.GetDirectoryName(path), parent_task.title));
        }

      } catch(Exception ex) { log.log_err(ex.Message); res.err = ex.Message; }
      return res;
    }

    public bool set_task_notes(long task_id, long file_id, string content, file_type.ft_type_content type, DateTime ct, DateTime lwt)
    {
      bool res = false;
      if(type == file_type.ft_type_content.info) {
        db_conn.exec(core.parse_query("lib-notes.set-task-notes", new string[,] { { "task_id", task_id.ToString() }, { "file_id", file_id.ToString() }
          , { "ct", ct.ToString("yyyy-MM-dd HH:mm:ss") }, { "lwt", lwt.ToString("yyyy-MM-dd HH:mm:ss") } })
          , pars: new System.Data.Common.DbParameter[] { new SqlParameter("@content", System.Data.SqlDbType.VarChar) { Value = content } });
        res = true;
      } else if(type == file_type.ft_type_content.source) {
        string key_from = "###FROM_NOTES###", key_to = "###TO_NOTES###";
        int from_n = content.IndexOf(key_from), to_n = from_n >= 0 ? content.IndexOf(key_to, from_n + 1) : -1;
        if(from_n >= 0 && to_n > 0) {
          string notes = content.Substring(from_n + key_from.Length, to_n - from_n - key_from.Length - 1).Trim(new char[] { ' ', '\n', '\r' });
          db_conn.exec(core.parse_query("lib-notes.set-task-notes", new string[,] { { "task_id", task_id.ToString() }, { "file_id", file_id.ToString() }
            , { "ct", ct.ToString("yyyy-MM-dd HH:mm:ss") }, { "lwt", lwt.ToString("yyyy-MM-dd HH:mm:ss") } })
            , pars: new System.Data.Common.DbParameter[] { new SqlParameter("@content", System.Data.SqlDbType.VarChar) { Value = notes } });
          res = true;
        }
      }
      if(res) fire_synch_event($"lette note task {task_id}");
      return res;
    }

    #endregion

  }

  public class synch_event_args : EventArgs
  {
    public string message { get; set; }
    public bool init { get; set; }
  }
}
