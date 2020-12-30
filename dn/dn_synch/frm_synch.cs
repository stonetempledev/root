using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Data.SqlClient;
using dn_lib;
using dn_lib.tools;
using dn_lib.db;
using dn_lib.xml;

namespace fsynch {
  public partial class frm_synch : Form {

    protected core _c = null;
    protected db_provider _conn = null;
    protected synch_machine _sm = null;

    // tasks
    protected DateTime? _loaded = null;
    protected List<string> _users = null;
    protected List<free_label> _labels = null;

    // synch folders
    synch _s = null;
    List<synch_folder> _folders = null;
    protected bool _synch = false;
    protected int _max_lines = 1000, _min_lines = 700;

    // mouse move
    protected bool _md; private Point _ll;

    public frm_synch(core c, db_provider conn) {
      _c = c; _conn = conn; InitializeComponent();
    }

    #region double buffered

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam);
    private const int SetExtendedStyle = 0x1036;
    private const int GetExtendedStyle = 0x1037;
    private const int BorderSelect = 0x00008000;
    private const int DoubleBuffer = 0x00010000;

    public static void enable_double_buffering(ListView listView) {
      if (listView == null) throw new ArgumentNullException("listView", "listView should not be null.");
      IntPtr handle = listView.Handle;
      int styles = SendMessage(handle, GetExtendedStyle, IntPtr.Zero, IntPtr.Zero).ToInt32();
      styles |= DoubleBuffer | BorderSelect;
      SendMessage(handle, SetExtendedStyle, IntPtr.Zero, (IntPtr)styles);
    }

    #endregion

    #region form

    protected void status_txt(string txt) { try { tlt_status.Text = txt; Application.DoEvents(); } catch { } }

    private void btn_ok_Click(object sender, EventArgs e) {
      Close();
    }

    private void lbl_title_MouseDown(object sender, MouseEventArgs e) { _md = true; _ll = e.Location; }

    private void lbl_title_MouseMove(object sender, MouseEventArgs e) {
      if (_md) { this.Location = new Point((this.Location.X - _ll.X) + e.X, (this.Location.Y - _ll.Y) + e.Y); this.Update(); }
    }

    private void lbl_title_MouseUp(object sender, MouseEventArgs e) { _md = false; }

    private void frm_synch_Shown(object sender, EventArgs e) {
      try {
        _s = main.create_synch();

        log_debug("pc name: " + Environment.MachineName);
        _sm = _s.get_synch_machine(Environment.MachineName);
        if (_sm == null || !_sm.active) {
          log_war("il pc '" + Environment.MachineName + "' non è configurato, è necessario inserirlo e attivarlo nella tabella synch_machines!");
          status_txt("in attesa di aggiornamento...");
          _sm = null;
          tmr_synch.Interval = 25 * 1000;
          tmr_synch.Start();
          return;
        }
        if (_sm.state != synch_machine.states.none) {
          log_war("c'è già un deepa synch attivo sul pc '" + Environment.MachineName + "', controllare la tabella synch_machines o i processi attivi su quella macchina!");
          status_txt("in attesa di chiusura synch...");
          _sm = null;
          tmr_synch.Interval = 25 * 1000;
          tmr_synch.Start();
          return;
        }

        _s.start_machine(_sm.synch_machine_id, main.local_ip());
        log_debug("secondi rilettura cartelle: " + _sm.seconds.ToString());
        log_debug("inizializzazione");
        try {
          _synch = true;
          reload_folders(true);
          tmr_synch.Interval = _sm.seconds * 1000;
          tmr_synch.Start();
        } finally { _synch = false; }
      } catch (Exception ex) { log_err(ex.Message); }
    }

    protected void add_msg_err(string txt) { append_line(txt, Color.Tomato); }

    protected void append_line(string txt, Color? clr = null) {
      try {
        rt_main.SelectionStart = rt_main.TextLength;
        rt_main.SelectionLength = 0;
        if (clr.HasValue) rt_main.SelectionColor = clr.Value;
        rt_main.AppendText(DateTime.Now.ToString("HH:mm:ss") + " - " + txt + Environment.NewLine);
        rt_main.SelectionColor = rt_main.ForeColor;
        if (rt_main.Lines.Length > _max_lines) {
          int start_index = rt_main.GetFirstCharIndexFromLine(0);
          int count = rt_main.GetFirstCharIndexFromLine((rt_main.Lines.Count() - _min_lines) + 1) - start_index;
          rt_main.Text = rt_main.Text.Remove(start_index, count);
        }
        rt_main.ScrollToCaret();

        Application.DoEvents();
      } catch { }
    }

    protected void add_msg_warning(string txt) { append_line(txt, Color.Yellow); }

    protected void log_debug(string txt) { try { log.log_info(txt); add_msg(txt); } catch { } }
    protected void log_err(string txt) { try { log.log_err(txt); add_msg_err(txt); } catch { } }
    protected void log_war(string txt) { try { log.log_warning(txt); add_msg_warning(txt); } catch { } }

    protected void add_msg(string txt) { append_line(txt, Color.WhiteSmoke); }

    private void frm_synch_Load(object sender, EventArgs e) {
      try {
      } catch (Exception ex) { MessageBox.Show(ex.Message, "Attenzione!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void tmr_synch_Tick(object sender, EventArgs e) {
      try {
        if (_synch) return;

        _synch = true;

        if (_sm == null) {
          _sm = _s.get_synch_machine(Environment.MachineName);
          if (_sm == null || !_sm.active || _sm.state != synch_machine.states.none) { _sm = null; return; }
          log_debug("secondi rilettura cartelle: " + _sm.seconds.ToString());
          log_debug("inizializzazione");
          _s.start_machine(_sm.synch_machine_id, main.local_ip());
          reload_folders();
          tmr_synch.Interval = _sm.seconds * 1000;
          return;
        }

        synch_machine sm = _s.get_synch_machine(Environment.MachineName);
        if (!sm.active) {
          log_war("deepa synch disattivato per il pc '" + Environment.MachineName + "'");
          _s.stop_machine(_sm.synch_machine_id);
          _sm = null;
          return;
        }

        if (sm.seconds != _sm.seconds) { _sm.seconds = sm.seconds; tmr_synch.Interval = _sm.seconds * 1000; }
        reload_folders();
      } catch (Exception ex) { log_err(ex.Message); } finally {
        _synch = false;
      }
    }

    private void btn_min_Click_1(object sender, EventArgs e) {
      this.WindowState = FormWindowState.Minimized;
    }

    private void frm_synch_FormClosing(object sender, FormClosingEventArgs e) {
      try { if (_sm != null) _s.stop_machine(_sm.synch_machine_id); } catch (Exception ex) { log.log_err(ex); }
    }

    #endregion

    #region load folders

    protected void reload_folders(bool first = false) {

      // ogni 5 minuti
      if (!_loaded.HasValue || (_loaded.HasValue && (DateTime.Now - _loaded.Value).TotalMinutes > 5)) {
        _loaded = DateTime.Now;

        _users = _conn.dt_table(main._c.parse_query("lib-notes.task-users")).Rows.Cast<DataRow>()
          .Select(r => db_provider.str_val(r["nome"])).ToList();

        _labels = _s.load_free_labels();
      }


      int seconds, folders, files, deleted;
      reload_folders(_users, _labels, out seconds, out folders, out files, out deleted, first);
      if (_sm != null) _s.last_synch_machine(_sm.synch_machine_id, folders, files, deleted, seconds);
    }

    protected void reload_folders(List<string> users, List<free_label> labels
      , out int seconds, out int folders, out int files, out int deleted, bool first = false) {
      seconds = folders = files = deleted = 0;
      try {
        DateTime start = DateTime.Now;
        status_txt("lettura cartelle...");

        // folders to synch
        _folders = _s.list_synch_folders(Environment.MachineName);
        if (first) {
          foreach (synch_folder f in _folders)
            log_debug(string.Format("cartella di sincronizzazione '{0}' - {1}, path: {2}"
              , f.title, f.des, f.local_path));
        }

        // leggo le cartelle
        _s.clean_readed();
        synch_results res = new synch_results();
        foreach (synch_folder f in _folders) {
          if (first) log_debug(string.Format("leggo la cartella {0}", f.local_path));
          res = init_synch_folder(f.id, f.local_path, users, labels);
        }
        folders = res.folders; files = res.files;
        deleted = _s.del_unreaded();
        if (deleted > 0) log_debug("cancellati " + deleted.ToString() + " files/folders/tasks");

        // contenuti tasks
        /*
        status_txt("lettura contenuti...");
        DataTable dt = _conn.dt_table(_c.parse_query("lib-notes.load-tasks-contents"));
        foreach (long task_id in dt.Rows.Cast<DataRow>().Select(r => db_provider.int_val(r["task_id"])).Distinct())
          _conn.exec(_c.parse_query("lib-notes.init-content", new string[,] { { "task_id", task_id.ToString() } }));

        List<long> tasks_notes = new List<long>();
        foreach (DataRow dr in dt.Rows) {
          try {
            long task_id = db_provider.long_val(dr["task_id"]);

            // è un file
            if (db_provider.int_val(dr["file_id"]) > 0 && db_provider.str_val(dr["load_extension"]) != "") {
              string path = Path.Combine(db_provider.str_val(dr["local_path"])
                , db_provider.str_val(dr["folder_path"]).Length > 0 ? db_provider.str_val(dr["folder_path"]).Substring(1) : ""
                , db_provider.str_val(dr["file_name"])), type = db_provider.str_val(dr["type_content"])
                , type_info = db_provider.str_val(dr["type_info"]);

              string txt = File.ReadAllText(path);
              if (!string.IsNullOrEmpty(txt.Trim())) {
                if ((type == "source" || type == "info") 
                  && (type_info != "notes" || (type_info == "notes" && tasks_notes.Contains(task_id)))) {
                  _conn.exec(_c.parse_query("lib-notes.ins-content", new string[,] { { "task_id", task_id.ToString() }
                    , { "file_id", db_provider.str_val(dr["file_notes_id"]) }
                    , { "title", "" }, { "extension", Path.GetExtension(path).ToLower() }})
                    , pars: new System.Data.Common.DbParameter[] { 
                  new SqlParameter("@content", System.Data.SqlDbType.VarChar) { Value = txt } });
                  log_debug("letto contenuto file '" + path + "'");
                }

                if (!tasks_notes.Contains(task_id)) {
                  ins_notes(task_id, db_provider.int_val(dr["file_notes_id"]), txt, path, type);
                  tasks_notes.Add(task_id);
                }
              }

            }
              // file interno alla cartella
            else if (db_provider.int_val(dr["folder_id"]) > 0 && db_provider.str_val(dr["load_extension_ff"]) != "") {
              string path = Path.Combine(db_provider.str_val(dr["local_path"])
                , db_provider.str_val(dr["folder_path"]).Length > 0 ? db_provider.str_val(dr["folder_path"]).Substring(1) : ""
                , db_provider.str_val(dr["file_name_ff"])), type = db_provider.str_val(dr["type_content_ff"])
                , type_info = db_provider.str_val(dr["type_info_ff"]);

              System.Text.Encoding e = encoding_type.GetType(path);
              string txt = File.ReadAllText(path, e);
              if (!string.IsNullOrEmpty(txt.Trim())) {
                if ((type == "source" || type == "info") 
                  && (type_info != "notes" || (type_info == "notes" && tasks_notes.Contains(task_id)))) {
                  _conn.exec(_c.parse_query("lib-notes.ins-content", new string[,] { { "task_id", task_id.ToString() }
                    , { "file_id", db_provider.str_val(dr["file_notes_id_ff"]) }
                    , { "title", Path.GetFileNameWithoutExtension(path).ToLower() }, { "extension", Path.GetExtension(path).ToLower() }})
                    , pars: new System.Data.Common.DbParameter[] { 
                  new SqlParameter("@content", System.Data.SqlDbType.VarChar) { Value = txt } });
                  log_debug("letto contenuto file '" + path + "'");
                }

                if (type_info == "notes" && !tasks_notes.Contains(task_id)) {
                  ins_notes(task_id, db_provider.int_val(dr["file_notes_id_ff"]), txt, path, type);
                  tasks_notes.Add(task_id);
                }
              }
            }
          } catch (Exception ex) { log_err(ex.Message); }
        }

        foreach (long task_id in dt.Rows.Cast<DataRow>().Select(r => db_provider.int_val(r["task_id"])).Distinct())
          _conn.exec(_c.parse_query("lib-notes.update-task-readed", new string[,] { { "task_id", task_id.ToString() } }));
      */

        seconds = (int)(DateTime.Now - start).TotalSeconds;
      } catch (Exception ex) { log_err(ex.Message); } finally {
        status_txt("");
      }
    }

    protected bool ins_notes(long task_id, long file_id, string content, string path, string type) {
      if (type == "info") {
        _conn.exec(_c.parse_query("lib-notes.ins-notes", new string[,] { { "task_id", task_id.ToString() }, { "file_id", file_id.ToString() } })
          , pars: new System.Data.Common.DbParameter[] { 
            new SqlParameter("@content", System.Data.SqlDbType.VarChar) { Value = content } });
        log_debug("lette note task '" + path + "'");
        return true;
      } else if (type == "source") {
        string key_from = "###FROM_NOTES###", key_to = "###TO_NOTES###";
        int from_n = content.IndexOf(key_from), to_n = from_n >= 0 ? content.IndexOf(key_to, from_n + 1) : -1;
        if (from_n >= 0 && to_n > 0) {
          string notes = content.Substring(from_n + key_from.Length, to_n - from_n - key_from.Length - 1).Trim(new char[] { ' ', '\n', '\r' });
          _conn.exec(_c.parse_query("lib-notes.ins-notes", new string[,] { { "task_id", task_id.ToString() }, { "file_id", file_id.ToString() } })
            , pars: new System.Data.Common.DbParameter[] { 
            new SqlParameter("@content", System.Data.SqlDbType.VarChar) { Value = notes } });
          log_debug("lette note task '" + path + "'");
          return true;
        }
      } 
      return false;
    }

    protected synch_results init_synch_folder(int synch_folder_id, string path
      , List<string> users, List<free_label> labels
      , long? parent_id = null, task parent_task = null, synch_results res = null) {
      if (res == null) res = new synch_results();
      try {
        // folders
        foreach (string fp in Directory.EnumerateDirectories(path)) {
          DirectoryInfo di = new DirectoryInfo(fp);

          // folder
          string tp; int cc = 0;
          long folder_id = _s.ins_folder(synch_folder_id, parent_id, di.Name, di.CreationTime, di.LastWriteTime, out tp, out cc);
          if (tp == "insert") log_debug("added folder: " + fp);
          else if (tp == "update" && cc > 0) log_debug("updated folder: " + fp);

          // task
          task t = null;
          if (parent_task == null) {
            t = task.parse_task(synch_folder_id, fp, di.CreationTime, di.LastWriteTime, users, labels, folder_id: folder_id);
            if (t != null) {
              t.id = _s.ins_task(t, out tp, out cc);
              if (tp == "insert") log_debug("added task: " + Path.Combine(path, t.title));
              else if (tp == "update" && cc > 0) log_debug("updated task: " + Path.Combine(path, t.title));
            }
          }
          res.folders++;

          res = init_synch_folder(synch_folder_id, Path.Combine(path, di.Name), users, labels, folder_id, t != null ? t : parent_task, res);
        }

        // files
        foreach (string fn in Directory.EnumerateFiles(path)) {
          FileInfo fi = new FileInfo(fn);

          // file
          string tp; int cc = 0;
          long file_id = _s.ins_file(synch_folder_id, parent_id, fi.Name, fi.Extension, fi.CreationTime, fi.LastWriteTime, out tp, out cc);
          if (tp == "insert") log_debug("added file " + file_id.ToString() + ": " + fn);
          else if (tp == "update" && cc > 0) log_debug("updated file " + file_id.ToString() + ": " + fn);

          // task
          if (parent_task == null) {
            task t = task.parse_task(synch_folder_id, fn, fi.CreationTime, fi.LastWriteTime, users, labels, file_id: file_id);
            _s.ins_task(t, out tp, out cc);
            if (tp == "insert") log_debug("added task: " + Path.Combine(path, t.title));
            else if (tp == "update" && cc > 0) log_debug("updated task: " + Path.Combine(path, t.title));
          }
          res.files++;
        }

        // task folder - dt_upd
        if (parent_task != null) {
          string cc = _conn.exec(_c.parse_query("lib-notes.upd-task-date", new string[,] { { "task_id", parent_task.id.ToString() } }));
          if (cc != "0") log_debug("updated task: " + Path.Combine(Path.GetDirectoryName(path), parent_task.title));
        }

      } catch (Exception ex) { log_err(ex.Message); }
      return res;
    }

    #endregion

  }

  public class synch_results {
    public int folders { get; set; }
    public int files { get; set; }
    public synch_results() { this.folders = this.files = 0; }
  }
}

/*

var _watcher = new FileSystemWatcher();
_watcher.Path = @"\\10.31.2.221\shared\";
_watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
_watcher.Filter = "*.txt";
_watcher.Created += new FileSystemEventHandler((x, y) =>Console.WriteLine("Created"));
_watcher.Error += new ErrorEventHandler( (x, y) =>Console.WriteLine("Error"));
_watcher.EnableRaisingEvents = true;
Console.ReadKey();
 
 NET USE Z: \\server2\Secondary\temp\watch_folder /user:Domain\UserName Password
 
 */