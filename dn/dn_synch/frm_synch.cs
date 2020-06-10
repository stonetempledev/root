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
using dn_lib;
using mlib;
using mlib.tools;
using mlib.db;
using mlib.xml;

namespace fsynch {
  public partial class frm_synch : Form {

    protected core _c = null;
    protected db_provider _conn = null;
    protected synch_machine _sm = null;

    // tasks
    protected DateTime? _loaded = null;
    protected List<string> _users = null;
    protected Dictionary<string, string> _states = null;

    // synch folders
    synch _s = null;
    List<synch_folder> _folders = null;
    protected bool _synch = false;

    // mouse move
    protected bool _md; private Point _ll;

    // lista messaggi
    protected Font _fbold;
    protected int _max_rows = 1500, _min_rows = 1200;

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
          _sm = null;
          tmr_synch.Interval = 25 * 1000;
          tmr_synch.Start();
          return;
        }
        if (_sm.state != synch_machine.states.none) {
          log_war("c'è già un deepa synch attivo sul pc '" + Environment.MachineName + "', controllare la tabella synch_machines o i processi attivi su quella macchina!");
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

    protected void add_msg_err(string txt) {
      try {
        lw_msg.Items.Add(new ListViewItem() { Text = txt, ForeColor = Color.Tomato, Font = _fbold });
        check_max_rows(); scroll_down(); Application.DoEvents();
      } catch { }
    }

    protected void add_msg_warning(string txt) {
      try {
        lw_msg.Items.Add(new ListViewItem() { Text = txt, ForeColor = Color.Orange, Font = _fbold });
        check_max_rows(); scroll_down(); Application.DoEvents();
      } catch { }
    }

    protected void scroll_down() { if (lw_msg.Items.Count > 0) lw_msg.Items[lw_msg.Items.Count - 1].EnsureVisible(); }

    protected void check_max_rows() {
      if (lw_msg.Items.Count > _max_rows) {
        for (int n = 0; n < lw_msg.Items.Count - _min_rows; n++)
          lw_msg.Items[0].Remove();
      }
    }

    protected void log_debug(string txt) { try { log.log_info(txt); add_msg(txt); } catch { } }
    protected void log_err(string txt) { try { log.log_err(txt); add_msg_err(txt); } catch { } }
    protected void log_war(string txt) { try { log.log_warning(txt); add_msg_warning(txt); } catch { } }

    protected void add_msg(string txt) {
      try {
        lw_msg.Items.Add(new ListViewItem() { Text = txt });
        check_max_rows(); scroll_down(); Application.DoEvents();
      } catch { }
    }

    private void frm_synch_Load(object sender, EventArgs e) {
      try {
        btn_max.Tag = "max";
        btn_max.Text = "\u2610";
        enable_double_buffering(lw_msg);
        _fbold = new Font(lw_msg.Font.FontFamily, lw_msg.Font.Size, FontStyle.Bold);
        resize_col();
      } catch (Exception ex) { MessageBox.Show(ex.Message, "Attenzione!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void lw_msg_Resize(object sender, EventArgs e) { try { resize_col(); } catch { } }

    protected void resize_col() { try { lw_msg.Columns[0].Width = lw_msg.ClientSize.Width - 5; } catch { } }

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
          reload_folders(true);
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

    private void btn_min_Click(object sender, EventArgs e) {
      try {
        if (btn_max.Tag.ToString() == "max") {
          this.WindowState = FormWindowState.Maximized;
          btn_max.Tag = "normal";
        } else {
          this.WindowState = FormWindowState.Normal;
          btn_max.Tag = "max";
        }
      } catch { }
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
      if (!_loaded.HasValue || (_loaded.HasValue && (DateTime.Now - _loaded.Value).TotalMinutes > 5)) {
        _loaded = DateTime.Now;

        _users = main._conn.dt_table(main._c.parse_query("synch.task-users")).Rows.Cast<DataRow>()
          .Select(r => db_provider.str_val(r["nome"])).ToList();

        _states = main._conn.dt_table(main._c.parse_query("synch.task-states")).AsEnumerable()
          .ToDictionary<DataRow, string, string>(r => db_provider.str_val(r["free_txt"]), r => db_provider.str_val(r["task_state"]));
      }


      int seconds, folders, files, deleted;
      reload_folders(_users, _states, out seconds, out folders, out files, out deleted, first);
      if (_sm != null) _s.last_synch_machine(_sm.synch_machine_id, folders, files, deleted, seconds);
    }

    protected void reload_folders(List<string> users, Dictionary<string, string> states
      , out int seconds, out int folders, out int files, out int deleted, bool first = false) {
      seconds = folders = files = deleted = 0;
      try {
        DateTime start = DateTime.Now;

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
          res = init_synch_folder(f.id, f.local_path, users, states);
        }
        folders = res.folders; files = res.files;
        deleted = _s.del_unreaded();
        if (deleted > 0) log_debug("cancellati " + deleted.ToString() + " files/folders/tasks");
        seconds = (int)(DateTime.Now - start).TotalSeconds;
      } catch (Exception ex) { log_err(ex.Message); } finally { }
    }

    protected synch_results init_synch_folder(int synch_folder_id, string path
      , List<string> users, Dictionary<string, string> states
      , long? parent_id = null, synch_results res = null) {
      if (res == null) res = new synch_results();
      try {
        // folders
        foreach (string fp in Directory.EnumerateDirectories(path)) {
          DirectoryInfo di = new DirectoryInfo(fp);

          // folder
          string tp;
          long folder_id = _s.ins_folder(synch_folder_id, parent_id, di.Name, out tp);
          if (tp == "insert") log_debug("added folder: " + fp);

          // task
          task t = task.parse_task(synch_folder_id, fp, di.LastWriteTime, users, states, folder_id: folder_id);
          _s.ins_task(t, out tp);
          if (tp == "insert") log_debug("added task: " + t.title);
          res.folders++;

          res = init_synch_folder(synch_folder_id, Path.Combine(path, di.Name), users, states, folder_id, res);
        }

        // files
        foreach (string fn in Directory.EnumerateFiles(path)) {
          FileInfo fi = new FileInfo(fn);

          // file
          string tp;
          long file_id = _s.ins_file(synch_folder_id, parent_id, fi.Name, out tp);
          if (tp == "insert") log_debug("added file: " + fn);

          // task
          task t = task.parse_task(synch_folder_id, fn, fi.LastWriteTime, users, states, file_id: file_id);
          _s.ins_task(t, out tp);
          if (tp == "insert") log_debug("added task: " + t.title);
          res.files++;
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